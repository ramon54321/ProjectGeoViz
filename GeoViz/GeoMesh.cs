using System;
using System.Collections.Generic;
using System.Linq;

namespace GeoViz
{
  public class GeoMesh
  {
    public GeoMesh() {}
    public GeoMesh(IEnumerable<GeoTriangle> geoTriangles)
    {
      foreach (var geoTriangle in geoTriangles)
      {
        AddTriangle(geoTriangle);
      }
    }

    public void UpdatePointsLines()
    {
      // Clear all points line references
      foreach (var geoTriangle in Triangles)
      {
        geoTriangle.a.lines.Clear();
        geoTriangle.b.lines.Clear();
        geoTriangle.c.lines.Clear();
      }

      // Foreach point in mesh, add line references to lines in mesh containing point
      foreach (var geoPoint in Points)
      {
        foreach (var geoLine in Lines.Where(l => ReferenceEquals(l.a, geoPoint) || ReferenceEquals(l.b, geoPoint)))
        {
          geoPoint.lines.Add(geoLine);
        }
      }
      
      // Lines.Clear();
      // foreach (var geoLine in Triangles.SelectMany(t => t.Lines))
      // {
      //   geoLine.a.lines.Add(geoLine);
      //   geoLine.b.lines.Add(geoLine);
      //   Lines.Add(geoLine);
      // }
    }

    public HashSet<GeoPoint> Points { get; set; } = new();
    public HashSet<GeoLine> Lines { get; set; } = new();
    public HashSet<GeoTriangle> Triangles { get; set; } = new();
    
    public IEnumerable<GeoLine> LinesInternal => Lines.Where(l => Triangles.SelectMany(t => new[] { t.ab, t.bc, t.ca }).Count(tl => tl.Equals(l)) > 1);
    
    public IEnumerable<GeoTriangle> GetContainingTriangles(IEnumerable<GeoPoint> boundaryGeoPoints)
    {
      var boundaryGeoPointsArray = boundaryGeoPoints.ToArray();
      var borderLines = new List<GeoLine>();
      for (var i = 0; i < boundaryGeoPointsArray.Length; i++)
      {
        var currentGeoPoint = boundaryGeoPointsArray[i];
        var nextGeoPoint = i == boundaryGeoPointsArray.Length - 1 ? boundaryGeoPointsArray[0] : boundaryGeoPointsArray[i + 1];
        borderLines.Add(Lines.First(l => l.Equals(new GeoLine(currentGeoPoint, nextGeoPoint, default))));
      }

      var internalLines = LinesInternal.ToHashSet();

      var untestedTriangles = Triangles.ToHashSet();
      var trianglesToTest = new Stack<GeoTriangle>();
      var testedTriangles = new HashSet<GeoTriangle>();
      
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
      if (floodBoundaryLines.All(l => borderLines.Contains(l))) return testedTriangles;
      return untestedTriangles;
    }
    
    public void AddTriangle(GeoTriangle geoTriangle)
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

    private bool ExistsTriangle(GeoPoint a, GeoPoint b, GeoPoint c)
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

    public GeoMesh Clone()
    {
      var geoMesh = new GeoMesh();
      foreach (var geoTriangle in Triangles)
      {
        geoMesh.AddTriangle(geoTriangle.Clone());
      }
      geoMesh.UpdatePointsLines();
      return geoMesh;
    }
  }
}
