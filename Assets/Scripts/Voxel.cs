﻿using System;
using UnityEngine;

[Serializable]
public class Voxel
{
    #region Public Variables
    public bool state;

    public Vector2 position;
    public Vector2 xEdgePosition;
    public Vector2 yEdgePosition;
    #endregion

    #region Constructors
    public Voxel(int x, int y, float size)
    {
        position.x = (x + 0.5f) * size;
        position.y = (y + 0.5f) * size;

        xEdgePosition = position;
        xEdgePosition.x += size * 0.5f;
        yEdgePosition = position;
        yEdgePosition.y += size * 0.5f;
    }
    #endregion


}
