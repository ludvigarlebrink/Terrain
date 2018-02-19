using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Name.Lod
{
    public class LODManager : MonoBehaviour
    {

        #region Public Variables
        public int xSize = 10;
        public int ySize = 10;
        public int zSize = 10;
        public float spacing = 1.0f;
        public float speed = 2.0f;
        public GameObject lodObject = null;
        public Transform cameraPathHolder = null;
        public Transform target = null;
        public Text buttonText = null;
        #endregion

        #region Private Variables
        private int index = -1;
        private int visited = -1;
        private Transform targetPoint;
        private static string[] text = { "Start Test", "Stop Test" };
        private bool isTestIsRunning = false;
        private Transform cam = null;
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
        private void Start()
        {
            cam = Camera.main.transform;

            ResetTest();


            for (int x = 0; x < xSize; ++x)
            {
                for (int y = 0; y < ySize; ++y)
                {
                    for (int z = 0; z < zSize; ++z)
                    {
                        GameObject obj = Instantiate(lodObject, new Vector3(x * spacing, y * spacing, z * spacing), Quaternion.identity) as GameObject;
                        obj.transform.parent = gameObject.transform;
                    }
                }
            }
        }

        private void Update()
        {
            if (visited == cameraPathHolder.childCount + 1)
            {
                isTestIsRunning = false;
                buttonText.text = text[isTestIsRunning ? 1 : 0];
                ResetTest();
            }

            if (isTestIsRunning)
            {

                cam.LookAt(target);
                cam.position = Vector3.MoveTowards(cam.position, targetPoint.position, speed * Time.deltaTime);
                if (Vector3.Distance(cam.position, targetPoint.position) < 0.1f)
                {

                    visited++;
                    index++;
                    index %= cameraPathHolder.childCount;
                    targetPoint = cameraPathHolder.GetChild(index);
                }
            }
        }

        private void ResetTest()
        {
            index = 0;
            visited = 0;
            targetPoint = cameraPathHolder.GetChild(0);
            cam.position = targetPoint.position;
            cam.LookAt(target);
        }

        #endregion
    }

}
