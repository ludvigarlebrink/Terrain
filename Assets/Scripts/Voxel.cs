using UnityEngine;

namespace Name.Terrain
{
    public class Voxel
    {
        #region Public Variables
        public bool state;
        public Vector3 position;
        public float value;
        #endregion

        #region Constructors
        public Voxel()
        {
        }

        public Voxel(float x, float y, float z, float value)
        {
            position = new Vector3(x, y, z);
            this.value = value;
        }
        #endregion
    }
}