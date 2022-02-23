using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Skinning/Skinning Trail")]
[RequireComponent(typeof(MeshRenderer))]
public class SkinningTrail : MonoBehaviour
{
    #region External object/asset references

    /// �G�t�F�N�g�̃\�[�X
    public SkinningSystem source
    {
        get { return _source; }
        set { _source = value; _reconfigured = true; }
    }

    [SerializeField]
    [Tooltip("Reference to an effect source.")]
    SkinningSystem _source;

    ///�@�g���C���̃��C�������_�����O
    public SkinningTrailNormal template
    {
        get { return _template; }
        set { _template = value; _reconfigured = true; }
    }

    [SerializeField]
    [Tooltip("Reference to a template object used for rendering trail lines.")]
    SkinningTrailNormal _template;

    #endregion

    #region Dynamics settings

    /// ���_�̈ړ��̐����@���p�[�e�B�N���ɂ���ꍇ�͂�����ӂ��������K�v
    public float speedLimit
    {
        get { return _speedLimit; }
        set { _speedLimit = value; }
    }

    [SerializeField]
    float _speedLimit = 0.4f;

    /// �R��
    public float drag
    {
        get { return _drag; }
        set { _drag = value; }
    }

    [SerializeField]
    float _drag = 5;

    #endregion

    #region Line width modifier

    ///�@�J�b�g���鑬�x
    public float cutoffSpeed
    {
        get { return _cutoffSpeed; }
        set { _cutoffSpeed = value; }
    }

    [SerializeField]
    float _cutoffSpeed = 0;

    /// ����
    public float speedToWidth
    {
        get { return _speedToWidth; }
        set { _speedToWidth = value; }
    }

    [SerializeField]
    float _speedToWidth = 0.02f;

    /// �ő啝
    public float maxWidth
    {
        get { return _maxWidth; }
        set { _maxWidth = value; }
    }

    [SerializeField]
    float _maxWidth = 0.05f;

    #endregion

    #region Other settings

    /// ����
    public int randomSeed
    {
        get { return _randomSeed; }
        set { _randomSeed = value; _reconfigured = true; }
    }

    [SerializeField]
    int _randomSeed = 0;

    #endregion

    #region Reconfiguration detection

    // �R���t�B�M�����[�V������Ό�
    bool _reconfigured;

#if UNITY_EDITOR

    /// �ύX�ʒm
    public void UpdateConfiguration()
    {
        _reconfigured = true;
    }

#endif

    #endregion

    #region Built-in assets

    [SerializeField] Shader _kernelShader;
    [SerializeField] Material _defaultMaterial;

    #endregion

    #region �A�j���[�V�����J�[�l���̊Ǘ�

    enum Kernels
    {
        InitializePosition, InitializeVelocity, InitializeOrthnorm,
        UpdatePosition, UpdateVelocity, UpdateOrthnorm
    }

    enum Buffers { Position, Velocity, Orthnorm }

    AnimationKernelSet<Kernels, Buffers> _kernel;

    void InvokeAnimationKernels()
    {
        if (_kernel == null)
            _kernel = new AnimationKernelSet<Kernels, Buffers>(_kernelShader, x => (int)x, x => (int)x);

        if (!_kernel.ready)
        {
            //�������A�j���[�V�����J�[�l��
            _kernel.Setup(_source.vertexCount, _template.historyLength);
            _kernel.material.SetTexture("_SourcePositionBuffer1", _source.positionBuffer);
            _kernel.material.SetFloat("_RandomSeed", _randomSeed);
            _kernel.Invoke(Kernels.InitializePosition, Buffers.Position);
            _kernel.Invoke(Kernels.InitializeVelocity, Buffers.Velocity);
            _kernel.Invoke(Kernels.InitializeOrthnorm, Buffers.Orthnorm);
        }
        else
        {
            // �\�[�X�|�W�V�����̃A�g���r���[�g��]������B
            _kernel.material.SetTexture("_SourcePositionBuffer0", _source.previousPositionBuffer);
            _kernel.material.SetTexture("_SourcePositionBuffer1", _source.positionBuffer);

            //  ���x�X�V�J�[�l�����Ăяo���B
            _kernel.material.SetTexture("_PositionBuffer", _kernel.GetLastBuffer(Buffers.Position));
            _kernel.material.SetTexture("_VelocityBuffer", _kernel.GetLastBuffer(Buffers.Velocity));
            _kernel.material.SetFloat("_SpeedLimit", _speedLimit);
            _kernel.Invoke(Kernels.UpdateVelocity, Buffers.Velocity);

            // �X�V���ꂽ���x�ňʒu�X�V�J�[�l�����Ăяo���B
            _kernel.material.SetTexture("_VelocityBuffer", _kernel.GetWorkingBuffer(Buffers.Velocity));
            _kernel.material.SetFloat("_Drag", Mathf.Exp(-_drag * Time.deltaTime));
            _kernel.Invoke(Kernels.UpdatePosition, Buffers.Position);

            // �X�V���ꂽ���x�Œ����X�V�J�[�l�����Ăяo���B
            _kernel.material.SetTexture("_PositionBuffer", _kernel.GetWorkingBuffer(Buffers.Position));
            _kernel.material.SetTexture("_OrthnormBuffer", _kernel.GetLastBuffer(Buffers.Orthnorm));
            _kernel.Invoke(Kernels.UpdateOrthnorm, Buffers.Orthnorm);
        }

        _kernel.SwapBuffers();
    }

    #endregion

    #region �����_���[�̊Ǘ�

    InitRendererAdapter _renderer;

    void UpdateRenderer()
    {
        if (_renderer == null)
            _renderer = new InitRendererAdapter(gameObject, _defaultMaterial);

        //�v���p�e�B�u���b�N�̍X�V
        var block = _renderer.propertyBlock;
        block.SetTexture("_PreviousPositionBuffer", _kernel.GetWorkingBuffer(Buffers.Position));
        block.SetTexture("_PreviousVelocityBuffer", _kernel.GetWorkingBuffer(Buffers.Velocity));
        block.SetTexture("_PreviousOrthnormBuffer", _kernel.GetWorkingBuffer(Buffers.Orthnorm));
        block.SetTexture("_PositionBuffer", _kernel.GetLastBuffer(Buffers.Position));
        block.SetTexture("_VelocityBuffer", _kernel.GetLastBuffer(Buffers.Velocity));
        block.SetTexture("_OrthnormBuffer", _kernel.GetLastBuffer(Buffers.Orthnorm));
        block.SetVector("_LineWidth", new Vector3(_maxWidth, _cutoffSpeed, _speedToWidth / _maxWidth));
        block.SetFloat("_RandomSeed", _randomSeed);

        _renderer.Update(_template.mesh);
    }

    #endregion

    #region MonoBehaviour

    void Reset()
    {
        _reconfigured = true;
    }

    void OnDestroy()
    {
        _kernel.Release();
    }

    void OnValidate()
    {
        _cutoffSpeed = Mathf.Max(_cutoffSpeed, 0);
        _speedToWidth = Mathf.Max(_speedToWidth, 0);
        _maxWidth = Mathf.Max(_maxWidth, 0);
    }

    void LateUpdate()
    {      
        if (_source == null || !_source.isReady) return;        

        if (_reconfigured)
        {
            if (_kernel != null) _kernel.Release();
            _reconfigured = false;
        }
        InvokeAnimationKernels();
        UpdateRenderer();
    }

    #endregion
}
