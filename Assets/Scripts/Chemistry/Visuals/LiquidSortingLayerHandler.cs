using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
namespace ChemSimDiploma.Chemistry.Visuals
{

public class LiquidSortingLayerHandler : MonoBehaviour
{
    private List<FillLevelAnimator> _fillLevelAnimators = new();
    [SerializeField] private string _liquidSortingLayerName = "Liquid";

    private int _nextSortingLayerID = 0;

    private void Start()
    {
        _fillLevelAnimators = GetComponentsInChildren<FillLevelAnimator>().ToList();
        foreach (var fillLevelAnimator in _fillLevelAnimators)
            AssignSortingLayer(fillLevelAnimator);
    }

    private void AssignSortingLayer(FillLevelAnimator fillLevelAnimator)
    {
        var spriteMask = fillLevelAnimator.GetComponent<SpriteMask>();
        spriteMask.isCustomRangeActive = true;
        spriteMask.frontSortingLayerID = SortingLayer.NameToID(_liquidSortingLayerName);
        spriteMask.frontSortingOrder = _nextSortingLayerID;
        spriteMask.backSortingLayerID = SortingLayer.NameToID(_liquidSortingLayerName);
        spriteMask.backSortingOrder = _nextSortingLayerID - 1;

        var liquid = fillLevelAnimator.LiquidRenderer;
        if (liquid.TryGetComponent<SortingGroup>(out var sortingGroup))
        {
            sortingGroup.sortingLayerName = _liquidSortingLayerName;
            sortingGroup.sortingOrder = _nextSortingLayerID;
        }
        else
        {
            liquid.sortingLayerName = _liquidSortingLayerName;
            liquid.sortingOrder = _nextSortingLayerID;
        }

        _nextSortingLayerID++;
    }
}
}
