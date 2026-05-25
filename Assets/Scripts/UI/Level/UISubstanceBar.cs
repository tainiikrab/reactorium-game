using UnityEngine;

namespace ChemSimDiploma.UI.Level
{
    [RequireComponent(typeof(RectTransform))]
    public class UISubstanceBar : MonoBehaviour
    {
        [SerializeField] private RectTransform _name;
        [SerializeField] private RectTransform _infoIcon;
        [SerializeField] private float _collapsedWidth = 64f;

        private RectTransform _rectTransform;
        private float _expandedWidth;

        private void Awake()
        {
            _rectTransform = (RectTransform)transform;
        }

        public void Collapse()
        {
            if (_name != null)
                _name.gameObject.SetActive(false);

            if (_infoIcon != null)
                _infoIcon.gameObject.SetActive(false);

            float currentWidth = _rectTransform.sizeDelta.x;
            if (currentWidth > _collapsedWidth)
                _expandedWidth = currentWidth;

            SetWidth(_collapsedWidth);
        }

        public void Open()
        {
            if (_name != null)
                _name.gameObject.SetActive(true);

            if (_infoIcon != null)
                _infoIcon.gameObject.SetActive(true);

            SetWidth(_expandedWidth);
        }

        private void SetWidth(float width)
        {
            Vector2 size = _rectTransform.sizeDelta;
            size.x = width;
            _rectTransform.sizeDelta = size;
        }
    }
}
