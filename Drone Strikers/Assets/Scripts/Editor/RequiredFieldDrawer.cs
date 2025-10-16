// RequiredFieldAttribute by adammyhre
// From: https://gist.github.com/adammyhre/a7c14b094ff2bdfb0a86df0579b4c539
// Displays information in the inspector if a field is not assigned a value

using DroneStrikers.Core.Editor;
using UnityEditor;
using UnityEngine;

namespace DroneStrikers.Editor
{
    [CustomPropertyDrawer(typeof(RequiredFieldAttribute))]
    public class RequiredFieldDrawer : PropertyDrawer
    {
        private readonly Texture2D _requiredIcon = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Textures/Editor/RequiredIcon.png");

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            EditorGUI.BeginChangeCheck();

            // If the field is required but unassigned, show the icon
            if (IsFieldUnassigned(property))
            {
                // Draw the property with space for the icon
                Rect fieldRect = new(position.x, position.y, position.width - 20f, position.height);
                EditorGUI.PropertyField(fieldRect, property, label);

                Rect iconRect = new(position.xMax - 18f, fieldRect.y, 18f, 18f);
                GUI.Label(iconRect, new GUIContent(_requiredIcon, "This field is required and is either missing or unassigned!"));
            }
            else
            {
                // Draw the property normally
                Rect fieldRect = new(position.x, position.y, position.width, position.height);
                EditorGUI.PropertyField(fieldRect, property, label);
            }

            if (EditorGUI.EndChangeCheck())
            {
                property.serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(property.serializedObject.targetObject);

                // Force a repaint of the hierarchy
                EditorApplication.RepaintHierarchyWindow();
            }

            EditorGUI.EndProperty();
        }

        private static bool IsFieldUnassigned(SerializedProperty property)
        {
            switch (property.propertyType)
            {
                case SerializedPropertyType.ObjectReference when property.objectReferenceValue:
                case SerializedPropertyType.ExposedReference when property.exposedReferenceValue:
                case SerializedPropertyType.LayerMask when property.intValue != 0:
                case SerializedPropertyType.String when !string.IsNullOrEmpty(property.stringValue):
                    return false;
                default:
                    return true;
            }
        }
    }
}