using Unity.Mathematics;
using UnityEngine;

[CreateAssetMenu(fileName = "NewNSpriteAnimation", menuName = "NSprites/Animation (frame sequence)")]
public class SpriteAnimation : ScriptableObject
{
    // Sprite here required because whe want to know UV of animation frame sequence on atlas
    public Sprite SpriteSheet;
    public int2 FrameCount = new(1);
    public float[] FrameDurations = new float[1] { 0.1f };

    #region Editor
#if UNITY_EDITOR
    private const float DefaultFrameDuration = .1f;

    [ContextMenu("Generate frame durations")]
    private void GenerateFrameDurationByGridSize()
    {
        var frameCount = FrameCount.x * FrameCount.y;
        var correctedFrameDurations = new float[frameCount];
        var minLength = math.min(FrameDurations.Length, correctedFrameDurations.Length);
        for (var i = 0; i < minLength; i++)
            correctedFrameDurations[i] = FrameDurations[i];
        for (var i = minLength; i < correctedFrameDurations.Length; i++)
            correctedFrameDurations[i] = DefaultFrameDuration;
        FrameDurations = correctedFrameDurations;
    }
#endif

    #endregion
}