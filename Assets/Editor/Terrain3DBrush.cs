using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace NameEditor.Terrain
{
    public class Terrain3DBrush
    {
        #region Public Variables
        public float strength = 100.0f;
        public float size = 10.0f;
        public float falloff = 10.0f;
        #endregion

        #region Public Methods
        public void DrawInspector()
        {
            EditorGUILayout.LabelField("Brush", EditorStyles.boldLabel);
            strength = EditorGUILayout.Slider("Strength", strength, 0.0f, 100.0f);
            size = EditorGUILayout.Slider("Size", size, 0.0f, 200.0f);
            size = EditorGUILayout.Slider("Size", size, 0.0f, 200.0f);
        }
        #endregion
    }
}
