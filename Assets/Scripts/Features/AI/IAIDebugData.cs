
using UnityEngine;

namespace Features.AI
{
    public interface IAIDebugData
    {
        bool HasPlayer { get; }
        bool InEngage  { get; }
        Vector2 SeekTarget { get; }
        float TargetHeadingRadians { get; }
    }
}