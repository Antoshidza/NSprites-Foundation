using Unity.Entities;
using UnityEngine;



namespace NSprites
{
    public class Transform2DAuthoring : MonoBehaviour
    {
        public class Transform2DAuthoringBaker : Baker<Transform2DAuthoring>
        {
            public override void Bake(Transform2DAuthoring authoring)
            {
                DependsOn(authoring.transform);
                AddComponentObject(GetEntityWithoutDependency(), new Transform2DRequest { Source = authoring.gameObject });
            }
        }
    }
}
