using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Name.Terrain
{
    [SelectionBase]
    public class Terrain3D : MonoBehaviour
    {
        public int chunks = 2;
        public int chunkSize = 4;
        public int resolution = 8;

        public TerrainChunk terrainChunk;

        public void Initialize()
        {
            terrainChunk = gameObject.GetComponent<TerrainChunk>();
            if (!terrainChunk)
            {
                terrainChunk = gameObject.AddComponent<TerrainChunk>();
            }

            terrainChunk.Initialize();
        }

        public void Shutdown()
        {
            terrainChunk.Deallocate();
        }

        public void Refresh()
        {
            if (!terrainChunk)
            {
                terrainChunk = gameObject.GetComponent<TerrainChunk>();
            }

            terrainChunk.CreateChunkGPU();
        }

        private void Awake()
        {

        }
    }
}