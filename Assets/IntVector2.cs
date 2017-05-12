using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct IntVector2
{
	public int x;
	public int y;

	public IntVector2 (int _x, int _y) {
		x = _x;
		y = _y;
	}

	public Vector2 toFloat() {
		return new Vector2 (x, y);
	}

	public static bool operator ==(IntVector2 c1, IntVector2 c2) {
		return (c1.x == c2.x && c1.y == c2.y);
	}

	public static bool operator !=(IntVector2 c1, IntVector2 c2) {
		return (c1.x != c2.x || c1.y != c2.y);
	}

	public static IntVector2 operator *(IntVector2 c1, IntVector2 c2) {
		return new IntVector2(c1.x * c2.x, c1.x * c2.y);
	}

	public static Vector2 operator *(Vector2 c1, IntVector2 c2) {
		return new Vector2(c1.x * c2.x, c1.y * c2.y);
	}

	public static IntVector2 operator -(IntVector2 c1, IntVector2 c2) {
		return new IntVector2(c1.x - c2.x, c1.y - c2.y);
	}

	public static IntVector2 operator +(IntVector2 c1, IntVector2 c2) {
		return new IntVector2(c1.x + c2.x, c1.x + c2.y);
	}

	public static Vector2 operator +(Vector2 c1, IntVector2 c2) {
		return new Vector2(c1.x + c2.x, c1.x + c2.y);
	}

	public static Vector2 operator /(IntVector2 c1, float c2)  {
		return new Vector2 (c1.x * 1.0f / c2, c1.x * 1.0f / c2);
	}
}