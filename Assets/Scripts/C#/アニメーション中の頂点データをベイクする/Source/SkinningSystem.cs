using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Skinning/System")]
[RequireComponent(typeof(SkinnedMeshRenderer))]
public class SkinningSystem : MonoBehaviour
{
    #region 変数＋設定項目

    [SerializeField]
    SkinningModel _model;

    #endregion

    #region プロパティ

    ///　頂点数
    public int vertexCount
    {
        get { return _model != null ? _model.vertexCount : 0; }
    }

    /// バッファの確認
    public bool isReady
    {
        get { return _frameCount > 1; }
    }

    /// 頂点位置をベイクしたテクスチャ
    public RenderTexture positionBuffer
    {
        get { return _shiftFlag ? _positionBuffer1 : _positionBuffer0; }
    }

    /// 前フレームの頂点位置ベイクしたテクスチャ
    public RenderTexture previousPositionBuffer
    {
        get { return _shiftFlag ? _positionBuffer0 : _positionBuffer1; }
    }

    ///法線をべいくしたテクスチャ
    public RenderTexture normalBuffer
    {
        get { return _normalBuffer; }
    }

    /// 正接をベイクしたテクスチャ
    public RenderTexture tangentBuffer
    {
        get { return _tangentBuffer; }
    }

    #endregion

    #region Internal resources

    // 情報の代替シェーダー
    [SerializeField] Shader m_replacementShader;
    [SerializeField] Shader _replacementShaderPosition;
    [SerializeField] Shader _replacementShaderNormal;
    [SerializeField] Shader _replacementShaderTangent;

    // 置換タグ
    [SerializeField] Material _placeholderMaterial;

    #endregion

    #region 変数

    // Vertex attribute buffers.
    RenderTexture _positionBuffer0;
    RenderTexture _positionBuffer1;
    RenderTexture _normalBuffer;
    RenderTexture _tangentBuffer;

    //レンダリングターゲット
    RenderBuffer[] _mrt0;
    RenderBuffer[] _mrt1;
    bool _shiftFlag;

    // 頂点ベイクカメラ
    Camera _bakeBufferCamera;

    // 1Fと2Fで除去のために使う
    int _frameCount;

    // 頂点ベイクのレンダーテクスチャ作成
    RenderTexture CreateBuffer()
    {
        RenderTextureFormat format = SkinningContents.supportedBufferFormat;
        RenderTexture rt = new RenderTexture(_model.vertexCount, 1, 0, format);
        rt.filterMode = FilterMode.Point;
        return rt;
    }

    // レンダラーの設定をオーバーライド
    void OverrideRenderer()
    {
        SkinnedMeshRenderer smr = GetComponent<SkinnedMeshRenderer>();
        smr.sharedMesh = _model.mesh;
        smr.material = _placeholderMaterial;
        smr.receiveShadows = false;

        // 頂点ベイクカメラでの管理
        smr.enabled = false;
    }

    // 頂点ベイクカメラの生成
    void BuildCamera()
    {
        
        GameObject go = new GameObject("Camera");
        go.hideFlags = HideFlags.HideInHierarchy;

        
        Transform tr = go.transform;
        tr.parent = transform;
        tr.localPosition = Vector3.zero;
        tr.localRotation = Quaternion.identity;

        
        _bakeBufferCamera = go.AddComponent<Camera>();

        _bakeBufferCamera.renderingPath = RenderingPath.Forward;
        _bakeBufferCamera.clearFlags = CameraClearFlags.SolidColor;
        _bakeBufferCamera.depth = -10000; 

        _bakeBufferCamera.nearClipPlane = -100;
        _bakeBufferCamera.farClipPlane = 100;
        _bakeBufferCamera.orthographic = true;
        _bakeBufferCamera.orthographicSize = 100;

        _bakeBufferCamera.enabled = false; 

        
        CullingState culler = go.AddComponent<CullingState>();
        culler.target = GetComponent<SkinnedMeshRenderer>();
    }

    #endregion

    #region MonoBehaviour

    void Start()
    {
        // バッファの生成
        _positionBuffer0 = CreateBuffer();
        _positionBuffer1 = CreateBuffer();
        _normalBuffer = CreateBuffer();
        _tangentBuffer = CreateBuffer();

        
        _mrt0 = new[] {
                _positionBuffer0.colorBuffer,
                _normalBuffer.colorBuffer,
                _tangentBuffer.colorBuffer
            };
      
        _mrt1 = new[] {
                _positionBuffer1.colorBuffer,
                _normalBuffer.colorBuffer,
                _tangentBuffer.colorBuffer
            };
        OverrideRenderer();
        BuildCamera();

        _shiftFlag = true; 
    }

    void OnDestroy()
    {
        if (_positionBuffer0 != null) Destroy(_positionBuffer0);
        if (_positionBuffer1 != null) Destroy(_positionBuffer1);
        if (_normalBuffer != null) Destroy(_normalBuffer);
        if (_tangentBuffer != null) Destroy(_tangentBuffer);
    }

    void LateUpdate()
    {
        _shiftFlag = !_shiftFlag;
   
        if (_shiftFlag)
        {
            _bakeBufferCamera.targetTexture = _positionBuffer1;
            _bakeBufferCamera.RenderWithShader(_replacementShaderPosition, "Skinning");
            _bakeBufferCamera.targetTexture = _normalBuffer;
            _bakeBufferCamera.RenderWithShader(_replacementShaderNormal, "Skinning");
            _bakeBufferCamera.targetTexture = _tangentBuffer;
            _bakeBufferCamera.RenderWithShader(_replacementShaderTangent, "Skinning");
        }
        else
        {
            _bakeBufferCamera.targetTexture = _positionBuffer0;
            _bakeBufferCamera.RenderWithShader(_replacementShaderPosition, "Skinning");
            _bakeBufferCamera.targetTexture = _normalBuffer;
            _bakeBufferCamera.RenderWithShader(_replacementShaderNormal, "Skinning");
            _bakeBufferCamera.targetTexture = _tangentBuffer;
            _bakeBufferCamera.RenderWithShader(_replacementShaderTangent, "Skinning");
        }

        _frameCount++;
    }

    #endregion
    
}
