using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Name.Terrain { 

    public class MarchingCubesGPU : MonoBehaviour {

        // Size of the voxel array for each dimension.
        const int voxelArrSize = 64;

        // Size of the buffer to hold the vertices.
        // This is the maximum number of vertices marching cubes can produce, 5 triangles for each voxel.
        const int vertBuffSize = voxelArrSize * voxelArrSize * voxelArrSize * 3 * 5;

        // Compute shader.
        public ComputeShader marchingCS;

        // GPU data buffers.
        ComputeBuffer cubeEdgeFlags;
        ComputeBuffer triangleConnectionTable;
        ComputeBuffer meshBuffer;

    	void Start () {

            // There are 8 threads that run per each group so voxel array size must be divisible by 8
            if(voxelArrSize % 8 != 0)
            {

            }

            // Upload cube edges lookup table.
            cubeEdgeFlags = new ComputeBuffer(256, sizeof(int));
            cubeEdgeFlags.SetData(Terrain3DTables.EdgeTable);

            // Upload triangle connection lookup table.
            triangleConnectionTable = new ComputeBuffer(256 * 16, sizeof(int));
            triangleConnectionTable.SetData(Terrain3DTables.TriTable);
    	}
    	
    	// Update is called once per frame
    	void Update () {
    		
    	}
    }

}
