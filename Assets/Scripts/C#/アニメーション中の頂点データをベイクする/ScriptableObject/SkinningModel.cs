using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SkinningModel : ScriptableObject
{
    #region properties

    public int vertexCount
    {
        get { return _vertexCount; }
    }

    [SerializeField] int _vertexCount;

    public Mesh mesh
    {
        get { return _mesh; }
    }

    [SerializeField] Mesh _mesh;

    #endregion
    

    #if UNITY_EDITOR

    
    public void Initialize(Mesh source)
    {
       
        Vector3[] inVertices = source.vertices;
        Vector3[] inNormals = source.normals;
        Vector4[] inTangents = source.tangents;
        BoneWeight[] inBoneWeights = source.boneWeights;
        
        List<Vector3> outVertices = new List<Vector3>();
        List<Vector3> outNormals = new List<Vector3>();
        List<Vector4> outTangents = new List<Vector4>();
        List<BoneWeight> outBoneWeights = new List<BoneWeight>();

        for (var i = 0; i < inVertices.Length; i++)
        {
            if (!outVertices.Any(_ => _ == inVertices[i]))
            {
                outVertices.Add(inVertices[i]);
                outNormals.Add(inNormals[i]);
                outTangents.Add(inTangents[i]);
                outBoneWeights.Add(inBoneWeights[i]);
            }
        }

        List<Vector2> outUVs = Enumerable.Range(0, outVertices.Count).
            Select(i => Vector2.right * (i + 0.5f) / outVertices.Count).ToList();

        int[] indices = Enumerable.Range(0, outVertices.Count).ToArray();

        _mesh = Instantiate<Mesh>(source);
        _mesh.name = _mesh.name.Substring(0, _mesh.name.Length - 7);

        _mesh.colors = null;
        _mesh.uv2 = null;
        _mesh.uv3 = null;
        _mesh.uv4 = null;

        _mesh.subMeshCount = 0;
        _mesh.SetVertices(outVertices);
        _mesh.SetNormals(outNormals);
        _mesh.SetTangents(outTangents);
        _mesh.SetUVs(0, outUVs);
        _mesh.bindposes = source.bindposes;
        _mesh.boneWeights = outBoneWeights.ToArray();

        _mesh.subMeshCount = 1;
        _mesh.SetIndices(indices, MeshTopology.Points, 0);

        _mesh.UploadMeshData(true);

        _vertexCount = outVertices.Count;
    }

    #endif


    void OnEnable()
    {
    }
}