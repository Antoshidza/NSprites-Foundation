using Unity.Entities;



namespace NSprites
{
    public struct Parent2D : IComponentData
    {
        public Entity Value;
    }



    public struct PreviousParent2D : IComponentData
    {
        public Entity Value;
    }



    public struct Child2D : ICleanupBufferElementData
    {
        public Entity Value;
    }
}
