#nullable disable
using HarmonyLib;
using Il2Cpp;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using MelonLoader;
using UnityEngine;

namespace BaltaTools
{
    internal static class SplittingAxeRegistry
    {
        private const string SplittingAxeName = "GEAR_SplittingAxe";

        private const float BreakDownTimeModifier = 0.25f;
        private const float IceFishingHPDecreaseToClear = 2.5f;
        private const int IceFishingMinutesToClear = 15;
        private static GameObject _splittingAxeGameObject;

        public static GameObject GetSplittingAxeGameObject()
        {
            if (_splittingAxeGameObject != null)
            {
                return _splittingAxeGameObject;
            }

            Il2CppArrayBase<GearItem>allGearItems = Resources.FindObjectsOfTypeAll<GearItem>();

            GameObject firstFound = null;

            foreach (GearItem gearItem in allGearItems)
            {
                if (gearItem == null || gearItem.gameObject == null)
                {
                    continue;
                }

                if (gearItem.gameObject.name != SplittingAxeName)
                {
                    continue;
                }

                Initialize(gearItem);

                if (firstFound == null)
                {
                    firstFound = gearItem.gameObject;
                }
            }

            _splittingAxeGameObject = firstFound;

            return _splittingAxeGameObject;
        }

        public static void Initialize(GearItem gearItem)
        {
            if (gearItem == null || gearItem.gameObject == null)
            {
                return;
            }

            if (gearItem.gameObject.name != SplittingAxeName)
            {
                return;
            }

            _splittingAxeGameObject = gearItem.gameObject;

            //ApplyBreakDownItem(gearItem);
            //ApplyIceFishingHoleClear(gearItem);

            DebugHelper.Log("[BaltaTools] Splitting Axe initialized via Awake. " + "InstanceID=" + gearItem.GetInstanceID());
        }

        // Adding Breakdown and Icefishing components - not needed, because Toolcomponent from modcomponent does this succesfully
        /*private static void ApplyBreakDownItem(GearItem gearItem)
        {
            BreakDownItem breakDownItem = gearItem.m_BreakDownItem;

            if (breakDownItem == null)
            {
                breakDownItem = gearItem.gameObject.GetComponent<BreakDownItem>();
            }

            if (breakDownItem == null)
            {
                breakDownItem = gearItem.gameObject.AddComponent<BreakDownItem>();

                DebugHelper.Log("[BaltaTools] Splitting Axe: " + "BreakDownItem created.");
            }

            breakDownItem.m_BreakDownTimeModifier =
                BreakDownTimeModifier;

            gearItem.m_BreakDownItem =
                breakDownItem;
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

                DebugHelper.Log("[BaltaTools] Splitting Axe: " + "IceFishingHoleClearItem created.");
            }

            iceFishing.m_BreakIceAudio = "Play_IceBreakingChopping";
            iceFishing.m_HPDecreaseToClear = IceFishingHPDecreaseToClear;
            iceFishing.m_NumGameMinutesToClear = IceFishingMinutesToClear;
            gearItem.m_IceFishingHoleClearItem = iceFishing;
        }

        public static void ResetCache()
        {
            _splittingAxeGameObject = null;
        }
    }*/
    }

    [HarmonyPatch(typeof(GearItem), nameof(GearItem.Awake))]
    internal static class GearItem_Awake_SplittingAxe_Patch
    {
        private static void Postfix(GearItem __instance)
        {
            SplittingAxeRegistry.Initialize(__instance);
        }
    }
    internal static class GameObjectArrayInjectorAxe
    {
        public static Il2CppReferenceArray<GameObject>
            AppendIfMissing(Il2CppReferenceArray<GameObject> existing,string requiredExistingName,GameObject toAdd)
        {
            if (existing == null || toAdd == null)
            {
                return existing;
            }

            bool hasRequired = false;

            bool hasNew = false;

            foreach (GameObject go in existing)
            {
                if (go == null)
                {
                    continue;
                }

                if (go.name == requiredExistingName)
                {
                    hasRequired = true;
                }

                if (go == toAdd)
                {
                    hasNew = true;
                }
            }

            if (!hasRequired || hasNew)
            {
                return existing;
            }

            Il2CppReferenceArray<GameObject> newArray = new Il2CppReferenceArray<GameObject>(existing.Length + 1);

            for (int i = 0;
                 i < existing.Length;
                 i++)
            {
                newArray[i] = existing[i];
            }

            newArray[existing.Length] = toAdd;

            return newArray;
        }
    }
    [HarmonyPatch(typeof(BreakDown),"InitializeFilteredUsableTools")]
    internal static class BreakDown_AddSplittingAxe_Patch
    {
        private static void Prefix(BreakDown __instance)
        {
            if (__instance == null)
            {
                return;
            }

            GameObject axe = SplittingAxeRegistry.GetSplittingAxeGameObject();

            if (axe == null)
            {
                return;
            }

            __instance.m_UsableTools = GameObjectArrayInjectorAxe.AppendIfMissing(__instance.m_UsableTools,"GEAR_Hatchet",axe);
        }
    }
    [HarmonyPatch(typeof(Harvestable),"InitializeRequiredTools")]
    internal static class Harvestable_AddSplittingAxe_Patch
    {
        private static void Prefix(Harvestable __instance)
        {
            if (__instance == null)
            {
                return;
            }

            GameObject axe = SplittingAxeRegistry.GetSplittingAxeGameObject();

            if (axe == null)
            {
                return;
            }

            __instance.m_RequiredToolList = GameObjectArrayInjectorAxe.AppendIfMissing(__instance.m_RequiredToolList,"GEAR_Hatchet",axe);
        }
    }
    [HarmonyPatch(typeof(Panel_IceFishingHoleClear),"InitializeFilteredUsableTools")]
    internal static class Panel_IceFishingHoleClear_AddSplittingAxe_Patch
    {
        private static void Prefix(Panel_IceFishingHoleClear __instance)
        {
            if (__instance == null)
            {
                return;
            }

            Il2CppSystem.Collections.Generic.List<GameObject>usableTools = __instance.m_UsableToolItems;

            if (usableTools == null)
            {
                return;
            }

            bool hasHatchet = false;

            bool hasSplittingAxe = false;

            foreach (GameObject go in usableTools)
            {
                if (go == null)
                {
                    continue;
                }

                if (go.name == "GEAR_Hatchet")
                {
                    hasHatchet = true;
                }

                if (go.name == "GEAR_SplittingAxe")
                {
                    hasSplittingAxe = true;
                }
            }

            if (!hasHatchet || hasSplittingAxe)
            {
                return;
            }

            GameObject axe = SplittingAxeRegistry.GetSplittingAxeGameObject();

            if (axe == null)
            {
                return;
            }
            usableTools.Add(axe);
            DebugHelper.Log("[BaltaTools] Splitting Axe added " + "to Ice Fishing tool list.");
        }
    }
}