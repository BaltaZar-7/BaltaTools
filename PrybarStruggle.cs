#nullable disable
using HarmonyLib;
using Il2Cpp;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using MelonLoader;
using UnityEngine;

namespace BaltaTools
{
    internal static class ImprovisedPrybarStruggle
    {
        private const string ImprovisedPrybarName = "GEAR_ImprovisedPrybar";

        private const float BleedoutMinutesScale = 1f;
        private const bool CanPuncture = false;
        private const float DamageScalePercent = 1.25f;
        private const float FleeChanceScale = 1.5f;
        private const float TapIncrementScale = 1.1f;

        private static bool _useImprovisedMesh;
        private static Mesh _vanillaMesh;
        private static Material[] _vanillaMaterials;
        private static bool _cachedVanillaLook;

        public static void SetUseImprovisedMesh(bool use)
        {
            _useImprovisedMesh = use;

            DebugHelper.Log($"[BaltaTools] Prybar struggle mesh mode = " + $"{(use ? "IMPROVISED" : "VANILLA")}");
        }

        public static bool ShouldUseImprovisedMesh()
        {
            return _useImprovisedMesh;
        }

        public static void ApplyMeshSwap(bool useImprovised)
        {
            StruggleMeshTable table = FindActiveStruggleMeshTable();

            if (table == null || table.m_Prybar == null)
            {
                MelonLogger.Warning("[BaltaTools] No active " + "StruggleMeshTable/m_Prybar reference.");
                return;
            }

            MeshFilter prybarSlot = table.m_Prybar;

            MeshRenderer prybarRenderer = prybarSlot.GetComponent<MeshRenderer>();

            CacheVanillaLookIfNeeded(prybarSlot, prybarRenderer);

            if (useImprovised)
            {
                Mesh improvisedMesh = Main.ImprovisedPrybarFPHMesh;

                if (improvisedMesh == null)
                {
                    MelonLogger.Warning("[BaltaTools] FPH_ImprovisedPrybarMesh " + "not loaded yet. Vanilla mesh stays.");
                    return;
                }

                prybarSlot.sharedMesh = improvisedMesh;

                DebugHelper.Log($"[BaltaTools] Prybar struggle mesh → " + $"IMPROVISED ({improvisedMesh.name}, " + $"vertices={improvisedMesh.vertexCount})");
            }
            else
            {
                prybarSlot.sharedMesh = _vanillaMesh;

                if (prybarRenderer != null && _vanillaMaterials != null)
                {
                    prybarRenderer.sharedMaterials = _vanillaMaterials;
                }
                DebugHelper.Log("[BaltaTools] Prybar struggle mesh → VANILLA");
            }
        }
        private static void CacheVanillaLookIfNeeded(MeshFilter prybarSlot, MeshRenderer prybarRenderer)
        {
            if (_cachedVanillaLook)
            {
                return;
            }
            _vanillaMesh = prybarSlot.sharedMesh;

            _vanillaMaterials =
                prybarRenderer != null
                    ? prybarRenderer.sharedMaterials
                    : null;

            _cachedVanillaLook = true;
            DebugHelper.Log($"[BaltaTools] Vanilla Prybar mesh cached: " + $"{(_vanillaMesh == null ? "NULL" : _vanillaMesh.name)}");
        }

        private static StruggleMeshTable
            FindActiveStruggleMeshTable()
        {
            Il2CppArrayBase<StruggleMeshTable> allTables =
                Resources.FindObjectsOfTypeAll<StruggleMeshTable>();

            foreach (StruggleMeshTable table in allTables)
            {
                if (table != null &&
                    table.gameObject.activeInHierarchy &&
                    table.m_Prybar != null)
                {
                    return table;
                }
            }
            return null;
        }      
        private static void EnsureStruggleBonus(GearItem gearItem)
        {
            if (gearItem == null ||
                gearItem.gameObject == null)
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
                DebugHelper.Log($"[BaltaTools] {ImprovisedPrybarName}: " + "StruggleBonus created.");
            }

            struggleBonus.m_StruggleWeaponType = StruggleBonus.StruggleWeaponType.Prybar;

            struggleBonus.m_BleedoutMinutesScale = BleedoutMinutesScale;
            struggleBonus.m_CanPuncture = CanPuncture;
            struggleBonus.m_DamageScalePercent = DamageScalePercent;
            struggleBonus.m_FleeChanceScale = FleeChanceScale;
            struggleBonus.m_TapIncrementScale = TapIncrementScale;
            gearItem.m_StruggleBonus = struggleBonus;
        }

        public static GearItem
            GetImprovisedPrybarFromInventory()
        {
            Inventory inventory = GameManager.GetInventoryComponent();

            if (inventory == null)
            {
                return null;
            }

            GearItem improvisedPrybar = inventory.GetHighestConditionGearThatMatchesName(ImprovisedPrybarName);
            if (improvisedPrybar != null)
            {
                EnsureStruggleBonus(improvisedPrybar);
            }

            return improvisedPrybar;
        }
    }  
    [HarmonyPatch(typeof(Panel_WeaponPicker), nameof(Panel_WeaponPicker.Enable), new[] {typeof(bool), typeof(Il2CppSystem.Collections.Generic.List<GearItem>), typeof(float)})]
    internal static class Panel_WeaponPicker_Enable_PrybarPatch
    {
        private static void Prefix(bool enable, Il2CppSystem.Collections.Generic.List<GearItem> listItems, float durationSeconds)
        {
            if (!enable || listItems == null)
            {
                return;
            }

            GearItem improvisedPrybar = ImprovisedPrybarStruggle.GetImprovisedPrybarFromInventory();
            if (improvisedPrybar == null)
            {
                return;
            }

            if (!listItems.Contains(improvisedPrybar))
            {
                listItems.Add(improvisedPrybar);
                DebugHelper.Log("[BaltaTools] Improvised Prybar added to Struggle WeaponPicker list.");
            }
        }
    }

    [HarmonyPatch(typeof(Panel_WeaponPicker), nameof(Panel_WeaponPicker.SelectGridItem))]
    internal static class Panel_WeaponPicker_SelectGridItem_Patch
    {
        private static void Postfix(Panel_WeaponPicker __instance, WeaponPickerGridItem gridItem, bool isInContainer)
        {
            GearItem selectedGear = gridItem?.GetGearItem();
            if (selectedGear == null || selectedGear.gameObject == null)
            {
                return;
            }

            bool improvised = selectedGear.gameObject.name == "GEAR_ImprovisedPrybar";
            DebugHelper.Log($"[BaltaTools] SelectGridItem → {selectedGear.gameObject.name}, improvised={improvised}");
            ImprovisedPrybarStruggle.SetUseImprovisedMesh(improvised);
        }
    }

    [HarmonyPatch(typeof(PlayerStruggle), nameof(PlayerStruggle.BreakStruggle))]
    internal static class PlayerStruggle_BreakStruggle_RestoreMeshPatch
    {
        private static void Postfix()
        {
            ImprovisedPrybarStruggle.SetUseImprovisedMesh(false);
        }
    }

    [HarmonyPatch(typeof(Panel_WeaponPicker), nameof(Panel_WeaponPicker.GetBestItemPerCategory))]
    internal static class Panel_WeaponPicker_GetBestItemPerCategory_Patch
    {
        private static void Postfix(StruggleBonus.StruggleWeaponType type, ref GearItem __result)
        {
            if (type != StruggleBonus.StruggleWeaponType.Prybar)
            {
                return;
            }

            Inventory inventory = GameManager.GetInventoryComponent();
            if (inventory == null)
            {
                return;
            }

            GearItem improvisedPrybar = inventory.GetHighestConditionGearThatMatchesName("GEAR_ImprovisedPrybar");
            if (improvisedPrybar == null)
            {
                return;
            }

            __result = improvisedPrybar;
            DebugHelper.Log("[BaltaTools] GetBestItemPerCategory(Prybar) → GEAR_ImprovisedPrybar");
        }
    }

    [HarmonyPatch(typeof(PlayerAnimation), "EnableWeapon")]
    internal static class PlayerAnimation_EnableWeapon_PrybarMeshSwap_Patch
    {
        private static void Prefix(StruggleBonus.StruggleWeaponType weaponType)
        {
            if (weaponType != StruggleBonus.StruggleWeaponType.Prybar)
            {
                return;
            }
            ImprovisedPrybarStruggle.ApplyMeshSwap(ImprovisedPrybarStruggle.ShouldUseImprovisedMesh());
        }
    }
}