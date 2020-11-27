using UnityEditor;
using UnityEngine;

namespace Custom.Utility
{

    [CustomEditor(typeof(BodyMapper))]
    public class BodyMapperDrawer : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            BodyMapper _target = (BodyMapper)(object)target;

            DrawTransformArray<BodyPartMap>("Body Map", nameof(_target.m_transforms));
        }

        void DrawTransformArray<T>(string label, string propertyName) where T : struct, System.IConvertible
        {
            serializedObject.Update();

            EditorGUILayout.LabelField("");
            EditorGUILayout.LabelField(label, EditorStyles.boldLabel);

            System.Array _arr = System.Enum.GetValues(typeof(T));
            int _arrSize = _arr.Length;

            SerializedProperty _property = serializedObject.FindProperty(propertyName);

            if (_property == null) return;

            _property.arraySize = _arrSize;

            char _currentChar = System.Enum.GetName(typeof(T), 0)[0];
            for (int i = 0; i < _arrSize; i++)
            {
                var _element = _property.GetArrayElementAtIndex(i);
                string _code = System.Enum.GetName(typeof(T), i);

                if (!_code[0].Equals(_currentChar))
                {
                    _currentChar = _code[0];

                    EditorGUILayout.Separator();
                }

                _element.objectReferenceValue = EditorGUILayout.ObjectField(_code, _element.objectReferenceValue, typeof(Transform), true, new GUILayoutOption[0]);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}