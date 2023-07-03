#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Reflection;

namespace NSprites.Editor
{
    [CustomPropertyDrawer(typeof(SortingLayerAttribute))]
    public class SortingLayerPropertyDrawer : PropertyDrawer
    {
        private const string SortingLayerFieldMethodName = "SortingLayerField";
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.Integer && property.propertyType != SerializedPropertyType.String)
                Debug.LogError("SortedLayer property should be an integer or string ( the layer id )");
            else
                SortingLayerField(new GUIContent("Sorting Layer"), property, EditorStyles.popup, EditorStyles.label);
        }

        public static void SortingLayerField(GUIContent label, SerializedProperty layerID, GUIStyle style, GUIStyle labelStyle)
        {
            var methodInfo = typeof(EditorGUILayout).GetMethod(SortingLayerFieldMethodName, BindingFlags.Static | BindingFlags.NonPublic, null, new[] { typeof(GUIContent), typeof(SerializedProperty), typeof(GUIStyle), typeof(GUIStyle) }, null);

            if (methodInfo == null)
            {
                Debug.LogWarning($"{nameof(SortingLayerPropertyDrawer)} can't find {SortingLayerFieldMethodName}.");
                return;
            }
                
            var parameters = new object[] { label, layerID, style, labelStyle };
            methodInfo.Invoke(null, parameters);
        }       
    }
}
#endif