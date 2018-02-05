using UnityEngine;

public class VoxelMap : MonoBehaviour
{
    public float size = 2f;

    public int voxelResolution = 8;
    public int chunkResolution = 2;

    public VoxelChunk voxelChunkPrefab;

    private VoxelChunk[] voxelChunks;

    private VoxelStencil[] stencils =
    {
        new VoxelStencil(),
        new VoxelStencilCircle()
    };

    private float chunkSize;
    private float voxelSize;
    private float halfSize;

    private static string[] fillTypeNames = { "Filled", "Empty" };
    private static string[] stencilNames = { "Square", "Circle" };
    private static string[] radiusNames = { "0", "1", "2", "3", "4", "5" };
    private int fillTypeIndex;
    private int radiusIndex;
    private int stencilIndex;

    #region Private Methods
    private void Awake()
    {
        halfSize = size * 0.5f;
        chunkSize = size / chunkResolution;
        voxelSize = chunkSize / voxelResolution;

        voxelChunks = new VoxelChunk[chunkResolution * chunkResolution];
        for (int i = 0, y = 0; y < chunkResolution; y++)
        {
            for (int x = 0; x < chunkResolution; x++, i++)
            {
                CreateChunk(i, x, y);
            }
        }

        BoxCollider box = gameObject.AddComponent<BoxCollider>();
        box.size = new Vector3(size, size);
    }

    private void Update()
    {
        if (Input.GetMouseButton(0))
        {
            RaycastHit hitInfo;
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo))
            {
                if (hitInfo.collider.gameObject == gameObject)
                {
                    EditVoxels(transform.InverseTransformPoint(hitInfo.point));
                }
            }
        }
    }

    private void EditVoxels(Vector3 point)
    {
        int centerX = (int)((point.x + halfSize) / voxelSize);
        int centerY = (int)((point.y + halfSize) / voxelSize);

        int xStart = (centerX - radiusIndex) / voxelResolution;
        if (xStart < 0)
        {
            xStart = 0;
        }
        int xEnd = (centerX + radiusIndex) / voxelResolution;
        if (xEnd >= chunkResolution)
        {
            xEnd = chunkResolution - 1;
        }
        int yStart = (centerY - radiusIndex) / voxelResolution;
        if (yStart < 0)
        {
            yStart = 0;
        }
        int yEnd = (centerY + radiusIndex) / voxelResolution;
        if (yEnd >= chunkResolution)
        {
            yEnd = chunkResolution - 1;
        }

        VoxelStencil activeStencil = stencils[stencilIndex];
        activeStencil.Initialize(fillTypeIndex == 0, radiusIndex);

        int voxelYOffset = yStart * voxelResolution;
        for (int y = yStart; y <= yEnd; y++)
        {
            int i = y * chunkResolution + xStart;
            int voxelXOffset = xStart * voxelResolution;
            for (int x = xStart; x <= xEnd; x++, i++)
            {
                activeStencil.SetCenter(centerX - voxelXOffset, centerY - voxelYOffset);
                voxelChunks[i].Apply(activeStencil);
                voxelXOffset += voxelResolution;
            }
            voxelYOffset += voxelResolution;
        }
    }

    private void CreateChunk(int i, int x, int y)
    {
        VoxelChunk chunk = Instantiate(voxelChunkPrefab) as VoxelChunk;
        chunk.Initialize(voxelResolution, chunkSize);
        chunk.transform.parent = transform;
        chunk.transform.localPosition = new Vector3(x * chunkSize - halfSize, y * chunkSize - halfSize);
        voxelChunks[i] = chunk;
    }

    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(4f, 4f, 150f, 500f));
        GUILayout.Label("Fill Type");
        fillTypeIndex = GUILayout.SelectionGrid(fillTypeIndex, fillTypeNames, 2);
        GUILayout.Label("Radius");
        radiusIndex = GUILayout.SelectionGrid(radiusIndex, radiusNames, 6);
        GUILayout.Label("Stencil");
        stencilIndex = GUILayout.SelectionGrid(stencilIndex, stencilNames, 2);
        GUILayout.EndArea();
    }

    #endregion
}
