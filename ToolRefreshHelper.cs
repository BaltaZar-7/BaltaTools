#nullable disable
using HarmonyLib;
using Il2Cpp;
using MelonLoader;
using System.Reflection;
using UnityEngine;

namespace BaltaTools
{
    [HarmonyPatch(typeof(ToolsList),nameof(ToolsList.OnToolIncrease))]
    internal static class ToolsList_OnToolIncrease_Patch
    {
        private static void Postfix(ToolsList __instance)
        {
            DebugHelper.Log("[BaltaTools] ToolsList.OnToolIncrease() POSTFIX");

            ToolRefreshHelper.RefreshParentPanel(__instance);
        }
    }


    [HarmonyPatch(typeof(ToolsList),nameof(ToolsList.OnToolDecrease))]
    internal static class ToolsList_OnToolDecrease_Patch
    {
        private static void Postfix(
            ToolsList __instance)
        {
            DebugHelper.Log("[BaltaTools] ToolsList.OnToolDecrease() POSTFIX");

            ToolRefreshHelper.RefreshParentPanel(__instance);
        }
    }


    internal static class ToolRefreshHelper
    {
        private static MethodInfo _updateCleanLabels;
        private static MethodInfo _updateSharpenLabels;

        public static void RefreshParentPanel(ToolsList toolsList)
        {
            if (toolsList == null)
            {
                DebugHelper.Log("[BaltaTools] ToolsList = NULL");

                return;
            }

            Panel_Inventory_Examine panel = toolsList.GetComponentInParent<Panel_Inventory_Examine>();

            if (panel == null)
            {
                DebugHelper.Log("[BaltaTools] Parent Panel_Inventory_Examine = NULL");

                return;
            }

            GameObject selectedTool = toolsList.GetSelectedTool();

            DebugHelper.Log($"[BaltaTools] Tool switched to = " + $"{(selectedTool == null ? "NULL" : selectedTool.name)}");


            bool cleanPanelActive =
                panel.m_CleanPanel != null &&
                panel.m_CleanPanel.activeInHierarchy;

            bool sharpenPanelActive =
                panel.m_SharpenPanel != null &&
                panel.m_SharpenPanel.activeInHierarchy;

            DebugHelper.Log($"[BaltaTools] CleanPanel active = " + $"{cleanPanelActive}, " + $"SharpenPanel active = " + $"{sharpenPanelActive}");

            if (cleanPanelActive)
            {
                DebugHelper.Log("[BaltaTools] CLEAN PANEL detected.");

                RefreshCleanLabels(panel);
            }


            if (sharpenPanelActive)
            {
                DebugHelper.Log("[BaltaTools] SHARPEN PANEL detected.");

                RefreshSharpenLabels(panel);
            }
        }


        private static void RefreshCleanLabels(
            Panel_Inventory_Examine panel)
        {
            if (_updateCleanLabels == null)
            {
                _updateCleanLabels = AccessTools.Method(typeof(Panel_Inventory_Examine),"UpdateCleanLabels");
            }

            if (_updateCleanLabels == null)
            {
                MelonLogger.Error("[BaltaTools] Could not find UpdateCleanLabels().");

                return;
            }

            DebugHelper.Log("[BaltaTools] Calling UpdateCleanLabels().");

            _updateCleanLabels.Invoke(panel,null);

            DebugHelper.Log($"[BaltaTools] Clean label after refresh = " + $"{panel.m_Clean_TimeLabel.text}");
        }


        private static void RefreshSharpenLabels(
            Panel_Inventory_Examine panel)
        {
            if (_updateSharpenLabels == null)
            {
                _updateSharpenLabels = AccessTools.Method(typeof(Panel_Inventory_Examine),"UpdateSharpenLabels");
            }

            if (_updateSharpenLabels == null)
            {
                MelonLogger.Error("[BaltaTools] Could not find UpdateSharpenLabels().");

                return;
            }

            DebugHelper.Log("[BaltaTools] Calling UpdateSharpenLabels().");

            _updateSharpenLabels.Invoke(panel,null);

            DebugHelper.Log($"[BaltaTools] Sharpen label after refresh = " + $"{panel.m_Sharpen_TimeLabel.text}");
        }
    }
}