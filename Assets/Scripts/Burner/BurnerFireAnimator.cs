using UnityEngine;

namespace ChemSimDiploma.Burner
{
[RequireComponent(typeof(SpriteRenderer))]
public class BurnerFireAnimator : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _renderer;
    [SerializeField] private Transform _animatedTransform;

    [Header("Fade")]
    [SerializeField] private float _fadeInDuration = 0.35f;
    [SerializeField] private float _fadeOutDuration = 0.45f;

    [Header("Flicker")]
    [SerializeField] private float _alphaMin = 0.58f;
    [SerializeField] private float _alphaMax = 0.94f;

    [Header("Jitter")]
    [SerializeField] private float _positionJitterX = 0.06f;
    [SerializeField] private float _positionJitterY = 0.1f;
    [SerializeField] [Range(0f, 0.12f)] private float _scaleJitter = 0.035f;

    private Vector3 _baseLocalPosition;
    private Vector3 _baseLocalScale;
    private Color _baseColor;
    private float _noiseSeed;
    private float _fade;
    private bool _targetVisible;

    private void Awake()
    {
        if (_renderer == null)
            _renderer = GetComponent<SpriteRenderer>();
        if (_animatedTransform == null)
            _animatedTransform = transform;

        CacheBaseState();
        _noiseSeed = Random.Range(0f, 1000f);
        _fade = gameObject.activeSelf ? 1f : 0f;
        _targetVisible = _fade > 0f;
    }

    public void SetVisible(bool visible)
    {
        _targetVisible = visible;

        if (visible)
        {
            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
                _fade = 0f;
            }

            return;
        }

        if (!gameObject.activeSelf)
            _fade = 0f;
    }

    private void Update()
    {
        float targetFade = _targetVisible ? 1f : 0f;
        float fadeDuration = _targetVisible ? _fadeInDuration : _fadeOutDuration;

        if (fadeDuration <= 0f)
            _fade = targetFade;
        else
            _fade = Mathf.MoveTowards(_fade, targetFade, Time.deltaTime / fadeDuration);

        if (!_targetVisible && _fade <= 0f)
        {
            ResetVisual();
            gameObject.SetActive(false);
            return;
        }

        ApplyFlicker(_fade);
    }

    private void ApplyFlicker(float fadeMultiplier)
    {
        float time = Time.time;

        float alphaNoise = Mathf.PerlinNoise(time * 4.2f + _noiseSeed, _noiseSeed * 0.17f);
        float alphaSine = (Mathf.Sin(time * 11f + _noiseSeed) + 1f) * 0.5f;
        float alphaBlend = alphaNoise * 0.65f + alphaSine * 0.35f;
        float flickerAlpha = Mathf.Lerp(_alphaMin, _alphaMax, alphaBlend);

        float jitterX = (Mathf.PerlinNoise(time * 9f + _noiseSeed, 0.4f) - 0.5f) * 2f * _positionJitterX;
        float jitterY = (Mathf.PerlinNoise(time * 10.5f + _noiseSeed, 1.1f) - 0.5f) * 2f * _positionJitterY;
        float scaleNoise = Mathf.PerlinNoise(time * 7f + _noiseSeed, 2.3f);
        float scaleSine = (Mathf.Sin(time * 13f + _noiseSeed * 0.5f) + 1f) * 0.5f;
        float scaleBlend = scaleNoise * 0.55f + scaleSine * 0.45f;
        float scaleJitter = 1f + (scaleBlend - 0.5f) * 2f * _scaleJitter;
        float fadeScale = Mathf.Lerp(0.85f, 1f, fadeMultiplier);

        _animatedTransform.localPosition = _baseLocalPosition + new Vector3(jitterX, jitterY, 0f) * fadeMultiplier;
        _animatedTransform.localScale = _baseLocalScale * (scaleJitter * fadeScale);

        Color color = _baseColor;
        color.a = flickerAlpha * _baseColor.a * fadeMultiplier;
        _renderer.color = color;
    }

    private void CacheBaseState()
    {
        if (_animatedTransform != null)
        {
            _baseLocalPosition = _animatedTransform.localPosition;
            _baseLocalScale = _animatedTransform.localScale;
        }

        if (_renderer != null)
            _baseColor = _renderer.color;
    }

    private void ResetVisual()
    {
        if (_animatedTransform != null)
        {
            _animatedTransform.localPosition = _baseLocalPosition;
            _animatedTransform.localScale = _baseLocalScale;
        }

        if (_renderer != null)
            _renderer.color = _baseColor;
    }
}
}
