using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DroneStrikers.Core.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DroneStrikers.Editor
{
    [InitializeOnLoad]
    public static class HierarchyIconDrawer
    {
        private static readonly Texture2D RequiredIcon = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Textures/Editor/RequiredIcon.png");

        private static readonly Dictionary<Type, FieldInfo[]> CachedFieldInfo = new();

        static HierarchyIconDrawer() => EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyWindowItemOnGUI;

        private static void OnHierarchyWindowItemOnGUI(int instanceID, Rect selectionRect)
        {
            if (EditorUtility.InstanceIDToObject(instanceID) is not GameObject gameObject) return;

            foreach (Component component in gameObject.GetComponents<Component>())
            {
                if (component == null) continue;

                FieldInfo[] fields = GetCachedFieldsWithRequiredAttribute(component.GetType());
                if (fields == null) continue;

                if (fields.Any(field => IsFieldUnassigned(field.GetValue(component))))
                {
                    Rect iconRect = new(selectionRect.xMax - 20f, selectionRect.y, 18f, 18f);
                    GUI.Label(iconRect, new GUIContent(RequiredIcon, "One or more required fields are missing or unassigned."));
                    break;
                }
            }
        }

        private static FieldInfo[] GetCachedFieldsWithRequiredAttribute(Type componentType)
        {
            if (!CachedFieldInfo.TryGetValue(componentType, out FieldInfo[] fields))
            {
                fields = componentType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                List<FieldInfo> requiredFields = new();

                foreach (FieldInfo field in fields)
                {
                    bool isSerialized = field.IsPublic || field.IsDefined(typeof(SerializeField), false);
                    bool isRequired = field.IsDefined(typeof(RequiredFieldAttribute), false);

                    if (isSerialized && isRequired) requiredFields.Add(field);
                }

                fields = requiredFields.ToArray();
                CachedFieldInfo[componentType] = fields;
            }

            return fields;
        }

        private static bool IsFieldUnassigned(object fieldValue)
        {
            // Check for null or empty values
            if (fieldValue == null) return true;
            if (fieldValue.Equals(null)) return true;

            // Type-specific
            switch (fieldValue)
            {
                case string stringValue when string.IsNullOrEmpty(stringValue):
                case LayerMask { value: 0 }:
                    return true;
                case IEnumerable enumerable:
                    return enumerable.Cast<object>().Any(item => item == null);
                case InputActionReference ar when ar.Equals(null):
                    return true;
                default:
                    return false;
            }
        }
    }
}