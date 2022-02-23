using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Skinning/System")]
[RequireComponent(typeof(SkinnedMeshRenderer))]
public class SkinningSystem : MonoBehaviour
{
    #region 変数＋設定項目

    [SerializeField]
    SkinnerModel _model;

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
        get { return _swapFlag ? _positionBuffer1 : _positionBuffer0; }
    }

    /// 前フレームの頂点位置ベイクしたテクスチャ
    public RenderTexture previousPositionBuffer
    {
        get { return _swapFlag ? _positionBuffer0 : _positionBuffer1; }
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
    bool _swapFlag;

    // 頂点ベイクカメラ
    Camera _bakeCamera;

    // 1Fと2Fで除去のために使う
    int _frameCount;

    // 頂点ベイクのレンダーテクスチャ作成
    RenderTexture CreateBuffer()
    {
        var format = SkinningContents.supportedBufferFormat;
        var rt = new RenderTexture(_model.vertexCount, 1, 0, format);
        rt.filterMode = FilterMode.Point;
        return rt;
    }

    // レンダラーの設定をオーバーライド
    void OverrideRenderer()
    {
        var smr = GetComponent<SkinnedMeshRenderer>();
        smr.sharedMesh = _model.mesh;
        smr.material = _placeholderMaterial;
        smr.receiveShadows = false;

        // 頂点ベイクカメラでの管理
        smr.enabled = false;
    }

    // 頂点ベイクカメラの生成
    void BuildCamera()
    {
        
        var go = new GameObject("Camera");
        go.hideFlags = HideFlags.HideInHierarchy;

        
        var tr = go.transform;
        tr.parent = transform;
        tr.localPosition = Vector3.zero;
        tr.localRotation = Quaternion.identity;

        
        _bakeCamera = go.AddComponent<Camera>();

        _bakeCamera.renderingPath = RenderingPath.Forward;
        _bakeCamera.clearFlags = CameraClearFlags.SolidColor;
        _bakeCamera.depth = -10000; 

        _bakeCamera.nearClipPlane = -100;
        _bakeCamera.farClipPlane = 100;
        _bakeCamera.orthographic = true;
        _bakeCamera.orthographicSize = 100;

        _bakeCamera.enabled = false; 

        
        var culler = go.AddComponent<CullingState>();
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

        _swapFlag = true; 
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
        _swapFlag = !_swapFlag;
   
        if (_swapFlag)
        {
            _bakeCamera.targetTexture = _positionBuffer1;
            _bakeCamera.RenderWithShader(_replacementShaderPosition, "Skinner");
            _bakeCamera.targetTexture = _normalBuffer;
            _bakeCamera.RenderWithShader(_replacementShaderNormal, "Skinner");
            _bakeCamera.targetTexture = _tangentBuffer;
            _bakeCamera.RenderWithShader(_replacementShaderTangent, "Skinner");
        }
        else
        {
            _bakeCamera.targetTexture = _positionBuffer0;
            _bakeCamera.RenderWithShader(_replacementShaderPosition, "Skinner");
            _bakeCamera.targetTexture = _normalBuffer;
            _bakeCamera.RenderWithShader(_replacementShaderNormal, "Skinner");
            _bakeCamera.targetTexture = _tangentBuffer;
            _bakeCamera.RenderWithShader(_replacementShaderTangent, "Skinner");
        }

        _frameCount++;
    }

    #endregion
    
}
