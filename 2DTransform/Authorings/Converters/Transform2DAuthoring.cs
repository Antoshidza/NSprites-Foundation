using Unity.Entities;
using UnityEngine;

namespace NSprites
{
    public class Transform2DAuthoring : MonoBehaviour
    {
        private class Transform2DBaker : Baker<Transform2DAuthoring>
        {
            public override void Bake(Transform2DAuthoring authoring)
                => AddComponentObject(GetEntity(TransformUsageFlags.None), new Transform2DRequest { sourceGameObject = authoring.gameObject });
        }
    }
}
