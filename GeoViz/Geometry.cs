using System;
using System.Collections.Generic;
using System.Linq;

namespace GeoViz
{
  public static class Geometry
  {
    public static bool IsOnEdge<A, B, C>(GeoPoint<A> a, GeoPoint<B> b, GeoPoint<C> p)
    {
      return b.x <= MathF.Max(p.x, a.x) && b.x >= MathF.Min(p.x, a.x) &&
             b.y <= MathF.Max(p.y, a.y) && b.y >= MathF.Min(p.y, a.y);
    }

    public static int Orientation<A, B, C>(GeoPoint<A> a, GeoPoint<B> b, GeoPoint<C> r)
    {
      var val = (b.y - a.y) * (r.x - b.x) - (b.x - a.x) * (r.y - b.y);
      if (val == 0) return 0;
      return val > 0 ? 1 : -1; 
    }
    
    public static bool DoesIntersectExcludeEndpointsAndCollinear<A, B>(GeoLine<A> a, GeoLine<B> b)
    {
      var vA0 = a.a;
      var vA1 = a.b;
      var vB0 = b.a;
      var vB1 = b.b;
      var denominator = (vA1.x - vA0.x) * (vB1.y - vB0.y) - (vA1.y - vA0.y) * (vB1.x - vB0.x);
      if (denominator == 0) return false;
      var numerator = (vA0.y - vB0.y) * (vB1.x - vB0.x) - (vA0.x - vB0.x) * (vB1.y - vB0.y);
      var r = numerator / denominator;
      var numerator2 = (vA0.y - vB0.y) * (vA1.x - vA0.x) - ((vA0.x - vB0.x) * (vA1.y - vA0.y));
      var s = numerator2 / denominator;
      if (r <= 0 || r >= 1 || s <= 0 || s >= 1) return false;
      return true;
    }
    
    public static bool DoesIntersectExcludeEndpointsAndCollinear<A, B>(GeoLine<A> a, IEnumerable<GeoLine<B>> bs)
    {
      foreach (var geoLine in bs)
      {
        if (DoesIntersectExcludeEndpointsAndCollinear(a, geoLine)) return true;
      }
      return false;
    }

    public static bool DoesIntersect<A, B>(GeoLine<A> a, GeoLine<B> b)
    {
      var vA0 = a.a;
      var vA1 = a.b;
      var vB0 = b.a;
      var vB1 = b.b;
    
      var o1 = Orientation(vA0, vA1, vB0);
      var o2 = Orientation(vA0, vA1, vB1);
      var o3 = Orientation(vB0, vB1, vA0);
      var o4 = Orientation(vB0, vB1, vA1);
 
      if (o1 != o2 && o3 != o4) return true;
 
      if (o1 == 0 && IsOnEdge(vA0, vA1, vB0)) return true;
      if (o2 == 0 && IsOnEdge(vA0, vA1, vB1)) return true;
      if (o3 == 0 && IsOnEdge(vB0, vB1, vA0)) return true;
      if (o4 == 0 && IsOnEdge(vB0, vB1, vA1)) return true;
 
      return false;
    }
    
    public static bool DoesIntersect<A, B>(GeoLine<A> a, IEnumerable<GeoLine<B>> bs)
    {
      foreach (var geoLine in bs)
      {
        if (DoesIntersect(a, geoLine)) return true;
      }
      return false;
    }
    
    public static bool DoesIntersect<T>(GeoTriangle<T> a, GeoTriangle<T> b)
    {
      foreach (var aLine in a.Lines)
      {
        foreach (var bLine in b.Lines)
        {
          if (DoesIntersect(aLine, bLine)) return true;
        }
      }
      return false;
    }
    
    public static bool DoesIntersect<T>(List<GeoTriangle<T>> polygons, GeoTriangle<T> a)
    {
      foreach (var polygon in polygons)
      {
        if (DoesIntersect(polygon, a)) return true;
      }
      return false;
    }
    
    public static GeoPoint<A> Rotate<A>(GeoPoint<A> a, float angle)
    {
      return new GeoPoint<A>(
        a.x * MathF.Cos(angle) - a.y * MathF.Sin(angle),
        a.x * MathF.Sin(angle) + a.y * MathF.Cos(angle),
        default
      );
    }
    
    // public static GeoTriangle<> CreateSquare(GeoPoint center, float radius, float angle = 0f)
    // {
    //   var vbl = new GeoPoint(-1f, -1f) * radius;
    //   var vbr = new GeoPoint(1f, -1f) * radius;
    //   var vtl = new GeoPoint(-1f, 1f) * radius;
    //   var vtr = new GeoPoint(1f, 1f) * radius;
    //
    //   if (angle != 0f)
    //   {
    //     vbl = Rotate(vbl, angle);
    //     vbr = Rotate(vbr, angle);
    //     vtl = Rotate(vtl, angle);
    //     vtr = Rotate(vtr, angle);
    //   }
    //
    //   vbl += center;
    //   vbr += center;
    //   vtl += center;
    //   vtr += center;
    //
    //   var lineA = new GeoLine(vbl, vbr);
    //   var lineB = new GeoLine(vbr, vtr);
    //   var lineC = new GeoLine(vtr, vtl);
    //   var lineD = new GeoLine(vtl, vbl);
    //   var polygon = new GeoPolygon(new HashSet<GeoLine> { lineA, lineB, lineC, lineD });
    //   return polygon;
    // }

    public static IEnumerable<GeoTriangle<T>> Triangulate<T>(IEnumerable<GeoPoint<T>> geoPoints)
    {
      var points = geoPoints.ToList();
      if (points.Count < 3) return null;
      if (points.Count == 3) return new List<GeoTriangle<T>> { new(points[0], points[1], points[2]) };
      
      var xMin = points.First().x;
      var xMax = xMin;
      var yMin = points.First().y;
      var yMax = yMin;
      foreach (var geoPoint in points)
      {
        xMin = MathF.Min(xMin, geoPoint.x);
        xMax = MathF.Max(xMax, geoPoint.x);
        yMin = MathF.Min(yMin, geoPoint.y);
        yMax = MathF.Max(yMax, geoPoint.y);
      }

      var dX = xMax - xMin;
      var dY = yMax - yMin;
      var dMax = MathF.Max(dX, dY);
      var xMid = (xMin + xMax) / 2f;
      var yMid = (yMin + yMax) / 2f;

      var p0 = new GeoPoint<T>(xMid - 20 * dMax, yMid - dMax, default);
      var p1 = new GeoPoint<T>(xMid, yMid + 20 * dMax, default);
      var p2 = new GeoPoint<T>(xMid + 20 * dMax, yMid - dMax, default);
      var geoPointsSuperTriangle = new List<GeoPoint<T>> { p0, p1, p2 };
      var initialTriangles = new List<GeoTriangle<T>> { new(p0, p1, p2) };

      var triangulation = new HashSet<GeoTriangle<T>>(initialTriangles);
      foreach (var point in points)
      {
        var invalidTriangles = FindTrianglesWhereCircumcircleOverlapsPoint(triangulation, point);
        foreach (var triangle in invalidTriangles)
        {
          triangle.DisconnectPoints();
        }
        triangulation.RemoveWhere(t => invalidTriangles.Contains(t));
        
        var polygon = FindTrianglesBoundaryEdges(invalidTriangles);
        foreach (var line in polygon.Where(possibleEdge => possibleEdge.a != point && possibleEdge.b != point))
        {
          var triangle = new GeoTriangle<T>(point, line.a, line.b);
          triangulation.Add(triangle);
        }
      }
      foreach (var geoPoint in geoPointsSuperTriangle)
      {
        triangulation.RemoveWhere(t => t.ContainsPoint(geoPoint));
      }
      
      foreach (var geoTriangle in triangulation)
      {
        geoTriangle.a.lines.Clear();
        geoTriangle.b.lines.Clear();
        geoTriangle.c.lines.Clear();
      }
      foreach (var geoLine in triangulation.SelectMany(t => t.Lines))
      {
        geoLine.a.lines.Add(geoLine);
        geoLine.b.lines.Add(geoLine);
      }
      
      return triangulation;
    }

    public static IEnumerable<GeoLine<A>> FindTrianglesBoundaryEdges<A>(IEnumerable<GeoTriangle<A>> triangles)
    {
      var edges = new List<GeoLine<A>>();
      foreach (var triangle in triangles)
      {
        edges.Add(new GeoLine<A>(triangle.a, triangle.b, default));
        edges.Add(new GeoLine<A>(triangle.b, triangle.c, default));
        edges.Add(new GeoLine<A>(triangle.c, triangle.a, default));
      }
      var boundaryEdges = edges.GroupBy(e => e)
                               .Where(group => group.Count() == 1)
                               .Select(group => group.First());
      return boundaryEdges;
    }

    private static ISet<GeoTriangle<A>> FindTrianglesWhereCircumcircleOverlapsPoint<A, B>(IEnumerable<GeoTriangle<A>> triangles, GeoPoint<B> geoPoint)
    {
      return triangles.Where(t => t.IsPointInsideCircumcircle(geoPoint)).ToHashSet();
    }
    
    public static bool IsPointInTriangle<A, B>(GeoTriangle<A> geoTriangle, GeoPoint<B> p)
    {
      var p0 = geoTriangle.a;
      var p1 = geoTriangle.b;
      var p2 = geoTriangle.c;
      var s = (p0.x - p2.x) * (p.y - p2.y) - (p0.y - p2.y) * (p.x - p2.x);
      var t = (p1.x - p0.x) * (p.y - p0.y) - (p1.y - p0.y) * (p.x - p0.x);
      if (s < 0 != t < 0 && s != 0 && t != 0) return false;
      var d = (p2.x - p1.x) * (p.y - p1.y) - (p2.y - p1.y) * (p.x - p1.x);
      return d == 0 || (d < 0) == (s + t <= 0);
    }

    public static float Distance<A, B>(GeoPoint<A> a, GeoPoint<B> b)
    {
      var dx = a.x - b.x;
      var dy = a.y - b.y;
      return MathF.Sqrt(dx * dx +dy * dy);
    }

    public static GeoMesh<T> Merge<T>(GeoMesh<T> a, GeoMesh<T> b)
    {
      var aClone = a.Clone();
      var bClone = b.Clone();
      foreach (var bCloneTriangle in bClone.Triangles)
      {
        aClone.AddTriangle(bCloneTriangle);
      }
      return aClone;
    }
  }
}
