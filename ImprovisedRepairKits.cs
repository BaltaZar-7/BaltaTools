#nullable disable
using HarmonyLib;
using Il2Cpp;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppSystem.Collections.Generic;
using MelonLoader;
using UnityEngine;

namespace BaltaTools
{
    internal static class ImprovisedToolsRegistry
    {
        private const string ImprovisedSharpeningStoneName = "GEAR_ImprovisedSharpeningStone";
        private const string ImprovisedRifleCleaningKitName = "GEAR_ImprovisedRifleCleaningKit";

        private static ToolsItem _improvisedSharpeningStoneSample;
        private static ToolsItem _improvisedRifleCleaningKitSample;
        public static ToolsItem GetImprovisedSharpeningStone()
        {
            return Resolve(ImprovisedSharpeningStoneName,ref _improvisedSharpeningStoneSample);
        }

        public static ToolsItem GetImprovisedRifleCleaningKit()
        {
            return Resolve(ImprovisedRifleCleaningKitName,ref _improvisedRifleCleaningKitSample);
        }
        private static ToolsItem Resolve(string prefabName,ref ToolsItem cache)
        {
            if (cache != null)
            {
                return cache;
            }

            Il2CppArrayBase<GearItem> allGearItems = Resources.FindObjectsOfTypeAll<GearItem>();

            foreach (GearItem gearItem in allGearItems)
            {
                if (gearItem == null ||
                    gearItem.gameObject == null ||
                    gearItem.gameObject.name != prefabName)
                {
                    continue;
                }

                GameObject targetGameObject = gearItem.gameObject;

                ToolsItem toolsItem = targetGameObject.GetComponent<ToolsItem>();

                if (toolsItem == null)
                {
                    toolsItem = targetGameObject.AddComponent<ToolsItem>();
                    DebugHelper.Log("[BaltaTools] " + prefabName + ": ToolsItem created.");
                }

                ConfigureAsTool(toolsItem);

                gearItem.m_ToolsItem = toolsItem;

                cache = toolsItem;
                DebugHelper.Log("[BaltaTools] " + prefabName + ": resolved. InstanceID=" + gearItem.GetInstanceID()
                );
                return cache;
            }
            DebugHelper.Log("[BaltaTools] " + prefabName + " not found yet (Resources.FindObjectsOfTypeAll).");
            return null;
        }
        public static void InitializeToolsItem(GearItem gearItem)
        {
            if (gearItem == null || gearItem.gameObject == null)
            {
                return;
            }

            string itemName = gearItem.gameObject.name;

            if (itemName != ImprovisedSharpeningStoneName && itemName != ImprovisedRifleCleaningKitName)
            {
                return;
            }

            ToolsItem toolsItem = gearItem.m_ToolsItem;
            if (toolsItem == null)
            {
                toolsItem = gearItem.gameObject.GetComponent<ToolsItem>();
            }
            if (toolsItem == null)
            {
                toolsItem = gearItem.gameObject.AddComponent<ToolsItem>();
                DebugHelper.Log("[BaltaTools] " + itemName + ": ToolsItem created. InstanceID=" + gearItem.GetInstanceID());
            }

            ConfigureAsTool(toolsItem);
            gearItem.m_ToolsItem = toolsItem;

            if (itemName == ImprovisedSharpeningStoneName)
            {
                _improvisedSharpeningStoneSample = toolsItem;
            }
            else
            {
                _improvisedRifleCleaningKitSample = toolsItem;
            }
            DebugHelper.Log("[BaltaTools] " + itemName + ": ToolsItem configured. InstanceID=" + gearItem.GetInstanceID() + " TimeModifier=" + toolsItem.m_CraftingAndRepairTimeModifier);
        }

        private static void ConfigureAsTool(ToolsItem toolsItem)
        {
            toolsItem.m_CraftingAndRepairSkillModifier = 10f;
            toolsItem.m_CraftingAndRepairTimeModifier = 1.5f;
            toolsItem.m_CanOnlyCraftAndRepairClothes = false;
            toolsItem.m_ToolType = ToolsItem.ToolType.RepairOnly;
            toolsItem.m_CuttingToolType = ToolsItem.CuttingToolType.None;
            toolsItem.m_DegradePerHourCrafting = 10f;
            toolsItem.m_AppearInStoryOnly = false;
        }

        public static void EnsureInjected()
        {
            InjectSharpen();
            InjectClean();
        }

        private static void InjectSharpen()
        {
            ToolsItem improvised = GetImprovisedSharpeningStone();

            if (improvised == null)
            {
                return;
            }

            Il2CppArrayBase<Sharpenable> allSharpenables = Resources.FindObjectsOfTypeAll<Sharpenable>();
            foreach (Sharpenable sharpenable in allSharpenables)
            {
                if (sharpenable == null) continue;

                Il2CppReferenceArray<ToolsItem> choices = sharpenable.m_SharpenToolChoices;
                if (choices == null) continue;

                bool hasStone = false;
                bool hasImprovised = false;
                int liveCount = 0;

                foreach (ToolsItem tool in choices)
                {
                    if (tool == null) continue;
                    liveCount++;
                    if (tool.gameObject.name == "GEAR_SharpeningStone") hasStone = true;
                    if (tool.gameObject.name == ImprovisedSharpeningStoneName) hasImprovised = true;
                }

                bool needsRebuild = liveCount != choices.Length;
                bool needsAppend = hasStone && !hasImprovised;

                if (!needsRebuild && !needsAppend) continue;

                int newLength = liveCount + (needsAppend ? 1 : 0);
                Il2CppReferenceArray<ToolsItem> newArray = new Il2CppReferenceArray<ToolsItem>(newLength);
                int writeIndex = 0;

                foreach (ToolsItem tool in choices)
                {
                    if (tool == null) continue;
                    newArray[writeIndex++] = tool;
                }
                if (needsAppend)
                {
                    newArray[writeIndex] = improvised;
                }
                sharpenable.m_SharpenToolChoices = newArray;
                DebugHelper.Log("[BaltaTools] Sharpen tools refreshed (" + sharpenable.gameObject.name + "): rebuilt=" + needsRebuild + ", appended=" + needsAppend);
            }
        }

        private static void InjectClean()
        {
            ToolsItem improvised = GetImprovisedRifleCleaningKit();

            if (improvised == null)
            {
                return;
            }

            Il2CppArrayBase<Cleanable> allCleanables = Resources.FindObjectsOfTypeAll<Cleanable>();
            foreach (Cleanable cleanable in allCleanables)
            {
                if (cleanable == null) continue;

                Il2CppReferenceArray<ToolsItem> choices = cleanable.m_CleanToolChoices;
                if (choices == null) continue;

                bool hasKit = false;
                bool hasImprovised = false;
                int liveCount = 0;

                foreach (ToolsItem tool in choices)
                {
                    if (tool == null) continue;
                    liveCount++;
                    if (tool.gameObject.name == "GEAR_RifleCleaningKit") hasKit = true;
                    if (tool.gameObject.name == ImprovisedRifleCleaningKitName) hasImprovised = true;
                }

                bool needsRebuild = liveCount != choices.Length;
                bool needsAppend = hasKit && !hasImprovised;

                if (!needsRebuild && !needsAppend) continue;

                int newLength = liveCount + (needsAppend ? 1 : 0);
                Il2CppReferenceArray<ToolsItem> newArray = new Il2CppReferenceArray<ToolsItem>(newLength);
                int writeIndex = 0;

                foreach (ToolsItem tool in choices)
                {
                    if (tool == null) continue;
                    newArray[writeIndex++] = tool;
                }

                if (needsAppend)
                {
                    newArray[writeIndex] = improvised;
                }

                cleanable.m_CleanToolChoices = newArray;
                DebugHelper.Log("[BaltaTools] Clean tools refreshed (" + cleanable.gameObject.name + "): rebuilt=" + needsRebuild + ", appended=" + needsAppend);
            }
        }

        public static void ResetCache()
        {
            _improvisedSharpeningStoneSample = null;
            _improvisedRifleCleaningKitSample = null;
        }
    }

    [HarmonyPatch(typeof(Panel_Inventory), nameof(Panel_Inventory.Enable), new[] { typeof(bool) })]
    internal static class Panel_Inventory_Enable_InjectTools_Patch
    {
        private static void Postfix(bool enable)
        {
            if (!enable) return;
            ImprovisedToolsRegistry.EnsureInjected();
        }
    }

    [HarmonyPatch(typeof(Repairable), "GetFilteredRepairToolChoices")]
    internal static class Repairable_AddImprovisedTools_Patch
    {
        private static void Postfix(Repairable __instance, Il2CppSystem.Collections.Generic.List<ToolsItem> __result)
        {
            if (__result == null) return;

            Il2CppReferenceArray<ToolsItem> original = __instance.m_RepairToolChoices;
            if (original == null) return;

            bool needsWhetstone = false;
            bool needsCleaningKit = false;

            foreach (ToolsItem tool in original)
            {
                if (tool == null) continue;
                if (tool.gameObject.name == "GEAR_SharpeningStone") needsWhetstone = true;
                if (tool.gameObject.name == "GEAR_RifleCleaningKit") needsCleaningKit = true;
            }

            if (needsWhetstone)
            {
                ToolsItem improvisedStone = ImprovisedToolsRegistry.GetImprovisedSharpeningStone();
                if (improvisedStone != null && !__result.Contains(improvisedStone))
                {
                    __result.Add(improvisedStone);
                }
            }

            if (needsCleaningKit)
            {
                ToolsItem improvisedKit = ImprovisedToolsRegistry.GetImprovisedRifleCleaningKit();

                if (improvisedKit != null && !__result.Contains(improvisedKit))
                {
                    __result.Add(improvisedKit);
                }
            }
        }
    }

    [HarmonyPatch(typeof(GearItem), nameof(GearItem.Awake))]
    internal static class GearItem_Awake_ImprovisedTools_Patch
    {
        private static void Postfix(GearItem __instance)
        {
            if (__instance == null || __instance.gameObject == null) return;

            string itemName = __instance.gameObject.name;
            if (itemName != "GEAR_ImprovisedSharpeningStone" && itemName != "GEAR_ImprovisedRifleCleaningKit")
            {
                return;
            }

            ImprovisedToolsRegistry.InitializeToolsItem(__instance);
            ApplyDegradeOnUse(__instance);
        }

        private static void ApplyDegradeOnUse(GearItem gearItem)
        {
            DegradeOnUse degradeOnUse = gearItem.m_DegradeOnUse;
            if (degradeOnUse == null)
            {
                degradeOnUse = gearItem.gameObject.GetComponent<DegradeOnUse>();
            }
            if (degradeOnUse == null)
            {
                degradeOnUse = gearItem.gameObject.AddComponent<DegradeOnUse>();
                DebugHelper.Log("[BaltaTools] DegradeOnUse created for " + gearItem.gameObject.name + " InstanceID=" + gearItem.GetInstanceID());
            }

            degradeOnUse.m_DegradeHP = 10f;
            gearItem.m_DegradeOnUse = degradeOnUse;
        }
    }
}