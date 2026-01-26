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

	public static (int u, int v) PositionToHexaCell(float radius, float x, float y)
	{
		float c_h = radius * 1.5f; // height of a hex cell
		float c_w = Mathf.Sqrt(3) * radius; // width of a hex cell

		int v = Mathf.RoundToInt(x / c_h);
		int u = Mathf.RoundToInt((y - (v % 2) * (radius * Mathf.Sqrt(3) / 2)) / c_w);
		return (u, v);
	}

	public static (float x, float y) HexaCellToPosition(float radius, int u, int v)
	{
		float c_h = radius * 1.5f; // height of a hex cell
		float c_w = Mathf.Sqrt(3) * radius; // width of a hex cell

		float x = v * c_h;
		float y = u * c_w + (v % 2) * (radius * Mathf.Sqrt(3) / 2);
		return (x, y);
	}

	public static Vector3 GetHexaCellCenter(Vector3 origin, float radius, Vector3 position, bool horizontal = true)
	{
		float p_x = position.x - origin.x;
		float p_y = position.y - origin.y;

		if (!horizontal)
			(p_x, p_y) = (p_y, p_x);

		(int u, int v) = PositionToHexaCell(radius, p_x, p_y);

		var (c_x, c_y) = HexaCellToPosition(radius, u, v);

		if (!horizontal)
			(c_x, c_y) = (c_y, c_x);

		return origin + new Vector3(c_x, c_y, 0);
	}
}