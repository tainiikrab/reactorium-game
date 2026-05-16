using UnityEngine;
using UnityEngine.Rendering;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ChemSimDiploma.Chemistry.Visuals
{
public class LiquidSortingLayerHandler : MonoBehaviour
{
    [SerializeField] private string _liquidSortingLayerName = "Liquid";

    private int _nextSortingLayerID;

    private void OnEnable()
    {
        RefreshSortingLayers();
    }

    private void OnValidate()
    {
        if (isActiveAndEnabled)
            RefreshSortingLayers();
    }

    [ContextMenu("Refresh Sorting Layers")]
    private void RefreshSortingLayers()
    {
        _nextSortingLayerID = 0;

        foreach (FillLevelAnimator fillLevelAnimator in GetComponentsInChildren<FillLevelAnimator>(true))
            AssignSortingLayer(fillLevelAnimator);
    }

    private void AssignSortingLayer(FillLevelAnimator fillLevelAnimator)
    {
        var spriteMask = fillLevelAnimator.GetComponent<SpriteMask>();
        if (!spriteMask) return;

        int sortingLayerId = SortingLayer.NameToID(_liquidSortingLayerName);

        spriteMask.isCustomRangeActive = true;
        spriteMask.frontSortingLayerID = sortingLayerId;
        spriteMask.frontSortingOrder = _nextSortingLayerID;
        spriteMask.backSortingLayerID = sortingLayerId;
        spriteMask.backSortingOrder = _nextSortingLayerID - 1;

        SpriteRenderer liquid = fillLevelAnimator.LiquidRenderer;
        if (!liquid)
            liquid = fillLevelAnimator.GetComponentInChildren<SpriteRenderer>(true);
        if (!liquid) return;

        var sortingGroup = liquid.GetComponent<SortingGroup>();
        if (sortingGroup)
        {
            sortingGroup.sortingLayerName = _liquidSortingLayerName;
            sortingGroup.sortingOrder = _nextSortingLayerID;
        }
        else
        {
            liquid.sortingLayerName = _liquidSortingLayerName;
            liquid.sortingOrder = _nextSortingLayerID;
        }

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            EditorUtility.SetDirty(spriteMask);
            EditorUtility.SetDirty(liquid);
            if (sortingGroup)
                EditorUtility.SetDirty(sortingGroup);
        }
#endif

        _nextSortingLayerID++;
    }
}
}