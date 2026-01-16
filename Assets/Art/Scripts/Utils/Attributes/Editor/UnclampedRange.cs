#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(UnclampedRangeAttribute))]
public class UnclampedRangeDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        UnclampedRangeAttribute range = (UnclampedRangeAttribute)attribute;

        EditorGUI.BeginProperty(position, label, property);

        // Draw label and float field
        position = EditorGUI.PrefixLabel(position, label);

        float value = property.floatValue;

        float padding = 4f; // or 2f for ultra-tight spacing
        float sliderWidth = position.width * 0.7f;
        float fieldWidth = position.width - sliderWidth - padding;

        Rect sliderRect = new(position.x, position.y, sliderWidth, position.height);
        Rect fieldRect = new(position.x + sliderWidth, position.y, fieldWidth, position.height);

        value = GUI.HorizontalSlider(sliderRect, value, range.min, range.max);
        value = EditorGUI.FloatField(fieldRect, value);

        property.floatValue = value;

        EditorGUI.EndProperty();
    }
}
#endif