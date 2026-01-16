using UnityEngine;

public class UnclampedRangeAttribute : PropertyAttribute
{
    public float min;
    public float max;

    public UnclampedRangeAttribute(float min, float max)
    {
        this.min = min;
        this.max = max;
    }
}
