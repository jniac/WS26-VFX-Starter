using UnityEngine;

public static class GeometryUtils
{
	public enum DimensionMode { Radius, SideLength }

	public static float SideLengthToRadius(int sides, float sideLength)
	{
		return sideLength / (2 * Mathf.Sin(Mathf.PI / sides));
	}

	public static float RadiusToSideLength(int sides, float radius)
	{
		return 2 * radius * Mathf.Sin(Mathf.PI / sides);
	}
}