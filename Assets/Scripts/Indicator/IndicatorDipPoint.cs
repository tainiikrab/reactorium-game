using UnityEngine;

namespace ChemSimDiploma.Indicator
{
/// <summary>
/// Точка погружения индикаторной бумаги (обычно у child Sprite колбы), отдельно от <see cref="SceneObjectController.Draggable.InteractPoint"/> для налива.
/// </summary>
public class IndicatorDipPoint : MonoBehaviour
{
    [SerializeField] private Transform _anchor;

    public Transform AttachTransform => _anchor != null ? _anchor : transform;

    private void Awake()
    {
        if (_anchor == null)
            _anchor = transform;
    }
}
}