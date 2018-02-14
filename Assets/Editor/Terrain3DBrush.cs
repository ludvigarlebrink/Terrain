using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Name.Terrain;

namespace NameEditor.Terrain
{
    public struct BrushHit
    {
        public Voxel voxel;
        public float influence;

        public BrushHit(Voxel voxel, float influence)
        {
            this.voxel = voxel;
            this.influence = influence;
        }
    }
}

namespace NameEditor.Terrain
{
    public class Terrain3DBrush
    {
        #region Public Variables
        public float strength = 10.0f;
        public float size = 50.0f;
        public float falloff = 10.0f;
        #endregion

        #region Private Variables

        RaycastHit hitPoint;
        #endregion

        #region Public Methods
        public float Size
        {
            get
            {
                return size;
            }

            set
            {
                this.size = value;
            }
        }

        public void DrawInspector()
        {
            EditorGUILayout.LabelField("Brush", EditorStyles.boldLabel);
            strength = EditorGUILayout.Slider("Strength", strength, 0.0f, 100.0f);
            size = EditorGUILayout.Slider("Size", size, 0.0f, 200.0f);
            falloff = EditorGUILayout.Slider("Falloff", falloff, 0.0f, 200.0f);
        }

        public void DrawBrush()
        {
            // Cast a ray through a screen point and return the hit point.
            Camera cam = Camera.current;
            if (!cam)
            {
                return;
            }

            SceneView sceneView = SceneView.currentDrawingSceneView;
            if (!sceneView)
            {
                return;
            }

            Event e = Event.current;

            Vector3 mousePosition = e.mousePosition;
            mousePosition.y = sceneView.camera.pixelHeight - e.mousePosition.y;

            Ray ray = cam.ScreenPointToRay(mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
                float length = (cam.transform.position - hit.point).magnitude / 30.0f;
                Handles.color = Color.blue;
                Handles.DrawLine(hit.point, hit.point + hit.normal * length);
                Handles.CircleHandleCap(0, hit.point, Quaternion.LookRotation(hit.normal), size + 5.0f, EventType.Repaint);
                Handles.CircleHandleCap(0, hit.point, Quaternion.LookRotation(hit.normal), size + falloff, EventType.Repaint);
                hitPoint = hit;
            }
        }

        public void DrawPrevious()
        {
            Camera cam = Camera.current;
            if (!cam)
            {
                return;
            }

            float length = (cam.transform.position - hitPoint.point).magnitude / 30.0f;
            Handles.color = Color.blue;
            Handles.DrawLine(hitPoint.point, hitPoint.point + hitPoint.normal * length);
            Handles.CircleHandleCap(0, hitPoint.point, Quaternion.LookRotation(hitPoint.normal), size + 5.0f, EventType.Repaint);
            Handles.CircleHandleCap(0, hitPoint.point, Quaternion.LookRotation(hitPoint.normal), size + falloff, EventType.Repaint);


        }

        public BrushHit[] Paint(Terrain3D terrain3D)
        {
            List<BrushHit> brushHits = new List<BrushHit>();

            // Cast a ray through a screen point and return the hit point.
            Camera cam = Camera.current;
            if (!cam)
            {
                return new BrushHit[0];
            }

            SceneView sceneView = SceneView.currentDrawingSceneView;
            if (!sceneView)
            {
                return new BrushHit[0];
            }

            Event e = Event.current;

            Vector3 mousePosition = e.mousePosition;
            mousePosition.y = sceneView.camera.pixelHeight - e.mousePosition.y;

            Ray ray = cam.ScreenPointToRay(mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
                float length = (cam.transform.position - hit.point).magnitude / 30.0f;
                Handles.color = Color.blue;
                Handles.DrawLine(hit.point, hit.point + hit.normal * length);
                Handles.CircleHandleCap(0, hit.point, Quaternion.LookRotation(hit.normal), size + 5.0f, EventType.Repaint);
                Handles.CircleHandleCap(0, hit.point, Quaternion.LookRotation(hit.normal), size + falloff, EventType.Repaint);

                // Transform the hit point from world space to local space
                Vector3 localHit = terrain3D.transform.InverseTransformPoint(hit.point);
                TerrainChunk chunk = terrain3D.terrainChunk;

                int hitX = (int)(localHit.x / chunk.multiplier);
                int hitY = (int)(localHit.y / chunk.multiplier);
                int hitZ = (int)(localHit.z / chunk.multiplier);

                int hitIndex = hitX + chunk.size * hitY + chunk.size2 * hitZ;

                brushHits.Add(new BrushHit(chunk.voxels[hitIndex], strength * 0.1f * Time.deltaTime));
            }

            return brushHits.ToArray();
        }
        #endregion
    }
}
