using NSprites.Authoring;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace NSprites.Editor
{
    [CustomEditor(typeof(SpriteRendererAuthoring))]
    [CanEditMultipleObjects]
    public class SpriteRendererAuthoringEditor : UnityEditor.Editor
    {
        private const string SpriteFieldName = "PropertyField:Sprite";

        public override VisualElement CreateInspectorGUI()
        {
            var authoring = (SpriteRendererAuthoring)target;
            var root = new VisualElement();
            InspectorElement.FillDefaultInspector(root, serializedObject, this);
            
            // call configure logic on next frame, because nothing created yet
            root.RegisterCallback<GeometryChangedEvent>(_ => ConfigureInspector(root, authoring));

            return root;
        }

        private static void ConfigureInspector(VisualElement root, SpriteRendererAuthoring authoring)
        {
            var spriteField = root.Q<PropertyField>(SpriteFieldName);
            if (spriteField == null)
            {
                Debug.LogException(new NSpritesException($"{nameof(SpriteRendererAuthoringEditor)} can't find field named {SpriteFieldName}, which supposed to be {nameof(Sprite)} field"));
                return;
            }
                
            spriteField.RegisterValueChangeCallback(_ => authoring.OnSpriteChanged());
        }
    }
}