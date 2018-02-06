using UnityEngine;

namespace Name.Terrain
{
    enum Neighbours
    {
        LowerNorth = 0,
        LowerNorthEast,
        LowerEast,
        LowerSouthEast,
        LowerSouth,
        LowerSouthWest,
        LowerWest,
        LowerNorthWest,
        MiddleNorth,
        MiddleNorthEast,
        MiddleEast,
        MiddleSouthEast,
        MiddleSouth,
        MiddleSouthWest,
        MiddleWest,
        MiddleNorthWest,
        UpperNorth,
        UpperNorthEast,
        UpperEast,
        UpperSouthEast,
        UpperSouth,
        UpperSouthWest,
        UpperWest,
        UpperNorthWest
    }
}

namespace Name.Terrain
{
    public class Voxel
    {
        #region Public Variables
        public bool state;
        public Vector3 position;
        public Voxel[] neighbours;
        #endregion

        #region Constructors
        public Voxel()
        {
            neighbours = new Voxel[24];
            for (int i = 0; i < neighbours.Length; ++i)
            {
                neighbours[i] = null;
            }
        }

        public Voxel(float x, float y, float z)
        {
            position = new Vector3(x, y, z);
            neighbours = new Voxel[24];
            for (int i = 0; i < neighbours.Length; ++i)
            {
                neighbours[i] = null;
            }
        }
        #endregion
    }
}