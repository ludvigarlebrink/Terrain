using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace NameEditor.Terrain
{
    public class Terrain3DElevation
    {
        #region Private Variables
        private string[] elevationTools =
        {
            "Raise",
            "Smoothen",
            "Flatten"
        };

        private int selectedElevationTool = 0;

        private Terrain3DBrush brush = new Terrain3DBrush();
        #endregion

        public void DrawInspector()
        {
            brush.DrawInspector();

            EditorGUILayout.LabelField("Tools", EditorStyles.boldLabel);
            selectedElevationTool = GUILayout.Toolbar(selectedElevationTool, elevationTools);

            switch (selectedElevationTool)
            {
                case 0:
                    EditorGUILayout.HelpBox(
                        "Raise or Lower Terrain\n" +
                        "- Hold left mouse button to raise\n" +
                        "- Hold ctrl + left mouse button to lower",
                        MessageType.None, true);

                    EditorGUILayout.Toggle("Inverse", false);

                    break;

                case 1:
                    EditorGUILayout.HelpBox(
                        "Smoothen or Roughen Terrain\n" +
                        "- Hold left mouse button to smoothen\n" +
                        "- Hold ctrl + left mouse button to roughen",
                        MessageType.None, true);

                    EditorGUILayout.Toggle("Inverse", false);

                    break;

                case 2:

                    break;

                default:
                    break;
            }
        }
    }
}
