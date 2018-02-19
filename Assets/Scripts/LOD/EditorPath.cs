using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Name.Lod
{

    public class EditorPath : MonoBehaviour
    {
        #region Public Variables
        public Color rayColor = Color.white;
        public Color sphereColor = Color.white;
        public Color labelColor = Color.green;
        public float sphereRadius = 5.0f;
        public List<Transform> path_objs = new List<Transform>();
        #endregion

        #region Private Variables
        private Transform[] transformArray;
        private GUIStyle style = new GUIStyle();
        #endregion

        #region Private Functions
        private void OnDrawGizmos()
        {
            style.normal.textColor = labelColor;
            style.fontSize = 20;
            transformArray = GetComponentsInChildren<Transform>();
            path_objs.Clear();

            foreach (Transform path_obj in transformArray)
            {
                if (path_obj != this.transform)
                {
                    path_objs.Add(path_obj);
                }
            }

            Vector3 from;
            Vector3 to;
            for (int a = 0; a < path_objs.Count; a++)
            {
                from = path_objs[a].position;
                to = path_objs[(a + 1) % path_objs.Count].position;
                Gizmos.color = sphereColor;
                Gizmos.DrawWireSphere(from, sphereRadius);
                Gizmos.color = rayColor;
                Gizmos.DrawLine(from, to);
                from.y += 20.0f;
                Handles.Label(from, "" + a, style);

            }
        }
        #endregion
    }

}
