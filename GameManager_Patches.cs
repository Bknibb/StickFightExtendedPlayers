using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Xml;
using UnityEngine;

namespace StickFightExtendedPlayers
{
    [HarmonyPatch]
    public class GameManager_Patches_StartMapSequence
    {
        static Type GetNestedMoveType()
        {
            var nestedTypes = typeof(GameManager).GetNestedTypes(BindingFlags.Instance | BindingFlags.NonPublic);
            Type nestedType = null;

            foreach (var type in nestedTypes)
            {
                if (type.Name.Contains("StartMapSequence"))
                {
                    nestedType = type;
                    break;
                }
            }

            return nestedType;
        }
        static MethodBase TargetMethod()
        {
            return AccessTools.Method(GetNestedMoveType(), "MoveNext");
        }
        static readonly FieldInfo f_this = AccessTools.Field(GetNestedMoveType(), "$this");
        static readonly FieldInfo f_currentMapInfo = AccessTools.Field(typeof(GameManager), nameof(GameManager.currentMapInfo));
        static readonly FieldInfo f_spawnPoints = AccessTools.Field(typeof(MapInfo), nameof(MapInfo.spawnPoints));
        static readonly FieldInfo f_x = AccessTools.Field(typeof(Vector3), nameof(Vector3.x));
        static readonly FieldInfo f_y = AccessTools.Field(typeof(Vector3), nameof(Vector3.y));
        static readonly FieldInfo f_z = AccessTools.Field(typeof(Vector3), nameof(Vector3.z));
        static readonly ConstructorInfo c_Vector3 = AccessTools.Constructor(typeof(Vector3), [typeof(float), typeof(float), typeof(float)]);
        static readonly MethodInfo m_Transform = AccessTools.Method(typeof(GameManager_Patches_StartMapSequence), nameof(GameManager_Patches_StartMapSequence.Transform));
        static readonly MethodInfo m_LogWarning = AccessTools.Method(typeof(Debug), "LogWarning", [typeof(object)]);
        static readonly MethodInfo m_RefreshMapStatic = AccessTools.Method(typeof(SpawnEditor), nameof(SpawnEditor.RefreshMapStatic));
        static readonly MethodInfo m_GetWithExtraSpawnPoints = AccessTools.Method(typeof(GameManager_Patches_StartMapSequence), nameof(GameManager_Patches_StartMapSequence.GetWithExtraSpawnPoints));
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var ifEnd = generator.DefineLabel();
            Label? invalidLabel = null;
            var list = instructions.ToList();
            object bLocal = null;
            int? numI = null;
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].opcode == OpCodes.Ldfld && list[i].LoadsField(f_spawnPoints))
                {
                    yield return list[i];
                    yield return new CodeInstruction(OpCodes.Call, m_GetWithExtraSpawnPoints);
                }
                else if (list[i].opcode == OpCodes.Call && list[i - 3].opcode == OpCodes.Ldstr && list[i - 3].operand is string text1 && text1 == "StartMapSequence load failed")
                {
                    yield return new CodeInstruction(OpCodes.Call, m_RefreshMapStatic).WithLabels(list[i].ExtractLabels());
                    yield return list[i];
                }
                else if (list[i].opcode == OpCodes.Ldstr && list[i].operand is string text2 && text2 == "Trying to use invalid spawnpoint")
                {
                    invalidLabel = list[i].labels[0];
                    i++;
                }
                else if (list[i].opcode == OpCodes.Ldc_I4_0 && list[i - 2].opcode == OpCodes.Ldstr && list[i - 2].operand is string text3 && text3 == "Trying to use invalid spawnpoint")
                {
                    numI = i;
                    bLocal = list[i - 10].operand;
                    yield return new CodeInstruction(OpCodes.Ldloc_S, list[i + 1].operand).WithLabels(invalidLabel.Value);
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, f_this);
                    yield return new CodeInstruction(OpCodes.Ldfld, f_currentMapInfo);
                    yield return new CodeInstruction(OpCodes.Ldfld, f_spawnPoints);
                    yield return new CodeInstruction(OpCodes.Ldlen);
                    yield return new CodeInstruction(OpCodes.Conv_I4);
                    yield return new CodeInstruction(OpCodes.Rem);
                } else if (list[i].opcode == OpCodes.Stloc_S && numI.HasValue && i - 9 == numI.Value)
                {
                    yield return list[i];
                    list[i + 1].labels.Add(ifEnd);

                    // if ((int)b >= this.currentMapInfo.spawnPoints.Length)
                    yield return new CodeInstruction(OpCodes.Ldloc_S, bLocal);
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, f_this);
                    yield return new CodeInstruction(OpCodes.Ldfld, f_currentMapInfo);
                    yield return new CodeInstruction(OpCodes.Ldfld, f_spawnPoints);
                    yield return new CodeInstruction(OpCodes.Ldlen);
                    yield return new CodeInstruction(OpCodes.Conv_I4);
                    yield return new CodeInstruction(OpCodes.Blt_S, ifEnd);

                    // vector = new Vector3(vector.x, vector.y, vector.z + GameManager_Patches_StartMapSequence.Transform((int)b / this.currentMapInfo.spawnPoints.Length));
                    yield return new CodeInstruction(OpCodes.Ldloca_S, list[i].operand);
                    yield return new CodeInstruction(OpCodes.Ldloc_S, list[i].operand);
                    yield return new CodeInstruction(OpCodes.Ldfld, f_x);
                    yield return new CodeInstruction(OpCodes.Ldloc_S, list[i].operand);
                    yield return new CodeInstruction(OpCodes.Ldfld, f_y);
                    yield return new CodeInstruction(OpCodes.Ldloc_S, list[i].operand);
                    yield return new CodeInstruction(OpCodes.Ldfld, f_z);
                    yield return new CodeInstruction(OpCodes.Ldloc_S, bLocal);
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, f_this);
                    yield return new CodeInstruction(OpCodes.Ldfld, f_currentMapInfo);
                    yield return new CodeInstruction(OpCodes.Ldfld, f_spawnPoints);
                    yield return new CodeInstruction(OpCodes.Ldlen);
                    yield return new CodeInstruction(OpCodes.Conv_I4);
                    yield return new CodeInstruction(OpCodes.Div);
                    yield return new CodeInstruction(OpCodes.Call, m_Transform);
                    yield return new CodeInstruction(OpCodes.Add);
                    yield return new CodeInstruction(OpCodes.Call, c_Vector3);

                    // Debug.LogWarning(vector.z);
                    //yield return new CodeInstruction(OpCodes.Ldloc_S, list[i].operand);
                    //yield return new CodeInstruction(OpCodes.Ldfld, f_z);
                    //yield return new CodeInstruction(OpCodes.Box, typeof(float));
                    //yield return new CodeInstruction(OpCodes.Call, m_LogWarning);
                }
                else
                {
                    yield return list[i];
                }
            }
        }
        public static float Transform(int n)
        {
            float magnitude = (float)Math.Ceiling(n / 2.0) * Plugin.PLAYER_SPACING;
            return (n % 2 == 1 ? -magnitude : magnitude);
        }
        public static Transform[] GetWithExtraSpawnPoints(Transform[] normalTransforms)
        {
            string thisMapName = ((SingleMapUI)SpawnEditor.f_LastPlayedMap.GetValue(MapSelectionHandler.Instance)).MapName;
            for (int i = 0; i < Plugin.SPAWN_POINT_HOST.transform.childCount; i++)
            {
                GameObject.Destroy(Plugin.SPAWN_POINT_HOST.transform.GetChild(i).gameObject);
            }
            if (!Plugin.SPAWN_POINTS.ContainsKey(thisMapName)) return normalTransforms;
            return normalTransforms.Concat(Plugin.SPAWN_POINTS[thisMapName].Select(vec =>
            {
                var spawnPoint = new GameObject("SpawnPoint");
                spawnPoint.transform.parent = Plugin.SPAWN_POINT_HOST.transform;
                spawnPoint.transform.localPosition = vec;
                return spawnPoint.transform;
            })).ToArray();
        }
    }
}
