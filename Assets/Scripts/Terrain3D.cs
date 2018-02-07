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

        private TerrainChunk terrainChunk;

        public void Initialize()
        {
            terrainChunk = gameObject.AddComponent<TerrainChunk>();
            terrainChunk.Initialize();
        }

        public void Refresh()
        {
            terrainChunk.CreateChunk();
        }

        private void Awake()
        {
        }
    }
}