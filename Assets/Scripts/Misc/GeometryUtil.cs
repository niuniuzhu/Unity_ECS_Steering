using System;
using System.Collections.Generic;
using Unity.Mathematics;

namespace Steering
{
	/// <summary>
	/// A collection of helper methods for 2D geometry.
	/// </summary>
	public static class GeometryUtil
	{
		/// <summary>
		/// given a plane and a ray this function determines how far along the ray
		/// an intersection occurs. Returns negative if the ray is parallel.
		/// </summary>
		/// <param name="rayOrigin">The origin point of the ray.</param>
		/// <param name="rayHeading">The heading of the ray.</param>
		/// <param name="planePoint">A point on the plane.</param>
		/// <param name="planeNormal">The normal vector of the plane.</param>
		/// <returns>The distance along the ray at which intersection occurs, or -1 if the ray is parallel.</returns>
		/// <remarks>TODO: bad behaviour. -1 is ambiguous. nullable return type?</remarks>
		public static float RayPlaneIntersectionDistance(
			float2 rayOrigin,
			float2 rayHeading,
			float2 planePoint,  // any point on the plane
			float2 planeNormal )
		{
			var d = -math.dot( planeNormal, planePoint );
			var numer = math.dot( planeNormal, rayOrigin ) + d;
			var denom = math.dot( planeNormal, rayHeading );

			if ( Math.Abs( denom ) == 0 )
				return -1.0f;

			return -numer / denom;
		}

		/// <summary>
		/// Determines where a point is in relation to a plane in two-dimensional space.
		/// </summary>
		/// <param name="point">The point.</param>
		/// <param name="planePoint">Any point on the plane.</param>
		/// <param name="planeNormal">The normal vector of the plane.</param>
		/// <returns>1 if the point is in the direction of the normal from the plane, 0 if it lies on the plane, -1 otherwise.</returns>
		public static int WhereIsPoint( float2 point, float2 planePoint, float2 planeNormal )
		{
			var d = math.dot( point - planePoint, planeNormal );

			if ( d > 0 )
				return 1;
			else if ( d < 0 )
				return -1;

			return 0;
		}

		// Nonsense..
		/*
        public static float GetRayCircleIntersect(
            float2 rayOrigin,
            float2 rayHeading,
            float2 circleOrigin,
            float radius)
        {
            float2 toCircle = circleOrigin - rayOrigin;
            float v = toCircle.Dot(rayHeading);
            float d = radius * radius - (toCircle.sqrMagnitude - v * v);

            // If there was no intersection, return -1
            if (d < 0.0) return (-1.0);

            // Return the distance to the [first] intersecting point
            return (v - Math.Sqrt(d));
        }

        // Also nonsense..
        public static bool DoRayCircleIntersect(
            float2 RayOrigin,
            float2 RayHeading,
            float2 CircleOrigin,
            float radius)
        {
            float2 ToCircle = CircleOrigin - RayOrigin;
            float length = ToCircle.Length;
            float v = ToCircle.Dot(RayHeading);
            float d = radius * radius - (length * length - v * v);

            // If there was no intersection, return -1
            return (d < 0.0);
        }
        */

		/// <summary>
		/// Given a point P and a circle of radius R centered at C this function
		/// determines the two points on the circle that intersect with the
		/// tangents from P to the circle. Returns false if P is within the circle.
		/// thanks to Dave Eberly for this one.
		/// </summary>
		/// <param name="circleOrigin">The centre of the circle.</param>
		/// <param name="circleRadius">The radius of the circle.</param>
		/// <param name="point">The point.</param>
		/// <param name="t1">The first tangent point.</param>
		/// <param name="t2">The second tangent point.</param>
		/// <returns>True if two tangent points exist, false if the point is inside or on the circle.</returns>
		public static bool GetTangentPoints(
			float2 circleOrigin,
			float circleRadius,
			float2 point,
			out float2 t1,
			out float2 t2 )
		{
			var toPoint = point - circleOrigin;
			var squaredLength = math.lengthsq( toPoint );
			var squaredRadius = circleRadius * circleRadius;

			if ( squaredLength <= squaredRadius )
			{
				t1 = t2 = float2.zero;
				return false; // P is inside or on the circle
			}

			var invSqrLen = 1 / squaredLength;
			var root = ( float )Math.Sqrt( Math.Abs( squaredLength - squaredRadius ) );

			t1 = new float2(
				circleOrigin.x + circleRadius * ( circleRadius * toPoint.x - toPoint.y * root ) * invSqrLen,
				circleOrigin.y + circleRadius * ( circleRadius * toPoint.y + toPoint.x * root ) * invSqrLen );

			t2 = new float2(
				circleOrigin.x + circleRadius * ( circleRadius * toPoint.x + toPoint.y * root ) * invSqrLen,
				circleOrigin.y + circleRadius * ( circleRadius * toPoint.y - toPoint.x * root ) * invSqrLen );

			return true;
		}

		/// <summary>
		/// Calculates the closest distance between a line segment AB and a point P.
		/// </summary>
		/// <param name="a">One end of line segment.</param>
		/// <param name="b">Other end of line segment.</param>
		/// <param name="p">The point.</param>
		/// <returns>The closest distance between the line segment and the point.</returns>
		public static float DistanceToLineSegment( float2 a, float2 b, float2 p )
		{
			// If the angle is obtuse between AP and AB is obtuse then the closest vertex must be A.
			var dotA = math.dot( p - a, b - a );
			if ( dotA <= 0 ) { return math.distance( a, p ); }

			// If the angle is obtuse between BP and BA is obtuse then the closest vertex must be B.
			var dotB = math.dot( p - b, a - b );
			if ( dotB <= 0 ) { return math.distance( b, p ); }

			// Otherwise, calculate the point along AB that is the closest to P
			var closestPoint = a + ( b - a ) * dotA / ( dotA + dotB );
			return math.distance( p, closestPoint );
		}

		/// <summary>
		/// Calculates the square of (thus avoiding square root calculation) the closest distance between a line segment AB and a point P.
		/// </summary>
		/// <param name="a">One end of line segment.</param>
		/// <param name="b">Other end of line segment.</param>
		/// <param name="p">The point.</param>
		/// <returns>The square of the closest distance between the line segment and the point.</returns>
		public static float DistanceToLineSegmentSquared( float2 a, float2 b, float2 p )
		{
			// If the angle is obtuse between AP and AB is obtuse then the closest vertex must be A
			var dotA = math.dot( p - a, b - a );
			if ( dotA <= 0 ) { return math.lengthsq( a - p ); }

			// If the angle is obtuse between BP and BA is obtuse then the closest vertex must be B
			var dotB = math.dot( p - b, a - b );
			if ( dotB <= 0 ) { return math.lengthsq( b - p ); }

			// Otherwise, calculate the point along AB that is the closest to P
			var closestPoint = a + ( b - a ) * dotA / ( dotA + dotB );
			return math.lengthsq( p - closestPoint );
		}

		/// <summary>
		/// Returns true if two line segments AB and CD intersect.
		/// </summary>
		/// <param name="a">One end of the first line segment.</param>
		/// <param name="b">Other end of first line segment.</param>
		/// <param name="c">One end of the second line segment.</param>
		/// <param name="d">Other end of second line segment.</param>
		/// <returns>True if the two segments intersect, otherwise false.</returns>
		public static bool LineSegmentIntersectionExists( float2 a, float2 b, float2 c, float2 d )
		{
			var rTop = ( a.y - c.y ) * ( d.x - c.x ) - ( a.x - c.x ) * ( d.y - c.y );
			var sTop = ( a.y - c.y ) * ( b.x - a.x ) - ( a.x - c.x ) * ( b.y - a.y );
			var bot = ( b.x - a.x ) * ( d.y - c.y ) - ( b.y - a.y ) * ( d.x - c.x );

			if ( bot == 0 )
			{
				// The segments are parallel
				return false;
			}

			var invBot = 1.0f / bot;
			var r = rTop * invBot;
			var s = sTop * invBot;

			if ( r > 0 && r < 1 && s > 0 && s < 1 )
			{
				// The segments intersect
				return true;
			}

			// The segments do not intersect
			return false;
		}

		/// <summary>
		/// Returns the distance from A at which a line segment AB intersects with a line segment CD, or -1 if the lines to not intersect.
		/// </summary>
		/// <param name="a">One end of the first line segment.</param>
		/// <param name="b">Other end of first line segment.</param>
		/// <param name="c">One end of the second line segment.</param>
		/// <param name="d">Other end of second line segment.</param>
		/// <returns>The distance from a at which the intersection occurs.</returns>
		public static float LineSegmentIntersectionDistance( float2 a, float2 b, float2 c, float2 d )
		{
			var rTop = ( a.y - c.y ) * ( d.x - c.x ) - ( a.x - c.x ) * ( d.y - c.y );
			var sTop = ( a.y - c.y ) * ( b.x - a.x ) - ( a.x - c.x ) * ( b.y - a.y );
			var bot = ( b.x - a.x ) * ( d.y - c.y ) - ( b.y - a.y ) * ( d.x - c.x );

			if ( bot == 0 )
			{
				// parallel
				if ( rTop == 0 && sTop == 0 )
					return 0;

				return -1;
			}

			var r = rTop / bot;
			var s = sTop / bot;

			if ( r > 0 && r < 1 && s > 0 && s < 1 )
				return math.distance( a, b ) * r;
			else
				return -1;
		}

		/// <summary>
		/// Finds the point of intersection of two line segments, if it exists.
		/// </summary>
		/// <param name="a">One end of the first line segment.</param>
		/// <param name="b">Other end of first line segment.</param>
		/// <param name="c">One end of the second line segment.</param>
		/// <param name="d">Other end of second line segment.</param>
		/// <returns>True if the line segment ab intersects cd.</returns>
		public static bool LineSegmentIntersectionPoint( float2 a, float2 b, float2 c, float2 d, out float2 result )
		{
			var aycy = a.y - c.y;
			var dxcx = d.x - c.x;
			var axcx = a.x - c.x;
			var dycy = d.y - c.y;
			var bxax = b.x - a.x;
			var byay = b.y - a.y;

			var rTop = aycy * dxcx - axcx * dycy;
			var rBot = bxax * dycy - byay * dxcx;
			var sTop = aycy * bxax - axcx * byay;
			var sBot = bxax * dycy - byay * dxcx;

			if ( rBot == 0 || sBot == 0 )
			{
				// lines are parallel
				result = float2.zero;
				return false;
			}

			var r = rTop / rBot;
			var s = sTop / sBot;

			if ( r >= 0 && r <= 1 && s >= 0 && s <= 1 )
			{
				result = a + r * ( b - a );
				return true;
			}
			else
			{
				result = float2.zero;
				return false;
			}
		}

		/// <summary>
		/// Tests two polygons for intersection. *Does not check for enclosure*.
		/// </summary>
		/// <param name="object1">The first polygon.</param>
		/// <param name="object2">The second polygon.</param>
		/// <returns>True if the two polygons intersect, otherwise false.</returns>
		public static bool ObjectIntersectionExists( IList<float2> object1, IList<float2> object2 )
		{
			for ( var i = 0; i < object1.Count - 1; i++ )
			{
				for ( var j = 0; j < object2.Count - 1; j++ )
				{
					if ( LineSegmentIntersectionExists( object2[j], object2[j + 1], object1[i], object1[i + 1] ) )
						return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Tests a line segment against a polygon for intersection. *Does not check for enclosure*.
		/// </summary>
		/// <param name="a">One end of line segment.</param>
		/// <param name="b">Other end of line segment.</param>
		/// <param name="object">The polygon.</param>
		/// <returns>True if the line segment and polygon intersect, otherwise false.</returns>
		public static bool ObjectIntersectionExists( float2 a, float2 b, IList<float2> @object )
		{
			for ( var i = 0; i < @object.Count - 1; i++ )
			{
				if ( LineSegmentIntersectionExists( a, b, @object[i], @object[i + 1] ) )
					return true;
			}

			return false;
		}

		/// <summary>
		/// Checks whether two circles overlap.
		/// </summary>
		/// <param name="c1">The centre of the first circle.</param>
		/// <param name="r1">The radius of the first circle.</param>
		/// <param name="c2">The centre of the second circle.</param>
		/// <param name="r2">The radius of the second circle.</param>
		/// <returns>True if the two circles overlap (but not if they touch at precisely one point), otherwise false.</returns>
		public static bool TwoCirclesOverlapped( float2 c1, float r1, float2 c2, float r2 ) =>
			math.lengthsq( c1 - c2 ) < ( r1 + r2 ) * ( r1 + r2 );

		/// <summary>
		/// returns true if one circle encloses the other.
		/// </summary>
		/// <param name="c1">The centre of the first circle.</param>
		/// <param name="r1">The radius of the first circle.</param>
		/// <param name="c2">The centre of the second circle.</param>
		/// <param name="r2">The radius of the second circle.</param>
		/// <returns>True if one circle encloses the other.</returns>
		public static bool TwoCirclesEnclosed( float2 c1, float r1, float2 c2, float r2 ) =>
			math.lengthsq( c1 - c2 ) < ( r1 - r2 ) * ( r1 - r2 );

		/// <summary>
		/// Given two circles this function calculates the intersection points of any overlap.
		/// returns false if no overlap found. see http://astronomy.swin.edu.au/~pbourke/geometry/2circle/
		/// for an explanation.
		/// </summary>
		/// <param name="c1">The centre of the first circle.</param>
		/// <param name="r1">The radius of the first circle.</param>
		/// <param name="c2">The centre of the second circle.</param>
		/// <param name="r2">The radius of the second circle.</param>
		/// <returns>A tuple containing the two intersection points or null.</returns>
		public static (float2? p1, float2? p2) TwoCirclesIntersectionPoints( float2 c1, float r1, float2 c2, float r2 )
		{
			// first check to see if they overlap
			if ( !TwoCirclesOverlapped( c1, r1, c2, r2 ) )
				return (null, null);

			// calculate the distance between the circle centers
			var d = math.distance( c1, c2 );

			// Now calculate the distance from the center of each circle to the center
			// of the line which connects the intersection points.
			var a = ( r1 - r2 + ( d * d ) ) / ( 2 * d );

			// MAYBE A TEST FOR EXACT OVERLAP?

			// calculate the point P2 which is the center of the line which
			// connects the intersection points
			var p2 = new float2( c1.x + a * ( c2.x - c1.x ) / d, c1.y + a * ( c2.y - c1.y ) / d );

			// calculate first point
			var h1 = ( float )Math.Sqrt( ( r1 * r1 ) - ( a * a ) );
			var intersect1 = new float2( p2.x - h1 * ( c2.y - c1.y ) / d, p2.y + h1 * ( c2.x - c1.x ) / d );

			// calculate second point
			var h2 = ( float )Math.Sqrt( ( r2 * r2 ) - ( a * a ) );
			var intersect2 = new float2( p2.x + h2 * ( c2.y - c1.y ) / d, p2.y - h2 * ( c2.x - c1.x ) / d );

			return (intersect1, intersect2);
		}

		/// <summary>
		/// Calculates the area of the intersection of two circles. see http://mathforum.org/library/drmath/view/54785.html for an explanation.
		/// </summary>
		/// <param name="c1">The centre of the first circle.</param>
		/// <param name="r1">The radius of the first circle.</param>
		/// <param name="c2">The centre of the second circle.</param>
		/// <param name="r2">The radius of the second circle.</param>
		/// <returns>The area of the intersection of the two circles.</returns>
		public static float TwoCirclesIntersectionArea( float2 c1, float r1, float2 c2, float r2 )
		{
			if ( !TwoCirclesOverlapped( c1, r1, c2, r2 ) )
			{
				return 0.0f; // no overlap
			}

			// calculate the distance between the circle centers
			var d = math.distance( c1, c2 );

			// find the angles given that A and B are the two circle centers
			// and C and D are the intersection points
			var cad = 2 * ( float )Math.Acos( ( r1 * r1 + d * d - r2 * r2 ) / ( r1 * d * 2 ) );
			var cbd = 2 * ( float )Math.Acos( ( r2 * r2 + d * d - r1 * r1 ) / ( r2 * d * 2 ) );

			// Then we find the segment of each of the circles cut off by the
			// chord CD, by taking the area of the sector of the circle BCD and
			// subtracting the area of triangle BCD. Similarly we find the area
			// of the sector ACD and subtract the area of triangle ACD.
			return 0.5f * cbd * r2 * r2 - 0.5f * r2 * r2 * ( float )Math.Sin( cbd ) + 0.5f * cad * r1 * r1 - 0.5f * r1 * r1 * ( float )Math.Sin( cad );
		}

		/// <summary>
		/// Calculates the area of a circle.
		/// </summary>
		/// <param name="radius">The radius of the circle.</param>
		/// <returns>The area of the circle.</returns>
		public static float CircleArea( float radius )
		{
			return ( float )Math.PI * radius * radius;
		}

		/// <summary>
		/// returns true if the point p is within the radius of the given circle
		/// </summary>
		/// <param name="center">The centre of the circle.</param>
		/// <param name="radius">The radius of the circle.</param>
		/// <param name="p">The point.</param>
		/// <returns>True if the point lies within the circle, false if it is outside or on the circumference.</returns>
		public static bool PointInCircle( float2 center, float radius, float2 p )
		{
			return math.lengthsq( center - p ) < radius * radius;
		}

		/// <summary>
		/// returns true if the line segment AB intersects with a circle at position P with radius radius
		/// </summary>
		/// <param name="a">One end of the line segment.</param>
		/// <param name="b">Other end of the line segment.</param>
		/// <param name="c">The centre of the circle.</param>
		/// <param name="radius">The radius of the circle.</param>
		/// <returns>True if the line segment intersects the circle, otherwise false.</returns>
		public static bool LineSegmentCircleIntersection( float2 a, float2 b, float2 c, float radius )
		{
			return DistanceToLineSegmentSquared( a, b, c ) < radius * radius;
		}

		/// <summary>
		/// Given a line segment AB and a circle position and radius, this function
		/// determines if there is an intersection and stores the position of the
		/// closest intersection in the reference IntersectionPoint. returns false
		/// if no intersection point is found.
		/// </summary>
		/// <param name="a">One end of the line segment.</param>
		/// <param name="b">Other end of the line segment.</param>
		/// <param name="pos">The centre of the circle.</param>
		/// <param name="radius">The radius of the circle.</param>
		/// <returns>The intersection point closest to a any exist, otherwise null.</returns>
		public static float2? GetLineSegmentCircleClosestIntersectionPoint( float2 a, float2 b, float2 pos, float radius )
		{
			float2? intersectionPoint = null;

			// To make the following calculations easier, move the circle into the local space defined by the vector AB with origin at A
			var toBNorm = math.normalize( b - a );
			//todo
			var tmpX = toBNorm.x;
			toBNorm.x = toBNorm.y;
			toBNorm.y = -tmpX;
			var localPos = TransformUtil.ToLocalSpace( a, toBNorm, toBNorm ).Apply( pos );

			// if the local position + the radius is negative then the circle lays behind
			// point A so there is no intersection possible. If the local x pos minus the
			// radius is greater than length A-B then the circle cannot intersect the
			// line segment
			if ( ( localPos.x + radius >= 0 ) && ( localPos.x - radius ) * ( localPos.x - radius ) <= math.lengthsq( b - a ) )
			{
				// if the distance from the x axis to the object's position is less
				// than its radius then there is a potential intersection.
				if ( Math.Abs( localPos.y ) < radius )
				{
					// now to do a line/circle intersection test. If the center of the
					// circle is at (x, y) then the intersection points are at
					// (x +/- sqrt(r^2 - y^2), 0). We only need to look at the smallest
					// positive value of x.
					var x = localPos.x;
					var y = localPos.y;

					var ip = x - ( float )Math.Sqrt( radius * radius - y * y );

					if ( ip <= 0 )
					{
						ip = x + ( float )Math.Sqrt( radius * radius - y * y );
					}

					intersectionPoint = a + toBNorm * ip;
				}
			}

			return intersectionPoint;
		}

		/// <summary>
		/// Wraps an input vector around if it exceeds given maxima - creating a toroidal world space.
		/// </summary>
		/// <param name="v">The input vector.</param>
		/// <param name="maxX">The maximum x-position - beyond which the input vector should be wrapped around.</param>
		/// <param name="maxY">The maximum y-position - beyond which the input vector should be wrapped around.</param>
		/// <returns>The input vector, wrapped around if necessary.</returns>
		public static float2 WrapAround( float2 v, int maxX, int maxY )
		{
			// TODO: possible bug - sets to zero rather than taking overlap into account?
			float x = v.x, y = v.y;

			if ( x > maxX ) { x = 0.0f; }
			if ( x < 0 ) { x = maxX; }
			if ( y < 0 ) { y = maxY; }
			if ( y > maxY ) { y = 0.0f; }

			return new float2( x, y );
		}

		/// <summary>
		/// Returns true if the point p is inside a specified region.
		/// </summary>
		/// <param name="p">The point.</param>
		/// <param name="minXY">The corner of the region with minimal x an y values.</param>
		/// <param name="maxXY">The corner of the region with maximal x an y values.</param>
		/// <returns>True if the point lies within the region, otherwise false.</returns>
		public static bool IsInsideRegion( float2 p, float2 minXY, float2 maxXY )
		{
			return ( p.x >= minXY.x ) && ( p.x <= maxXY.x ) && ( p.y >= minXY.y ) && ( p.y <= maxXY.y );
		}

		/// <summary>
		/// Determines whether two axis-aligned rectangles overlap.
		/// </summary>
		/// <param name="minXY1">The minimum coordinate of the first rectangle.</param>
		/// <param name="maxXY1">The maximum coordinate of the first rectangle.</param>
		/// <param name="minXY2">The minimum coordinate of the second rectangle.</param>
		/// <param name="maxXY2">The maximum coordinate of the second rectangle.</param>
		/// <returns>True if the rectangles overlap, otherwise false.</returns>
		public static bool RegionsOverlap( float2 minXY1, float2 maxXY1, float2 minXY2, float2 maxXY2 )
		{
			return !( minXY2.x > maxXY1.x || maxXY2.x < minXY1.x || minXY2.y > maxXY1.y || maxXY2.y < minXY1.y );
		}

		/// <summary>
		/// Determines if one position is withing the "field of view" of another.
		/// </summary>
		/// <param name="posFirst">The position of the viewer.</param>
		/// <param name="facingFirst">The facing of the viewer.</param>
		/// <param name="posSecond">The position of the viewee.</param>
		/// <param name="fov">The field of view of the viewer, in radians.</param>
		/// <returns>True if the viewee is in the field of view of the viewer, otherwise false.</returns>
		public static bool IsSecondInFOVOfFirst( float2 posFirst, float2 facingFirst, float2 posSecond, float fov )
		{
			var toTarget = math.normalize( posSecond - posFirst );
			return math.dot( facingFirst, toTarget ) >= ( float )Math.Cos( fov / 2.0f );
		}
	}
}
