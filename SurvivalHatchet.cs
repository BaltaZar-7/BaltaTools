#nullable disable
using HarmonyLib;
using Il2Cpp;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppTLD.Gear;
using MelonLoader;
using UnityEngine;

namespace BaltaTools
{
    internal static class SurvivalHatchetRegistry
    {
        private const string SurvivalHatchetName = "GEAR_SurvivalHatchet";

        private const float BreakDownTimeModifier = 0.45f;
        private const float IceFishingHPDecreaseToClear = 2.5f;
        private const int IceFishingMinutesToClear = 20;
        private const string IceFishingBreakIceAudio = "Play_IceBreakingChopping";

        private static GameObject _survivalHatchetGameObject;

        public static void Initialize(GearItem gearItem)
        {
            if (gearItem == null || gearItem.gameObject == null)
            {
                return;
            }

            if (gearItem.gameObject.name != SurvivalHatchetName)
            {
                return;
            }

            _survivalHatchetGameObject = gearItem.gameObject;

            ApplyToolsItemValues(gearItem);
            ApplyCanOpeningItem(gearItem);
            //ApplyBreakDownItem(gearItem);
            //ApplyIceFishingHoleClear(gearItem);

            DebugHelper.Log(
                "[BaltaTools] Survival Hatchet initialized. " +
                "InstanceID=" +
                gearItem.GetInstanceID());
        }

        // FALLBACK

        public static GameObject
            GetSurvivalHatchetGameObject()
        {
            if (_survivalHatchetGameObject != null)
            {
                return _survivalHatchetGameObject;
            }

            Il2CppArrayBase<GearItem>allGearItems = Resources.FindObjectsOfTypeAll<GearItem>();

            GameObject firstFound = null;

            foreach (GearItem gearItem in allGearItems)
            {
                if (gearItem == null || gearItem.gameObject == null)
                {
                    continue;
                }

                if (gearItem.gameObject.name != SurvivalHatchetName)
                {
                    continue;
                }

                Initialize(gearItem);

                if (firstFound == null)
                {
                    firstFound = gearItem.gameObject;
                }
            }
            _survivalHatchetGameObject = firstFound;
            return _survivalHatchetGameObject;
        }

        // ToolsItem

        private static void
            ApplyToolsItemValues(GearItem survivalHatchet)
        {
            if (survivalHatchet == null || survivalHatchet.gameObject == null)
            {
                return;
            }

            ToolsItem toolsItem = survivalHatchet.m_ToolsItem;

            if (toolsItem == null)
            {
                toolsItem = survivalHatchet.gameObject.GetComponent<ToolsItem>();
            }


            if (toolsItem == null)
            {
                MelonLogger.Warning("[BaltaTools] Survival Hatchet: " + "ToolsItem not found.");
                return;
            }

            toolsItem.m_CraftingAndRepairSkillModifier = 0f;
            toolsItem.m_CraftingAndRepairTimeModifier = 0.75f;
            toolsItem.m_DegradePerHourCrafting = 1f;

            survivalHatchet.m_ToolsItem = toolsItem;
        }

        // Adding Breakdown and Icefishing components - not needed, because Toolcomponent from modcomponent does this succesfully
        /*private static void ApplyBreakDownItem(GearItem survivalHatchet)
        {
            if (survivalHatchet == null || survivalHatchet.gameObject == null)
            {
                return;
            }

            BreakDownItem breakDownItem = survivalHatchet.m_BreakDownItem;


            if (breakDownItem == null)
            {
                breakDownItem = survivalHatchet.gameObject.GetComponent<BreakDownItem>();
            }

            if (breakDownItem == null)
            {
                breakDownItem = survivalHatchet.gameObject.AddComponent<BreakDownItem>();


                DebugHelper.Log("[BaltaTools] Survival Hatchet: " + "BreakDownItem created.");
            }

            breakDownItem.m_BreakDownTimeModifier = BreakDownTimeModifier;

            survivalHatchet.m_BreakDownItem = breakDownItem;
        }


        // Ice Fishing - Same, not needed

        private static void ApplyIceFishingHoleClear(GearItem survivalHatchet)
        {
            if (survivalHatchet == null || survivalHatchet.gameObject == null)
            {
                return;
            }

            IceFishingHoleClearItem
                iceFishing = survivalHatchet.m_IceFishingHoleClearItem;


            if (iceFishing == null)
            {
                iceFishing = survivalHatchet.gameObject.GetComponent<IceFishingHoleClearItem>();
            }


            if (iceFishing == null)
            {
                iceFishing = survivalHatchet.gameObject.AddComponent<IceFishingHoleClearItem>();

                DebugHelper.Log("[BaltaTools] Survival Hatchet: " + "IceFishingHoleClearItem created.");
            }

            iceFishing.m_BreakIceAudio = IceFishingBreakIceAudio;
            iceFishing.m_HPDecreaseToClear = IceFishingHPDecreaseToClear;
            iceFishing.m_NumGameMinutesToClear = IceFishingMinutesToClear;

            survivalHatchet.m_IceFishingHoleClearItem = iceFishing;
        }*/

        // Can Opening

        private static void
            ApplyCanOpeningItem(GearItem survivalHatchet)
        {
            if (survivalHatchet == null || survivalHatchet.gameObject == null)
            {
                return;
            }

            CanOpeningItem canOpening = survivalHatchet.m_CanOpeningItem;

            if (canOpening == null)
            {
                canOpening = survivalHatchet.gameObject.GetComponent<CanOpeningItem>();
            }

            if (canOpening == null)
            {
                canOpening = survivalHatchet.gameObject.AddComponent<CanOpeningItem>();

                DebugHelper.Log("[BaltaTools] Survival Hatchet: " + "CanOpeningItem created.");
            }


            // TODO:
            // Ezeket a vanilla Hatchet dumpjából állítsuk be.
            //
            // canOpening.m_Type = ...;
            // canOpening.m_CanOpeningAudio = "...";
            // canOpening.m_CanOpeningLengthSeconds = ...;


            survivalHatchet.m_CanOpeningItem = canOpening;
        }

        public static void ResetCache()
        {
            _survivalHatchetGameObject = null;
        }
    }
    internal static class GameObjectArrayInjectorHatchet
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

            Il2CppReferenceArray<GameObject> newArray = new Il2CppReferenceArray<GameObject>(
                    existing.Length + 1);

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
    internal static class BreakDown_AddSurvivalHatchet_Patch
    {
        private static void Prefix(BreakDown __instance)
        {
            if (__instance == null)
            {
                return;
            }

            GameObject hatchet = SurvivalHatchetRegistry.GetSurvivalHatchetGameObject();

            if (hatchet == null)
            {
                return;
            }

            __instance.m_UsableTools = GameObjectArrayInjectorHatchet.AppendIfMissing(__instance.m_UsableTools,"GEAR_Hatchet",hatchet);
        }
    }


    [HarmonyPatch(typeof(Harvestable),"InitializeRequiredTools")]
    internal static class Harvestable_AddSurvivalHatchet_Patch
    {
        private static void Prefix(Harvestable __instance)
        {
            if (__instance == null)
            {
                return;
            }

            GameObject hatchet = SurvivalHatchetRegistry.GetSurvivalHatchetGameObject();

            if (hatchet == null)
            {
                return;
            }

            __instance.m_RequiredToolList = GameObjectArrayInjectorHatchet.AppendIfMissing(__instance.m_RequiredToolList,"GEAR_Hatchet",hatchet);
        }
    }


    [HarmonyPatch(typeof(Panel_IceFishingHoleClear),"InitializeFilteredUsableTools")]
    internal static class Panel_IceFishingHoleClear_AddSurvivalHatchet_Patch
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
            bool hasSurvivalHatchet = false;

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

                if (go.name == "GEAR_SurvivalHatchet")
                {
                    hasSurvivalHatchet = true;
                }
            }

            if (!hasHatchet || hasSurvivalHatchet)
            {
                return;
            }

            GameObject hatchet = SurvivalHatchetRegistry.GetSurvivalHatchetGameObject();

            if (hatchet == null)
            {
                return;
            }

            usableTools.Add(hatchet);

            DebugHelper.Log("[BaltaTools] Survival Hatchet added " + "to Ice Fishing tool list.");
        }
    }


    [HarmonyPatch(typeof(GearItem),nameof(GearItem.Awake))]
    internal static class GearItem_Awake_SurvivalHatchet_Patch
    {
        private static void Postfix(GearItem __instance)
        {
            SurvivalHatchetRegistry.Initialize(__instance);
        }
    }

    // Add to blueprints where vanilla hatchet can also be used
    [HarmonyPatch(typeof(BlueprintData),"InitializeOptionalTools")]
    internal static class BlueprintData_InitializeOptionalTools_SurvivalHatchet_Patch
    {
        private static void Postfix(BlueprintData __instance)
        {
            if (__instance == null)
            {
                return;
            }

            ToolsItem requiredTool = __instance.m_RequiredTool;

            if (requiredTool == null || requiredTool.gameObject == null)
            {
                return;
            }

            bool vanillaHatchetFound = requiredTool.gameObject.name == "GEAR_Hatchet";


            Il2CppSystem.Collections.Generic.List<ToolsItem>list = __instance.m_FilteredOptionalTools;

            if (list == null)
            {
                return;
            }


            for (int i = 0;
                 i < list.Count;
                 i++)
            {
                ToolsItem existing = list[i];

                if (existing == null ||
                    existing.gameObject == null)
                {
                    continue;
                }

                if (existing.gameObject.name == "GEAR_Hatchet")
                {
                    vanillaHatchetFound = true;
                    break;
                }
            }


            if (!vanillaHatchetFound)
            {
                return;
            }


            GameObject survivalHatchet = SurvivalHatchetRegistry.GetSurvivalHatchetGameObject();

            if (survivalHatchet == null)
            {
                return;
            }


            ToolsItem survivalToolsItem = survivalHatchet.GetComponent<ToolsItem>();

            if (survivalToolsItem == null)
            {
                return;
            }


            if (!list.Contains(survivalToolsItem))
            {
                list.Add(survivalToolsItem);

                DebugHelper.Log("[BaltaTools] Survival Hatchet added " + "to blueprint optional tools.");
            }
        }
    }

    [HarmonyPatch(typeof(CraftingRequirementMultiTool),"Enable")]
    internal static class CraftingRequirementMultiTool_Enable_SurvivalHatchet_Patch
    {
        private static void Prefix(CraftingRequirementMultiTool __instance,Panel_Crafting ownerPanel,BlueprintData bp)
        {
            if (__instance == null || bp == null)
            {
                return;
            }
            Il2CppSystem.Collections.Generic.List<ToolsItem>filteredOptionalTools = bp.m_FilteredOptionalTools;

            if (filteredOptionalTools == null)
            {
                return;
            }

            for (int i = filteredOptionalTools.Count - 1;
                 i >= 0;
                 i--)
            {
                ToolsItem entry = filteredOptionalTools[i];

                if (entry == null)
                {
                    filteredOptionalTools.RemoveAt(i);

                    DebugHelper.Log("[BaltaTools][Blueprint] Dead ToolsItem removed at index " + i);
                    continue;
                }

                if (entry.gameObject == null)
                {
                    filteredOptionalTools.RemoveAt(i);

                    DebugHelper.Log("[BaltaTools][Blueprint] ToolsItem with NULL GameObject removed at index " + i);
                }
            }

            ToolsItem requiredTool = bp.m_RequiredTool;

            string requiredName = "<NULL>";


            if (requiredTool != null && requiredTool.gameObject != null)
            {
                requiredName = requiredTool.gameObject.name;
            }
            Il2CppSystem.Collections.Generic.List<ToolsItem>list = bp.m_FilteredOptionalTools;

            if (list == null)
            {
                return;
            }
            bool vanillaHatchetFound = requiredName == "GEAR_Hatchet";


            for (int i = 0;
                 i < list.Count;
                 i++)
            {
                ToolsItem existing = list[i];

                if (existing == null || existing.gameObject == null)
                {
                    continue;
                }

                if (existing.gameObject.name == "GEAR_Hatchet")
                {
                    vanillaHatchetFound = true;
                    break;
                }
            }

            if (!vanillaHatchetFound)
            {
                return;
            }

            GameObject survivalHatchet = SurvivalHatchetRegistry.GetSurvivalHatchetGameObject();
            if (survivalHatchet == null)
            {
                return;
            }

            ToolsItem survivalToolsItem = survivalHatchet.GetComponent<ToolsItem>();
            if (survivalToolsItem == null)
            {
                return;
            }

            if (!list.Contains(survivalToolsItem))
            {
                list.Add(survivalToolsItem);
                DebugHelper.Log("[BaltaTools] Survival Hatchet added " + "before MultiTool.Enable.");
            }
        }
    }
}