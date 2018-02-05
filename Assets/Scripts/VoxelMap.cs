using UnityEngine;

public class VoxelMap : MonoBehaviour
{
    public float size = 2f;

    public int voxelResolution = 8;
    public int chunkResolution = 2;

    public VoxelChunk voxelChunkPrefab;

    private VoxelChunk[] voxelChunks;

    private float chunkSize;
    private float voxelSize;
    private float halfSize;

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
        int voxelX = (int)((point.x + halfSize) / voxelSize);
        int voxelY = (int)((point.y + halfSize) / voxelSize);
        int chunkX = voxelX / voxelResolution;
        int chunkY = voxelY / voxelResolution;
        voxelX -= chunkX * voxelResolution;
        voxelY -= chunkY * voxelResolution;
        VoxelStencil activeStencil = new VoxelStencil();
        voxelChunks[chunkY * chunkResolution + chunkX].Apply(voxelX, voxelY, activeStencil);
        Debug.Log(voxelX + ", " + voxelY + " in chunk " + chunkX + ", " + chunkY);

    }

    private void CreateChunk(int i, int x, int y)
    {
        VoxelChunk chunk = Instantiate(voxelChunkPrefab) as VoxelChunk;
        chunk.Initialize(voxelResolution, chunkSize);
        chunk.transform.parent = transform;
        chunk.transform.localPosition = new Vector3(x * chunkSize - halfSize, y * chunkSize - halfSize);
        voxelChunks[i] = chunk;
    }
}
