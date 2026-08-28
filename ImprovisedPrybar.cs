#nullable disable
using HarmonyLib;
using Il2Cpp;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using MelonLoader;
using UnityEngine;

namespace BaltaTools
{
    internal static class ImprovisedPrybarRegistry
    {
        private const string PrybarName = "GEAR_ImprovisedPrybar";
        private const float IceFishingHPDecreaseToClear = 2f;
        private const int IceFishingMinutesToClear = 45;

        private static GameObject _prybarGameObject;

        public static GameObject GetPrybarGameObject()
        {
            if (_prybarGameObject != null)
            {
                return _prybarGameObject;
            }

            Il2CppArrayBase<GearItem>
                allGearItems =
                    Resources.FindObjectsOfTypeAll<GearItem>();

            GameObject firstFound = null;

            foreach (GearItem gearItem in allGearItems)
            {
                if (gearItem == null ||
                    gearItem.gameObject == null)
                {
                    continue;
                }

                if (gearItem.gameObject.name != PrybarName)
                {
                    continue;
                }

                Initialize(gearItem);

                if (firstFound == null)
                {
                    firstFound =
                        gearItem.gameObject;
                }
            }

            _prybarGameObject =
                firstFound;

            return _prybarGameObject;
        }

        public static GearItem GetPrybarGearItem()
        {
            return _prybarGameObject == null ? null : _prybarGameObject.GetComponent<GearItem>();
        }

        public static void Initialize(GearItem gearItem)
        {
            if (gearItem == null || gearItem.gameObject == null) return;
            if (gearItem.gameObject.name != PrybarName) return;

            _prybarGameObject = gearItem.gameObject;
            ApplyForceLockItem(gearItem);
            ApplyIceFishingHoleClear(gearItem);
            DebugHelper.Log("[BaltaTools] Improvised Prybar initialized via Awake. InstanceID=" + gearItem.GetInstanceID());
        }

        private static void ApplyForceLockItem(GearItem gearItem)
        {
            ForceLockItem forceLockItem = gearItem.m_ForceLockItem;
            if (forceLockItem == null)
            {
                forceLockItem = gearItem.gameObject.GetComponent<ForceLockItem>();
            }
            if (forceLockItem == null)
            {
                forceLockItem = gearItem.gameObject.AddComponent<ForceLockItem>();
                DebugHelper.Log("[BaltaTools] " + PrybarName + ": ForceLockItem component added.");
            }

            forceLockItem.m_ForceLockAudio = "PLAY_LOCKERPRYOPEN1";
            forceLockItem.m_LocalizedProgressText = new LocalizedString() { m_LocalizationID = "GAMEPLAY_Forcing" };
            gearItem.m_ForceLockItem = forceLockItem;
        }

        private static void ApplyIceFishingHoleClear(GearItem gearItem)
        {
            IceFishingHoleClearItem iceFishing = gearItem.m_IceFishingHoleClearItem;
            if (iceFishing == null)
            {
                iceFishing = gearItem.gameObject.GetComponent<IceFishingHoleClearItem>();
            }
            if (iceFishing == null)
            {
                iceFishing = gearItem.gameObject.AddComponent<IceFishingHoleClearItem>();
                DebugHelper.Log("[BaltaTools] " + PrybarName + ": IceFishingHoleClearItem created.");
            }

            iceFishing.m_BreakIceAudio = "Play_IceBreakingChopping";
            iceFishing.m_HPDecreaseToClear = IceFishingHPDecreaseToClear;
            iceFishing.m_NumGameMinutesToClear = IceFishingMinutesToClear;
            gearItem.m_IceFishingHoleClearItem = iceFishing;
        }

        public static void ResetCache()
        {
            _prybarGameObject = null;
        }
        public static void EnsurePrybarComponents(GameObject targetGameObject)
        {
            if (targetGameObject == null)
            {
                return;
            }


            GearItem gearItem =
                targetGameObject.GetComponent<GearItem>();

            if (gearItem == null)
            {
                return;
            }


            ApplyForceLockItem(
                gearItem);

            ApplyIceFishingHoleClear(
                gearItem);
        }
    }

    [HarmonyPatch(typeof(GearItem), nameof(GearItem.Awake))]
    internal static class GearItem_Awake_ImprovisedPrybar_Patch
    {
        private static void Postfix(GearItem __instance)
        {
            ImprovisedPrybarRegistry.Initialize(__instance);
        }
    }

    [HarmonyPatch(typeof(Panel_IceFishingHoleClear), "InitializeFilteredUsableTools")]
    internal static class Panel_IceFishingHoleClear_AddPrybar_Patch
    {
        private static void Prefix(Panel_IceFishingHoleClear __instance)
        {
            Il2CppSystem.Collections.Generic.List<GameObject> usableTools = __instance.m_UsableToolItems;
            if (usableTools == null) return;

            bool hasHatchetType = false;
            bool hasPrybar = false;
            foreach (GameObject go in usableTools)
            {
                if (go == null) continue;
                if (go.name == "GEAR_Hatchet") hasHatchetType = true;
                if (go.name == "GEAR_ImprovisedPrybar") hasPrybar = true;
            }

            if (!hasHatchetType || hasPrybar) return;

            GameObject prybarGo = ImprovisedPrybarRegistry.GetPrybarGameObject();
            if (prybarGo != null) usableTools.Add(prybarGo);
        }
    }

    internal static class ImprovisedPrybarForceLock
    {
        private const string VanillaPrybarName = "GEAR_Prybar";

        public static GearItem GetImprovisedPrybarFromInventory()
        {
            Inventory inventory = GameManager.GetInventoryComponent();

            if (inventory == null)
            {
                return null;
            }


            GearItem improvisedPrybar = inventory.GetHighestConditionGearThatMatchesName("GEAR_ImprovisedPrybar");


            if (improvisedPrybar != null && improvisedPrybar.gameObject != null)
            {
                ImprovisedPrybarRegistry.EnsurePrybarComponents(improvisedPrybar.gameObject);
            }

            return improvisedPrybar;
        }

        public static bool IsPrybarLock(Lock lockInstance)
        {
            if (lockInstance == null) return false;
            GearItem requiredTool = lockInstance.m_GearPrefabToForceLock;
            if (requiredTool == null || requiredTool.gameObject == null) return false;
            return requiredTool.gameObject.name == VanillaPrybarName;
        }
    }


    [HarmonyPatch(typeof(Lock), nameof(Lock.PlayerHasRequiredToolToUnlock))]
    internal static class Lock_PlayerHasRequiredToolToUnlock_Patch
    {
        private static void Postfix(Lock __instance, ref bool __result)
        {
            if (__result || !ImprovisedPrybarForceLock.IsPrybarLock(__instance)) return;
            GearItem improvisedPrybar = ImprovisedPrybarForceLock.GetImprovisedPrybarFromInventory();
            if (improvisedPrybar == null) return;
            __result = true;
        }
    }

    [HarmonyPatch(typeof(Lock), nameof(Lock.CanForceLock))]
    internal static class Lock_CanForceLock_Patch
    {
        private static void Postfix(Lock __instance, ref bool __result)
        {
            if (__result || !ImprovisedPrybarForceLock.IsPrybarLock(__instance)) return;
            GearItem improvisedPrybar = ImprovisedPrybarForceLock.GetImprovisedPrybarFromInventory();
            if (improvisedPrybar == null) return;
            __result = true;
        }
    }

    [HarmonyPatch(typeof(Lock), nameof(Lock.GetGearItemToForceLock))]
    internal static class Lock_GetGearItemToForceLock_Patch
    {
        private static void Postfix(Lock __instance, ref GearItem __result)
        {
            if (!ImprovisedPrybarForceLock.IsPrybarLock(__instance)) return;
            GearItem improvisedPrybar = ImprovisedPrybarForceLock.GetImprovisedPrybarFromInventory();
            if (improvisedPrybar == null) return;
            __result = improvisedPrybar;
        }
    }
}