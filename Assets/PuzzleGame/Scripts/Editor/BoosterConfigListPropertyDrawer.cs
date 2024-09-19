using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace PuzzleGame.Editor
{
    [CustomPropertyDrawer(typeof(BoosterConfigList))]
    public class BoosterConfigListPropertyDrawer : PropertyDrawer
    {
        ReorderableList reorderableList;
 
        float titleWidth = 75;
        float countWidth = 60;
        float f = 14;
    
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (reorderableList == null)
                CreateList(property);

            reorderableList.DoList(position);
        }
    
        void CreateList(SerializedProperty property)
        {
            reorderableList = new ReorderableList(
                property.serializedObject,
                property.FindPropertyRelative(nameof(BoosterConfigList.list)),
                true,
                true,
                true,
                true
            );

            reorderableList.drawHeaderCallback += rect => EditorGUI.LabelField(rect, property.displayName);

            reorderableList.drawElementCallback += (Rect rect, int index, bool isActive, bool isFocused) => 
            {
                SerializedProperty element = reorderableList.serializedProperty.GetArrayElementAtIndex(index);

                rect.y += EditorGUIUtility.standardVerticalSpacing;
            
                float height = EditorGUIUtility.singleLineHeight;
                float boosterWidth = rect.width - titleWidth - countWidth - f;

                EditorGUI.PropertyField(new Rect(rect.x, rect.y, boosterWidth, height),
                    element.FindPropertyRelative(nameof(BoosterConfig.booster)), GUIContent.none);
            
                EditorGUI.LabelField(new Rect(rect.x + boosterWidth + f, rect.y, titleWidth, height), "Start Count");
            
                EditorGUI.PropertyField(new Rect(rect.x + boosterWidth + titleWidth + f, rect.y, countWidth, height),
                    element.FindPropertyRelative(nameof(BoosterConfig.startCount)), GUIContent.none);
            };
        }
    
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (reorderableList == null)
                CreateList(property);

            return reorderableList.GetHeight();
        }
    }
}
