using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using UnityEngine;

namespace StickFightExtendedPlayers
{
    [HarmonyPatch(typeof(Controller), "SetCollision")]
    public class Controller_Patches_SetCollision
    {
        static readonly FieldInfo f_playerID = AccessTools.Field(typeof(Controller), "playerID");
        static readonly MethodInfo m_GetNewLayerID = AccessTools.Method(typeof(Controller_Patches_SetCollision), nameof(Controller_Patches_SetCollision.GetNewLayerID));
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var list = instructions.ToList();
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].opcode == OpCodes.Ldc_I4_8 && list[i + 2].LoadsField(f_playerID))
                {

                } else if (list[i].opcode == OpCodes.Add && list[i - 1].LoadsField(f_playerID))
                {
                    yield return new CodeInstruction(OpCodes.Call, m_GetNewLayerID);
                } else
                {
                    yield return list[i];
                }
            }
        }
        public static int GetNewLayerID(int playerID)
        {
            //return 8 + (playerID % Plugin.NORMAL_PLAYERS);
            if (playerID < Plugin.NORMAL_PLAYERS)
            {
                return 8 + playerID;
            } else
            {
                return Plugin.AVAILABLE_LAYERS[playerID - Plugin.NORMAL_PLAYERS];
            }
        }
    }
}
