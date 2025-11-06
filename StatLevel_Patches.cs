using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StickFightExtendedPlayers
{
    [HarmonyPatch(typeof(StatLevel), "PlayerIDToColor")]
    public class StatLevel_Patches_PlayerIDToColor
    {
        static bool Prefix(int id, ref string __result)
        {
            if (id >= Plugin.NORMAL_PLAYERS)
            {
                __result = Plugin.NEW_COLOURS.Keys.ToList()[id - Plugin.NORMAL_PLAYERS].ToUpper();
                return false;
            }
            return true;
        }
    }
}
