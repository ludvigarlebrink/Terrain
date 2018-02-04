using UnityEngine;

[SelectionBase]
public class VoxelChunk : MonoBehaviour
{
    private int resolution;

    public GameObject voxelPrefab;

    private bool[] voxels;

    private float voxelSize;

    public void Initialize(int resolution, float size)
    {
        this.resolution = resolution;
        voxelSize = size / resolution;
        voxels = new bool[resolution * resolution];

        for (int i = 0, y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++, i++)
            {
                CreateVoxel(i, x, y);
            }
        }
    }

    private void CreateVoxel(int i, int x, int y)
    {
        GameObject obj = Instantiate(voxelPrefab) as GameObject;
        obj.transform.parent = transform;
        obj.transform.localPosition = new Vector3((x + 0.5f) * voxelSize, (y + 0.5f) * voxelSize);
        obj.transform.localScale = Vector3.one * voxelSize * 0.9f;
    }
}
