using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Name.Terrain;

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

        bool isMouseIsDown = false;
        bool isButtonIsDown = false;
        Vector3 currentMousePosition = Vector3.zero;
        Vector3 oldMousePosition = Vector2.zero;

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
                    EditorGUILayout.HelpBox(
                        "Flatten Terrain\n" +
                        "- Hold left mouse button to flatten\n" +
                        "- Hold ctrl + left mouse button to sample height",
                        MessageType.None, true);

                    EditorGUILayout.Toggle("Inverse", false);

                    break;

                default:
                    break;
            }
        }

        public void Update(Terrain3D terrain3D)
        {
            Event e = Event.current;

            if ((e.type == EventType.KeyDown) && e.keyCode == (KeyCode.B))
            {
                isButtonIsDown = true;
                GUIUtility.hotControl = 0;
            }

            if ((e.type == EventType.KeyUp) && e.keyCode == (KeyCode.B))
            {
                isButtonIsDown = false;
            }

            if ((e.type == EventType.MouseDown) && e.button == 0)
            {
                isMouseIsDown = true;
                GUIUtility.hotControl = 0;
            }

            if (((e.type == EventType.MouseUp) && e.button == 0) || e.alt)
            {
                isMouseIsDown = false;
            }

            if (isButtonIsDown)
            {
                Vector3 currentMousePosition = Event.current.mousePosition;
                float xMovement = oldMousePosition.x - currentMousePosition.x;
                oldMousePosition = currentMousePosition;

                if (xMovement != 0.0f)
                {
                    brush.Size += xMovement / 3;
                }


            }

            if (isMouseIsDown)
            {
                BrushHit[] hits = brush.Paint(terrain3D);

                for (int i = 0; i < hits.Length; ++i)
                {
                    hits[i].voxel.Value += hits[i].influence;
                }

                terrain3D.Refresh();
            }
            else
            {
                if (!isButtonIsDown)
                {
                    brush.DrawBrush();
                }
                else
                {
                    brush.DrawPrevious();
                }

            }
        }
    }
}
