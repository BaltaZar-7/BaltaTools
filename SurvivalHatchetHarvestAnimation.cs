#nullable disable
using HarmonyLib;
using MelonLoader;
using UnityEngine;
using Il2Cpp;
using Il2CppTLD.Gameplay;
using Il2CppInterop.Runtime.InteropTypes.Arrays;

namespace BaltaTools
{
    internal static class SurvivalHatchetHarvestAnimation
    {
        private static bool _useSurvivalMaterial;

        private static MeshRenderer _currentSurvivalHatchetRenderer;

        // Survival Hatchet -> vanilla Hatchet timeline animation
        [HarmonyPatch(typeof(BodyHarvestTimelineSet),nameof(BodyHarvestTimelineSet.GetHarvestTimeline))]
        public static class BodyHarvestTimelineSet_GetHarvestTimeline_SurvivalHatchet_Patch
        {
            private static GearItem _vanillaHatchetReference;

            static void Prefix(ref GearItem toolUsed)
            {
                if (toolUsed == null || toolUsed.gameObject == null)
                {
                    return;
                }

                if (toolUsed.gameObject.name != "GEAR_SurvivalHatchet")
                {
                    SetUseSurvivalMaterial(false);
                    return;
                }

                SetUseSurvivalMaterial(true);

                GearItem vanillaHatchet =
                    FindVanillaHatchetReference();

                if (vanillaHatchet != null)
                {
                    toolUsed = vanillaHatchet;

                    DebugHelper.Log("[BaltaTools][HarvestTimeline] " + "SurvivalHatchet -> GEAR_Hatchet");
                }
            }
            private static GearItem FindVanillaHatchetReference()
            {
                if (_vanillaHatchetReference != null)
                {
                    return _vanillaHatchetReference;
                }

                Il2CppArrayBase<GearItem> allGearItems = Resources.FindObjectsOfTypeAll<GearItem>();

                foreach (GearItem gearItem in allGearItems)
                {
                    if (gearItem == null || gearItem.gameObject == null)
                    {
                        continue;
                    }

                    if (gearItem.gameObject.name == "GEAR_Hatchet")
                    {
                        _vanillaHatchetReference = gearItem;
                        return _vanillaHatchetReference;
                    }
                }
                return null;
            }
        }

        // Cinematic Hatchet visual swap

        [HarmonyPatch(typeof(TLD_SpawnedAnimationTrack),"SpawnObject")]
        internal static class Patch_SpawnedAnimationTrack_SwapHatchetMesh
        {
            private static void Postfix(TLD_SpawnedAnimationTrack __instance, GameObject __result)
            {
                if (__result == null || __instance == null)
                {
                    return;
                }

                GameObject prefab = __instance.m_PrefabObject;

                if (prefab == null || prefab.name != "Cine_Harvest_Hatchet")
                {
                    return;
                }

                if (!ShouldUseSurvivalHatchetVisual())
                {
                    return;
                }

                MelonCoroutines.Start(ApplySurvivalHatchetVisualDelayed(__result));
            }

            private static System.Collections.IEnumerator
                ApplySurvivalHatchetVisualDelayed(GameObject spawnedRoot)
            {
                yield return null;

                int attempts = 0;

                while (attempts < 10)
                {
                    SkinnedMeshRenderer renderer = spawnedRoot.GetComponentInChildren<SkinnedMeshRenderer>(true);

                    if (renderer != null && renderer.gameObject.activeInHierarchy)
                    {
                        DebugHelper.Log("[BaltaTools][diag] " + "Renderer active after " + attempts + " extra frame(s).");
                        break;
                    }

                    attempts++;

                    yield return null;
                }

                ApplySurvivalHatchetVisual(spawnedRoot);
            }

            private static bool
                ShouldUseSurvivalHatchetVisual()
            {
                return _useSurvivalMaterial;
            }

            private static void ApplySurvivalHatchetVisual(GameObject spawnedRoot)
            {
                if (spawnedRoot == null)
                {
                    return;
                }

                _currentSurvivalHatchetRenderer = null;

                // Vanilla SkinnedMeshRenderer
                SkinnedMeshRenderer targetSkinnedRenderer = spawnedRoot.GetComponentInChildren<SkinnedMeshRenderer>(true);

                if (targetSkinnedRenderer == null)
                {
                    MelonLogger.Warning("[BaltaTools] Not found " + "SkinnedMeshRenderer on spawned object.");

                    return;
                }

                // Child to base_jnt bone

                Transform baseBone = FindBoneByName(targetSkinnedRenderer, "base_jnt");

                if (baseBone == null)
                {
                    MelonLogger.Warning("[BaltaTools] Not found " + "bone named'base_jnt'.");
                    return;
                }

                // Own custom mesh
                Mesh survivalMesh = SurvivalHatchetVisualAssets.GetMesh();

                if (survivalMesh == null)
                {
                    MelonLogger.Warning("[BaltaTools] Survival Hatchet mesh " + "not available.");
                    return;
                }


                // NORMAL material

                Material normalMaterial = SurvivalHatchetVisualAssets.GetMaterial();

                if (normalMaterial == null)
                {
                    MelonLogger.Warning("[BaltaTools] Survival Hatchet normal material " + "not available.");
                    return;
                }

                // Debug

                Vector3 rendererPosRelativeToBone = baseBone.InverseTransformPoint(targetSkinnedRenderer.transform.position);

                DebugHelper.Log("[BaltaTools][diag] Renderer position " + "relative to base_jnt: " + rendererPosRelativeToBone);
                DebugHelper.Log("[BaltaTools][diag] NORMAL material = " + normalMaterial.name);
                DebugHelper.Log("[BaltaTools][diag] BLOODY material currently cached = " + SurvivalHatchetVisualAssets.GetBloodyMaterialName());

                // Turn off Vanilla renderer
                targetSkinnedRenderer.enabled = false;

                // Own prop (survival hatchet model)
                GameObject staticReplacement = new GameObject("BaltaTools_SurvivalHatchetCineProp");


                staticReplacement.transform.SetParent(baseBone, false);
                staticReplacement.transform.localPosition = Vector3.zero;
                staticReplacement.transform.localRotation = Quaternion.identity;
                staticReplacement.transform.localScale = Vector3.one;
                staticReplacement.layer = targetSkinnedRenderer.gameObject.layer;

                MeshFilter replacementFilter = staticReplacement.AddComponent<MeshFilter>();

                replacementFilter.sharedMesh = survivalMesh;


                MeshRenderer replacementRenderer = staticReplacement.AddComponent<MeshRenderer>();

                replacementRenderer.sharedMaterial = normalMaterial;


                // Global reference for Anim stage 2
                _currentSurvivalHatchetRenderer = replacementRenderer;
                DebugHelper.Log("[BaltaTools] Survival Hatchet statikus prop " + "created under '" + baseBone.name + ".");
                DebugHelper.Log("[BaltaTools][diag] Initial material = " + replacementRenderer.sharedMaterial.name);
                DebugHelper.Log("[BaltaTools][diag] Current Survival renderer cached = " + (_currentSurvivalHatchetRenderer != null));
            }


            private static Transform FindBoneByName(SkinnedMeshRenderer renderer, string boneName)
            {
                Transform[] bones = renderer.bones;

                if (bones == null)
                {
                    return null;
                }

                for (int i = 0;
                     i < bones.Length;
                     i++)
                {
                    if (bones[i] != null && bones[i].name == boneName)
                    {
                        return bones[i];
                    }
                }

                return null;
            }
        }

        // Stage 2 = bloody material
        [HarmonyPatch(typeof(BloodSplatterFromBodyHarvest),nameof(BloodSplatterFromBodyHarvest.OnSplatter))]
        internal static class BloodSplatterFromBodyHarvest_OnSplatter_SurvivalHatchet_Patch
        {
            private static void Postfix(BloodSplatterFromBodyHarvest __instance, int stageIndex)
            {
                if (__instance == null)
                {
                    return;
                }
                if (!_useSurvivalMaterial)
                {
                    return;
                }
                if (stageIndex != 2)
                {
                    return;
                }
                DebugHelper.Log("[BaltaTools][Blood] " + "Stage 2 received.");

                MeshRenderer renderer = _currentSurvivalHatchetRenderer;

                if (renderer == null)
                {
                    MelonLogger.Warning("[BaltaTools][Blood] " + "Current Survival Hatchet renderer = NULL.");
                    return;
                }

                if (renderer.gameObject == null)
                {
                    MelonLogger.Warning("[BaltaTools][Blood] " + "Current Survival Hatchet GameObject = NULL.");
                    return;
                }

                Material bloodyMaterial = SurvivalHatchetVisualAssets.GetBloodyMaterial();

                if (bloodyMaterial == null)
                {
                    MelonLogger.Warning("[BaltaTools][Blood] " + "SurvivalHatchetMatBloody = NULL.");
                    return;
                }

                renderer.sharedMaterial = bloodyMaterial;
                DebugHelper.Log("[BaltaTools][Blood] " + "Stage 2 -> material switched: " + bloodyMaterial.name);
            }
        }

        public static void SetUseSurvivalMaterial(bool use)
        {
            _useSurvivalMaterial = use;

            if (!use)
            {
                _currentSurvivalHatchetRenderer = null;
            }
            DebugHelper.Log("[BaltaTools] Survival Hatchet Material used = " + use);
        }

        internal static class SurvivalHatchetVisualAssets
        {
            private static Mesh _mesh;
            private static Material _material;
            private static Material _bloodyMaterial;

            public static Mesh GetMesh()
            {
                if (_mesh != null)
                {
                    return _mesh;
                }
                _mesh = Main.SurvivalHatchetCINEMesh;

                if (_mesh == null)
                {
                    MelonLogger.Warning("[BaltaTools] " + "CINE_Harvest_SurvivalHatchet Mesh " + "loading failure.");
                }
                else
                {
                    DebugHelper.Log("[BaltaTools] Survival cinematic mesh loaded: " + _mesh.name);
                }
                return _mesh;
            }

            public static Material GetMaterial()
            {
                if (_material != null)
                {
                    return _material;
                }

                _material = Main.SurvivalHatchetMat;

                if (_material == null)
                {
                    MelonLogger.Warning("[BaltaTools] Survival Hatchet normal material " + "loading failure.");
                }
                else
                {
                    DebugHelper.Log("[BaltaTools] Survival cinematic normal material: " + _material.name);
                }
                return _material;
            }

            public static Material GetBloodyMaterial()
            {
                if (_bloodyMaterial != null)
                {
                    return _bloodyMaterial;
                }

                _bloodyMaterial = Main.SurvivalHatchetMatBloody;

                if (_bloodyMaterial == null)
                {
                    MelonLogger.Warning("[BaltaTools] Survival Hatchet bloody material " + "loading failure.");
                }
                else
                {
                    DebugHelper.Log("[BaltaTools] Survival cinematic bloody material: " + _bloodyMaterial.name);
                }
                return _bloodyMaterial;
            }

            public static string GetBloodyMaterialName()
            {
                if (Main.SurvivalHatchetMatBloody == null)
                {
                    return "<NULL>";
                }
                return Main.SurvivalHatchetMatBloody.name;
            }
        }
    }
}