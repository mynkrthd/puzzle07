using PuzzleGame.Gameplay;
using UnityEditor;
using UnityEngine;

namespace PuzzleGame.Editor
{
    [CustomPropertyDrawer(typeof(LastChance))]
    public class LastChancePropertyDrawer : PropertyDrawer
    {
        const float padding = 2;
        int fieldAmount = 1;
    
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Rect pos = new Rect(position);
            pos = EditorGUI.PrefixLabel(pos, GUIContent.none);
            EditorGUI.indentLevel = 0;
        
            EditorGUI.PropertyField(pos, property.FindPropertyRelative("LastChanceType"), true);

            var type = property.FindPropertyRelative("LastChanceType");
            if (type.enumValueIndex == 0)
            {
                EditorGUI.indentLevel = 1;

                fieldAmount = 2;
                Rect propPos = new Rect(pos);
                propPos.y += EditorGUIUtility.singleLineHeight + padding;
                EditorGUI.PropertyField(propPos, property.FindPropertyRelative("MaxNumber"), true);
            }
            else if (type.enumValueIndex == 2 || type.enumValueIndex == 3)
            {
                EditorGUI.indentLevel = 1;

                fieldAmount = 2;
                Rect propPos = new Rect(pos);
                propPos.y += EditorGUIUtility.singleLineHeight + padding;
                EditorGUI.PropertyField(propPos, property.FindPropertyRelative("LinesCount"), true);
            }
            else
                fieldAmount = 1;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return fieldAmount * EditorGUIUtility.singleLineHeight + padding;
        }
    }
}
