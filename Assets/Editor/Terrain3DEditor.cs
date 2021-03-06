﻿using System.Collections;
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

        private bool isEditing = false;

        private Terrain3DElevation elevation = new Terrain3DElevation();
        private Terrain3DPaint paint = new Terrain3DPaint();

        private AutomatedTest automatedTest = new AutomatedTest();

        private bool isRunningTest = false;
        private bool testIsDone = false;

        public override void OnInspectorGUI()
        {
            Terrain3D terrain3D = (Terrain3D)target;

            selectedTab = GUILayout.Toolbar(selectedTab, tabs);

            switch (selectedTab)
            {
                case 0:
                    elevation.DrawInspector();
                    break;

                case 1:
                    paint.DrawInspector();
                    break;

                case 2:
                    DrawProperties();
                    break;

                default:
                    break;
            }

            GUILayout.BeginHorizontal();

            if (isEditing)
            {
                if (GUILayout.Button("Disable Editing"))
                {
                    isEditing = false;
                }
            }
            else
            {
                if (GUILayout.Button("Enable Editing"))
                {
                    isEditing = true;
                }
            }

            if (GUILayout.Button("Force Refresh"))
            {

            }

            GUILayout.EndHorizontal();

            EditorGUILayout.LabelField("Profiling", EditorStyles.boldLabel);

            if (isRunningTest)
            {
                if (GUILayout.Button("Stop Test"))
                {
                    EditorApplication.update -= Update;
                    isRunningTest = false;
                    terrain3D.Initialize();
                    terrain3D.Refresh();

                }
            }
            else
            {
                if (GUILayout.Button("Run Test"))
                {
                    EditorApplication.update += Update;
                    isRunningTest = true;
                    terrain3D.Initialize();
                    terrain3D.Refresh();
                    automatedTest.Start();
                }
            }
        }

        private void OnEnable()
        {
            Terrain3D terrain3D = (Terrain3D)target;
            terrain3D.Initialize();
            terrain3D.Refresh();
            Tools.hidden = true;

            EditorUtility.SetSelectedRenderState(terrain3D.gameObject.GetComponent<Renderer>(), EditorSelectedRenderState.Hidden);
        }

        private void OnDisable()
        {
            Terrain3D terrain3D = (Terrain3D)target;
            terrain3D.Refresh();
            Tools.hidden = false;
        }

        private void Update()
        {
            Terrain3D terrain3D = (Terrain3D)target;
            automatedTest.Update(terrain3D);
        }

        private void OnSceneGUI()
        {
            Terrain3D terrain3D = (Terrain3D)target;

            Event e = Event.current;

            switch (selectedTab)
            {
                case 0:
                    elevation.Update(terrain3D);
                    break;

                default:
                    break;
            }

            SceneView.RepaintAll();
        }

        [MenuItem("GameObject/Create Other/Terrain3D")]
        private static void Create()
        {
            GameObject terrain3DGameObject = new GameObject("Terrain3D");
            Terrain3D terrain3D = (Terrain3D)terrain3DGameObject.AddComponent(typeof(Terrain3D));
            terrain3D.Initialize();
            terrain3D.Refresh();
            Terrain3DObject terrain3DObject = CreateInstance<Terrain3DObject>();
            AssetDatabase.CreateAsset(terrain3DObject, "Assets/Terrain3D.asset");
            AssetDatabase.SaveAssets();
        }

        private void DrawProperties()
        {
            Terrain3D terrain3D = (Terrain3D)target;

            terrain3D.chunks = EditorGUILayout.IntField("Chunks", terrain3D.chunks);
            terrain3D.chunkSize = EditorGUILayout.IntField("Chunk Size", terrain3D.chunkSize);
            terrain3D.resolution = EditorGUILayout.IntField("Resolution", terrain3D.resolution);
        }

        private void Initialize()
        {
            Terrain3D terrain3D = (Terrain3D)target;
            terrain3D.Initialize();
        }

        private void Refresh()
        {
            Terrain3D terrain3D = (Terrain3D)target;
            terrain3D.Refresh();
        }
    }
}
