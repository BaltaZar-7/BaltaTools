#nullable disable
using MelonLoader;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using ExpandedTradingFramework.Framework;

namespace BaltaTools
{
    public class Main : MelonMod
    {
        public static Mesh ImprovisedPrybarFPHMesh;
        public static Mesh SurvivalHatchetFPHMesh;
        public static Mesh SurvivalHatchetCINEMesh;
        public static Material SurvivalHatchetMat;
        public static Material SurvivalHatchetMatBloody;

        public override void OnInitializeMelon()
        {
            MelonCoroutines.Start(PreloadImprovisedPrybarMesh());
            MelonCoroutines.Start(PreloadSurvivalHatchetMesh());
            MelonCoroutines.Start(PreloadSurvivalHatchetMeshCINE());
            MelonCoroutines.Start(PreloadSurvivalHatchetMaterial());
            MelonCoroutines.Start(PreloadSurvivalHatchetMaterialBloody());

            MelonLogger.Msg("[BaltaTools] BaltaTools initialized");
            DebugHelper.Init();

            CustomTradeDefinition[] enabledTrades = BaltaToolsTradeList.GetEnabledTrades();
            int registered = CustomTradeRegistry.RegisterExternalTrades(enabledTrades);
            MelonLogger.Msg($"[BaltaTools] {registered}/{enabledTrades.Length} Custom trades registered.");
        }
        private static System.Collections.IEnumerator PreloadImprovisedPrybarMesh()
        {
            while (true)
            {
                if (ImprovisedPrybarFPHMesh != null)
                {
                    yield break;
                }

                DebugHelper.Log("[BaltaTools] Waiting for " + "FPH_ImprovisedPrybarMesh Addressable...");

                AsyncOperationHandle<Mesh> handle = Addressables.LoadAssetAsync<Mesh>("FPH_ImprovisedPrybarMesh");

                yield return handle;

                if (handle.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded && handle.Result != null)
                {
                    ImprovisedPrybarFPHMesh = handle.Result;

                    MelonLogger.Msg($"[BaltaTools] FPH_ImprovisedPrybarMesh loaded successfully. " + $"Vertices={ImprovisedPrybarFPHMesh.vertexCount}, " + $"Name={ImprovisedPrybarFPHMesh.name}");

                    yield break;
                }

                DebugHelper.Log("[BaltaTools] FPH_ImprovisedPrybarMesh " + "load failed. Retrying in 1 second.");

                yield return new WaitForSeconds(1f);
            }
        }
        private static System.Collections.IEnumerator PreloadSurvivalHatchetMesh()
        {
            while (true)
            {
                if (SurvivalHatchetFPHMesh != null)
                {
                    yield break;
                }

                DebugHelper.Log("[BaltaTools] Waiting for " + "FPH_SurvivalHatchetMesh Addressable...");

                AsyncOperationHandle<Mesh> handle = Addressables.LoadAssetAsync<Mesh>("FPH_SurvivalHatchetMesh");

                yield return handle;

                if (handle.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded && handle.Result != null)
                {
                    SurvivalHatchetFPHMesh = handle.Result;

                    MelonLogger.Msg($"[BaltaTools] FPH_SurvivalHatchetMesh loaded successfully. " + $"Vertices={SurvivalHatchetFPHMesh.vertexCount}, " + $"Name={SurvivalHatchetFPHMesh.name}");

                    yield break;
                }

                DebugHelper.Log("[BaltaTools] SurvivalHatchet " + "load failed. Retrying in 1 second.");

                yield return new WaitForSeconds(1f);
            }
        }
        private static System.Collections.IEnumerator PreloadSurvivalHatchetMeshCINE()
        {
            while (true)
            {
                if (SurvivalHatchetCINEMesh != null)
                {
                    yield break;
                }

                DebugHelper.Log("[BaltaTools] Waiting for " + "Cine_Harvest_SurvivalHatchetMesh Addressable...");

                AsyncOperationHandle<Mesh> handle = Addressables.LoadAssetAsync<Mesh>("Cine_Harvest_SurvivalHatchetMesh");

                yield return handle;

                if (handle.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded && handle.Result != null)
                {
                    SurvivalHatchetCINEMesh = handle.Result;

                    MelonLogger.Msg($"[BaltaTools] Cine_Harvest_SurvivalHatchetMesh loaded successfully. " + $"Vertices={SurvivalHatchetCINEMesh.vertexCount}, " + $"Name={SurvivalHatchetCINEMesh.name}");

                    yield break;
                }

                DebugHelper.Log("[BaltaTools] SurvivalHatchetCINE " + "load failed. Retrying in 1 second.");

                yield return new WaitForSeconds(1f);
            }
        }
        private static System.Collections.IEnumerator PreloadSurvivalHatchetMaterial()
        {
            while (true)
            {
                if (SurvivalHatchetMat != null)
                {
                    yield break;
                }

                DebugHelper.Log("[BaltaTools] Waiting for " + "Cine_Harvest_SurvivalHatchetMaterial Addressable...");

                AsyncOperationHandle<Material> handle = Addressables.LoadAssetAsync<Material>("SurvivalHatchetMat");

                yield return handle;

                if (handle.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded && handle.Result != null)
                {
                    SurvivalHatchetMat = handle.Result;

                    MelonLogger.Msg($"[BaltaTools] Cine_Harvest_SurvivalHatchetMaterial loaded successfully. " + $"Name={SurvivalHatchetMat.name}");

                    yield break;
                }

                DebugHelper.Log("[BaltaTools] SurvivalHatchetMaterial " + "load failed. Retrying in 1 second.");

                yield return new WaitForSeconds(1f);
            }
        }
        private static System.Collections.IEnumerator PreloadSurvivalHatchetMaterialBloody()
        {
            while (true)
            {
                if (SurvivalHatchetMatBloody != null)
                {
                    yield break;
                }

                DebugHelper.Log("[BaltaTools] Waiting for " + "Cine_Harvest_SurvivalHatchetBloodyMaterial Addressable...");

                AsyncOperationHandle<Material> handle = Addressables.LoadAssetAsync<Material>("SurvivalHatchetBloodMat");

                yield return handle;

                if (handle.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded && handle.Result != null)
                {
                    SurvivalHatchetMatBloody = handle.Result;

                    MelonLogger.Msg($"[BaltaTools] Cine_Harvest_SurvivalHatchetBloodyMaterial loaded successfully. " + $"Name={SurvivalHatchetMatBloody.name}");

                    yield break;
                }

                DebugHelper.Log("[BaltaTools] SurvivalHatchetBloodyMaterial " + "load failed. Retrying in 1 second.");

                yield return new WaitForSeconds(1f);
            }
        }

        public override void OnSceneWasUnloaded(int buildIndex, string sceneName)
        {
            ImprovisedToolsRegistry.ResetCache();
            //SplittingAxeRegistry.ResetCache();
            ImprovisedPrybarRegistry.ResetCache();
            SurvivalHatchetRegistry.ResetCache();

            DebugHelper.Log($"[BaltaTools] Scene unloaded ({sceneName}), registry caches deleted.");
        }
    }
}