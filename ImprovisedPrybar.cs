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
        private const float IceFishingHPDecreaseToClear = 1.6f;
        private const int IceFishingMinutesToClear = 40;
        private static GameObject _representativeGameObject;
        private static bool _loggedMissing;

        public static GameObject GetPrybarGameObject()
        {
            if (_representativeGameObject != null)
            {
                EnsurePrybarComponents(_representativeGameObject);

                return _representativeGameObject;
            }

            Il2CppArrayBase<GearItem> allGearItems = Resources.FindObjectsOfTypeAll<GearItem>();

            foreach (GearItem gearItem in allGearItems)
            {
                if (gearItem == null ||
                    gearItem.gameObject == null ||
                    gearItem.gameObject.name != PrybarName)
                {
                    continue;
                }

                _representativeGameObject = gearItem.gameObject;

                EnsurePrybarComponents(_representativeGameObject);

                return _representativeGameObject;
            }

            if (!_loggedMissing)
            {
                MelonLogger.Warning($"[BaltaTools] {PrybarName} még sehol nem található.");

                _loggedMissing = true;
            }

            return null;
        }

        public static GearItem GetPrybarGearItem()
        {
            GameObject prybar = GetPrybarGameObject();

            if (prybar == null)
            {
                return null;
            }

            return prybar.GetComponent<GearItem>();
        }

        public static void EnsurePrybarComponents(GameObject targetGameObject)
        {
            if (targetGameObject == null)
            {
                return;
            }

            GearItem gearItem = targetGameObject.GetComponent<GearItem>();

            if (gearItem == null)
            {
                return;
            }

            ApplyForceLockItem(gearItem);
            ApplyIceFishingHoleClear(gearItem);
        } 

        private static void ApplyForceLockItem(GearItem gearItem)
        {
            DebugHelper.Log($"[BaltaTools] ApplyForceLockItem() called for " + $"{gearItem.gameObject.name}");

            ForceLockItem forceLockItem =
                gearItem.m_ForceLockItem;

            if (forceLockItem == null)
            {
                forceLockItem =
                    gearItem.gameObject.GetComponent<ForceLockItem>();

                DebugHelper.Log($"[BaltaTools] GetComponent<ForceLockItem>() = " + $"{(forceLockItem == null ? "NULL" : "FOUND")}");
            }

            if (forceLockItem == null)
            {
                forceLockItem = gearItem.gameObject.AddComponent<ForceLockItem>();

                DebugHelper.Log($"[BaltaTools] {PrybarName}: " + "ForceLockItem component hozzáadva.");
            }

            forceLockItem.m_ForceLockAudio = "PLAY_LOCKERPRYOPEN1";

            forceLockItem.m_LocalizedProgressText = new LocalizedString()
                {
                    m_LocalizationID = "GAMEPLAY_Forcing"
                };

            gearItem.m_ForceLockItem = forceLockItem;

            DebugHelper.Log($"[BaltaTools] {PrybarName}: ForceLockItem beállítva. " + $"GearItem.m_ForceLockItem != null = " + $"{(gearItem.m_ForceLockItem != null)}");
        }

        private static void ApplyIceFishingHoleClear(
            GearItem gearItem)
        {
            IceFishingHoleClearItem iceFishingHoleClearItem = gearItem.m_IceFishingHoleClearItem;

            if (iceFishingHoleClearItem == null)
            {
                iceFishingHoleClearItem = gearItem.gameObject.GetComponent<IceFishingHoleClearItem>();
            }

            if (iceFishingHoleClearItem == null)
            {
                iceFishingHoleClearItem = gearItem.gameObject.AddComponent<IceFishingHoleClearItem>();

                DebugHelper.Log($"[BaltaTools] {PrybarName}: " + "IceFishingHoleClearItem létrehozva.");
            }

            iceFishingHoleClearItem.m_BreakIceAudio = "Play_IceBreakingChopping";

            iceFishingHoleClearItem.m_HPDecreaseToClear = IceFishingHPDecreaseToClear;

            iceFishingHoleClearItem.m_NumGameMinutesToClear = IceFishingMinutesToClear;

            gearItem.m_IceFishingHoleClearItem = iceFishingHoleClearItem;
        }
        public static void ResetCache()
        {
            _representativeGameObject = null;
            _loggedMissing = false;
        }
    }

    [HarmonyPatch(typeof(Panel_IceFishingHoleClear), "InitializeFilteredUsableTools")]
    public static class Panel_IceFishingHoleClear_AddPrybar_Patch
    {
        static void Prefix(Panel_IceFishingHoleClear __instance)
        {
            Il2CppSystem.Collections.Generic.List<GameObject> usableTools = __instance.m_UsableToolItems;
            if (usableTools == null)
            {
                return;
            }

            bool hasHatchetType = false;
            bool hasPrybar = false;

            foreach (GameObject go in usableTools)
            {
                if (go == null)
                {
                    continue;
                }

                if (go.name == "GEAR_Hatchet")
                {
                    hasHatchetType = true;
                }

                if (go.name == "GEAR_ImprovisedPrybar")
                {
                    hasPrybar = true;
                }
            }

            if (!hasHatchetType || hasPrybar)
            {
                return;
            }

            GameObject prybarGo = ImprovisedPrybarRegistry.GetPrybarGameObject();
            if (prybarGo != null)
            {
                usableTools.Add(prybarGo);
            }
        }
    }
    internal static class ImprovisedPrybarForceLock
    {
        private const string VanillaPrybarName = "GEAR_Prybar";
        private const string ImprovisedPrybarName = "GEAR_ImprovisedPrybar";

        public static GearItem GetImprovisedPrybarFromInventory()
        {
            ImprovisedPrybarRegistry.GetPrybarGameObject();

            GearItem improvisedPrybar = GameManager.GetInventoryComponent().GetHighestConditionGearThatMatchesName("GEAR_ImprovisedPrybar");

            if (improvisedPrybar != null)
            {
                ImprovisedPrybarRegistry.EnsurePrybarComponents(
                    improvisedPrybar.gameObject);
            }

            return improvisedPrybar;
        }

        public static bool IsPrybarLock(Lock lockInstance)
        {
            if (lockInstance == null)
            {
                return false;
            }

            GearItem requiredTool =
                lockInstance.m_GearPrefabToForceLock;

            if (requiredTool == null)
            {
                return false;
            }

            return requiredTool.gameObject != null &&
                   requiredTool.gameObject.name == VanillaPrybarName;
        }
    }


    [HarmonyPatch(typeof(Lock),nameof(Lock.PlayerHasRequiredToolToUnlock))]
    internal static class Lock_PlayerHasRequiredToolToUnlock_Patch
    {
        private static void Postfix(Lock __instance, ref bool __result)
        {
            if (__result)
            {
                return;
            }

            if (!ImprovisedPrybarForceLock.IsPrybarLock(__instance))
            {
                return;
            }

            GearItem improvisedPrybar = ImprovisedPrybarForceLock.GetImprovisedPrybarFromInventory();

            if (improvisedPrybar == null)
            {
                return;
            }

            __result = true;

            DebugHelper.Log($"[BaltaTools] Improvised prybar accepted for lock: " + $"{__instance.gameObject.name}");
        }
    }


    [HarmonyPatch(typeof(Lock),nameof(Lock.CanForceLock))]
    internal static class Lock_CanForceLock_Patch
    {
        private static void Postfix(
            Lock __instance,
            ref bool __result)
        {
            if (__result)
            {
                return;
            }

            if (!ImprovisedPrybarForceLock.IsPrybarLock(__instance))
            {
                return;
            }

            GearItem improvisedPrybar = ImprovisedPrybarForceLock.GetImprovisedPrybarFromInventory();

            if (improvisedPrybar == null)
            {
                return;
            }

            __result = true;

            DebugHelper.Log($"[BaltaTools] CanForceLock overridden for: " + $"{__instance.gameObject.name}");
        }
    }


    [HarmonyPatch(typeof(Lock),nameof(Lock.GetGearItemToForceLock))]
    internal static class Lock_GetGearItemToForceLock_Patch
    {
        private static void Postfix(
            Lock __instance,
            ref GearItem __result)
        {
            if (!ImprovisedPrybarForceLock.IsPrybarLock(__instance))
            {
                return;
            }

            GearItem improvisedPrybar = ImprovisedPrybarForceLock.GetImprovisedPrybarFromInventory();

            if (improvisedPrybar == null)
            {
                return;
            }

            __result = improvisedPrybar;

            DebugHelper.Log($"[BaltaTools] ForceLock tool replaced with: " + $"{improvisedPrybar.gameObject.name}");
        }
    }
}