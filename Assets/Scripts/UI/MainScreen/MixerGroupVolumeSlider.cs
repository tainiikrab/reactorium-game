using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
namespace ChemSimDiploma.UI
{

[RequireComponent(typeof(Slider))]
public sealed class MixerGroupVolumeSlider : MonoBehaviour
{
    private const float SilenceDb = -80f;

    [SerializeField] private AudioMixer _mixer;
    [SerializeField] private string _exposedVolumeParameter;

    private Slider _slider;

    private void Awake()
    {
        _slider = GetComponent<Slider>();
    }

    private void OnEnable()
    {
        _slider.onValueChanged.AddListener(ApplyVolume);
        ApplyVolume(_slider.value);
    }

    private void OnDisable()
    {
        _slider.onValueChanged.RemoveListener(ApplyVolume);
    }

    private void ApplyVolume(float slider01)
    {
        if (_mixer == null || string.IsNullOrEmpty(_exposedVolumeParameter))
            return;

        slider01 = Mathf.Clamp01(slider01);
        float db;
        if (slider01 <= 1e-5f)
            db = SilenceDb;
        else
            db = Mathf.Clamp(20f * Mathf.Log10(slider01), SilenceDb, 0f);

        _mixer.SetFloat(_exposedVolumeParameter, db);
    }
}
}
