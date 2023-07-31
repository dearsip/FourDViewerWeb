#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UI;
using UnityEngine;

[CustomEditor(typeof(StepSlider))]
public class StepSliderEditor : SliderEditor
{
    SerializedProperty step;

    protected override void OnEnable() {
        base.OnEnable();
        step = serializedObject.FindProperty("step");
    }
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        base.OnInspectorGUI();
        EditorGUILayout.PropertyField(step, new GUIContent("Step"));
        serializedObject.ApplyModifiedProperties();
    }
}
#endif