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
    [HarmonyPatch(typeof(ControllerHandler), "Awake")]
    public class ControllerHandler_Patches_Awake
    {
        static void Prefix(ControllerHandler __instance, List<Controller> ___mPlayers)
        {
            List<Material> materials = Plugin.NEW_COLOURS.Select(kvp =>
            {
                Material mat = new Material(__instance.colors[0]);
                mat.name = kvp.Key;
                mat.color = kvp.Value;
                return mat;
            }).ToList();
            __instance.colors = [..__instance.colors, ..materials];
            ___mPlayers.Capacity = Plugin.MAX_PLAYERS;
        }
    }
    [HarmonyPatch(typeof(ControllerHandler), "CreatePlayer")]
    public class ControllerHandler_Patches_CreatePlayer
    {
        public static FieldInfo f_MAX_PLAYERS = AccessTools.Field(typeof(Plugin), nameof(Plugin.MAX_PLAYERS));
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (var instruction in instructions)
            {
                if (instruction.opcode == OpCodes.Ldc_I4_4)
                {
                    yield return new CodeInstruction(OpCodes.Ldsfld, f_MAX_PLAYERS);
                } else
                {
                    yield return instruction;
                }
            }
        }
    }
}
