#nullable disable
using MelonLoader;

namespace BaltaTools
{
    public class Main : MelonMod
    {
        public override void OnInitializeMelon()
        {
            MelonLogger.Msg("[BaltaTools] BaltaTools initialized");
            DebugHelper.Init();
            //HarvestDiscoveryDiag.DumpRelevantMembers();
        }

        public override void OnSceneWasUnloaded(int buildIndex, string sceneName)
        {
            ImprovisedToolsRegistry.ResetCache();
            SplittingAxeRegistry.ResetCache();
            ImprovisedPrybarRegistry.ResetCache();

            DebugHelper.Log($"[BaltaTools] Scene betöltve ({sceneName}), registry cache-ek törölve.");
        }
    }
}