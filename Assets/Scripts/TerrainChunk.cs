using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Name.Terrain
{
    public struct VertPoint
    {
        public int[] value;

        public VertPoint(int size)
        {
            value = new int[size];

            for (int i = 0; i < size; ++i)
            {
                value[i] = 0;
            }
        }
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

        public Vector3 offset = new Vector3(0, 0, 0);

        public List<Voxel> voxels = new List<Voxel>();

        public int size = 15;
        public int multiplier;
        public int size2;
        #endregion

        #region Private Variables
        private List<Vector3> vertices = new List<Vector3>();
        private List<Vector2> uvs = new List<Vector2>();
        private List<int> triangles = new List<int>();
        private Vector3[] vertexList = new Vector3[12];

        private int[] resolutions = { 1, 2, 8, 4, 16, 32, 128, 64 };
        private VertPoint[] vertPoint = new VertPoint[12];
        
        private int isolevel = 0;
        private Mesh mesh;

        private float axisMin = 0.0f;
        private float axisMax = 120.0f;
        private float axisRange;

        private Camera cam;
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

            // Initialize VertPoint. Used to store scalar values and index of points for each case.
            vertPoint[0].value = new int[] { 0, 1, 0, 0, 1 };
            vertPoint[1].value = new int[] { 1, 3, 1, 1, 3 };
            vertPoint[2].value = new int[] { 2, 3, 2, 2, 3 };
            vertPoint[3].value = new int[] { 0, 2, 0, 0, 2 };
            vertPoint[4].value = new int[] { 4, 5, 4, 4, 5 };
            vertPoint[5].value = new int[] { 5, 7, 5, 5, 7 };
            vertPoint[6].value = new int[] { 6, 7, 6, 6, 7 };
            vertPoint[7].value = new int[] { 4, 6, 4, 4, 6 };
            vertPoint[8].value = new int[] { 0, 4, 0, 0, 4 };
            vertPoint[9].value = new int[] { 1, 5, 1, 1, 5 };
            vertPoint[10].value = new int[] { 3, 7, 3, 3, 7 };
            vertPoint[11].value = new int[] { 2, 6, 2, 2, 6 };
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

        public void CreateChunk()
        {
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
                            storedScalars[j] = voxels[(int)basePoints[j]].value;
                        }

                        // Initialize cubeindex
                        int cubeIndex = 0;

                        // First part of the algorithm uses a table which maps the vertices under the isosurface to the
                        // intersecting edges. An 8 bit index is formed where each bit corresponds to a vertex.
                        for(int j = 0; j < storedScalars.Length; ++j)
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
                            if((bits & resValue) != 0)
                            {
                                alpha = (isolevel - storedScalars[vertPoint[index].value[0]]) / (storedScalars[vertPoint[index].value[1]] - storedScalars[vertPoint[index].value[2]]);
                                vertexList[index] = Vector3.Lerp(voxels[(int)basePoints[vertPoint[index].value[3]]].position, voxels[(int)basePoints[vertPoint[index].value[4]]].position, alpha);
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

            // Build the Mesh:
            mesh.Clear();

            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.uv = uvs.ToArray();

            mesh.RecalculateNormals(60);
            GetComponent<MeshCollider>().sharedMesh = mesh;

            vertices.Clear();
            uvs.Clear();
            triangles.Clear();
        }
        #endregion
    }
}
