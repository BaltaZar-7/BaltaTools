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

        private static GameObject _representativeGameObject;
        private static bool _loggedMissing;

        public static GameObject GetSplittingAxeGameObject()
        {
            bool foundAny = false;

            Il2CppArrayBase<GearItem> allGearItems = Resources.FindObjectsOfTypeAll<GearItem>();
            foreach (GearItem gearItem in allGearItems)
            {
                if (gearItem == null || gearItem.gameObject.name != SplittingAxeName)
                {
                    continue;
                }

                ApplyBreakDownTimeModifier(gearItem);
                ApplyIceFishingHoleClear(gearItem);

                _representativeGameObject = gearItem.gameObject;
                foundAny = true;
            }

            if (!foundAny)
            {
                if (!_loggedMissing)
                {
                    MelonLogger.Warning($"[BaltaTools] {SplittingAxeName} még sehol nem található.");
                    _loggedMissing = true;
                }

                return null;
            }

            return _representativeGameObject;
        }

        private static void ApplyBreakDownTimeModifier(GearItem gearItem)
        {
            BreakDownItem breakDownItem = gearItem.m_BreakDownItem;

            if (breakDownItem == null)
            {
                breakDownItem = gearItem.gameObject.GetComponent<BreakDownItem>();
            }

            if (breakDownItem == null)
            {
                breakDownItem = gearItem.gameObject.AddComponent<BreakDownItem>();
            }

            breakDownItem.m_BreakDownTimeModifier = BreakDownTimeModifier;
        }

        private static void ApplyIceFishingHoleClear(GearItem gearItem)
        {
            IceFishingHoleClearItem iceFishingHoleClearItem = gearItem.m_IceFishingHoleClearItem;

            if (iceFishingHoleClearItem == null)
            {
                iceFishingHoleClearItem = gearItem.gameObject.GetComponent<IceFishingHoleClearItem>();
            }

            if (iceFishingHoleClearItem == null)
            {
                iceFishingHoleClearItem = gearItem.gameObject.AddComponent<IceFishingHoleClearItem>();
            }

            iceFishingHoleClearItem.m_BreakIceAudio = "Play_IceBreakingChopping";
            iceFishingHoleClearItem.m_HPDecreaseToClear = IceFishingHPDecreaseToClear;
            iceFishingHoleClearItem.m_NumGameMinutesToClear = IceFishingMinutesToClear;
        }
        public static void ResetCache()
        {
            _representativeGameObject = null;
            _loggedMissing = false;
        }
    }

    internal static class GameObjectArrayInjector
    {
        public static Il2CppReferenceArray<GameObject> AppendIfMissing(
            Il2CppReferenceArray<GameObject> existing,
            string requiredExistingName,
            GameObject toAdd)
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
            for (int i = 0; i < existing.Length; i++)
            {
                newArray[i] = existing[i];
            }
            newArray[existing.Length] = toAdd;
            return newArray;
        }
    }

    [HarmonyPatch(typeof(BreakDown), "InitializeFilteredUsableTools")]
    public static class BreakDown_AddSplittingAxe_Patch
    {
        static void Prefix(BreakDown __instance)
        {
            GameObject axeGo = SplittingAxeRegistry.GetSplittingAxeGameObject();
            __instance.m_UsableTools = GameObjectArrayInjector.AppendIfMissing(__instance.m_UsableTools, "GEAR_Hatchet", axeGo);
        }
    }

    [HarmonyPatch(typeof(Harvestable), "InitializeRequiredTools")]
    public static class Harvestable_AddSplittingAxe_Patch
    {
        static void Prefix(Harvestable __instance)
        {
            GameObject axeGo = SplittingAxeRegistry.GetSplittingAxeGameObject();
            __instance.m_RequiredToolList = GameObjectArrayInjector.AppendIfMissing(__instance.m_RequiredToolList, "GEAR_Hatchet", axeGo);
        }
    }

    [HarmonyPatch(typeof(Panel_IceFishingHoleClear), "InitializeFilteredUsableTools")]
    public static class Panel_IceFishingHoleClear_AddSplittingAxe_Patch
    {
        static void Prefix(Panel_IceFishingHoleClear __instance)
        {
            Il2CppSystem.Collections.Generic.List<GameObject> usableTools = __instance.m_UsableToolItems;
            if (usableTools == null)
            {
                return;
            }

            bool hasHatchetType = false;
            bool hasAxeType = false;

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

                if (go.name == "GEAR_SplittingAxe")
                {
                    hasAxeType = true;
                }
            }

            if (!hasHatchetType || hasAxeType)
            {
                return;
            }

            GameObject axeGo = SplittingAxeRegistry.GetSplittingAxeGameObject();
            if (axeGo != null)
            {
                usableTools.Add(axeGo);
            }
        }
    }
}