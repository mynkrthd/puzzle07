using UnityEditor;
using UnityEngine;

namespace PuzzleGame.Themes.Editor
{
    [CustomPropertyDrawer(typeof(Label))]
    public class LabelPropertyDrawer : PropertyDrawer
    {
        const float Padding = 2;
        int fieldsAmount = 1;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Rect pos = new Rect(position);
            pos = EditorGUI.PrefixLabel(pos, GUIContent.none);
            EditorGUI.indentLevel = 0;

            EditorGUI.PropertyField(
                pos,
                property.FindPropertyRelative("labelType"),
                new GUIContent(property.displayName),
                true
            );
        
            if (property.FindPropertyRelative("labelType").enumValueIndex == 1)
            {
                EditorGUI.indentLevel = 1;

                fieldsAmount = 2;
                Rect propPos = new Rect(pos);
                propPos.y += EditorGUIUtility.singleLineHeight + Padding;
                EditorGUI.PropertyField(propPos, property.FindPropertyRelative("collection"), true);
            }
            else
                fieldsAmount = 1;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return fieldsAmount * EditorGUIUtility.singleLineHeight + Padding;
        }
    }
}
