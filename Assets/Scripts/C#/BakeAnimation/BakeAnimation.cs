using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BakeAnimation : MonoBehaviour
{
    [SerializeField]
    SkinnedMeshRenderer targetSMR;

    private void Start()
    {
        ReCreateMesh();
    }
    private void LateUpdate()
    {
         
    }
    void SwapBuffer()
    {
        
    }
    void ReCreateMesh()
    {
        Mesh mesh = new Mesh();
        Mesh orMesh = targetSMR.sharedMesh;
        List<Vector3> vertices = new List<Vector3>(orMesh.vertices);
        List<Vector3> normals = new List<Vector3>(orMesh.normals);
        List<Vector4> tangents = new List<Vector4>(orMesh.tangents);
        List<BoneWeight> boneWeights = new List<BoneWeight>(orMesh.boneWeights);
        int[] indices = new int[orMesh.vertexCount];
        List<Vector2> uv = new List<Vector2>();
        for (int i = 0; i < orMesh.vertexCount; i++)
        {
            uv.Add(new Vector2(((float)i + 0.5f) / (float)orMesh.vertexCount, 0));
            indices[i] = i;
        }
        mesh.subMeshCount = 1;
        mesh.SetVertices(vertices);
        mesh.SetNormals(normals);
        mesh.SetTangents(tangents);
        mesh.SetIndices(indices, MeshTopology.Points, 0);
        mesh.SetUVs(0, uv);
        mesh.bindposes = orMesh.bindposes;
        mesh.boneWeights = boneWeights.ToArray();

        mesh.UploadMeshData(true);

        targetSMR.sharedMesh = mesh;
    }
}
