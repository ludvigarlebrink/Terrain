using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Name.Terrain;

namespace NameEditor.Terrain
{
    [CustomEditor(typeof(Terrain3DObject))]
    public class Terrain3DObjectEditor : Editor
    {
        public override void OnInspectorGUI()
        {

        }

        [MenuItem("Assets/Create/Terrain3D")]
        private static void Create()
        {
            Terrain3DObject terrain3DObject = CreateInstance<Terrain3DObject>();
            AssetDatabase.CreateAsset(terrain3DObject, "Assets/Terrain3D.asset");
            AssetDatabase.SaveAssets();
        }
    }
}