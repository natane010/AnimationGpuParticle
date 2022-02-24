using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SkinningParticleModel : ScriptableObject
{
    public Mesh[] shapes { get { return _shapes; } }

    [SerializeField] Mesh[] _shapes = new Mesh[1];

    public int maxInstanceCount { get { return _maxInstanceCount; } }

    [SerializeField] int _maxInstanceCount = 8192;

    public int instanceCount { get { return _instanceCount; } }

    [SerializeField] int _instanceCount;

    public Mesh mesh { get { return _mesh; } }

    [SerializeField] Mesh _mesh;

    [SerializeField] Mesh _defaultShape;

    Mesh GetShape(int index)
    {
        if (_shapes == null || _shapes.Length == 0) return _defaultShape;
        Mesh mesh = _shapes[index % _shapes.Length];
        return mesh == null ? _defaultShape : mesh;
    }


#if UNITY_EDITOR

    public void RebuildMesh()
    {
        List<Vector3> vtx_out = new List<Vector3>();
        List<Vector3> nrm_out = new List<Vector3>();
        List<Vector4> tan_out = new List<Vector4>();
        List<Vector2> uv0_out = new List<Vector2>();
        List<Vector2> uv1_out = new List<Vector2>();
        List<int> idx_out = new List<int>();

        int vertexCount = 0;
        _instanceCount = 0;

        while (_instanceCount < maxInstanceCount)
        {
            Mesh mesh = GetShape(_instanceCount);
            Vector3[] vtx_in = mesh.vertices;

            if (vertexCount + vtx_in.Length > 65535) break;

            vtx_out.AddRange(vtx_in);
            nrm_out.AddRange(mesh.normals);
            tan_out.AddRange(mesh.tangents);
            uv0_out.AddRange(mesh.uv);

            var uv1 = new Vector2(_instanceCount + 0.5f, 0);
            uv1_out.AddRange(Enumerable.Repeat(uv1, vtx_in.Length));

            idx_out.AddRange(mesh.triangles.Select(i => i + vertexCount));

            vertexCount += vtx_in.Length;
            _instanceCount++;
        }

        uv1_out = uv1_out.Select(x => x / instanceCount).ToList();

        _mesh.Clear();
        _mesh.SetVertices(vtx_out);
        _mesh.SetNormals(nrm_out);
        _mesh.SetUVs(0, uv0_out);
        _mesh.SetUVs(1, uv1_out);
        _mesh.SetIndices(idx_out.ToArray(), MeshTopology.Triangles, 0);
        _mesh.bounds = new Bounds(Vector3.zero, Vector3.one * 1000);
        _mesh.UploadMeshData(true);
    }

#endif


    void OnValidate()
    {
        _maxInstanceCount = Mathf.Clamp(_maxInstanceCount, 4, 8192);
    }

    void OnEnable()
    {
        if (_mesh == null)
        {
            _mesh = new Mesh();
            _mesh.name = "Skinner Particle Template";
        }
    }

}
