using System;
using System.Collections.Generic;
using NSprites.Modules;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace NSprites
{
    /// <summary>
    /// Adds basic render components such as <see cref="UVAtlas"/>, <see cref="UVTilingAndOffset"/>, <see cref="Scale2D"/>, <see cref="Pivot"/>.
    /// Optionally adds sorting components, removes built-in 3D transforms and adds 2D transforms.
    /// </summary>
    public class SpriteRendererAuthoring : MonoBehaviour
    {
        private class Baker : Baker<SpriteRendererAuthoring>
        {
            public override void Bake(SpriteRendererAuthoring authoring)
            {
                if (!authoring.IsValid)
                    return;

                DependsOn(authoring);
                DependsOn(authoring._sprite);
                
                this.BakeSpriteBase(authoring.RenderData);

                var entity = GetEntity(TransformUsageFlags.None);

                BakeSpriteRender
                (
                    this,
                    entity,
                    authoring,
                    NSpritesUtils.GetTextureST(authoring._sprite),
                    authoring.UVTilingAndOffset,
                    authoring._pivot,
                    authoring.ScaledSize,
                    flipX: authoring._flip.x,
                    flipY: authoring._flip.y
                );
                if (!authoring._disableSorting)
                    BakeSpriteSorting
                    (
                        this,
                        entity,
                        authoring._sortingIndex,
                        authoring._sortingLayer,
                        authoring._staticSorting
                    );
            }
        }
        
        public enum DrawMode
        {
            /// <summary> Sprite will be simply stretched to it's size. </summary>
            Simple,
            /// <summary> Sprite will be tiled depending in it's size and native size (default sprite size). </summary>
            Tiled
        }

        [SerializeField] protected Sprite _sprite;
        private Sprite _lastAssignedSprite;
        [SerializeField] protected SpriteRenderData _spriteRenderData;
        [SerializeField] protected bool _overrideSpriteTexture = true;
        [SerializeField] protected float2 _pivot = new(.5f);
        [SerializeField] private float2 _size;
        [Tooltip("Prevents changing Size when Sprite changed")][SerializeField] private bool _lockSize;
        [SerializeField] protected DrawMode _drawMode;
        [SerializeField] private float4 _tilingAndOffset = new(1f, 1f, 0f, 0f);
        [SerializeField] protected bool2 _flip;
        
        [Header("Sorting")]
        [SerializeField] protected bool _disableSorting;
        [SerializeField] protected bool _staticSorting;
        [SerializeField] protected int _sortingIndex;
        [SerializeField] protected int _sortingLayer;
        [SortingLayer]
        [SerializeField] private int _unitySortingLayer;

        public static float2 GetSpriteSize(Sprite sprite) => new(sprite.bounds.size.x, sprite.bounds.size.y);
        public virtual float2 ScaledSize => _size * new float2(transform.lossyScale.x, transform.lossyScale.y);
        public float2 Size
        {
            get => _size;
            set
            {
                if(_lockSize)
                    return;
                _size = value;
            }
        }
        /// <summary> "Default" sprite size which would be a size of same sprite being placed on scene as a unity's built-in SpriteRenderer. </summary>
        public virtual float2 NativeSpriteSize => GetSpriteSize(_sprite);
        public virtual float4 UVTilingAndOffset
        {
            get => _drawMode switch
            {
                // just return default user defined Tiling&Offset from inspector
                DrawMode.Simple => _tilingAndOffset,
                // while _size of sprite can be different it's UVs stay the same - in range [(0,0) ; (1,1)]
                // so in this case we want to get ration of size to sprite NativeSize (which should be "default" sprite size depending on it's import data) and then correct Tiling part in that ratio
                DrawMode.Tiled => new(_tilingAndOffset.xy * _size / NativeSpriteSize, _tilingAndOffset.zw),
                
                _ => throw new ArgumentOutOfRangeException($"{GetType().Name}.{nameof(UVTilingAndOffset)} ({nameof(SpriteRendererAuthoring)}) {gameObject.name}: can't handle draw mode {_drawMode}")
            };
        }

        protected virtual void OnValidate()
        {
            if (_sprite != _lastAssignedSprite && _sprite != null)
            {
                _lastAssignedSprite = _sprite;

                if(!_lockSize)
                    SetNativeSize();
            }
        }

        [ContextMenu("Set Native Size")]
        private void SetNativeSize() => _size = NativeSpriteSize;
        [ContextMenu("Debug layer")]
        private void DebugLayer() => Debug.Log(UnityEngine.SortingLayer.GetLayerValueFromID(_unitySortingLayer));

        public static void BakeSpriteRender<TAuthoring>(Baker<TAuthoring> baker, in Entity entity, TAuthoring authoring, in float4 uvAtlas, in float4 uvTilingAndOffset, in float2 pivot, in float2 scale, bool flipX = false, bool flipY = false, bool add2DTransform = true)
            where TAuthoring : MonoBehaviour
        {
            if (baker == null)
            {
                Debug.LogError(new NSpritesException($"Passed baker is null"), authoring.gameObject);
                return;
            }
            if (authoring == null)
            {
                Debug.LogError(new NSpritesException($"Passed authoring object is null"), authoring.gameObject);
                return;
            }

            baker.AddComponent(entity, new UVAtlas { value = uvAtlas });
            baker.AddComponent(entity, new UVTilingAndOffset { value = uvTilingAndOffset });
            baker.AddComponent(entity, new Pivot { value = pivot });
            baker.AddComponent(entity, new Scale2D { value = scale });
            baker.AddComponent(entity, new Flip { Value = new(flipX ? -1 : 0, flipY ? -1 : 0) });

            if (add2DTransform)
            {
                baker.AddComponentObject(entity, new Transform2DRequest { sourceGameObject = authoring.gameObject });
                baker.DependsOn(authoring.transform);
            }
        }

        public static void BakeSpriteSorting<TAuthoring>(Baker<TAuthoring> baker, in Entity entity, int sortingIndex, int sortingLayer, bool staticSorting = false)
            where TAuthoring : MonoBehaviour
        {
            if (baker == null)
            {
                Debug.LogError(new NSpritesException($"Passed baker is null"));
                return;
            }

            baker.AddComponent<VisualSortingTag>(entity);
            baker.AddComponent<SortingValue>(entity);
            baker.AddComponent(entity, new SortingIndex { value = sortingIndex });
            baker.AddSharedComponent(entity, new SortingLayer { index = sortingLayer });
            if (staticSorting)
                baker.AddComponent<SortingStaticTag>(entity);
        }

        private static readonly Dictionary<Texture, Material> OverridedMaterials = new();
        private static readonly int MainTexPropertyID = Shader.PropertyToID("_MainTex");

        protected Material GetOrCreateOverridedMaterial(Texture texture)
        {
            if (!OverridedMaterials.TryGetValue(texture, out var material))
                material = CreateOverridedMaterial(texture);
#if UNITY_EDITOR //for SubScene + domain reload
            else if (material == null)
            {
                _ = OverridedMaterials.Remove(texture);
                material = CreateOverridedMaterial(texture);
            }
#endif
            return material;
        }
        protected Material CreateOverridedMaterial(Texture texture)
        {
            var material = new Material(_spriteRenderData.Material);
            material.SetTexture(MainTexPropertyID, _sprite.texture);
            OverridedMaterials.Add(texture, material);
            return material;
        }

        protected virtual SpriteRenderData RenderData
        {
            get
            {
                if (_spriteRenderData.Material == null)
                {
                    Debug.LogException(new NSpritesException($"{nameof(_spriteRenderData.Material)} is null"), gameObject);
                    return default;
                }

                if (_overrideSpriteTexture && _sprite != null)
                    // create new instance with overrided material
                    return new()
                    {
                        Material = GetOrCreateOverridedMaterial(_sprite.texture),
                        PropertiesSet = _spriteRenderData.PropertiesSet
                    };
                return _spriteRenderData;
            }
        }

        protected virtual bool IsValid
        {
            get
            {
                if (_sprite == null)
                {
                    Debug.LogWarning(new NSpritesException($"{nameof(_sprite)} is null"), gameObject);
                    return false;
                }

                if (_spriteRenderData.Material == null)
                {
                    Debug.LogWarning(new NSpritesException($"{nameof(SpriteRenderData.Material)} is null"), gameObject);
                    return false;
                }

                if (_spriteRenderData.PropertiesSet == null)
                {
                    Debug.LogWarning(new NSpritesException($"{nameof(SpriteRenderData.PropertiesSet)} is null"), gameObject);
                    return false;
                }

                return true;
            }
        }
    }
}
