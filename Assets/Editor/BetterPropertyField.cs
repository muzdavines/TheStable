using UnityEngine;
using UnityEditor;
using System.Collections;

public static class BetterPropertyField {
	/// <summary>
	/// Draws a serialized property (including children) fully, even if it's an instance of a custom serializable class.
	/// Supersedes EditorGUILayout.PropertyField(serializedProperty, true);
	/// </summary>
	/// <param name="_serializedProperty">Serialized property.</param>
	public static void DrawSerializedProperty (SerializedProperty _serializedProperty) {
		if(_serializedProperty == null) {
			EditorGUILayout.HelpBox("SerializedProperty was null!", MessageType.Error);
			return;
		}
		var serializedProperty = _serializedProperty.Copy();
		int startingDepth = serializedProperty.depth;
		EditorGUI.indentLevel = serializedProperty.depth;
		DrawPropertyField(serializedProperty);
		while (serializedProperty.NextVisible(serializedProperty.isExpanded && !PropertyTypeHasDefaultCustomDrawer(serializedProperty.propertyType)) && serializedProperty.depth > startingDepth) {
			EditorGUI.indentLevel = serializedProperty.depth;
			DrawPropertyField(serializedProperty);
		}
	}

	static void DrawPropertyField (SerializedProperty serializedProperty) {
		if(serializedProperty.propertyType == SerializedPropertyType.Generic) {
			serializedProperty.isExpanded = EditorGUILayout.Foldout(serializedProperty.isExpanded, serializedProperty.displayName, true);
		} else {
			EditorGUILayout.PropertyField(serializedProperty);
		}
	}

	static bool PropertyTypeHasDefaultCustomDrawer(SerializedPropertyType type) {
		return 
		type == SerializedPropertyType.AnimationCurve ||
		type == SerializedPropertyType.Bounds || 
		type == SerializedPropertyType.Color || 
		type == SerializedPropertyType.Gradient ||
		type == SerializedPropertyType.LayerMask ||
		type == SerializedPropertyType.ObjectReference || 
		type == SerializedPropertyType.Rect || 
		type == SerializedPropertyType.Vector2 || 
		type == SerializedPropertyType.Vector3;
	}
}