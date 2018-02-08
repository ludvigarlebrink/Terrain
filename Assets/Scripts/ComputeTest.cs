using System.Collections;
using System.Collections.Generic;
using UnityEngine;

struct Box
{
    public Vector3 position;
    float dummy1;
    public Vector3 normal;
    float dummy2;
}



public class ComputeTest : MonoBehaviour
{
    public Shader shader;
    public ComputeShader computeShader;

    private ComputeBuffer outputBuffer;
    private int _kernel;
    private Material material;

    void Start()
    {
        RunShader();
    }

    void RunShader()
    {
        _kernel = computeShader.FindKernel("CSMain");

        Vector3[] Array = new Vector3[80];
        for (int i = 0; i < 80; ++i){
            Array[i] = new Vector3(100.0f, 23.0f, 12.0f);
        }

        outputBuffer = new ComputeBuffer(Array.Length, sizeof(float) * 3); //Output buffer  contains      vertices (float3 = Vector3 -> 12 bytes)
        outputBuffer.SetData(Array);

        computeShader.SetBuffer(_kernel, "output", outputBuffer);

        computeShader.Dispatch(_kernel, Array.Length % 8, 1, 1);

        Vector3[] data = new Vector3[outputBuffer.count];
        outputBuffer.GetData(data);

        outputBuffer.Dispose();

        foreach (Vector3 d in data)
        {
            Debug.Log(d);
        }

        // material.SetPass(0);
        // material.SetBuffer("buf_Points", outputBuffer);
        // Graphics.DrawProcedural(MeshTopology.Points, data.Length);
    }
}