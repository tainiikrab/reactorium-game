#if UNITY_EDITOR
using ChemSimDiploma.Tasks.Data;
using UnityEditor;
using UnityEngine;

namespace ChemSimDiploma.Tasks.Editor
{
[CustomPropertyDrawer(typeof(LevelTaskEntry))]
public class LevelTaskEntryDrawer : PropertyDrawer
{
    private const float VerticalSpacing = 2f;
    private const float SectionPadding = 6f;

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (!property.isExpanded)
            return EditorGUIUtility.singleLineHeight;

        float height = EditorGUIUtility.singleLineHeight + VerticalSpacing;
        height += GetFieldBlockHeight(property, "_taskId");
        height += GetFieldBlockHeight(property, "_description");
        height += GetFieldBlockHeight(property, "_type");
        height += VerticalSpacing + GetConditionSectionHeight(property);
        return height + VerticalSpacing;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        string header = BuildHeader(property, label);
        var foldoutRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, header, true);

        if (!property.isExpanded)
        {
            EditorGUI.EndProperty();
            return;
        }

        EditorGUI.indentLevel++;
        float y = position.y + EditorGUIUtility.singleLineHeight + VerticalSpacing;

        y = DrawProperty(ref position, property, "_taskId", y);
        y = DrawProperty(ref position, property, "_description", y);
        y = DrawProperty(ref position, property, "_type", y);

        SerializedProperty typeProp = property.FindPropertyRelative("_type");
        var taskType = (LevelTaskType)typeProp.enumValueIndex;

        y += VerticalSpacing;
        y = DrawConditionSection(position, property, taskType, y);

        EditorGUI.indentLevel--;
        EditorGUI.EndProperty();
    }

    private static string BuildHeader(SerializedProperty property, GUIContent label)
    {
        SerializedProperty taskId = property.FindPropertyRelative("_taskId");
        SerializedProperty description = property.FindPropertyRelative("_description");
        SerializedProperty type = property.FindPropertyRelative("_type");

        string id = string.IsNullOrWhiteSpace(taskId.stringValue) ? "—" : taskId.stringValue;
        string typeLabel = ((LevelTaskType)type.enumValueIndex).ToString();

        if (!string.IsNullOrWhiteSpace(description.stringValue))
        {
            string shortDesc = description.stringValue.Length > 36
                ? description.stringValue.Substring(0, 36) + "…"
                : description.stringValue;
            return $"{label.text}  ·  {id}  ·  {typeLabel}  —  {shortDesc}";
        }

        return $"{label.text}  ·  {id}  ·  {typeLabel}";
    }

    private static float DrawProperty(ref Rect position, SerializedProperty parent, string relativeName, float y)
    {
        SerializedProperty prop = parent.FindPropertyRelative(relativeName);
        float height = EditorGUI.GetPropertyHeight(prop, true);
        var rect = new Rect(position.x, y, position.width, height);
        EditorGUI.PropertyField(rect, prop, true);
        return y + height + VerticalSpacing;
    }

    private static float GetFieldBlockHeight(SerializedProperty parent, string relativeName)
    {
        SerializedProperty prop = parent.FindPropertyRelative(relativeName);
        return EditorGUI.GetPropertyHeight(prop, true) + VerticalSpacing;
    }

    private static float GetConditionSectionHeight(SerializedProperty property)
    {
        SerializedProperty typeProp = property.FindPropertyRelative("_type");
        var taskType = (LevelTaskType)typeProp.enumValueIndex;
        SerializedProperty conditionProp = GetConditionProperty(property, taskType);

        float inner = taskType == LevelTaskType.TakeIndicatorFromBox
            ? EditorGUIUtility.singleLineHeight * 2f
            : conditionProp != null
                ? EditorGUI.GetPropertyHeight(conditionProp, true)
                : EditorGUIUtility.singleLineHeight;

        return EditorGUIUtility.singleLineHeight + SectionPadding * 2f + inner + VerticalSpacing;
    }

    private static float DrawConditionSection(Rect position, SerializedProperty property, LevelTaskType taskType,
        float y)
    {
        string sectionTitle = taskType switch
        {
            LevelTaskType.MixAcidAndBase => "Условие: смешивание",
            LevelTaskType.ContainerHasLiquid => "Условие: раствор в колбе",
            LevelTaskType.IndicatorPhInRange => "Условие: pH индикатора",
            LevelTaskType.TakeIndicatorFromBox => "Условие: палочка из коробки",
            _ => "Условие"
        };

        SerializedProperty conditionProp = GetConditionProperty(property, taskType);
        float innerHeight = taskType == LevelTaskType.TakeIndicatorFromBox
            ? EditorGUIUtility.singleLineHeight * 2f
            : conditionProp != null
                ? EditorGUI.GetPropertyHeight(conditionProp, true)
                : EditorGUIUtility.singleLineHeight;

        float boxHeight = EditorGUIUtility.singleLineHeight + SectionPadding * 2f + innerHeight;
        var boxRect = new Rect(position.x, y, position.width, boxHeight);

        if (Event.current.type == EventType.Repaint)
        {
            GUIStyle boxStyle = EditorStyles.helpBox;
            boxStyle.Draw(boxRect, false, false, false, false);
        }

        var titleRect = new Rect(
            boxRect.x + SectionPadding,
            boxRect.y + SectionPadding * 0.5f,
            boxRect.width - SectionPadding * 2f,
            EditorGUIUtility.singleLineHeight);

        EditorGUI.LabelField(titleRect, sectionTitle, EditorStyles.boldLabel);

        var fieldRect = new Rect(
            boxRect.x + SectionPadding,
            titleRect.yMax + VerticalSpacing,
            boxRect.width - SectionPadding * 2f,
            innerHeight);

        if (taskType == LevelTaskType.TakeIndicatorFromBox)
        {
            EditorGUI.LabelField(
                fieldRect,
                "Выполняется при успешном IndicatorBoxController.TrySpawnStick (тап по коробке).",
                EditorStyles.wordWrappedMiniLabel);
        }
        else if (conditionProp != null)
        {
            EditorGUI.PropertyField(fieldRect, conditionProp, GUIContent.none, true);
        }

        return y + boxHeight;
    }

    private static SerializedProperty GetConditionProperty(SerializedProperty property, LevelTaskType taskType)
    {
        return taskType switch
        {
            LevelTaskType.MixAcidAndBase => property.FindPropertyRelative("_mixAcidBase"),
            LevelTaskType.ContainerHasLiquid => property.FindPropertyRelative("_hasLiquid"),
            LevelTaskType.IndicatorPhInRange => property.FindPropertyRelative("_indicatorPh"),
            LevelTaskType.TakeIndicatorFromBox => null,
            _ => null
        };
    }
}
}
#endif