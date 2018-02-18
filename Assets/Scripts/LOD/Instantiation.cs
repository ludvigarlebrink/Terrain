using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Name.Lod
{

    public class Instantiation : MonoBehaviour
    {

        #region Public Variables
        public int width = 5;
        public int depth = 5;
        public float spacing = 2.0f;
        public GameObject LODObject;
        public Transform Camera;
        public float speed = 2.0f;
        public Transform CameraPathHolder;
        public Transform Target;
        #endregion

        #region Private Variables
        private int index;
        private int visited;
        private Transform targetPoint;
        private bool isTestIsRunning;
        #endregion

        #region Public Functions
        public void RunTest(bool status)
        {
            isTestIsRunning = status;
        }

        public void ToggleLOD()
        {
            LODGroup[] children;
            children = GetComponentsInChildren<LODGroup>();

            foreach (LODGroup group in children)
            {
                group.enabled = !group.enabled;
            }
        }

        #endregion

        #region Private Functions
        private void Start()
        {

            index = 0;
            visited = 0;
            targetPoint = CameraPathHolder.GetChild(index);
            Camera.position = targetPoint.position;
            Camera.LookAt(Target);

            for (int x = 0; x < width; ++x)
            {
                for (int z = 0; z < depth; ++z)
                {
                    GameObject obj = Instantiate(LODObject, new Vector3(x * spacing, 0, z * spacing), Quaternion.identity) as GameObject;
                    obj.transform.parent = gameObject.transform;

                }
            }
        }

        private void Update()
        {
            if (visited == CameraPathHolder.childCount + 1)
            {
                isTestIsRunning = false;
                visited = 0;
                index = 0;
            }

            if (isTestIsRunning)
            {

                Camera.LookAt(Target);
                Camera.position = Vector3.MoveTowards(Camera.position, targetPoint.position, speed * Time.deltaTime);
                if (Vector3.Distance(Camera.position, targetPoint.position) < 0.1f)
                {

                    visited++;
                    index++;
                    index %= CameraPathHolder.childCount;
                    targetPoint = CameraPathHolder.GetChild(index);
                }
            }
        }

        #endregion
    }

}
