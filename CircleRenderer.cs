using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace StickFightExtendedPlayers
{
    public class CircleRenderer : MonoBehaviour
    {
        private bool initialized = false;
        private int segments = 64;
        private float radius = 0.25f;
        private Color color = Color.red;
        public int Segments { get { return segments; } set { if (initialized) { Recreate(); } segments = value; } }
        public float Radius { get { return radius; } set { if (initialized) { Recreate(); } radius = value; } }
        public Color Color { get { return color; } set { if (initialized) { line.startColor = value; line.endColor = value; } color = value; } }

        private LineRenderer line;

        void Start()
        {
            line = gameObject.AddComponent<LineRenderer>();
            line.useWorldSpace = false;
            line.loop = true;
            line.widthMultiplier = 0.05f;
            line.material = new Material(Shader.Find("Sprites/Default"));
            line.startColor = color;
            line.endColor = color;
            Recreate();
            initialized = true;
        }
        void Recreate()
        {
            Vector3[] points = new Vector3[segments];
            for (int i = 0; i < segments; i++)
            {
                float angle = 2 * Mathf.PI * i / segments;
                points[i] = new Vector3(0, Mathf.Sin(angle) * radius, Mathf.Cos(angle) * radius);
            }

            line.positionCount = segments;
            line.SetPositions(points);
        }
    }
}
