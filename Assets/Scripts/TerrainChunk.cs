using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        #endregion

        #region Private Variables
        private List<Voxel> voxels = new List<Voxel>();
        private List<float> values = new List<float>();

        private List<Vector3> vertices = new List<Vector3>();
        private List<Vector2> uvs = new List<Vector2>();
        private List<int> triangles = new List<int>();
        private Vector3[] vertexList = new Vector3[12];

        private int[] resolutions = { 1, 2, 8, 4, 16, 32, 128, 64 };
        private int size = 15;
        private int multiplier;
        private int size2;
        private int isolevel = 0;
        private Mesh mesh;

        private float axisMin = 0.0f;
        private float axisMax = 120.0f;
        private float axisRange;

        private Camera cam;
        #endregion

        #region Private Methods
        private void Awake()
        {
            cam = Camera.main;

            multiplier = (int)(axisMax / size);
            axisRange = axisMax - axisMin;
            size2 = size * size;

            mesh = new Mesh();
            GetComponent<MeshFilter>().mesh = mesh;

            Initialize();
            CreateChunk();
        }

        private void Update()
        {
            // Reduce block.
            if (Input.GetMouseButton(1))
            {
                Ray ray = cam.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, 999f))
                {
                    Vector3 localHit = transform.InverseTransformPoint(hit.point);

                    int hitX = (int)(localHit.x / multiplier);
                    int hitY = (int)(localHit.y / multiplier);
                    int hitZ = (int)(localHit.z / multiplier);

                    if (hitX >= 0 && hitY > 0 && hitZ >= 0)
                    {
                        if (hitX < size - 1 && hitY < size - 1 && hitZ < size - 1)
                        {
                            values[hitX + size * hitY + size2 * hitZ] -= strength;
                            values[(hitX + size * hitY + size2 * hitZ) + size] -= strength * Time.deltaTime;
                            CreateChunk();
                        }
                    }
                }
            }
            else if (Input.GetMouseButton(0))
            {
                Ray ray = cam.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, 999f))
                {
                    Vector3 localHit = transform.InverseTransformPoint(hit.point);

                    int hitX = (int)(localHit.x / multiplier);
                    int hitY = (int)(localHit.y / multiplier);
                    int hitZ = (int)(localHit.z / multiplier);

                    values[hitX + size * hitY + size2 * hitZ] += addStrength;
                    values[(hitX + size * hitY + size2 * hitZ) + size] += addStrength * Time.deltaTime;

                    CreateChunk();
                }
            }
        }

        private void OnDrawGizmos()
        {
            foreach (Voxel voxel in voxels)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(voxel.position, 0.5f);
            }
        }

        private void Initialize()
        {
            for (int z = 0; z < size; ++z)
            {
                for (int y = 0; y < size; ++y)
                {
                    for (int x = 0; x < size; ++x)
                    {
                        float coordX = axisMin + axisRange * x / (size - 1);
                        float coordY = axisMin + axisRange * y / (size - 1);
                        float coordZ = axisMin + axisRange * z / (size - 1);

                        voxels.Add(new Voxel(coordX, coordY, coordZ));

                        int value = -1;
                        float wall = 0;
                        if (y == 0)
                        {
                            value = (int)wall;
                        }

                        values.Add(value);
                    }
                }
            }
        }

        private float[] GetPoints(int x, int y, int z)
        {
            float[] points = new float[8];

            points[0] = x + size * y + size2 * z;   // Point
            points[1] = points[0] + 1;              // PointX
            points[2] = points[0] + size;           // PointY
            points[3] = points[2] + 1;              // PointXY
            points[4] = points[0] + size2;          // PointZ
            points[5] = points[1] + size2;          // PointXZ
            points[6] = points[2] + size2;          // PointYZ
            points[7] = points[3] + size2;          // PointXYZ

            return points;
        }

        private void CreateChunk()
        {
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
                        float[] p = GetPoints(x, y, z);

                        // Scalars corresponding to vertices.
                        float[] v = new float[p.Length];

                        for (int index = 0; index < p.Length; ++index)
                        {
                            v[index] = values[(int)p[index]];
                        }

                        int cubeIndex = 0;

                        for(int index = 0; index < v.Length; ++index)
                        {
                            cubeIndex |= v[index] < isolevel ? resolutions[index] : 0;
                        }

                        int bits = CubeTables.EdgeTable[cubeIndex];

                        if (bits == 0)
                        {
                            continue;
                        }

                        float alpha = 0.5f;

                        if ((bits & 1) != 0)
                        {
                            alpha = (isolevel - v[0]) / (v[1] - v[0]);
                            vertexList[0] = Vector3.Lerp(voxels[(int)p[0]].position, voxels[(int)p[1]].position, alpha);
                        }

                        if ((bits & 2) != 0)
                        {
                            alpha = (isolevel - v[1]) / (v[3] - v[1]);
                            vertexList[1] = Vector3.Lerp(voxels[(int)p[1]].position, voxels[(int)p[3]].position, alpha);
                        }

                        if ((bits & 4) != 0)
                        {
                            alpha = (isolevel - v[2]) / (v[3] - v[2]);
                            vertexList[2] = Vector3.Lerp(voxels[(int)p[2]].position, voxels[(int)p[3]].position, alpha);
                        }

                        if ((bits & 8) != 0)
                        {
                            alpha = (isolevel - v[0]) / (v[2] - v[0]);
                            vertexList[3] = Vector3.Lerp(voxels[(int)p[0]].position, voxels[(int)p[2]].position, alpha);
                        }

                        if ((bits & 16) != 0)
                        {
                            alpha = (isolevel - v[4]) / (v[5] - v[4]);
                            vertexList[4] = Vector3.Lerp(voxels[(int)p[4]].position, voxels[(int)p[5]].position, alpha);
                        }

                        if ((bits & 32) != 0)
                        {
                            alpha = (isolevel - v[5]) / (v[7] - v[5]);
                            vertexList[5] = Vector3.Lerp(voxels[(int)p[5]].position, voxels[(int)p[7]].position, alpha);
                        }

                        if ((bits & 64) != 0)
                        {
                            alpha = (isolevel - v[6]) / (v[7] - v[6]);
                            vertexList[6] = Vector3.Lerp(voxels[(int)p[6]].position, voxels[(int)p[7]].position, alpha);
                        }

                        if ((bits & 128) != 0)
                        {
                            alpha = (isolevel - v[4]) / (v[6] - v[4]);
                            vertexList[7] = Vector3.Lerp(voxels[(int)p[4]].position, voxels[(int)p[6]].position, alpha);
                        }

                        if ((bits & 256) != 0)
                        {
                            alpha = (isolevel - v[0]) / (v[4] - v[0]);
                            vertexList[8] = Vector3.Lerp(voxels[(int)p[0]].position, voxels[(int)p[4]].position, alpha);
                        }

                        if ((bits & 512) != 0)
                        {
                            alpha = (isolevel - v[1]) / (v[5] - v[1]);
                            vertexList[9] = Vector3.Lerp(voxels[(int)p[1]].position, voxels[(int)p[5]].position, alpha);
                        }

                        if ((bits & 1024) != 0)
                        {
                            alpha = (isolevel - v[3]) / (v[7] - v[3]);
                            vertexList[10] = Vector3.Lerp(voxels[(int)p[3]].position, voxels[(int)p[7]].position, alpha);
                        }

                        if ((bits & 2048) != 0)
                        {
                            alpha = (isolevel - v[2]) / (v[6] - v[2]);
                            vertexList[11] = Vector3.Lerp(voxels[(int)p[2]].position, voxels[(int)p[6]].position, alpha);
                        }

                        cubeIndex <<= 4;

                        int i = 0;
                        while (CubeTables.TriTable[cubeIndex + i] != -1)
                        {
                            int index1 = CubeTables.TriTable[cubeIndex + i];
                            int index2 = CubeTables.TriTable[cubeIndex + i + 1];
                            int index3 = CubeTables.TriTable[cubeIndex + i + 2];

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

                        // Release allocated objects...?
                        p = null;
                        v = null;
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
