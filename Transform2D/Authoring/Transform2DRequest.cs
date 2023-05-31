using Unity.Entities;
using UnityEngine;



namespace NSprites
{
    [TemporaryBakingType]
    public class Transform2DRequest : IComponentData
    {
        public GameObject Source;
    }
}
