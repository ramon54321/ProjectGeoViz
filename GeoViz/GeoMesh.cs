using System;
using System.Collections.Generic;
using System.Linq;

namespace GeoViz
{
  public class GeoMesh<T>
  {
    public GeoMesh() {}
    public GeoMesh(IEnumerable<GeoTriangle<T>> geoTriangles)
    {
      foreach (var geoTriangle in geoTriangles)
      {
        AddTriangle(geoTriangle);
      }
    }

    public HashSet<GeoPoint<T>> Points { get; set; } = new();
    public HashSet<GeoLine<T>> Lines { get; set; } = new();
    public HashSet<GeoTriangle<T>> Triangles { get; set; } = new();
    
    public IEnumerable<GeoLine<T>> LinesInternal => Lines.Where(l => Triangles.SelectMany(t => new[] { t.ab, t.bc, t.ca }).Count(tl => tl.Equals(l)) > 1);
    
    public IEnumerable<GeoTriangle<T>> GetContainingTriangles(IEnumerable<GeoPoint<T>> boundaryGeoPoints)
    {
      var boundaryGeoPointsArray = boundaryGeoPoints.ToArray();
      var borderLines = new List<GeoLine<T>>();
      for (var i = 0; i < boundaryGeoPointsArray.Length; i++)
      {
        var currentGeoPoint = boundaryGeoPointsArray[i];
        var nextGeoPoint = i == boundaryGeoPointsArray.Length - 1 ? boundaryGeoPointsArray[0] : boundaryGeoPointsArray[i + 1];
        borderLines.Add(Lines.First(l => l.Equals(new GeoLine<T>(currentGeoPoint, nextGeoPoint, default))));
      }

      var internalLines = LinesInternal.ToHashSet();

      var untestedTriangles = Triangles.ToHashSet();
      var trianglesToTest = new Stack<GeoTriangle<T>>();
      var testedTriangles = new HashSet<GeoTriangle<T>>();
      
      var firstTriangle = Triangles.First();
      
      trianglesToTest.Push(firstTriangle);
      untestedTriangles.Remove(firstTriangle);

      while (trianglesToTest.Count > 0)
      {
        var testTriangle = trianglesToTest.Pop();
        testedTriangles.Add(testTriangle);
        var internalNonBorderLines = testTriangle.Lines.Where(l => internalLines.Contains(l)).Where(l => !borderLines.Contains(l)).ToList();
        if (internalNonBorderLines.Count == 0) continue;
        foreach (var internalNonBorderLine in internalNonBorderLines)
        {
          foreach (var geoTriangle in untestedTriangles.Where(t => t.Lines.Contains(internalNonBorderLine)))
          {
            trianglesToTest.Push(geoTriangle);
            untestedTriangles.Remove(geoTriangle);
          }
        }
      }

      var floodBoundaryLines = Geometry.FindTrianglesBoundaryEdges(testedTriangles).ToArray();
      Console.WriteLine(floodBoundaryLines.Length);
      if (floodBoundaryLines.All(l => borderLines.Contains(l))) return testedTriangles;
      return untestedTriangles;
    }
    
    public void AddTriangle(GeoTriangle<T> geoTriangle)
    {
      var existsPointA = Points.Contains(geoTriangle.a);
      var existsPointB = Points.Contains(geoTriangle.b);
      var existsPointC = Points.Contains(geoTriangle.c);
      if (existsPointA && existsPointB && existsPointC && ExistsTriangle(geoTriangle.a, geoTriangle.b, geoTriangle.c)) return;
      if (existsPointA) geoTriangle.a = Points.First(p => p.Equals(geoTriangle.a));
      if (existsPointB) geoTriangle.b = Points.First(p => p.Equals(geoTriangle.b));
      if (existsPointC) geoTriangle.c = Points.First(p => p.Equals(geoTriangle.c));
      geoTriangle.UpdateLinesPoints();
      Points.Add(geoTriangle.a);
      Points.Add(geoTriangle.b);
      Points.Add(geoTriangle.c);
      Lines.Add(geoTriangle.ab);
      Lines.Add(geoTriangle.bc);
      Lines.Add(geoTriangle.ca);
      Triangles.Add(geoTriangle);
    }

    private bool ExistsTriangle(GeoPoint<T> a, GeoPoint<T> b, GeoPoint<T> c)
    {
      // TODO: Create triangle equals()
      foreach (var g in Triangles)
      {
        if (g.a.Equals(a) && g.b.Equals(b) && g.c.Equals(c)) return true;
        if (g.a.Equals(a) && g.c.Equals(b) && g.b.Equals(c)) return true;
        if (g.b.Equals(a) && g.a.Equals(b) && g.c.Equals(c)) return true;
        if (g.b.Equals(a) && g.c.Equals(b) && g.a.Equals(c)) return true;
        if (g.c.Equals(a) && g.a.Equals(b) && g.b.Equals(c)) return true;
        if (g.c.Equals(a) && g.b.Equals(b) && g.a.Equals(c)) return true;
      }
      return false;
    }

    public GeoMesh<T> Clone()
    {
      var geoMesh = new GeoMesh<T>();
      foreach (var geoTriangle in Triangles)
      {
        geoMesh.AddTriangle(geoTriangle.Clone());
      }
      return geoMesh;
    }
  }
}
