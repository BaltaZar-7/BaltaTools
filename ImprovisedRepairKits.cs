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

        /// <summary>
        /// Végigmegy minden jelenleg betöltött Sharpenable és Cleanable komponensen,
        /// és ha a "hivatalos" eszköz (fenőkő / puskatisztító készlet) szerepel a
        /// választható listájukban, hozzáadja az improvizált változatot is —
        /// közvetlenül a natív mezőbe írva.
        /// </summary>
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

                // Első körben: számoljuk meg, hány ÉLŐ (nem megsemmisült) elem van,
                // és nézzük meg, kell-e egyáltalán valamit módosítani.
                foreach (ToolsItem tool in choices)
                {
                    if (tool == null) // ez true lesz halott Unity-objektumra is (== operator overload)
                    {
                        continue;
                    }

                    liveCount++;

                    if (tool.gameObject.name == "GEAR_SharpeningStone")
                    {
                        hasStone = true;
                    }

                    if (improvised != null && tool == improvised)
                    {
                        hasImprovised = true;
                    }
                }

                bool needsRebuild = (liveCount != choices.Length); // volt halott elem
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
                        continue; // halott referenciát kihagyjuk
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
            if (improvised == null)
            {
                return;
            }

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

                foreach (ToolsItem tool in choices)
                {
                    if (tool == null)
                    {
                        continue;
                    }

                    if (tool.gameObject.name == "GEAR_RifleCleaningKit")
                    {
                        hasKit = true;
                    }

                    if (tool == improvised)
                    {
                        hasImprovised = true;
                    }
                }

                if (hasKit && !hasImprovised)
                {
                    Il2CppReferenceArray<ToolsItem> newArray = new Il2CppReferenceArray<ToolsItem>(choices.Length + 1);
                    for (int i = 0; i < choices.Length; i++)
                    {
                        newArray[i] = choices[i];
                    }
                    newArray[choices.Length] = improvised;
                    cleanable.m_CleanToolChoices = newArray;
                    DebugHelper.Log($"[BaltaTools] Cleaning kit hozzáadva: {cleanable.gameObject.name}");
                }
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
                    DebugHelper.Log($"[BaltaTools] {prefabName}: ToolsItem létrehozva.");
                }
                gearItem.m_ToolsItem = toolsItem;

                DegradeOnUse degradeOnUse = targetGameObject.GetComponent<DegradeOnUse>();
                if (degradeOnUse == null)
                {
                    degradeOnUse = targetGameObject.AddComponent<DegradeOnUse>();
                    degradeOnUse.m_DegradeHP = 10f;
                }

                gearItem.m_DegradeOnUse = degradeOnUse;

                cache = toolsItem;
                return cache;
            }

            if (!loggedMissing)
            {
                MelonLogger.Warning($"[BaltaTools] {prefabName} még nem található (Resources.FindObjectsOfTypeAll).");
                loggedMissing = true;
            }

            return null;
        }
        private static void ConfigureAsTool(ToolsItem toolsItem)
        {
            toolsItem.m_CraftingAndRepairSkillModifier = 10f;
            toolsItem.m_CraftingAndRepairTimeModifier = 2f;
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
    /*[HarmonyPatch(typeof(GearItem), "DegradeOnUse")]
    public static class GearItem_DegradeOnUse_Diag_Patch
    {
        static void Prefix(GearItem __instance, out float __state)
        {
            __state = __instance.CurrentHP;
        }

        static void Postfix(GearItem __instance, float __state)
        {
            if (__instance.gameObject.name != "GEAR_ImprovisedSharpeningStone" && __instance.gameObject.name != "GEAR_ImprovisedRifleCleaningKit")
            {
                return;
            }

            DebugHelper.Log($"[BaltaTools][diag] GearItem.DegradeOnUse() lefutott: {__instance.gameObject.name}, HP: {__state} -> {__instance.CurrentHP}");
        }
    }*/
}