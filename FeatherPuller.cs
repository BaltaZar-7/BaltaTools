#nullable disable
using HarmonyLib;
using Il2Cpp;
using MelonLoader;

namespace BaltaTools
{
    internal static class FeatherPluckerRegistry
    {
        private const string featherPluckerName = "GEAR_FeatherPlucker";
        private const float PtarmiganHarvestTimeMultiplier = 0.65f;

        public static bool PlayerHasfeatherPlucker()
        {
            GearItem featherPlucker = GameManager.GetInventoryComponent().GetHighestConditionGearThatMatchesName(featherPluckerName);
            return featherPlucker != null;
        }

        public static bool IsPtarmiganHarvest(BodyHarvest bodyHarvest)
        {
            if (bodyHarvest == null)
            {
                return false;
            }

            return bodyHarvest.gameObject.name.Contains("Ptarmigan");
        }

        public static float ApplyBonusIfEligible(BodyHarvest bodyHarvest, float baseMinutes)
        {
            if (!IsPtarmiganHarvest(bodyHarvest))
            {
                return baseMinutes;
            }

            if (!PlayerHasfeatherPlucker())
            {
                return baseMinutes;
            }

            float modifiedMinutes = baseMinutes * PtarmiganHarvestTimeMultiplier;
            //DebugHelper.Log($"[BaltaTools] featherPlucker bónusz alkalmazva: {baseMinutes} -> {modifiedMinutes}");
            return modifiedMinutes;
        }
    }

    [HarmonyPatch(typeof(Panel_BodyHarvest), "GetHarvestDurationMinutes")]
    public static class Panel_BodyHarvest_featherPlucker_Patch
    {
        static void Postfix(Panel_BodyHarvest __instance, ref float __result)
        {
            BodyHarvest bodyHarvest = __instance.m_BodyHarvest;
            __result = FeatherPluckerRegistry.ApplyBonusIfEligible(bodyHarvest, __result);
        }
    }
}