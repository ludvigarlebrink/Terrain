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
        #endregion

        #region Private Variables
        private List<Voxel> voxels = new List<Voxel>();

        private List<Vector3> vertices = new List<Vector3>();
        private List<Vector2> uvs = new List<Vector2>();
        private List<int> triangles = new List<int>();
        private Vector3[] vertexList = new Vector3[12];

        private int[] resolutions = { 1, 2, 8, 4, 16, 32, 128, 64 };
        private VertPoint[] vp = new VertPoint[12];
        
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

        #region Public Methods
        public void Initialize()
        {
            cam = Camera.main;

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

            // Initialize VertPoint. Used to store scalar values and index of points for each case.
            vp[0].value = new int[] { 0, 1, 0, 0, 1 };
            vp[1].value = new int[] { 1, 3, 1, 1, 3 };
            vp[2].value = new int[] { 2, 3, 2, 2, 3 };
            vp[3].value = new int[] { 0, 2, 0, 0, 2 };
            vp[4].value = new int[] { 4, 5, 4, 4, 5 };
            vp[5].value = new int[] { 5, 7, 5, 5, 7 };
            vp[6].value = new int[] { 6, 7, 6, 6, 7 };
            vp[7].value = new int[] { 4, 6, 4, 4, 6 };
            vp[8].value = new int[] { 0, 4, 0, 0, 4 };
            vp[9].value = new int[] { 1, 5, 1, 1, 5 };
            vp[10].value = new int[] { 3, 7, 3, 3, 7 };
            vp[11].value = new int[] { 2, 6, 2, 2, 6 };

        }
        #endregion

        #region Private Methods
        private void Update()
        {
            // Reduce block when right mouse button is pressed.
            if (Input.GetMouseButton(1))
            {
                // Cast a ray through a screen point and return the hit point
                Ray ray = cam.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, 999.0f))
                {
                    // Transform the hit point from world space to local space
                    Vector3 localHit = transform.InverseTransformPoint(hit.point);

                    int hitX = (int)(localHit.x / multiplier);
                    int hitY = (int)(localHit.y / multiplier);
                    int hitZ = (int)(localHit.z / multiplier);

                    if (hitX >= 0 && hitY > 0 && hitZ >= 0)
                    {
                        if (hitX < size - 1 && hitY < size - 1 && hitZ < size - 1)
                        {
                            voxels[hitX + size * hitY + size2 * hitZ].value -= strength;
                            voxels[(hitX + size * hitY + size2 * hitZ) + size].value -= strength * Time.deltaTime;
                            CreateChunk();
                        }
                    }
                }
            }
            // Raise block when left mouse is pressed.
            else if (Input.GetMouseButton(0))
            {
                // Cast a ray through a screen point and return the hit point
                Ray ray = cam.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, 999.0f))
                {
                    // Transform the hit point from world space to local space
                    Vector3 localHit = transform.InverseTransformPoint(hit.point);

                    int hitX = (int)(localHit.x / multiplier);
                    int hitY = (int)(localHit.y / multiplier);
                    int hitZ = (int)(localHit.z / multiplier);

                    voxels[hitX + size * hitY + size2 * hitZ].value += addStrength;
                    voxels[(hitX + size * hitY + size2 * hitZ) + size].value += addStrength * Time.deltaTime;

                    CreateChunk();
                }
            }
        }

        private void OnDrawGizmos()
        {
            foreach (Voxel voxel in voxels)
            {
                float color = Mathf.Clamp(Mathf.Abs(voxel.value), 0.0f, 1.0f);
                if (voxel.value < 0.0f)
                {
                    float secondaryColor = 1.0f - color;
                    Gizmos.color = new Color(color, 0.5f, 0.5f);
                }
                else
                {
                    float secondaryColor = 1.0f - color;
                    Gizmos.color = new Color(0.5f, color, 0.5f);
                }
                Gizmos.DrawSphere(voxel.position, 0.5f);
            }
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
                        float[] p = GetPoints(x, y, z);

                        // Store scalars corresponding to vertices.
                        float[] v = new float[p.Length];

                        for (int index = 0; index < p.Length; ++index)
                        {
                            v[index] = voxels[(int)p[index]].value;
                        }

                        // Initialize cubeindex
                        int cubeIndex = 0;

                        // First part of the algorithm uses a table which maps the vertices under the isosurface to the
                        // intersecting edges. An 8 bit index is formed where each bit corresponds to a vertex.
                        for(int index = 0; index < v.Length; ++index)
                        {
                            cubeIndex |= v[index] < isolevel ? resolutions[index] : 0;
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
                                alpha = (isolevel - v[vp[index].value[0]]) / (v[vp[index].value[1]] - v[vp[index].value[2]]);
                                vertexList[index] = Vector3.Lerp(voxels[(int)p[vp[index].value[3]]].position, voxels[(int)p[vp[index].value[4]]].position, alpha);
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
