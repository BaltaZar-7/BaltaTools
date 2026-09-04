#nullable disable
using System.Collections.Generic;
using HarmonyLib;
using Il2Cpp;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppTLD.Gear;
using MelonLoader;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace BaltaTools
{
    internal static class ToolBeltWeightReductionRegistry
    {
        private static readonly string[] TargetItemNames = {"GEAR_SurvivalHatchet", "GEAR_FeatherPlucker", "GEAR_ImprovisedPrybar"};

        private static readonly Dictionary<string, GearItemData> DataCache = new();

        public static void InjectInto(WeightReductionBuff buff)
        {
            if (buff == null || buff.m_GearItem == null)
                return;

            if (buff.m_GearItem.name != "GEAR_ToolBelt")
                return;

            Il2CppSystem.Collections.Generic.List<GearItemData> validTargets = buff.m_ValidTargets;
            if (validTargets == null)
                return;

            bool changed = false;

            foreach (string itemName in TargetItemNames)
            {
                GearItemData data = ResolveByName(itemName);
                if (data != null && !validTargets.Contains(data))
                {
                    validTargets.Add(data);
                    changed = true;
                    DebugHelper.Log($"[BaltaTools] {itemName} added to ToolBelt targets.");
                }
            }

            if (changed)
                buff.m_IsDirty = true;
        }

        private static GearItemData ResolveByName(string name)
        {
            if (DataCache.TryGetValue(name, out GearItemData cached) && cached != null)
                return cached;

            GearItemData data = TryLoadFromAddressables(name);
            if (data == null)
                data = TryFindViaLiveGearItem(name);

            if (data != null)
                DataCache[name] = data;
            else
                MelonLogger.Warning($"[BaltaTools] could not unlock: {name} ");

            return data;
        }

        private static GearItemData TryLoadFromAddressables(string key)
        {
            AsyncOperationHandle<GearItemData> handle = Addressables.LoadAssetAsync<GearItemData>(key);
            GearItemData result = handle.WaitForCompletion();
            if (handle.Status != AsyncOperationStatus.Succeeded || result == null)
                return null;

            return result;
        }

        private static GearItemData TryFindViaLiveGearItem(string prefabName)
        {
            Il2CppArrayBase<GearItem> allGearItems = Resources.FindObjectsOfTypeAll<GearItem>();
            foreach (GearItem gearItem in allGearItems)
            {
                if (gearItem == null || gearItem.gameObject == null || gearItem.gameObject.name != prefabName)
                    continue;

                if (gearItem.m_GearItemData != null)
                    return gearItem.m_GearItemData;
            }

            return null;
        }
    }

    [HarmonyPatch(typeof(WeightReductionBuff), "InitializeTargets")]
    internal static class WeightReductionBuff_InitializeTargets_InjectExtraTargets_Patch
    {
        static void Postfix(WeightReductionBuff __instance)
        {
            ToolBeltWeightReductionRegistry.InjectInto(__instance);
        }
    }
}