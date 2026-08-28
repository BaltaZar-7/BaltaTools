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
        private static ToolsItem _improvisedSharpeningStone;
        private static ToolsItem _improvisedRifleCleaningKit;
        private static bool _loggedMissingSharpeningStone;
        private static bool _loggedMissingRifleCleaningKit;

        public static ToolsItem GetImprovisedSharpeningStone()
        {
            return Resolve("GEAR_ImprovisedSharpeningStone", ref _improvisedSharpeningStone, ref _loggedMissingSharpeningStone);
        }

        public static ToolsItem GetImprovisedRifleCleaningKit()
        {
            return Resolve("GEAR_ImprovisedRifleCleaningKit", ref _improvisedRifleCleaningKit, ref _loggedMissingRifleCleaningKit);
        }

        /// It iterates through all currently loaded Sharpenable and Cleanable components,
        /// and if vanilla tools (whetstone / gun cleaning kit) appears in
        /// their list of options, it adds the improvised version as well —
        /// writing it directly into the native field.
        public static void EnsureInjected()
        {
            InjectSharpen();
            InjectClean();
        }
        private static void InjectSharpen()
        {
            ToolsItem improvised = GetImprovisedSharpeningStone();

            Il2CppArrayBase<Sharpenable> allSharpenables = Resources.FindObjectsOfTypeAll<Sharpenable>();
            foreach (Sharpenable sharpenable in allSharpenables)
            {
                if (sharpenable == null)
                {
                    continue;
                }

                Il2CppReferenceArray<ToolsItem> choices = sharpenable.m_SharpenToolChoices;
                if (choices == null)
                {
                    continue;
                }

                bool hasStone = false;
                bool hasImprovised = false;
                int liveCount = 0;

                // First, let's count how many LIVE (non-destroyed) elements there are, and see if anything needs to be modified at all.
                foreach (ToolsItem tool in choices)
                {
                    if (tool == null) continue;

                    liveCount++;

                    if (tool.gameObject.name == "GEAR_SharpeningStone")
                    {
                        hasStone = true;
                    }

                    if (tool.gameObject.name == "GEAR_ImprovisedSharpeningStone")
                    {
                        hasImprovised = true;
                    }
                }

                bool needsRebuild = (liveCount != choices.Length); // there was a dead ref
                bool needsAppend = hasStone && improvised != null && !hasImprovised;

                if (!needsRebuild && !needsAppend)
                {
                    continue;
                }

                int newLength = liveCount + (needsAppend ? 1 : 0);
                Il2CppReferenceArray<ToolsItem> newArray = new Il2CppReferenceArray<ToolsItem>(newLength);

                int writeIndex = 0;
                foreach (ToolsItem tool in choices)
                {
                    if (tool == null)
                    {
                        continue; // skip dead reference
                    }
                    newArray[writeIndex++] = tool;
                }

                if (needsAppend)
                {
                    newArray[writeIndex] = improvised;
                }

                sharpenable.m_SharpenToolChoices = newArray;
                DebugHelper.Log($"[BaltaTools] Sharpen tools frissítve ({sharpenable.gameObject.name}): halott elemek eltávolítva={needsRebuild}, hozzáadva={needsAppend}");
            }
        }

        private static void InjectClean()
        {
            ToolsItem improvised = GetImprovisedRifleCleaningKit();

            Il2CppArrayBase<Cleanable> allCleanables = Resources.FindObjectsOfTypeAll<Cleanable>();
            foreach (Cleanable cleanable in allCleanables)
            {
                if (cleanable == null)
                {
                    continue;
                }

                Il2CppReferenceArray<ToolsItem> choices = cleanable.m_CleanToolChoices;
                if (choices == null)
                {
                    continue;
                }

                bool hasKit = false;
                bool hasImprovised = false;
                int liveCount = 0;

                foreach (ToolsItem tool in choices)
                {
                    if (tool == null) continue;

                    liveCount++;

                    if (tool.gameObject.name == "GEAR_RifleCleaningKit")
                    {
                        hasKit = true;
                    }

                    if (tool.gameObject.name == "GEAR_ImprovisedRifleCleaningKit")
                    {
                        hasImprovised = true;
                    }
                }

                bool needsRebuild = liveCount != choices.Length; // dead ref
                bool needsAppend = hasKit && improvised != null && !hasImprovised;

                if (!needsRebuild && !needsAppend)
                {
                    continue;
                }

                int newLength = liveCount + (needsAppend ? 1 : 0);
                Il2CppReferenceArray<ToolsItem> newArray = new Il2CppReferenceArray<ToolsItem>(newLength);

                int writeIndex = 0;
                foreach (ToolsItem tool in choices)
                {
                    if (tool == null)
                    {
                        continue; // dead reference skipped
                    }

                    newArray[writeIndex++] = tool;
                }

                if (needsAppend)
                {
                    newArray[writeIndex] = improvised;
                }

                cleanable.m_CleanToolChoices = newArray;
                DebugHelper.Log($"[BaltaTools] Clean tools frissítve ({cleanable.gameObject.name}): halott elemek eltávolítva={needsRebuild}, hozzáadva={needsAppend}");
            }
        }
        private static ToolsItem Resolve(string prefabName, ref ToolsItem cache, ref bool loggedMissing)
        {
            if (cache != null)
            {
                return cache;
            }

            Il2CppArrayBase<GearItem> allGearItems = Resources.FindObjectsOfTypeAll<GearItem>();
            foreach (GearItem gearItem in allGearItems)
            {
                if (gearItem == null || gearItem.gameObject.name != prefabName)
                {
                    continue;
                }

                GameObject targetGameObject = gearItem.gameObject;
                ToolsItem toolsItem = targetGameObject.GetComponent<ToolsItem>();

                if (toolsItem == null)
                {
                    toolsItem = targetGameObject.AddComponent<ToolsItem>();
                    ConfigureAsTool(toolsItem);
                    DebugHelper.Log($"[BaltaTools] {prefabName}: ToolsItem created.");
                }
                gearItem.m_ToolsItem = toolsItem;
                cache = toolsItem;
                return cache;
            }

            if (!loggedMissing)
            {
                MelonLogger.Warning($"[BaltaTools] {prefabName} not found yet (Resources.FindObjectsOfTypeAll).");
                loggedMissing = true;
            }

            return null;
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
        public static void ResetCache()
        {
            _improvisedSharpeningStone = null;
            _improvisedRifleCleaningKit = null;
            _loggedMissingSharpeningStone = false;
            _loggedMissingRifleCleaningKit = false;
        }
    }

    [HarmonyPatch(typeof(Panel_Inventory), nameof(Panel_Inventory.Enable), new[] { typeof(bool) })]
    internal static class Panel_Inventory_Enable_InjectTools_Patch
    {
        static void Postfix(bool enable)
        {
            if (!enable)
            {
                return;
            }

            ImprovisedToolsRegistry.EnsureInjected();
        }
    }

    [HarmonyPatch(typeof(Repairable), "GetFilteredRepairToolChoices")]
    public static class Repairable_AddImprovisedTools_Patch
    {
        static void Postfix(Repairable __instance, Il2CppSystem.Collections.Generic.List<ToolsItem> __result)
        {
            ImprovisedToolsRegistry.EnsureInjected();

            if (__result == null)
            {
                return;
            }

            Il2CppReferenceArray<ToolsItem> original = __instance.m_RepairToolChoices;
            if (original == null)
            {
                return;
            }

            bool needsWhetstone = false;
            bool needsCleaningKit = false;

            foreach (ToolsItem tool in original)
            {
                if (tool == null)
                {
                    continue;
                }

                if (tool.gameObject.name == "GEAR_SharpeningStone")
                {
                    needsWhetstone = true;
                }

                if (tool.gameObject.name == "GEAR_RifleCleaningKit")
                {
                    needsCleaningKit = true;
                }
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
    internal static class GearItem_Awake_ImprovisedTools_Degrade_Patch
    {
        private static void Postfix(GearItem __instance)
        {
            if (__instance == null ||
                __instance.gameObject == null)
            {
                return;
            }

            string itemName = __instance.gameObject.name;

            if (itemName != "GEAR_ImprovisedSharpeningStone" &&
                itemName != "GEAR_ImprovisedRifleCleaningKit")
            {
                return;
            }

            ApplyDegradeOnUse(__instance);
        }

        private static void ApplyDegradeOnUse(GearItem gearItem)
        {
            if (gearItem == null || gearItem.gameObject == null)
            {
                return;
            }

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
            DebugHelper.Log("[BaltaTools] DegradeOnUse assigned to " + gearItem.gameObject.name + " InstanceID=" + gearItem.GetInstanceID() + " HP=" + degradeOnUse.m_DegradeHP);
        }
    }
}