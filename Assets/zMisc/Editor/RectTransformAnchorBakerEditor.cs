using UnityEngine;
using UnityEditor;

public static class RectTransformAnchorBakerEditor
{
    [MenuItem("CONTEXT/RectTransform/Bake Anchors (Keep Visual Position)")]
    private static void Bake(MenuCommand command)
    {
        var target = command.context as RectTransform;
        if (target == null)
            return;

        var parent = target.parent as RectTransform;
        if (parent == null)
            return;

        Undo.RecordObject(target, "Bake Anchors");

        Rect parentRect = parent.rect;
        var localPos = (Vector2)parent.InverseTransformPoint(target.position);

        var normalized = new Vector2(
            parentRect.width != 0 ? localPos.x / parentRect.width + 0.5f : 0.5f,
            parentRect.height != 0 ? localPos.y / parentRect.height + 0.5f : 0.5f
        );

        target.anchorMin = normalized;
        target.anchorMax = normalized;
        target.anchoredPosition = Vector2.zero;

        EditorUtility.SetDirty(target);
    }
}