using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Name.Lod
{
    public class LODManager : MonoBehaviour
    {

        #region Public Variables
        public int width = 5;
        public int depth = 5;
        public float spacing = 2.0f;
        public float speed = 2.0f;
        public GameObject LODObject;
        public Transform Camera;
        public Transform CameraPathHolder;
        public Transform Target;
        public Text buttonText;
        #endregion

        #region Private Variables
        private int index;
        private int visited;
        private Transform targetPoint;
        private static string[] text = { "Start Test", "Stop Test" };
        private bool isTestIsRunning = false;
        #endregion

        #region Public Functions
        public void RunTest()
        {
            isTestIsRunning = !isTestIsRunning;
            buttonText.text = text[isTestIsRunning ? 1 : 0];

            if (isTestIsRunning == false)
            {
                ResetTest();
            }
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
        private void ResetTest()
        {
            index = 0;
            visited = 0;
            targetPoint = CameraPathHolder.GetChild(0);
            Camera.position = targetPoint.position;
            Camera.LookAt(Target);
        }

        private void Start()
        {
            ResetTest();

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
                buttonText.text = text[isTestIsRunning ? 1 : 0];
                ResetTest();
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
