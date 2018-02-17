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
        #endregion

        #region Private Functions
        private void Start()
        {

            for (int x = 0; x < width; ++x)
            {
                for (int z = 0; z < depth; ++z)
                {
                    Instantiate(LODObject, new Vector3(x * spacing, 0, z * spacing), Quaternion.identity);
                }
            }
        }
        #endregion
    }

}
