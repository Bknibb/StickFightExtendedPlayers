using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using HarmonyLib.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace StickFightExtendedPlayers
{
    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public const string PLUGIN_GUID = "StickFightExtendedPlayers";
        public const string PLUGIN_NAME = "StickFightExtendedPlayers";
        public const string PLUGIN_VERSION = "1.0.0";
        public static Plugin Instance { get; private set; }
        public static int MAX_PLAYERS = 12;
        public static Dictionary<string, Color> NEW_COLOURS = new Dictionary<string, Color>() {
            { "purple", new Color(0.6f, 0.2f, 1f) },
            { "mint", new Color(0f, 0.7f, 0.4f) },
            { "actual orange", new Color(1f, 0.4f, 0.25f) },
            { "dark blue", new Color(0.2f, 0.3f, 0.5f) },
            { "hot pink", new Color(1f, 0.2f, 0.4f) },
            { "dark green", new Color(0.2f, 0.4f, 0.2f) },
            { "pink", new Color(0.8f, 0.4f, 0.6f) },
            { "aqua", new Color(0f, 0.7f, 0.7f) }
        };
        public static int NORMAL_PLAYERS = 4;
        public static float PLAYER_SPACING = 2f;
        public static Dictionary<string, List<Vector3>> SPAWN_POINTS;
        public static GameObject SPAWN_POINT_HOST;
        public static PropertyInfo p_OrphanedEntries = AccessTools.Property(typeof(ConfigFile), "OrphanedEntries");
        private void Awake()
        {
            Instance = this;
            SPAWN_POINT_HOST = new GameObject("SpawnPointHost");
            Harmony harmony = new Harmony(PLUGIN_GUID);
            harmony.PatchAll();
            harmony.PatchAll(typeof(TomlTypeConverter_Patches));
            LoadSpawnPoints();
            GameObject spawnEditor = new GameObject("SpawnEditor");
            spawnEditor.AddComponent<SpawnEditor>();
        }
        public static void LoadSpawnPoints()
        {
            SPAWN_POINTS = new Dictionary<string, List<Vector3>>(DefaultSpawnPoints.DEFAULT_SPAWN_POINTS);
            //SPAWN_POINTS.Add(Path.GetFileNameWithoutExtension(file), JsonConvert.DeserializeObject<List<Vector3>>(File.ReadAllText(file), new UnityVector3Converter()));
            foreach (var conf in (Dictionary<ConfigDefinition, string>)p_OrphanedEntries.GetValue(Instance.Config, null))
            {
                if (conf.Key.Section == "Spawn Points")
                {
                    try
                    {
                        SPAWN_POINTS[conf.Key.Key] = TomlTypeConverter.ConvertToValue<List<Vector3>>(conf.Value);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning($"Failed to load \"{conf.Key.Key}\"");
                        Debug.LogException(ex);
                    }
                }
            }
            //foreach (var kvp in SPAWN_POINTS)
            //{
            //    bool first = true;
            //    StringBuilder sb = new StringBuilder();
            //    foreach (var vec in kvp.Value)
            //    {
            //        if (first)
            //        {
            //            first = false;
            //        }
            //        else
            //        {
            //            sb.Append(", ");
            //        }
            //        sb.Append("new Vector3(");
            //        sb.Append(Math.Round(vec.x, 3).ToString() + "f, ");
            //        sb.Append(Math.Round(vec.y, 3).ToString() + "f, ");
            //        sb.Append(Math.Round(vec.z, 3).ToString() + "f)");
            //    }
            //    Debug.Log("{ \"" + kvp.Key + "\", new List<Vector3>() { " + sb.ToString() + " } },");
            //}
        }
        public static void SaveSpawnPoints()
        {
            List<ConfigDefinition> toRemove = new List<ConfigDefinition>();
            foreach (var conf in Instance.Config)
            {
                if (conf.Key.Section == "Spawn Points")
                {
                    toRemove.Add(conf.Key);
                }
            }
            foreach (var conf in toRemove)
            {
                Instance.Config.Remove(conf);
            }
            foreach (var kvp in SPAWN_POINTS)
            {
                //File.WriteAllText(Utility.CombinePaths(spawnPointsPath, $"{kvp.Key}.json"), JsonConvert.SerializeObject(kvp.Value, Formatting.Indented, new UnityVector3Converter()));
                try
                {
                    var newValue = kvp.Value.Select(vec => new Vector3((float)Math.Round(vec.x, 3), (float)Math.Round(vec.y, 3), (float)Math.Round(vec.z, 3))).ToList();
                    if (DefaultSpawnPoints.DEFAULT_SPAWN_POINTS.ContainsKey(kvp.Key) && DefaultSpawnPoints.DEFAULT_SPAWN_POINTS[kvp.Key].SequenceEqual(newValue))
                    {
                        continue;
                    }
                    Instance.Config.Bind("Spawn Points", kvp.Key, DefaultSpawnPoints.DEFAULT_SPAWN_POINTS.ContainsKey(kvp.Key) ? DefaultSpawnPoints.DEFAULT_SPAWN_POINTS[kvp.Key] : []).Value = newValue;
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"Failed to save \"{kvp.Key}\"");
                    Debug.LogException(ex);
                }
            }
        }
    }
}
