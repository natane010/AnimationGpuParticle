using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Skinning/System")]
[RequireComponent(typeof(SkinnedMeshRenderer))]
public class SkinningSystem : MonoBehaviour
{
    #region �ϐ��{�ݒ荀��

    [SerializeField]
    SkinningModel _model;

    #endregion

    #region �v���p�e�B

    ///�@���_��
    public int vertexCount
    {
        get { return _model != null ? _model.vertexCount : 0; }
    }

    /// �o�b�t�@�̊m�F
    public bool isReady
    {
        get { return _frameCount > 1; }
    }

    /// ���_�ʒu���x�C�N�����e�N�X�`��
    public RenderTexture positionBuffer
    {
        get { return _shiftFlag ? _positionBuffer1 : _positionBuffer0; }
    }

    /// �O�t���[���̒��_�ʒu�x�C�N�����e�N�X�`��
    public RenderTexture previousPositionBuffer
    {
        get { return _shiftFlag ? _positionBuffer0 : _positionBuffer1; }
    }

    ///�@�����ׂ��������e�N�X�`��
    public RenderTexture normalBuffer
    {
        get { return _normalBuffer; }
    }

    /// ���ڂ��x�C�N�����e�N�X�`��
    public RenderTexture tangentBuffer
    {
        get { return _tangentBuffer; }
    }

    #endregion

    #region Internal resources

    // ���̑�փV�F�[�_�[
    [SerializeField] Shader m_replacementShader;
    [SerializeField] Shader _replacementShaderPosition;
    [SerializeField] Shader _replacementShaderNormal;
    [SerializeField] Shader _replacementShaderTangent;

    // �u���^�O
    [SerializeField] Material _placeholderMaterial;

    #endregion

    #region �ϐ�

    // Vertex attribute buffers.
    RenderTexture _positionBuffer0;
    RenderTexture _positionBuffer1;
    RenderTexture _normalBuffer;
    RenderTexture _tangentBuffer;

    //�����_�����O�^�[�Q�b�g
    RenderBuffer[] _mrt0;
    RenderBuffer[] _mrt1;
    bool _shiftFlag;

    // ���_�x�C�N�J����
    Camera _bakeBufferCamera;

    // 1F��2F�ŏ����̂��߂Ɏg��
    int _frameCount;

    // ���_�x�C�N�̃����_�[�e�N�X�`���쐬
    RenderTexture CreateBuffer()
    {
        RenderTextureFormat format = SkinningContents.supportedBufferFormat;
        RenderTexture rt = new RenderTexture(_model.vertexCount, 1, 0, format);
        rt.filterMode = FilterMode.Point;
        return rt;
    }

    // �����_���[�̐ݒ���I�[�o�[���C�h
    void OverrideRenderer()
    {
        SkinnedMeshRenderer smr = GetComponent<SkinnedMeshRenderer>();
        smr.sharedMesh = _model.mesh;
        smr.material = _placeholderMaterial;
        smr.receiveShadows = false;

        // ���_�x�C�N�J�����ł̊Ǘ�
        smr.enabled = false;
    }

    // ���_�x�C�N�J�����̐���
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
        // �o�b�t�@�̐���
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
