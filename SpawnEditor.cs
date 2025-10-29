using BepInEx;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace StickFightExtendedPlayers
{
    public class SpawnEditor : MonoBehaviour
    {
        public static SpawnEditor Instance { get; private set; }
        public bool Enabled = false;
        public bool NeedsToRefresh = false;
        public static FieldInfo f_LastPlayedMap = AccessTools.Field(typeof(MapSelectionHandler), "m_LastPlayedMap");
        public static MapWrapper currentMapIndex = null;
        void Awake()
        {
            Instance = this;
        }
        void Update()
        {
            if (UnityInput.Current.GetKeyDown(KeyCode.F4))
            {
                Enabled = !Enabled;
            }
            if (Enabled && GameManager.Instance?.currentMapInfo?.spawnPoints != null)
            {
                if (UnityInput.Current.GetMouseButtonDown(1))
                {
                    Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(UnityInput.Current.mousePosition);
                    mouseWorldPos.x = 0f;
                    string thisMapName = GetMapName(currentMapIndex);
                    if (UnityInput.Current.GetKey(KeyCode.LeftControl))
                    {
                        if (Plugin.SPAWN_POINTS.ContainsKey(thisMapName))
                        {
                            Plugin.SPAWN_POINTS[thisMapName].RemoveAt(Plugin.SPAWN_POINTS[thisMapName].FindIndex(spawnPoint => Vector3.Distance(new Vector3(0, spawnPoint.y, spawnPoint.z), mouseWorldPos) <= 0.25f));
                            if (Plugin.SPAWN_POINTS[thisMapName].Count == 0) Plugin.SPAWN_POINTS.Remove(thisMapName);
                        }
                    } else
                    {
                        if (!Plugin.SPAWN_POINTS.ContainsKey(thisMapName)) { Plugin.SPAWN_POINTS.Add(thisMapName, new List<Vector3>()); }
                        Plugin.SPAWN_POINTS[thisMapName].Add(mouseWorldPos);
                    }
                    Plugin.SaveSpawnPoints();
                    RefreshMap(currentMapIndex);
                }
                if (NeedsToRefresh)
                {
                    RefreshMap(currentMapIndex);
                }
            } else
            {
                for (int i = 0; i < transform.childCount; i++)
                {
                    GameObject.Destroy(transform.GetChild(i).gameObject);
                }
                NeedsToRefresh = true;
            }
        }
        public static string GetMapName(MapWrapper mapIndex)
        {
            if (mapIndex != null && mapIndex.MapType == 0 && BitConverter.ToInt32(mapIndex.MapData, 0) == 102)
            {
                return "Intermission";
            }
            return ((SingleMapUI)f_LastPlayedMap.GetValue(MapSelectionHandler.Instance)).MapName;
        }
        public void RefreshMap(MapWrapper mapIndex)
        {
            currentMapIndex = mapIndex;
            if (!Enabled) return;
            NeedsToRefresh = false;
            string thisMapName = GetMapName(mapIndex);
            for (int i = 0; i < transform.childCount; i++)
            {
                GameObject.Destroy(transform.GetChild(i).gameObject);
            }
            foreach (var spawnPoint in GameManager.Instance?.currentMapInfo?.spawnPoints)
            {
                GameObject circle = new GameObject("NormalSpawnPoint");
                circle.transform.parent = transform;
                circle.transform.position = spawnPoint.localPosition;
                CircleRenderer circleRenderer = circle.AddComponent<CircleRenderer>();
                circleRenderer.Color = Color.yellow;
            }
            if (Plugin.SPAWN_POINTS.ContainsKey(thisMapName))
            {
                foreach (var spawnPoint in Plugin.SPAWN_POINTS[thisMapName])
                {
                    GameObject circle = new GameObject("NormalSpawnPoint");
                    circle.transform.parent = transform;
                    circle.transform.position = spawnPoint;
                    CircleRenderer circleRenderer = circle.AddComponent<CircleRenderer>();
                    circleRenderer.Color = Color.magenta;
                }
            }
        }
        public static void RefreshMapStatic(MapWrapper mapIndex)
        {
            Instance.RefreshMap(mapIndex);
        }
    }
}
