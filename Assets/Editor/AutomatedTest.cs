using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEditor;
using Name.Terrain;

namespace NameEditor.Terrain
{
    public class AutomatedTest
    {
        private System.Random random;

        private Voxel currentVoxel = null;

        private int indexX = -1;
        private int indexY = -1;
        private int indexZ = -1;

        private int numColumns = 0;

        public void Start()
        {
            random = new System.Random(34);
        }

        public void Update(Terrain3D terrain3D)
        {
            if (currentVoxel == null || currentVoxel.Value >= Mathf.Abs(0.95f))
            {
                NextVoxel(terrain3D);
            }

            currentVoxel.Value += 0.1f;
            terrain3D.Refresh();
        }

        private  void NextVoxel(Terrain3D terrain3D)
        {
            if (numColumns < 10)
            {
                if (indexY == -1)
                {
                    indexY = terrain3D.terrainChunk.size / 2;
                    indexX = random.Next() % terrain3D.terrainChunk.size;
                    indexZ = random.Next() % terrain3D.terrainChunk.size;
                }
                else if (indexY < terrain3D.terrainChunk.size)
                {
                    ++indexY;
                }
                else
                {
                    indexY = terrain3D.terrainChunk.size / 2;
                    indexX = random.Next() % terrain3D.terrainChunk.size;
                    indexZ = random.Next() % terrain3D.terrainChunk.size;
                    ++numColumns;

                    if(numColumns >= 10)
                    {
                        NextVoxel(terrain3D);
                        return;
                    }
                }
            }
            else
            {
                indexY = terrain3D.terrainChunk.size - 1;
                indexX = random.Next() % terrain3D.terrainChunk.size;
                indexZ = random.Next() % terrain3D.terrainChunk.size;
            }


            int index = indexX + indexY * terrain3D.terrainChunk.size + indexZ * terrain3D.terrainChunk.size2;

            currentVoxel = terrain3D.terrainChunk.voxels[index];
        }
    }
}
