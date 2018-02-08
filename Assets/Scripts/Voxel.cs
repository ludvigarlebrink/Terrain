using UnityEngine;

namespace Name.Terrain
{
    public class Voxel
    {
        #region Public Variables
        public Vector3 position;
        private float value;
        public float Value
        {
            get
            {
                return value;
            }

            set
            {
                if (value > 1.0f)
                {
                    this.value = 1.0f;
                }
                else if (value < -1.0f)
                {
                    this.value = -1.0f;
                }

                this.value = value;
            }
        }
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