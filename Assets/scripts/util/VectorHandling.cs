using System;
using UnityEngine;

namespace Gridsnap.Morons
{
	public abstract class VectorHandling
	{
	    public static Vector3 add(Vector3 v, float adjust)
	    {
	        v.x += adjust;
	        v.y += adjust;
	        v.z += adjust;
	        return v;
	    }

		public static Vector3 multiply(Vector3 v, Vector3 v2)
	    {
	        v.x *= v2.x;
	        v.y *= v2.y;
	        v.z *= v2.z;
	        return v;
	    }
	
		public static Vector3 findAllMax(Vector3 a, Vector3 b)
	    {
	        Vector3 ret;
	
	        ret = Vector3.zero;
	        ret.x = Mathf.Max(a.x, b.x);
	        ret.y = Mathf.Max(a.y, b.y);
	        ret.z = Mathf.Max(a.z, b.z);
	
	        return ret;
	    }
	
	    public static float flatDistance(Vector3 a, Vector3 b)
	    {
	        Vector2 flatA, flatB;
	
	        flatA = new Vector2(a.x, a.z);
	        flatB = new Vector2(b.x, b.z);
	
	        return Vector2.Distance(flatA, flatB);
	    }
	
	    public static float flatDistance(GameObject a, GameObject b)
	    {
	        return flatDistance(a.transform.position, b.transform.position);
	    }

		public static Vector3 evaluateCurve(Vector3 unit, AnimationCurve curve)
		{
			unit.x = curve.Evaluate(unit.x);
			unit.y = curve.Evaluate(unit.y);
			unit.z = curve.Evaluate(unit.z);
			return unit;
		}

		/// <summary>
		///		Returns a 2d vector who uses target.x/target.z as x/y.
		///		Primarily useful for rects.
		/// <summary>
		public static Vector2 rectPosition(Vector3 target)
		{
			return new Vector2(target.x, target.z);
		}

		/// <summary>
		///		Translates a real-world size into a rect-compatible size.
		/// </summary>
		public static Vector2 rectSize(Vector3 size)
		{
			return new Vector2(size.x, size.z);
		}

		/// <summary>
		///		Takes a real-world position and size, translates it into a Rect whose x/y represent position, and whose sizes semantically match the Rect interface.
		/// </summary>
		public static Rect rect(Vector3 position, Vector3 size)
		{
			return new Rect(rectPosition(position), rectSize(size));
		}

		public static bool approximately(Vector3 a, Vector3 b)
		{
			return 
				Mathf.Approximately(a.x, b.x) &&
				Mathf.Approximately(a.y, b.y) &&
				Mathf.Approximately(a.z, b.z);
		}

		public static Vector2 convXY(Vector3 vector)
		{
			return new Vector2(vector.x, vector.y);
		}

		public static Vector3 gridsnap(Vector3 vector, float gridSize)
		{
			vector.x = Mathf.RoundToInt(vector.x / gridSize) * gridSize;
			vector.y = Mathf.RoundToInt(vector.y / gridSize) * gridSize;
			vector.z = Mathf.RoundToInt(vector.z / gridSize) * gridSize;

			return vector;
		}

		public static Vector2 gridsnap(Vector2 vector, float gridSize)
		{
			vector.x = Mathf.RoundToInt(vector.x / gridSize) * gridSize;
			vector.y = Mathf.RoundToInt(vector.y / gridSize) * gridSize;

			return vector;
		}

		public static float angleBetween(Vector3 a, Vector3 b)
		{
			return Mathf.Atan2(a.y - b.y, a.x - b.x) * Mathf.Rad2Deg + 90f;
		}

		public static float cardinalAngleBetween(Vector3 a, Vector3 b)
		{
			float angle;

			angle = angleBetween(a, b);

			if(angle >= -45f && angle <= 45f)
				return 0f;
			
			if(angle >= 45f && angle <= 135f)
				return 90f;
			
			if(angle >= -135f && angle <= -45f)
				return -90f;

			return 180f;
		}

		public static bool lineSide(Vector2 point, Vector2 lineStart, Vector2 lineEnd)
		{
			return ((lineEnd.x - lineStart.x)*(point.y - lineStart.y) - (lineEnd.y - lineStart.y)*(point.x - lineStart.x)) > 0;
		}

		public static float lineSegmentDistance(Vector2 p, Vector2 v, Vector2 w)
		{
			Vector2 projection;
			float length, t;
			
			length = Mathf.Pow((w - v).magnitude, 2);  // i.e. |w-v|^2 -  avoid a sqrt

			if (Mathf.Approximately(length, 0f)) 
				return Vector3.Distance(p, v);

			// Consider the line extending the segment, parameterized as v + t (w - v).
			// We find projection of point p onto the line. 
			// It falls where t = [(p-v) . (w-v)] / |w-v|^2
			// We clamp t from [0,1] to handle points outside the segment vw.
			t = Mathf.Max(0, Mathf.Min(1, Vector2.Dot(p - v, w - v) / length));

			projection = v + t * (w - v);
			return Vector2.Distance(p, projection);
		}

		public static Vector2 findPointAroundRect(Vector2 origin, Rect rect)
		{
			Vector2 tl, tr, br, bl;
			Vector2 diff, ret;

			tl = new Vector2(rect.xMin, rect.yMin);
			tr = new Vector2(rect.xMax, rect.yMin);
			br = new Vector2(rect.xMax, rect.yMax);
			bl = new Vector2(rect.xMin, rect.yMax);

			diff = origin - rect.center;
			ret = new Vector2(100, 100);

			// somewhere right
			if(diff.x > 0)
			{
				// try dead right
				if(lineIntersect(origin, rect.center, tr, br, ref ret))
					return ret;

				// top?
				if(diff.y <= 0)
				{
					if(lineIntersect(origin, rect.center, tl, tr, ref ret))
						return ret;
				}

				// bottom.
				lineIntersect(origin, rect.center, bl, br, ref ret);
				return ret;
			}

			// somewhere left
			if(lineIntersect(origin, rect.center, tl, bl, ref ret))
				return ret;
			
			// top?
			if(diff.y <= 0)
			{
				if(lineIntersect(origin, rect.center, tl, tr, ref ret))
					return ret;
			}

			// bottom.
			lineIntersect(origin, rect.center, bl, br, ref ret);
			return ret;			
		}

		// This horrible code stolen from https://forum.unity3d.com/threads/line-intersection.17384/#post-967202
		public static bool lineIntersect(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, ref Vector2 intersection)
		{
			float Ax, Bx, Cx, Ay, By, Cy, d, e, f, num/*,offset*/;
			float x1lo, x1hi, y1lo, y1hi;

			Ax = p2.x - p1.x;
			Bx = p3.x - p4.x;

			// X bound box test/
			if (Ax < 0)
			{
				x1lo = p2.x; x1hi = p1.x;
			}
			else
			{
				x1hi = p2.x; x1lo = p1.x;
			}

			if (Bx > 0)
			{
				if (x1hi < p4.x || p3.x < x1lo) return false;
			}
			else
			{
				if (x1hi < p3.x || p4.x < x1lo) return false;
			}

			Ay = p2.y - p1.y;
			By = p3.y - p4.y;

			// Y bound box test//
			if (Ay < 0)
			{
				y1lo = p2.y; y1hi = p1.y;
			}
			else
			{
				y1hi = p2.y; y1lo = p1.y;
			}

			if (By > 0)
			{
				if (y1hi < p4.y || p3.y < y1lo) return false;
			}
			else
			{
				if (y1hi < p3.y || p4.y < y1lo) return false;
			}

			Cx = p1.x - p3.x;
			Cy = p1.y - p3.y;
			d = By * Cx - Bx * Cy;  // alpha numerator//
			f = Ay * Bx - Ax * By;  // both denominator//

			// alpha tests//
			if (f > 0)
			{
				if (d < 0 || d > f) return false;
			}
			else
			{
				if (d > 0 || d < f) return false;
			}

			e = Ax * Cy - Ay * Cx;  // beta numerator//
			// beta tests //

			if (f > 0)
			{
				if (e < 0 || e > f) return false;
			}
			else
			{
				if (e > 0 || e < f) return false;
			}

			// check if they are parallel
			if (f == 0) return false;

			// compute intersection coordinates //
			num = d * Ax; // numerator //

			//    offset = same_sign(num,f) ? f*0.5f : -f*0.5f;   // round direction //
			//    intersection.x = p1.x + (num+offset) / f;
			intersection.x = p1.x + num / f;
			num = d * Ay;

			//    offset = same_sign(num,f) ? f*0.5f : -f*0.5f;
			//    intersection.y = p1.y + (num+offset) / f;
			intersection.y = p1.y + num / f;
			return true;
		}

	}
}
