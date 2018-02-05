using UnityEngine;

[SelectionBase]
public class VoxelChunk : MonoBehaviour
{
    private int resolution;

    private bool[] voxels;

    private float voxelSize;

    private Material[] voxelMaterials;

    public GameObject voxelPrefab;

    #region Public Methods
    public void Initialize(int resolution, float size)
    {
        this.resolution = resolution;
        voxelSize = size / resolution;
        voxels = new bool[resolution * resolution];
        voxelMaterials = new Material[voxels.Length];

        for (int i = 0, y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++, i++)
            {
                CreateVoxel(i, x, y);
            }
        }

        SetVoxelColors();
    }

    public void Apply(int x, int y, VoxelStencil stencil)
    {
        voxels[y * resolution + x] = stencil.Apply(x, y);
        SetVoxelColors();
    }
    #endregion

    #region Private Methods
    private void SetVoxelColors()
    {
        for (int i = 0; i < voxels.Length; i++)
        {
            voxelMaterials[i].color = voxels[i] ? Color.black : Color.white;
        }
    }

    private void CreateVoxel(int i, int x, int y)
    {
        GameObject obj = Instantiate(voxelPrefab) as GameObject;
        obj.transform.parent = transform;
        obj.transform.localPosition = new Vector3((x + 0.5f) * voxelSize, (y + 0.5f) * voxelSize);
        obj.transform.localScale = Vector3.one * voxelSize * 0.9f;
        voxelMaterials[i] = obj.GetComponent<MeshRenderer>().material;
    }
    #endregion
}
