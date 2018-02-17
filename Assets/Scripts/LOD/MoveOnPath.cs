using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Name.Lod
{

    public class MoveOnPath : MonoBehaviour
    {

        #region Public Variables
        public float speed = 2.0f;
        public Transform CameraPathHolder;
        public Transform Target;
        Transform targetPoint;
        int index;
        #endregion

        #region Private Functions
        private void Start()
        {
            index = 0;
            targetPoint = CameraPathHolder.GetChild(index);
            transform.position = targetPoint.position;
        }

        private void Update()
        {
            transform.LookAt(Target);
            transform.position = Vector3.MoveTowards(transform.position, targetPoint.position, speed * Time.deltaTime);
            if (Vector3.Distance(transform.position, targetPoint.position) < 0.1f)
            {
                index++;
                index %= CameraPathHolder.childCount;
                targetPoint = CameraPathHolder.GetChild(index);
            }
        }
        #endregion
    }

}
