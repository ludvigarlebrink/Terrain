using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Voxel
{
    #region Public Variables
    public bool state;
    public Vector3 position;
    #endregion

    #region Constructors
    public Voxel()
    {
    }

    public Voxel(float x, float y, float z)
    {
        position = new Vector3(x, y, z);
    }
    #endregion
}
