using HarmonyLib;
using InControl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;

namespace StickFightExtendedPlayers
{
    public class DeviceManager_Patches
    {
        [HarmonyPatch(typeof(XInputDeviceManager), MethodType.Constructor)]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> XInput_Constructor_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (var ins in instructions)
            {
                if (ins.opcode == OpCodes.Ldc_I4_4)
                {
                    yield return new CodeInstruction(OpCodes.Ldsfld, ControllerHandler_Patches_CreatePlayer.f_MAX_PLAYERS);
                } else
                {
                    yield return ins;
                }
            }
        }
        [HarmonyPatch(typeof(XInputDeviceManager), "Worker")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> XInput_Worker_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (var ins in instructions)
            {
                if (ins.opcode == OpCodes.Ldc_I4_4)
                {
                    yield return new CodeInstruction(OpCodes.Ldsfld, ControllerHandler_Patches_CreatePlayer.f_MAX_PLAYERS);
                }
                else
                {
                    yield return ins;
                }
            }
        }
        [HarmonyPatch(typeof(XInputDeviceManager), "Update")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> XInput_Update_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (var ins in instructions)
            {
                if (ins.opcode == OpCodes.Ldc_I4_4)
                {
                    yield return new CodeInstruction(OpCodes.Ldsfld, ControllerHandler_Patches_CreatePlayer.f_MAX_PLAYERS);
                }
                else
                {
                    yield return ins;
                }
            }
        }
        [HarmonyPatch(typeof(OuyaEverywhereDeviceManager), MethodType.Constructor)]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> OuyaEverywhere_Constructor_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (var ins in instructions)
            {
                if (ins.opcode == OpCodes.Ldc_I4_4)
                {
                    yield return new CodeInstruction(OpCodes.Ldsfld, ControllerHandler_Patches_CreatePlayer.f_MAX_PLAYERS);
                }
                else
                {
                    yield return ins;
                }
            }
        }
        [HarmonyPatch(typeof(OuyaEverywhereDeviceManager), "Update")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> OuyaEverywhere_Update_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (var ins in instructions)
            {
                if (ins.opcode == OpCodes.Ldc_I4_4)
                {
                    yield return new CodeInstruction(OpCodes.Ldsfld, ControllerHandler_Patches_CreatePlayer.f_MAX_PLAYERS);
                }
                else
                {
                    yield return ins;
                }
            }
        }
        [HarmonyPatch(typeof(XboxOneInputDeviceManager), MethodType.Constructor)]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> XboxOne_Constructor_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (var ins in instructions)
            {
                if (ins.opcode == OpCodes.Ldc_I4_8)
                {
                    yield return new CodeInstruction(OpCodes.Ldsfld, ControllerHandler_Patches_CreatePlayer.f_MAX_PLAYERS);
                }
                else
                {
                    yield return ins;
                }
            }
        }
        [HarmonyPatch(typeof(XboxOneInputDeviceManager), "Update")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> XboxOne_Update_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (var ins in instructions)
            {
                if (ins.opcode == OpCodes.Ldc_I4_8)
                {
                    yield return new CodeInstruction(OpCodes.Ldsfld, ControllerHandler_Patches_CreatePlayer.f_MAX_PLAYERS);
                }
                else
                {
                    yield return ins;
                }
            }
        }
    }
}
