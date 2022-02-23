using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

public class SkinningVert : MonoBehaviour
{
   
    [SerializeField] private ComputeShader vcs;
    [SerializeField] Transform[] bones;
    [SerializeField] private int count = 10000;
    //[SerializeField] private ComputeShader cs = null;
    [SerializeField] private Mesh particleMesh = null;
    //[SerializeField] private MeshFilter targetMeshFilter = null;
    [SerializeField] private Material particleMat = null;
    [SerializeField] private Color m_color = Color.blue;

    struct SkinInVert
    {
        public Vector3 pos;
        public Vector3 nor;
        public Vector4 tan;
    };

    struct InVert2Skin
    {
        public float weight0;
        public float weight1;
        public float weight2;
        public float weight3;
        public int index0;
        public int index1;
        public int index2;
        public int index3;
    };

    struct OutSkinVert
    {
        Vector3 pos;
        Vector3 norm;
        Vector4 tang;
    };

    struct AnimationTransformParticle
    {
        public Vector3 targetPosition;
        public Vector3 position;
        public Vector4 color;
        public float scale;
    };

    Mesh mesh;
    int vertCount;
    ComputeBuffer sourceVBO;
    ComputeBuffer sourceSkin;
    ComputeBuffer mBones;
    private ComputeBuffer _particleBuffer = null;
    private ComputeBuffer _argBuffer = null;
    private uint[] _args = new uint[5] { 0, 0, 0, 0, 0, };

    int kernel = 0;

    Matrix4x4[] boneMatrices;

    void SetBoneMatrices()
    {
        for (int i = 0; i < boneMatrices.Length; i++)
        {
            boneMatrices[i] = transform.worldToLocalMatrix * bones[i].localToWorldMatrix * mesh.bindposes[i];
        }
        mBones.SetData(boneMatrices);
    }

    void ComputeSkinning()
    {
        //kernel = vcs.FindKernel("UpdatePositionAsAnimation");
        vcs.SetBuffer(kernel, "m_SourceVBO", sourceVBO);
        vcs.SetBuffer(kernel, "m_SourceSkin", sourceVBO);
        vcs.SetBuffer(kernel, "m_mBones", sourceVBO);
        vcs.SetInt("m_VertCount", vertCount);
        vcs.Dispatch(kernel,64 + 1, 1, 1);

    }

    private void Init()
    {
        SkinnedMeshRenderer smr = GetComponentInChildren<SkinnedMeshRenderer>();
        mesh = smr.sharedMesh;
        vertCount = mesh.vertexCount;

        kernel = vcs.FindKernel("UpdatePositionAsAnimation");

        List<Vector3> vertices = new List<Vector3>();

        SkinInVert[] inV = Enumerable.Range(0, vertCount).Select
            (
                idx => new SkinInVert()
                {
                    pos = mesh.vertices[idx],
                    nor = mesh.normals[idx],
                    tan = mesh.tangents[idx]
                }
            ).ToArray();
        sourceVBO = new ComputeBuffer(vertCount, Marshal.SizeOf(typeof(SkinInVert)));
        sourceVBO.SetData(inV);

        foreach (var item in inV)
        {
            vertices.Add(item.pos);
        }
        

        InVert2Skin[] inSkin = mesh.boneWeights.Select
            (
                weight => new InVert2Skin()
                {
                    weight0 = weight.weight0,
                    weight1 = weight.weight1,
                    weight2 = weight.weight2,
                    weight3 = weight.weight3,
                    index0 = weight.boneIndex0,
                    index1 = weight.boneIndex1,
                    index2 = weight.boneIndex2,
                    index3 = weight.boneIndex3
                }
            ).ToArray();
        sourceSkin = new ComputeBuffer(vertCount, Marshal.SizeOf(typeof(InVert2Skin)));
        sourceSkin.SetData(inSkin);

        bones = smr.bones;
        mBones = new ComputeBuffer(bones.Length, Marshal.SizeOf(typeof(Matrix4x4)));
        boneMatrices = bones.Select((b, idx) => transform.worldToLocalMatrix * b.localToWorldMatrix * mesh.bindposes[idx]).ToArray();

        AnimationTransformParticle[] atp = new AnimationTransformParticle[count];

        for (int i = 0; i < count; i++)
        {
            atp[i] = new AnimationTransformParticle
            {
                targetPosition = vertices[i % vertices.Count],
                position = vertices[i % vertices.Count] + Random.insideUnitSphere * 10f,
                color = m_color,
                scale = Random.Range(0.01f, 0.02f),
            };
        }
        _particleBuffer = new ComputeBuffer(count, Marshal.SizeOf(typeof(AnimationTransformParticle)));
        _particleBuffer.SetData(atp);

        vcs.SetBuffer(kernel, "_Particles", _particleBuffer);

        particleMat.SetBuffer("_Particles", _particleBuffer);

        int subMeshIndex = 0;

        _args[0] = mesh.GetIndexCount(subMeshIndex);
        _args[1] = (uint)count;
        _args[2] = mesh.GetIndexStart(subMeshIndex);
        _args[3] = mesh.GetBaseVertex(subMeshIndex);

        _argBuffer = new ComputeBuffer(1, sizeof(uint) * _args.Length, ComputeBufferType.IndirectArguments);
        _argBuffer.SetData(_args);
    }


    #region MonoBehaviour
    private void Start()
    {
        Init();     
    }
    private void OnDestroy()
    {
        new[] { sourceVBO, sourceSkin, mBones }.ToList().ForEach(b => b.Dispose());
        _particleBuffer?.Release();
        _argBuffer?.Release();
        
    }
    private void Update()
    {
        SetBoneMatrices();
        ComputeSkinning();
        vcs.SetFloat("_DeltaTime", Time.deltaTime);
        vcs.Dispatch(kernel, count / 64, 1, 1);
        Graphics.DrawMeshInstancedIndirect(particleMesh, 0, particleMat, new Bounds(Vector3.zero, Vector3.one * 32f), _argBuffer);
    }
    #endregion
}
