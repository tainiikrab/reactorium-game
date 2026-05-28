using PrimeTween;
using UnityEngine;
using UnityEngine.UI;

namespace ChemSimDiploma.UI
{
public class HandbookSubstanceBar : MonoBehaviour
{
    private static readonly Color InactiveColor = Color.white;

    [SerializeField] private Color _activatedColor;
    [SerializeField] private Image _image;

    [Header("Animation")]
    [SerializeField] private float _colorDuration = 0.22f;
    [SerializeField] private Ease _activateEase = Ease.OutCubic;
    [SerializeField] private Ease _deactivateEase = Ease.OutQuad;

    private Tween _colorTween;

    private void OnDisable()
    {
        StopColorTween();
    }

    public void Enable()
    {
        AnimateColor(_activatedColor, _activateEase);
    }

    public void Disable()
    {
        AnimateColor(InactiveColor, _deactivateEase);
    }

    public void DisableImmediate()
    {
        StopColorTween();
        _image.color = InactiveColor;
    }

    private void AnimateColor(Color target, Ease ease)
    {
        if (_image == null)
            return;

        StopColorTween();
        Color from = _image.color;
        _colorTween = Tween.Custom(_image, 0f, 1f, _colorDuration,
            (img, t) => img.color = Color.Lerp(from, target, t), ease);
    }

    private void StopColorTween()
    {
        if (_colorTween.isAlive)
            _colorTween.Stop();
    }
}
}
