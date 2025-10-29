using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;

namespace StickFightExtendedPlayers
{
    [HarmonyPatch(typeof(WinCounterUI), "Start")]
    public class WinCounterUI_Patches_Start
    {
        static void Prefix(WinCounterUI __instance, ref TextMeshProUGUI[] ___mPlayerWinTexts, ref Color[] ___mPlayerColors)
        {
            ___mPlayerColors = [..___mPlayerColors, ..Plugin.NEW_COLOURS.Values];
            List<TextMeshProUGUI> newTexts = new List<TextMeshProUGUI>();
            for (int i = ___mPlayerWinTexts.Length; i <= Plugin.MAX_PLAYERS; i++)
            {
                GameObject newObject = GameObject.Instantiate(___mPlayerWinTexts[0].transform.gameObject, __instance.transform);
                newObject.name = $"Player{i}WinCounterText";
                newTexts.Add(newObject.GetComponent<TextMeshProUGUI>());
            }
            ___mPlayerWinTexts = [..___mPlayerWinTexts, ..newTexts];
        }
    }
    [HarmonyPatch(typeof(WinCounterUI), "RefreshWinTexts")]
    public class WinCounterUI_Patches_RefreshWinTexts
    {
        static void Prefix(WinCounterUI __instance, bool ___isEnabled, TextMeshProUGUI[] ___mPlayerWinTexts)
        {
            if (ControllerHandler.Instance == null || !___isEnabled)
            {
                return;
            }
            if (ControllerHandler.Instance.players.Count <= Plugin.NORMAL_PLAYERS)
            {
                return;
            }
            float fromX = ___mPlayerWinTexts[0].transform.localPosition.x;
            float toX = ___mPlayerWinTexts[1].transform.localPosition.x;
            float x = fromX;
            float change = (toX - fromX) / (ControllerHandler.Instance.players.Count - Plugin.NORMAL_PLAYERS + 1);
            int count = 0;
            foreach (var text in ___mPlayerWinTexts)
            {
                count++;
                if (count > Plugin.NORMAL_PLAYERS)
                {
                    x += change;
                    text.transform.localPosition = new Vector3(x, text.transform.localPosition.y, text.transform.localPosition.z);
                }
            }
        }
    }
}
