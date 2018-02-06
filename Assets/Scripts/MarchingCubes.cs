using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace name.algorithm
{
    [RequireComponent(typeof(MeshRenderer), typeof(MeshFilter), typeof(MeshCollider))]
    public class MarchingCubes : MonoBehaviour
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
                            values[(hitX + size * hitY + size2 * hitZ) + size] -= strength * 0.1f;
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
                    values[(hitX + size * hitY + size2 * hitZ) + size] += addStrength * 0.1f;

                    CreateChunk();
                }

                Debug.Log("CLICKED!");
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

        private int CalculateCubeIndex(int x, int y, int z)
        {
            float point = x + size * y + size2 * z;
            float pointX = point + 1;
            float pointY = point + size;
            float pointXY = pointY + 1;
            float pointZ = point + size2;
            float pointXZ = pointX + size2;
            float pointYZ = pointY + size2;
            float pointXYZ = pointXY + size2;

            float value0 = values[(int)point];
            float value1 = values[(int)pointX];
            float value2 = values[(int)pointY];
            float value3 = values[(int)pointXY];
            float value4 = values[(int)pointZ];
            float value5 = values[(int)pointXZ];
            float value6 = values[(int)pointYZ];
            float value7 = values[(int)pointXYZ];

            int cubeIndex = 0;

            cubeIndex |= value0 < isolevel ? 1 : 0;
            cubeIndex |= value1 < isolevel ? 2 : 0;
            cubeIndex |= value2 < isolevel ? 8 : 0;
            cubeIndex |= value3 < isolevel ? 4 : 0;
            cubeIndex |= value4 < isolevel ? 16 : 0;
            cubeIndex |= value5 < isolevel ? 32 : 0;
            cubeIndex |= value6 < isolevel ? 128 : 0;
            cubeIndex |= value7 < isolevel ? 64 : 0;

            return cubeIndex;
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
                        float point = x + size * y + size2 * z;
                        float pointX = point + 1;
                        float pointY = point + size;
                        float pointXY = pointY + 1;
                        float pointZ = point + size2;
                        float pointXZ = pointX + size2;
                        float pointYZ = pointY + size2;
                        float pointXYZ = pointXY + size2;

                        float value0 = values[(int)point];
                        float value1 = values[(int)pointX];
                        float value2 = values[(int)pointY];
                        float value3 = values[(int)pointXY];
                        float value4 = values[(int)pointZ];
                        float value5 = values[(int)pointXZ];
                        float value6 = values[(int)pointYZ];
                        float value7 = values[(int)pointXYZ];

                        int cubeIndex = 0;

                        cubeIndex |= value0 < isolevel ? 1 : 0;
                        cubeIndex |= value1 < isolevel ? 2 : 0;
                        cubeIndex |= value2 < isolevel ? 8 : 0;
                        cubeIndex |= value3 < isolevel ? 4 : 0;
                        cubeIndex |= value4 < isolevel ? 16 : 0;
                        cubeIndex |= value5 < isolevel ? 32 : 0;
                        cubeIndex |= value6 < isolevel ? 128 : 0;
                        cubeIndex |= value7 < isolevel ? 64 : 0;

                        int bits = CubeTables.EdgeTable[cubeIndex];

                        if (bits == 0)
                        {
                            continue;
                        }

                        float mu = 0.5f;

                        if ((bits & 1) != 0)
                        {
                            mu = (isolevel - value0) / (value1 - value0);
                            vertexList[0] = LerpSelf(voxels[(int)point].position, voxels[(int)pointX].position, mu);
                        }

                        if ((bits & 2) != 0)
                        {
                            mu = (isolevel - value1) / (value3 - value1);
                            vertexList[1] = LerpSelf(voxels[(int)pointX].position, voxels[(int)pointXY].position, mu);
                        }

                        if ((bits & 4) != 0)
                        {
                            mu = (isolevel - value2) / (value3 - value2);
                            vertexList[2] = LerpSelf(voxels[(int)pointY].position, voxels[(int)pointXY].position, mu);
                        }

                        if ((bits & 8) != 0)
                        {
                            mu = (isolevel - value0) / (value2 - value0);
                            vertexList[3] = LerpSelf(voxels[(int)point].position, voxels[(int)pointY].position, mu);
                        }

                        if ((bits & 16) != 0)
                        {
                            mu = (isolevel - value4) / (value5 - value4);
                            vertexList[4] = LerpSelf(voxels[(int)pointZ].position, voxels[(int)pointXZ].position, mu);
                        }

                        if ((bits & 32) != 0)
                        {
                            mu = (isolevel - value5) / (value7 - value5);
                            vertexList[5] = LerpSelf(voxels[(int)pointXZ].position, voxels[(int)pointXYZ].position, mu);
                        }

                        if ((bits & 64) != 0)
                        {
                            mu = (isolevel - value6) / (value7 - value6);
                            vertexList[6] = LerpSelf(voxels[(int)pointYZ].position, voxels[(int)pointXYZ].position, mu);
                        }

                        if ((bits & 128) != 0)
                        {
                            mu = (isolevel - value4) / (value6 - value4);
                            vertexList[7] = LerpSelf(voxels[(int)pointZ].position, voxels[(int)pointYZ].position, mu);
                        }

                        if ((bits & 256) != 0)
                        {
                            mu = (isolevel - value0) / (value4 - value0);
                            vertexList[8] = LerpSelf(voxels[(int)point].position, voxels[(int)pointZ].position, mu);
                        }

                        if ((bits & 512) != 0)
                        {
                            mu = (isolevel - value1) / (value5 - value1);
                            vertexList[9] = LerpSelf(voxels[(int)pointX].position, voxels[(int)pointXZ].position, mu);
                        }

                        if ((bits & 1024) != 0)
                        {
                            mu = (isolevel - value3) / (value7 - value3);
                            vertexList[10] = LerpSelf(voxels[(int)pointXY].position, voxels[(int)pointXYZ].position, mu);
                        }

                        if ((bits & 2048) != 0)
                        {
                            mu = (isolevel - value2) / (value6 - value2);
                            vertexList[11] = LerpSelf(voxels[(int)pointY].position, voxels[(int)pointYZ].position, mu);
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

        private Vector3 LerpSelf(Vector3 o, Vector3 v, float alpha)
        {
            Vector3 value = o;
            value.x += (v.x - o.x) * alpha;
            value.y += (v.y - o.y) * alpha;
            value.z += (v.z - o.z) * alpha;
            return value;
        }
        #endregion
    }

}
