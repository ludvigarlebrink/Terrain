using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Name.Terrain;

namespace NameEditor.Terrain
{
    [CustomEditor(typeof(Terrain3D))]
    public class Terrain3DEditor : Editor
    {
        private string[] tabs =
        {
            "Elevation",
            "Paint",
            "Properties"
        };

        private int selectedTab = 0;

        private float brushStrength = 100.0f;
        private float brushSize = 10.0f;

        public override void OnInspectorGUI()
        {
            Terrain3D terrain3D = target as Terrain3D;

            selectedTab = GUILayout.Toolbar(selectedTab, tabs);

            switch (selectedTab)
            {
                case 0:
                    DrawElevation();
                    break;

                case 1:
                    DrawPaint();
                    break;

                case 2:
                    DrawProperties();
                    break;

                default:
                    break;
            }
        }

        [MenuItem("GameObject/Create Other/Terrain3D")]
        private static void Create()
        {
            GameObject terrain3D = new GameObject("Terrain3D");
            terrain3D.AddComponent(typeof(Terrain3D));
            Terrain3DObject terrain3DObject = CreateInstance<Terrain3DObject>();
            AssetDatabase.CreateAsset(terrain3DObject, "Assets/Terrain3D.asset");
            AssetDatabase.SaveAssets();
        }

        private void DrawElevation()
        {
            Terrain3D terrain3D = target as Terrain3D;

            brushStrength = EditorGUILayout.Slider(brushStrength, 0.0f, 100.0f);
            brushSize = EditorGUILayout.Slider(brushSize, 0.0f, 200.0f);
        }

        private void DrawPaint()
        {
            Terrain3D terrain3D = target as Terrain3D;

            brushStrength = EditorGUILayout.Slider(brushStrength, 0.0f, 100.0f);
            brushSize = EditorGUILayout.Slider(brushSize, 0.0f, 200.0f);
        }

        private void DrawProperties()
        {
            Terrain3D terrain3D = target as Terrain3D;

            terrain3D.chunks = EditorGUILayout.IntField("Chunks", terrain3D.chunks);
            terrain3D.chunkSize = EditorGUILayout.IntField("Chunk Size", terrain3D.chunkSize);
            terrain3D.resolution = EditorGUILayout.IntField("Resolution", terrain3D.resolution);
        }
    }
}