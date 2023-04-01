using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace NSprites
{
    /// <summary>
    /// Adds basic render components such as <see cref="UVAtlas"/>, <see cref="UVTilingAndOffset"/>, <see cref="Scale2D"/>, <see cref="Pivot"/>.
    /// Optionally adds sorting components, removes built-in 3D transforms and adds 2D transforms.
    /// </summary>
    public class SpriteRendererAuthoring : SpriteRendererAuthoringBase
    {
        private class Baker : Baker<SpriteRendererAuthoring>
        {
            public override void Bake(SpriteRendererAuthoring authoring)
            {
                if (!authoring.IsValid)
                    return;

                DependsOn(authoring);
                DependsOn(authoring._sprite);

                var entity = GetEntity(TransformUsageFlags.None);

                BakeSpriteRender
                (
                    this,
                    entity,
                    authoring,
                    NSpritesUtils.GetTextureST(authoring._sprite),
                    authoring._tilingAndOffset,
                    authoring._pivot,
                    authoring.VisualSize,
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

        [SerializeField] protected Sprite _sprite;
        private Sprite _lastAssignedSprite;
        [SerializeField] protected SpriteRenderData _spriteRenderData;
        [SerializeField] protected bool _overrideSpriteTexture = true;
        [SerializeField] protected float2 _pivot = new(.5f);
        [SerializeField] protected float2 _size;
        [Tooltip("Prevents changing Size when Sprite changed")][SerializeField] private bool _lockSize;
        [SerializeField] protected float4 _tilingAndOffset = new(1f, 1f, 0f, 0f);
        [SerializeField] protected bool2 _flip;
        
        [Header("Sorting")]
        [SerializeField] protected bool _disableSorting;
        [SerializeField] protected bool _staticSorting;
        [SerializeField] protected int _sortingIndex;
        [SerializeField] protected int _sortingLayer;

        public static float2 GetSpriteSize(Sprite sprite) => new(sprite.bounds.size.x, sprite.bounds.size.y);
        public virtual float2 VisualSize => _size * new float2(transform.lossyScale.x, transform.lossyScale.y);
        public float2 NativeSpriteSize => GetSpriteSize(_sprite);

        private void OnValidate()
        {
            if (_sprite != _lastAssignedSprite && _sprite != null)
            {
                _lastAssignedSprite = _sprite;

                if(!_lockSize)
                    _size = NativeSpriteSize;
            }
        }

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

        private static readonly Dictionary<Texture, Material> _overridedMaterials = new();

        protected Material GetOrCreateOverridedMaterial(Texture texture)
        {
            if (!_overridedMaterials.TryGetValue(texture, out var material))
                material = CreateOverridedMaterial(texture);
#if UNITY_EDITOR //for SubScene + domain reload
            else if (material == null)
            {
                _ = _overridedMaterials.Remove(texture);
                material = CreateOverridedMaterial(texture);
            }
#endif
            return material;
        }
        protected Material CreateOverridedMaterial(Texture texture)
        {
            var material = new Material(_spriteRenderData.Material);
            material.SetTexture("_MainTex", _sprite.texture);
            _overridedMaterials.Add(texture, material);
            return material;
        }

        protected override SpriteRenderData RenderData
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

        protected override bool IsValid
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

                return base.IsValid;
            }
        }
    }
}
