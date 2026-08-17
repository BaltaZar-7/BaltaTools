#nullable disable
using HarmonyLib;
using Il2Cpp;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppSystem.Collections.Generic;
using MelonLoader;
using UnityEngine;

namespace BaltaTools
{
    internal static class ImprovisedPrybarStruggle
    {
        private const string ImprovisedPrybarName =
            "GEAR_ImprovisedPrybar";

        // Vanilla GEAR_Prybar StruggleBonus értékei.
        private const float BleedoutMinutesScale = 1f;
        private const bool CanPuncture = false;
        private const float DamageScalePercent = 1.25f;
        private const float FleeChanceScale = 1.5f;
        private const float TapIncrementScale = 1.1f;

        private static MeshFilter _cachedImprovisedMesh;
        private static MeshFilter _cachedVanillaPrybarMesh;
        private static Mesh _vanillaPrybarSharedMesh;
        private static Material[] _vanillaPrybarSharedMaterials;
        private static MeshFilter _vanillaPrybarMeshFilterRef;
        private static MeshRenderer _vanillaPrybarMeshRendererRef;

        private static Mesh _improvisedSourceMesh;
        private static Material[] _improvisedSourceMaterials;

        private static bool _loggedMissingMesh;
        private static bool _loggedMissingVanillaMesh;

        private static bool _struggleTablesCached;

        public static void EnsureStruggleSetupOnAll()
        {
            CacheVanillaPrybarMesh();

            GearItem improvisedPrybar =
                FindImprovisedPrybarPrefab();

            if (improvisedPrybar != null)
            {
                EnsureStruggleBonus(improvisedPrybar);

                improvisedPrybar.CacheComponents();

                //CacheImprovisedMesh(improvisedPrybar);

                LogGearVisualState(improvisedPrybar);
                LogMeshState(improvisedPrybar);
            }

            Il2CppArrayBase<GearItem> allGearItems =
                Resources.FindObjectsOfTypeAll<GearItem>();

            foreach (GearItem gearItem in allGearItems)
            {
                if (gearItem == null ||
                    gearItem.gameObject == null ||
                    gearItem.gameObject.name != ImprovisedPrybarName)
                {
                    continue;
                }

                EnsureStruggleBonus(gearItem);

                gearItem.CacheComponents();

                LogGearVisualState(gearItem);
                LogMeshState(gearItem);
            }
        }
        private static void LogGearVisualState(
    GearItem gearItem)
        {
            if (gearItem == null)
            {
                return;
            }

            int meshRendererCount =
                gearItem.m_MeshRenderers == null
                    ? 0
                    : gearItem.m_MeshRenderers.Length;

            int skinnedRendererCount =
                gearItem.m_SkinnedMeshRenderers == null
                    ? 0
                    : gearItem.m_SkinnedMeshRenderers.Length;

            MelonLogger.Msg(
                $"[BaltaTools][diag] {gearItem.gameObject.name}: " +
                $"MeshRenderers={meshRendererCount}, " +
                $"SkinnedMeshRenderers={skinnedRendererCount}");
        }
        private static void LogMeshState(
    GearItem gearItem)
        {
            if (gearItem == null)
            {
                return;
            }

            MeshRenderer[] renderers =
                gearItem.GetComponentsInChildren<MeshRenderer>(true);

            foreach (MeshRenderer meshRenderer in renderers)
            {
                if (meshRenderer == null)
                {
                    continue;
                }

                MelonLogger.Msg(
                    $"[BaltaTools][diag] MeshRenderer: " +
                    $"{meshRenderer.gameObject.name}, " +
                    $"activeSelf={meshRenderer.gameObject.activeSelf}, " +
                    $"activeInHierarchy={meshRenderer.gameObject.activeInHierarchy}, " +
                    $"enabled={meshRenderer.enabled}");
            }
        }

        private static void EnsureImprovisedPrybarInGame()
        {
            GearItem improvisedPrybar =
                FindImprovisedPrybarPrefab();

            if (improvisedPrybar == null)
            {
                return;
            }

            EnsureStruggleBonus(improvisedPrybar);
            //CacheImprovisedMesh(improvisedPrybar);
        }

        private static GearItem FindImprovisedPrybarPrefab()
        {
            Il2CppArrayBase<GearItem> allGearItems =
                Resources.FindObjectsOfTypeAll<GearItem>();

            foreach (GearItem gearItem in allGearItems)
            {
                if (gearItem == null ||
                    gearItem.gameObject == null)
                {
                    continue;
                }

                if (gearItem.gameObject.name == ImprovisedPrybarName)
                {
                    return gearItem;
                }
            }

            return null;
        }

        private static void EnsureStruggleBonus(GearItem gearItem)
        {
            if (gearItem == null)
            {
                return;
            }

            GameObject targetGameObject =
                gearItem.gameObject;

            if (targetGameObject == null)
            {
                return;
            }

            StruggleBonus struggleBonus =
                gearItem.m_StruggleBonus;

            if (struggleBonus == null)
            {
                struggleBonus =
                    targetGameObject.GetComponent<StruggleBonus>();
            }

            if (struggleBonus == null)
            {
                struggleBonus =
                    targetGameObject.AddComponent<StruggleBonus>();

                MelonLogger.Msg(
                    $"[BaltaTools] {ImprovisedPrybarName}: " +
                    "StruggleBonus létrehozva.");
            }

            // KRITIKUS:
            // A vanilla Prybar kategóriát használjuk.
            // Ettől a vanilla struggle weapon picker ismerni fogja.
            struggleBonus.m_StruggleWeaponType =
                StruggleBonus.StruggleWeaponType.Prybar;

            struggleBonus.m_BleedoutMinutesScale =
                BleedoutMinutesScale;

            struggleBonus.m_CanPuncture =
                CanPuncture;

            struggleBonus.m_DamageScalePercent =
                DamageScalePercent;

            struggleBonus.m_FleeChanceScale =
                FleeChanceScale;

            struggleBonus.m_TapIncrementScale =
                TapIncrementScale;

            gearItem.m_StruggleBonus =
                struggleBonus;

            DebugHelper.Log(
                $"[BaltaTools] {ImprovisedPrybarName}: " +
                "StruggleBonus beállítva " +
                "(StruggleWeaponType=Prybar).");
        }

        public static GearItem GetImprovisedPrybarFromInventory()
        {
            Inventory inventory =
                GameManager.GetInventoryComponent();

            if (inventory == null)
            {
                return null;
            }

            GearItem improvisedPrybar =
                inventory.GetHighestConditionGearThatMatchesName(
                    ImprovisedPrybarName);

            if (improvisedPrybar != null)
            {
                EnsureStruggleBonus(improvisedPrybar);
            }

            return improvisedPrybar;
        }

        private static void CacheVanillaPrybarMesh()
        {
            if (_cachedVanillaPrybarMesh != null)
            {
                return;
            }

            Il2CppArrayBase<StruggleMeshTable> allTables =
                Resources.FindObjectsOfTypeAll<StruggleMeshTable>();

            foreach (StruggleMeshTable table in allTables)
            {
                if (table == null)
                {
                    continue;
                }

                if (table.m_Prybar != null)
                {
                    _cachedVanillaPrybarMesh =
                        table.m_Prybar;

                    MelonLogger.Msg(
                        $"[BaltaTools] Vanilla Prybar struggle mesh cache-elve: " +
                        $"{table.m_Prybar.gameObject.name}");

                    _struggleTablesCached = true;
                    return;
                }
            }

            if (!_loggedMissingVanillaMesh)
            {
                MelonLogger.Warning(
                    "[BaltaTools] Nem található vanilla Prybar struggle mesh.");
                _loggedMissingVanillaMesh = true;
            }
        }

        /*private static void CacheImprovisedMesh(GearItem gearItem)
        {
            if (_cachedImprovisedMesh != null)
            {
                return;
            }

            if (gearItem == null ||
                gearItem.gameObject == null)
            {
                return;
            }

            MeshFilter meshFilter =
                gearItem.gameObject.GetComponentInChildren<MeshFilter>(true);

            if (meshFilter == null)
            {
                if (!_loggedMissingMesh)
                {
                    MelonLogger.Warning(
                        $"[BaltaTools] {ImprovisedPrybarName}: " +
                        "nem található MeshFilter.");
                    _loggedMissingMesh = true;
                }

                return;
            }

            _cachedImprovisedMesh =
                meshFilter;

            MelonLogger.Msg(
                $"[BaltaTools] Improvised Prybar struggle mesh cache-elve: " +
                $"{meshFilter.gameObject.name}");
        }

        private static void CacheImprovisedMeshFromInventory()
        {
            if (_cachedImprovisedMesh != null)
            {
                return;
            }

            GearItem improvisedPrybar =
                GetImprovisedPrybarFromInventory();

            if (improvisedPrybar == null)
            {
                return;
            }

            CacheImprovisedMesh(improvisedPrybar);
        }*/
        private static MeshFilter _improvisedPrybarPropMeshFilter;

        private static void CaptureVanillaPrybarMeshFilter()
        {
            if (_vanillaPrybarMeshFilterRef != null)
            {
                return;
            }

            Il2CppArrayBase<StruggleMeshTable> allTables = Resources.FindObjectsOfTypeAll<StruggleMeshTable>();

            MelonLogger.Msg($"[BaltaTools][diag] Talált StruggleMeshTable példányok száma: {allTables.Length}");

            foreach (StruggleMeshTable table in allTables)
            {
                if (table == null)
                {
                    continue;
                }

                MelonLogger.Msg($"[BaltaTools][diag] StruggleMeshTable instanceId={table.GetInstanceID()}, " +
                    $"gameObject={table.gameObject.name}, activeInHierarchy={table.gameObject.activeInHierarchy}, " +
                    $"m_Prybar={(table.m_Prybar == null ? "NULL" : table.m_Prybar.gameObject.name)}, " +
                    $"m_PrybarInstanceId={(table.m_Prybar == null ? -1 : table.m_Prybar.GetInstanceID())}, " +
                    $"m_Prybar.activeInHierarchy={(table.m_Prybar == null ? false : table.m_Prybar.gameObject.activeInHierarchy)}");

                if (table.m_Prybar == null)
                {
                    continue;
                }

                // Csak AKTÍV táblát fogadjunk el - ez a whetstone-bugnál is bevált szűrés.
                if (!table.gameObject.activeInHierarchy)
                {
                    MelonLogger.Msg($"[BaltaTools][diag] Kihagyva (inaktív): {table.gameObject.name}");
                    continue;
                }

                _vanillaPrybarMeshFilterRef = table.m_Prybar;
                _vanillaPrybarSharedMesh = table.m_Prybar.sharedMesh;

                MeshRenderer renderer = table.m_Prybar.GetComponent<MeshRenderer>();
                if (renderer != null)
                {
                    _vanillaPrybarMeshRendererRef = renderer;
                    _vanillaPrybarSharedMaterials = renderer.sharedMaterials;
                }

                MelonLogger.Msg($"[BaltaTools] Vanilla Prybar MeshFilter/Renderer ELFOGADVA (aktív): {table.m_Prybar.gameObject.name}, instanceId={table.m_Prybar.GetInstanceID()}");
                return;
            }

            MelonLogger.Warning("[BaltaTools] Nem található AKTÍV vanilla Prybar MeshFilter a helyben-cseréhez.");
        }

        private static void CaptureImprovisedSourceMesh(GearItem sourceGearItem)
        {
            if (_improvisedSourceMesh != null)
            {
                return;
            }

            if (sourceGearItem == null || sourceGearItem.gameObject == null)
            {
                return;
            }

            MeshFilter[] allMeshFilters = sourceGearItem.gameObject.GetComponentsInChildren<MeshFilter>(true);

            MeshFilter bestMeshFilter = null;
            float bestVolume = 0f;

            foreach (MeshFilter mf in allMeshFilters)
            {
                if (mf == null || mf.sharedMesh == null)
                {
                    continue;
                }

                Vector3 size = mf.sharedMesh.bounds.size;
                float volume = size.x * size.y * size.z;

                MelonLogger.Msg($"[BaltaTools][diag] MeshFilter jelölt: {mf.gameObject.name}, mesh={mf.sharedMesh.name}, boundsSize={size}, volume={volume}");

                if (volume > bestVolume)
                {
                    bestVolume = volume;
                    bestMeshFilter = mf;
                }
            }

            if (bestMeshFilter == null)
            {
                MelonLogger.Warning("[BaltaTools] Improvised Prybar: nem található érdemi (nem degenerált) mesh.");
                return;
            }

            _improvisedSourceMesh = bestMeshFilter.sharedMesh;

            MeshRenderer sourceRenderer = bestMeshFilter.GetComponent<MeshRenderer>();
            if (sourceRenderer != null)
            {
                _improvisedSourceMaterials = sourceRenderer.sharedMaterials;
            }

            MelonLogger.Msg($"[BaltaTools] Improvised Prybar forrás mesh KIVÁLASZTVA: {bestMeshFilter.gameObject.name} ({_improvisedSourceMesh.name}), bounds={_improvisedSourceMesh.bounds.size}");
        }

        private static bool _useImprovisedPrybarMesh;

        public static void SetPrybarMesh(bool improvised)
        {
            CaptureVanillaPrybarMeshFilter();

            if (_vanillaPrybarMeshFilterRef == null)
            {
                MelonLogger.Warning("[BaltaTools] Nincs vanilla Prybar MeshFilter referencia, nem tudunk cserélni.");
                return;
            }

            if (improvised)
            {
                if (_improvisedSourceMesh == null)
                {
                    GearItem source = GetImprovisedPrybarFromInventory() ?? FindImprovisedPrybarPrefab();
                    CaptureImprovisedSourceMesh(source);
                }

                if (_improvisedSourceMesh == null)
                {
                    MelonLogger.Warning("[BaltaTools] Improvised Prybar mesh NULL, nem tudjuk használni.");
                    return;
                }

                _vanillaPrybarMeshFilterRef.sharedMesh = _improvisedSourceMesh;
                if (_vanillaPrybarMeshRendererRef != null && _improvisedSourceMaterials != null)
                {
                    _vanillaPrybarMeshRendererRef.sharedMaterials = _improvisedSourceMaterials;
                }
            }
            else
            {
                _vanillaPrybarMeshFilterRef.sharedMesh = _vanillaPrybarSharedMesh;
                if (_vanillaPrybarMeshRendererRef != null && _vanillaPrybarSharedMaterials != null)
                {
                    _vanillaPrybarMeshRendererRef.sharedMaterials = _vanillaPrybarSharedMaterials;
                }
            }

            _useImprovisedPrybarMesh = improvised;
            MelonLogger.Msg($"[BaltaTools] Prybar struggle mesh mode = {(improvised ? "IMPROVISED" : "VANILLA")}");
        }

        public static bool ShouldUseImprovisedPrybarMesh()
        {
            return _useImprovisedPrybarMesh;
        }

        /*public static MeshFilter GetImprovisedPrybarMesh()
        {
            GetImprovisedPrybarMesh();

            return _cachedImprovisedMesh;
        }*/

        public static bool IsImprovisedPrybar(
            GearItem gearItem)
        {
            if (gearItem == null ||
                gearItem.gameObject == null)
            {
                return false;
            }

            return gearItem.gameObject.name ==
                   ImprovisedPrybarName;
        }
    }

    // ------------------------------------------------------------
    // Inventory megnyitásakor biztosítjuk a StruggleBonus-t.
    // ------------------------------------------------------------

    [HarmonyPatch(
        typeof(Panel_Inventory),
        nameof(Panel_Inventory.Enable),
        new[] { typeof(bool) })]
    internal static class Panel_Inventory_Enable_EnsurePrybarStruggle_Patch
    {
        private static void Postfix(bool enable)
        {
            if (!enable)
            {
                return;
            }

            ImprovisedPrybarStruggle
                .EnsureStruggleSetupOnAll();
        }
    }

    // ------------------------------------------------------------
    // STRUGGLE WEAPON PICKER
    //
    // A vanilla picker listájába betesszük az improvised prybar-t,
    // ha az inventoryban ténylegesen rendelkezésre áll.
    // ------------------------------------------------------------

    [HarmonyPatch(
        typeof(Panel_WeaponPicker),
        nameof(Panel_WeaponPicker.Enable),
        new[]
        {
            typeof(bool),
            typeof(Il2CppSystem.Collections.Generic.List<GearItem>),
            typeof(float)
        })]
    internal static class Panel_WeaponPicker_Enable_PrybarPatch
    {
        private static void Prefix(
            bool enable,
            Il2CppSystem.Collections.Generic.List<GearItem> listItems,
            float durationSeconds)
        {
            if (!enable)
            {
                return;
            }

            if (listItems == null)
            {
                MelonLogger.Msg(
                    "[BaltaTools] WeaponPicker listItems == NULL.");
                return;
            }

            GearItem improvisedPrybar =
                ImprovisedPrybarStruggle
                    .GetImprovisedPrybarFromInventory();

            if (improvisedPrybar == null)
            {
                return;
            }

            bool alreadyPresent =
                listItems.Contains(improvisedPrybar);

            if (alreadyPresent)
            {
                MelonLogger.Msg(
                    "[BaltaTools] Improvised Prybar már benne van " +
                    "a WeaponPicker listában.");

                return;
            }

            listItems.Add(improvisedPrybar);

            MelonLogger.Msg(
                "[BaltaTools] Improvised Prybar hozzáadva " +
                "a Struggle WeaponPicker listájához.");
        }
    }

    // ------------------------------------------------------------
    // Amikor a játék kiválasztja a struggle fegyvert:
    //
    // vanilla Prybar → vanilla mesh
    // improvised Prybar → saját mesh
    //
    // Az animáció mindkettőnél ugyanaz: StruggleWeaponType.Prybar.
    // ------------------------------------------------------------

    /*[HarmonyPatch(
        typeof(PlayerStruggle),
        nameof(PlayerStruggle.OnWeaponPicked))]
    internal static class PlayerStruggle_OnWeaponPicked_PrybarMeshPatch
    {
        private static void Postfix(
            PlayerStruggle __instance,
            GearItem gearItem)
        {
            if (gearItem == null)
            {
                return;
            }

            bool improvised =
                ImprovisedPrybarStruggle
                    .IsImprovisedPrybar(gearItem);

            MelonLogger.Msg(
                $"[BaltaTools] PlayerStruggle.OnWeaponPicked(): " +
                $"{gearItem.gameObject.name}, " +
                $"improvised={improvised}");

            ImprovisedPrybarStruggle
                .SetPrybarMesh(improvised);
        }
    }*/
    [HarmonyPatch(
    typeof(Panel_WeaponPicker),
    nameof(Panel_WeaponPicker.SelectGridItem))]
    internal static class Panel_WeaponPicker_SelectGridItem_Patch
    {
        private static void Postfix(
            Panel_WeaponPicker __instance,
            WeaponPickerGridItem gridItem,
            bool isInContainer)
        {
            if (gridItem == null)
            {
                MelonLogger.Msg(
                    "[BaltaTools] SelectGridItem: gridItem = NULL");

                return;
            }

            GearItem selectedGear =
                gridItem.GetGearItem();

            if (selectedGear == null)
            {
                MelonLogger.Msg(
                    "[BaltaTools] SelectGridItem: " +
                    "gridItem.GetGearItem() = NULL");

                return;
            }

            if (selectedGear.gameObject == null)
            {
                MelonLogger.Msg(
                    "[BaltaTools] SelectGridItem: " +
                    "selectedGear.gameObject = NULL");

                return;
            }

            bool improvised =
                selectedGear.gameObject.name ==
                "GEAR_ImprovisedPrybar";

            MelonLogger.Msg(
                $"[BaltaTools] SelectGridItem → " +
                $"{selectedGear.gameObject.name}, " +
                $"improvised={improvised}");

            ImprovisedPrybarStruggle.SetPrybarMesh(
                improvised);
        }
    }

    // ------------------------------------------------------------
    // Struggle végén visszaállítjuk a vanilla mesh-t.
    // Ez azért kell, hogy a következő vanilla prybar struggle
    // biztosan ne az improvised mesh-sel induljon.
    // ------------------------------------------------------------

    [HarmonyPatch(
        typeof(PlayerStruggle),
        nameof(PlayerStruggle.BreakStruggle))]
    internal static class PlayerStruggle_BreakStruggle_RestoreMeshPatch
    {
        private static void Postfix()
        {
            ImprovisedPrybarStruggle
                .SetPrybarMesh(false);

            MelonLogger.Msg(
                "[BaltaTools] Struggle vége → vanilla Prybar mesh visszaállítva.");
        }
    }
    [HarmonyPatch(
        typeof(Panel_WeaponPicker),
        nameof(Panel_WeaponPicker.GetBestItemPerCategory))]
    internal static class Panel_WeaponPicker_GetBestItemPerCategory_Patch
    {
        private static void Postfix(
            StruggleBonus.StruggleWeaponType type,
            ref GearItem __result)
        {
            if (type != StruggleBonus.StruggleWeaponType.Prybar)
            {
                return;
            }

            Inventory inventory =
                GameManager.GetInventoryComponent();

            if (inventory == null)
            {
                return;
            }

            GearItem improvisedPrybar =
                inventory.GetHighestConditionGearThatMatchesName(
                    "GEAR_ImprovisedPrybar");

            if (improvisedPrybar == null)
            {
                return;
            }

            MelonLogger.Msg(
                $"[BaltaTools] GetBestItemPerCategory(Prybar): " +
                $"vanilla result = " +
                $"{(__result == null ? "NULL" : __result.gameObject.name)}");

            __result = improvisedPrybar;

            MelonLogger.Msg(
                "[BaltaTools] GetBestItemPerCategory(Prybar) → " +
                "GEAR_ImprovisedPrybar");
        }
    }
    /*[HarmonyPatch(
    typeof(StruggleMeshTable),
    nameof(StruggleMeshTable.GetMesh))]
    internal static class StruggleMeshTable_GetMesh_PrybarPatch
    {
        private static void Postfix(
            StruggleMeshTable __instance,
            StruggleBonus.StruggleWeaponType type,
            ref MeshFilter __result)
        {
            if (type !=
                StruggleBonus.StruggleWeaponType.Prybar)
            {
                return;
            }

            if (!ImprovisedPrybarStruggle
                .ShouldUseImprovisedPrybarMesh())
            {
                return;
            }

            MeshFilter improvisedMesh =
                ImprovisedPrybarStruggle
                    .GetImprovisedPrybarMesh();

            if (improvisedMesh == null)
            {
                return;
            }

            MelonLogger.Msg(
                $"[BaltaTools] StruggleMeshTable.GetMesh(Prybar) " +
                $"→ IMPROVISED ({improvisedMesh.gameObject.name})");

            __result = improvisedMesh;
        }
    }*/
    [HarmonyPatch(typeof(StruggleMeshTable), nameof(StruggleMeshTable.GetMesh))]
    internal static class StruggleMeshTable_GetMesh_Diag_Patch
    {
        private static void Postfix(StruggleMeshTable __instance, StruggleBonus.StruggleWeaponType type, ref MeshFilter __result)
        {
            MelonLogger.Msg($"[BaltaTools][diag] GetMesh hívva: tableInstanceId={__instance.GetInstanceID()}, " +
                $"type={type}, result={(__result == null ? "NULL" : __result.gameObject.name)}, " +
                $"resultInstanceId={(__result == null ? -1 : __result.GetInstanceID())}, " +
                $"result.activeInHierarchy={(__result == null ? false : __result.gameObject.activeInHierarchy)}");
        }
    }
}