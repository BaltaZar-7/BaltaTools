#nullable disable
using HarmonyLib;
using Il2Cpp;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using MelonLoader;
using UnityEngine;

namespace BaltaTools
{
    internal static class SurvivalHatchetStruggle
    {
        private const string SurvivalHatchetName = "GEAR_SurvivalHatchet";
        private static bool _useSurvivalHatchetMesh;
        private static Mesh _vanillaMesh;
        private static Material[] _vanillaMaterials;
        private static bool _cachedVanillaLook;

        public static void SetUseSurvivalHatchetMesh(bool use)
        {
            _useSurvivalHatchetMesh = use;
            DebugHelper.Log($"[BaltaTools] Hatchet struggle mesh mode = " + $"{(use ? "SURVIVAL HATCHET" : "VANILLA")}");
        }

        public static bool ShouldUseSurvivalHatchetMesh()
        {
            return _useSurvivalHatchetMesh;
        }

        private static void EnsureStruggleBonus(GearItem gearItem)
        {
            if (gearItem == null || gearItem.gameObject == null)
            {
                return;
            }

            GameObject targetGameObject = gearItem.gameObject;

            StruggleBonus struggleBonus = gearItem.m_StruggleBonus;

            if (struggleBonus == null)
            {
                struggleBonus = targetGameObject.GetComponent<StruggleBonus>();
            }

            if (struggleBonus == null)
            {
                struggleBonus = targetGameObject.AddComponent<StruggleBonus>();
                DebugHelper.Log($"[BaltaTools] {SurvivalHatchetName}: " + "StruggleBonus created.");
            }

            struggleBonus.m_StruggleWeaponType = StruggleBonus.StruggleWeaponType.Hatchet;

            gearItem.m_StruggleBonus = struggleBonus;
            DebugHelper.Log($"[BaltaTools] {SurvivalHatchetName}: " + "StruggleBonus set " + "(StruggleWeaponType=Hatchet).");
        }

        public static void ApplyMeshSwap(bool useSurvivalHatchet)
        {
            StruggleMeshTable table = FindActiveStruggleMeshTable();

            if (table == null || table.m_Hatchet == null)
            {
                MelonLogger.Warning("[BaltaTools] No active " + "StruggleMeshTable/m_Hatchet reference.");
                return;
            }

            MeshFilter hatchetSlot = table.m_Hatchet;

            MeshRenderer hatchetRenderer = hatchetSlot.GetComponent<MeshRenderer>();

            CacheVanillaLookIfNeeded(hatchetSlot, hatchetRenderer);

            if (useSurvivalHatchet)
            {
                Mesh survivalHatchetMesh = Main.SurvivalHatchetFPHMesh;

                if (survivalHatchetMesh == null)
                {
                    MelonLogger.Warning("[BaltaTools] " + "FPH_SurvivalHatchetMesh " + "not loaded yet " + "Vanilla Hatchet mesh stays.");
                    return;
                }

                hatchetSlot.sharedMesh = survivalHatchetMesh;
                DebugHelper.Log($"[BaltaTools] Hatchet struggle mesh → " + $"SURVIVAL HATCHET " + $"({survivalHatchetMesh.name}, " + $"vertices={survivalHatchetMesh.vertexCount})");
            }
            else
            {
                hatchetSlot.sharedMesh = _vanillaMesh;

                if (hatchetRenderer != null && _vanillaMaterials != null)
                {
                    hatchetRenderer.sharedMaterials = _vanillaMaterials;
                }
                DebugHelper.Log("[BaltaTools] Hatchet struggle mesh → VANILLA");
            }
        }

        private static void CacheVanillaLookIfNeeded(MeshFilter hatchetSlot, MeshRenderer hatchetRenderer)
        {
            if (_cachedVanillaLook)
            {
                return;
            }
            _vanillaMesh = hatchetSlot.sharedMesh;
            _vanillaMaterials = hatchetRenderer != null ? hatchetRenderer.sharedMaterials : null;
            _cachedVanillaLook = true;

            DebugHelper.Log($"[BaltaTools] Vanilla Hatchet struggle mesh " + $"cached: " + $"{(_vanillaMesh == null ? "NULL" : _vanillaMesh.name)}");
        }

        private static StruggleMeshTable
            FindActiveStruggleMeshTable()
        {
            Il2CppArrayBase<StruggleMeshTable> allTables = Resources.FindObjectsOfTypeAll<StruggleMeshTable>();
            foreach (StruggleMeshTable table in allTables)
            {
                if (table != null && table.gameObject.activeInHierarchy && table.m_Hatchet != null)
                {
                    return table;
                }
            }
            return null;
        }

        public static bool IsSurvivalHatchet(GearItem gearItem)
        {
            if (gearItem == null || gearItem.gameObject == null)
            {
                return false;
            }
            return gearItem.gameObject.name == SurvivalHatchetName;
        }
        public static GearItem GetSurvivalHatchetFromInventory()
        {
            Inventory inventory = GameManager.GetInventoryComponent();
            if (inventory == null)
            {
                return null;
            }

            GearItem survivalHatchet = inventory.GetHighestConditionGearThatMatchesName(SurvivalHatchetName);
            if (survivalHatchet != null)
            {
                EnsureStruggleBonus(survivalHatchet);
            }

            return survivalHatchet;
        }
    }
   
    [HarmonyPatch(typeof(Panel_WeaponPicker),nameof(Panel_WeaponPicker.SelectGridItem))]
    internal static class Panel_WeaponPicker_SelectGridItem_SurvivalHatchetPatch
    {
        private static void Postfix(Panel_WeaponPicker __instance, WeaponPickerGridItem gridItem, bool isInContainer)
        {
            if (gridItem == null)
            {
                return;
            }

            GearItem selectedGear = gridItem.GetGearItem();

            if (selectedGear == null || selectedGear.gameObject == null)
            {
                return;
            }

            bool isSurvivalHatchet = selectedGear.gameObject.name == "GEAR_SurvivalHatchet";

            DebugHelper.Log($"[BaltaTools] Hatchet SelectGridItem → " + $"{selectedGear.gameObject.name}, " + $"survival={isSurvivalHatchet}");

            SurvivalHatchetStruggle.SetUseSurvivalHatchetMesh(isSurvivalHatchet);
        }
    }

    [HarmonyPatch(typeof(PlayerStruggle),nameof(PlayerStruggle.BreakStruggle))]
    internal static class PlayerStruggle_BreakStruggle_RestoreHatchetMeshPatch
    {
        private static void Postfix()
        {
            SurvivalHatchetStruggle.SetUseSurvivalHatchetMesh(false);
        }
    }

    [HarmonyPatch(typeof(PlayerAnimation),"EnableWeapon")]
    internal static class PlayerAnimation_EnableWeapon_SurvivalHatchetMeshSwap_Patch
    {
        private static void Prefix(StruggleBonus.StruggleWeaponType weaponType)
        {
            if (weaponType != StruggleBonus.StruggleWeaponType.Hatchet)
            {
                return;
            }

            SurvivalHatchetStruggle.ApplyMeshSwap(SurvivalHatchetStruggle.ShouldUseSurvivalHatchetMesh());
        }
    }
    [HarmonyPatch(typeof(Panel_WeaponPicker),nameof(Panel_WeaponPicker.Enable),new[] {typeof(bool),typeof(Il2CppSystem.Collections.Generic.List<GearItem>),typeof(float)})]
    internal static class Panel_WeaponPicker_Enable_SurvivalHatchetPatch
    {
        private static void Prefix(bool enable,Il2CppSystem.Collections.Generic.List<GearItem> listItems,float durationSeconds)
        {
            if (!enable || listItems == null)
            {
                return;
            }

            GearItem survivalHatchet = SurvivalHatchetStruggle.GetSurvivalHatchetFromInventory();
            if (survivalHatchet == null)
            {
                return;
            }

            if (!listItems.Contains(survivalHatchet))
            {
                listItems.Add(survivalHatchet);
                MelonLogger.Msg("[BaltaTools] Survival Hatchet added to Struggle WeaponPicker list.");
            }
        }
    }

    [HarmonyPatch(typeof(Panel_WeaponPicker),nameof(Panel_WeaponPicker.GetBestItemPerCategory))]
    internal static class Panel_WeaponPicker_GetBestItemPerCategory_SurvivalHatchetPatch
    {
        private static void Postfix(StruggleBonus.StruggleWeaponType type, ref GearItem __result)
        {
            if (type != StruggleBonus.StruggleWeaponType.Hatchet)
            {
                return;
            }

            GearItem survivalHatchet = SurvivalHatchetStruggle.GetSurvivalHatchetFromInventory();
            if (survivalHatchet == null)
            {
                return;
            }
            __result = survivalHatchet;
            DebugHelper.Log("[BaltaTools] GetBestItemPerCategory(Hatchet) → GEAR_SurvivalHatchet");
        }
    }
}