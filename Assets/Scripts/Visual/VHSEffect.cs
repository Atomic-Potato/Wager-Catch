// Modified from: https://github.com/staffantan/unity-vhsglitch

using UnityEngine;
using UnityEngine.Video;

[ExecuteInEditMode]
[AddComponentMenu("Image Effects/GlitchEffect")]
[RequireComponent(typeof(Camera))]
[RequireComponent(typeof(VideoPlayer))]
public class VHSEffect : MonoBehaviour
{
	[SerializeField] Shader _shader;
	[SerializeField] VideoClip _VHSClip;

	private Material _material = null;
	private VideoPlayer _player;
    bool _isActive;

	void Awake()
	{
		_material = new Material(_shader);
		_player = GetComponent<VideoPlayer>();
		_player.isLooping = true;
		_player.renderMode = VideoRenderMode.APIOnly;
		_player.audioOutputMode = VideoAudioOutputMode.None;
		_player.clip = _VHSClip;
		_player.Play();
	}

    void Start()
    {
        TimeScaleManipulator.Instance.TimeScaleIncreasedBrodcaster.AddListener(EnableEffect);
        TimeScaleManipulator.Instance.TimeScaleDecreasedBrodcaster.AddListener(DisableEffect);
        TimeScaleManipulator.Instance.TimeScaleRestoredBroadcaster.AddListener(DisableEffect);
    }

	void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
        if (!_isActive)
        {
            Graphics.Blit(source, destination);
            return;
        }

		_material.SetTexture("_VHSTex", _player.texture);
		Graphics.Blit(source, destination, _material);
	}

    public void EnableEffect()
    {
        _isActive = true;
    }

    public void DisableEffect()
    {
        _isActive = false;
    }
}