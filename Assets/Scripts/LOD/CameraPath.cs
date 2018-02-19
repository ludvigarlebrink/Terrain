using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Name.Lod
{

    public class CameraPath : MonoBehaviour
    {
        #region Public Variables
        public Color rayColor = Color.white;
        public Color sphereColor = Color.white;
        public Color labelColor = Color.green;
        public float sphereRadius = 5.0f;
        public List<Transform> pathObjs = new List<Transform>();
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
            pathObjs.Clear();

            foreach (Transform path_obj in transformArray)
            {
                if (path_obj != this.transform)
                {
                    pathObjs.Add(path_obj);
                }
            }

            Vector3 from;
            Vector3 to;
            for (int a = 0; a < pathObjs.Count; a++)
            {
                from = pathObjs[a].position;
                to = pathObjs[(a + 1) % pathObjs.Count].position;
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
