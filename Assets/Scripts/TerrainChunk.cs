using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Name.Terrain
{
    struct Vert
    {
        public Vector4 position;
        public Vector2 uv;
    }
}

namespace Name.Terrain
{
    [RequireComponent(typeof(MeshRenderer), typeof(MeshFilter), typeof(MeshCollider))]
    public class TerrainChunk : MonoBehaviour
    {
        #region Public Variables
        public float strength = 1.0f;
        public float addStrength = 0.5f;
        public float zoomSpeed = 15.0f;

        public List<Voxel> voxels = new List<Voxel>();
        public List<Vector3> voxelPositions = new List<Vector3>();
        public List<float> voxelValues = new List<float>();

        // Compute shader for GPU solution.
        public ComputeShader marchingCubeCS;

        public int size = 16;
        public int multiplier;
        public int size2;
        #endregion

        #region Private Variables
        private List<Vector3> vertices = new List<Vector3>();
        private List<Vector2> uvs = new List<Vector2>();
        private List<int> triangles = new List<int>();
        private Vector3[] vertexList = new Vector3[12];

        private int[] resolutions = { 1, 2, 8, 4, 16, 32, 128, 64 };
        
        // X = non-shared scalar value | Y = shared scalar value | Z = first point  | W = second point
        private static Vector4[] vertPoint = new[]
        {
            new Vector4(1, 0, 0, 1),
            new Vector4(3, 1, 1, 3),
            new Vector4(3, 2, 2, 3),
            new Vector4(2, 0, 0, 2),
            new Vector4(5, 4, 4, 5),
            new Vector4(7, 5, 5, 7),
            new Vector4(7, 6, 6, 7),
            new Vector4(6, 4, 4, 6),
            new Vector4(4, 0, 0, 4),
            new Vector4(5, 1, 1, 5),
            new Vector4(7, 3, 3, 7),
            new Vector4(6, 2, 2, 6) 
        };
        
        private int isolevel = 0;
        private Mesh mesh;

        private float axisMin = 0.0f;
        private float axisMax = 120.0f;
        private float axisRange;

        private int vertexBufferSize;

        // GPU data buffers.
        private ComputeBuffer cubeEdgeFlags;
        private ComputeBuffer triangleConnectionTable;
        private ComputeBuffer vertexBuffer;
        private ComputeBuffer triangleBuffer;
        private ComputeBuffer voxelPositionsBuffer;
        private ComputeBuffer voxelValuesBuffer;

        private Profiler profiler = new Profiler();
        #endregion

        #region Public Methods
        public void CreateChunkCPU()
        {
            profiler.Start();

            // Direction swapper used for correctiong UV-coordinates
            int directionSwapper = 0;
            int vertexIndex = 0;

            int count = size - 1;
            for (int z = 0; z < count; ++z)
            {
                for (int y = 0; y < count; ++y)
                {
                    for (int x = 0; x < count; ++x)
                    {
                        // Index of base points, and also adjacent points on cube.
                        float[] basePoints = GetPoints(x, y, z);

                        // Store scalars corresponding to vertices.
                        float[] storedScalars = new float[basePoints.Length];

                        for (int j = 0; j < basePoints.Length; ++j)
                        {
                            storedScalars[j] = voxels[(int)basePoints[j]].Value;
                        }

                        // Initialize cubeindex
                        int cubeIndex = 0;

                        // First part of the algorithm uses a table which maps the vertices under the isosurface to the
                        // intersecting edges. An 8 bit index is formed where each bit corresponds to a vertex.
                        for (int j = 0; j < storedScalars.Length; ++j)
                        {
                            cubeIndex |= storedScalars[j] < isolevel ? resolutions[j] : 0;
                        }

                        int bits = Terrain3DTables.EdgeTable[cubeIndex];

                        // If no edges are crossed, continue to the next iteration.
                        if (bits == 0)
                        {
                            continue;
                        }

                        float alpha = 0.5f;
                        int resValue = 1;

                        // Check which edges are crossed and estimate the point location with a weighted average of scalar values at edge endpoints. 
                        // Cases 1 - 8          Horizontal edges at bottom of the cube
                        // Cases 16 - 128       Horizontal edges at top of the cube
                        // Cases 256 - 2048     Vertical edges of the cubes
                        for (int index = 0; index < 12; ++index)
                        {
                            if ((bits & resValue) != 0)
                            {
                                alpha = (isolevel - storedScalars[(int)vertPoint[index].y]) / (storedScalars[(int)vertPoint[index].x] - storedScalars[(int)vertPoint[index].y]);
                                vertexList[index] = Vector3.Lerp(voxels[(int)basePoints[(int)vertPoint[index].z]].position, voxels[(int)basePoints[(int)vertPoint[index].w]].position, alpha);
                            }

                            resValue = resValue * 2;
                        }

                        cubeIndex <<= 4;

                        int i = 0;
                        while (Terrain3DTables.TriTable[cubeIndex + i] != -1)
                        {
                            int index1 = Terrain3DTables.TriTable[cubeIndex + i];
                            int index2 = Terrain3DTables.TriTable[cubeIndex + i + 1];
                            int index3 = Terrain3DTables.TriTable[cubeIndex + i + 2];

                            vertices.Add(vertexList[index1]);
                            vertices.Add(vertexList[index2]);
                            vertices.Add(vertexList[index3]);

                            triangles.Add(vertexIndex);
                            triangles.Add(vertexIndex + 1);
                            triangles.Add(vertexIndex + 2);

                            directionSwapper = 1 - directionSwapper;

                            if (directionSwapper == 0)
                            {
                                uvs.Add(new Vector2(0, 0));
                                uvs.Add(new Vector2(0, 1));
                                uvs.Add(new Vector2(1, 1));
                            }
                            else
                            {
                                uvs.Add(new Vector2(1, 0));
                                uvs.Add(new Vector2(0, 0));
                                uvs.Add(new Vector2(1, 1));
                            }

                            vertexIndex += 3;
                            i += 3;
                        }
                    }
                }
            }

            // Build the Mesh.
            BuildMesh(vertices.ToArray(), triangles.ToArray(), uvs.ToArray());

            // Clear lists
            ClearLists();

            profiler.Stop();
        }

        public void CreateChunkGPU()
        {
            Dispatch();

            ReadBackMesh(vertexBuffer);
        }
        #endregion

        #region Private Methods
        public void Initialize()
        {
            multiplier = (int)(axisMax / size);
            axisRange = axisMax - axisMin;
            size2 = size * size;

            mesh = new Mesh();
            GetComponent<MeshFilter>().mesh = mesh;

            for (int z = 0; z < size; ++z)
            {
                for (int y = 0; y < size; ++y)
                {
                    for (int x = 0; x < size; ++x)
                    {
                        float coordX = axisMin + axisRange * x / (size - 1);
                        float coordY = axisMin + axisRange * y / (size - 1);
                        float coordZ = axisMin + axisRange * z / (size - 1);

                        float value = -1.0f;
                        float wall = 1.0f;
                        if (y < size / 2)
                        {
                            value = (int)wall;
                        }

                        voxelPositions.Add(new Vector3(coordX, coordY, coordZ));
                        voxelValues.Add(value);

                        voxels.Add(new Voxel(coordX, coordY, coordZ, value));
                    }
                }
            }

            for (int i = 0, z = 0; z < size; ++z)
            {
                for (int y = 0; y < size; ++y)
                {
                    for (int x = 0; x < size; ++x, ++i)
                    {

                        // voxels.Add(new Voxel(coordX, coordY, coordZ, value));
                    }
                }
            }

            InitializeComputeResources();
        }

        public void Deallocate()
        {
            cubeEdgeFlags.Release();
            triangleConnectionTable.Release();
            vertexBuffer.Release();
            triangleBuffer.Release();
            voxelPositionsBuffer.Release();
            voxelValuesBuffer.Release();
        }

        private void InitializeComputeResources()
        {
            // There are 8 threads that run per each group so voxel array size must be divisible by 8
            if (size % 8 != 0)
            {
                throw new System.ArgumentException("Voxel array size must be divisible by 8");
            }

            // Hold the generated voxel positions
            voxelPositionsBuffer = new ComputeBuffer(voxelPositions.Count, sizeof(float) * 3);
            voxelPositionsBuffer.SetData(voxelPositions.ToArray());

            // Hold the generated voxel values
            voxelValuesBuffer = new ComputeBuffer(voxelValues.Count, sizeof(float) * 1);
            voxelValuesBuffer.SetData(voxelValues.ToArray());

            // Buffer to hold the vertices generated by marching cubes
            vertexBufferSize = size * size * size * 3 * 5;
            vertexBuffer = new ComputeBuffer(vertexBufferSize, sizeof(float) * 6);

            // Clear all the mesh vertices to -1
            // Vertices that are generated will have a value of 1
            float[] val = new float[vertexBufferSize * 6];
            for (int i = 0; i < vertexBufferSize * 6; ++i)
            {
                val[i] = -1.0f;
            }
            vertexBuffer.SetData(val);

            // Buffer to hold the triangles generated by marching cube.
            triangleBuffer = new ComputeBuffer(vertexBufferSize, sizeof(int));
            triangleBuffer.SetData(val);

            // Upload cube edges lookup table.
            cubeEdgeFlags = new ComputeBuffer(256, sizeof(int));
            cubeEdgeFlags.SetData(Terrain3DTables.EdgeTable);

            // Upload triangle connection lookup table.
            triangleConnectionTable = new ComputeBuffer(256 * 16, sizeof(int));
            triangleConnectionTable.SetData(Terrain3DTables.TriTable);

            // Set variables
            marchingCubeCS.SetInt("_Size", size);
            marchingCubeCS.SetInt("_Size2", size2);
            marchingCubeCS.SetFloat("_Isolevel", isolevel);
            
            // Set buffers
            marchingCubeCS.SetBuffer(0, "_VoxelsPos", voxelPositionsBuffer);
            marchingCubeCS.SetBuffer(0, "_VoxelsVal", voxelValuesBuffer);
            marchingCubeCS.SetBuffer(0, "_Vertices", vertexBuffer);
            marchingCubeCS.SetBuffer(0, "_Triangles", triangleBuffer);
            marchingCubeCS.SetBuffer(0, "_CubeEdgeFlags", cubeEdgeFlags);
            marchingCubeCS.SetBuffer(0, "_TriangleConnectionTable", triangleConnectionTable);
        }

        private void Dispatch()
        {
            marchingCubeCS.Dispatch(0, vertexBufferSize / 8, vertexBufferSize / 8, vertexBufferSize / 8);
        }

        private void OnDrawGizmos()
        {
            //foreach (Voxel voxel in voxels)
            //{
            //    float color = Mathf.Clamp(Mathf.Abs(voxel.value), 0.0f, 1.0f);
            //    if (voxel.value < 0.0f)
            //    {
            //        float secondaryColor = 1.0f - color;
            //        Gizmos.color = new Color(color, 0.5f, 0.5f);
            //    }
            //    else
            //    {
            //        float secondaryColor = 1.0f - color;
            //        Gizmos.color = new Color(0.5f, color, 0.5f);
            //    }
            //    Gizmos.DrawSphere(voxel.position, 0.5f);
            //}
        }

        private void OnDestroy()
        {
            cubeEdgeFlags.Release();
            triangleConnectionTable.Release();
            vertexBuffer.Release();
            triangleBuffer.Release();
            voxelPositionsBuffer.Release();
            voxelValuesBuffer.Release();
        }

        private float[] GetPoints(int x, int y, int z)
        {
            float[] points = new float[8];

            // Point.
            points[0] = x + size * y + size2 * z;

            // PointX.
            points[1] = points[0] + 1;

            // PointY.
            points[2] = points[0] + size;

            // PointXY.
            points[3] = points[2] + 1;

            // PointZ.
            points[4] = points[0] + size2;

            // PointXZ.
            points[5] = points[1] + size2;

            // PointYZ.
            points[6] = points[2] + size2;

            // PointXYZ.
            points[7] = points[3] + size2;

            return points;
        }

        private void BuildMesh(Vector3[] vertices, int[] triangles, Vector2[] uvs)
        {
            // Build the Mesh:
            mesh.Clear();
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.uv = uvs;

            mesh.RecalculateNormals();
            GetComponent<MeshCollider>().sharedMesh = mesh;
        }

        private void ClearLists()
        {
            vertices.Clear();
            uvs.Clear();
            triangles.Clear();
        }

        void ReadBackMesh(ComputeBuffer meshBuffer)
        {
            // Get the data out of the vertex buffer
            Name.Terrain.Vert[] verts = new Name.Terrain.Vert[vertexBufferSize];
            vertexBuffer.GetData(verts);

            int idx = 0;

            for (int i = 0; i < vertexBufferSize; ++i)
            {
                if (verts[i].position.w != -1)
                {
                    vertices.Add(verts[i].position);
                    uvs.Add(verts[i].uv);
                    triangles.Add(idx++);
                }

            }

            // Build the Mesh.
            BuildMesh(vertices.ToArray(), triangles.ToArray(), uvs.ToArray());

            // Clear lists
            ClearLists();
        }

        #endregion
    }
}
