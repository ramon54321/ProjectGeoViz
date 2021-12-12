using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace GeoViz
{
  public enum PathfindingLineKind
  {
    Internal,
    Boundary,
    Connection,
  }
  
  public struct CollisionBlock
  {
    public PathfindingLineKind pathfindingLineKind;
    public float weight;
  }
  
  public class Triangulation : IRenderable
  {
    private readonly List<GeoMesh<CollisionBlock>> _geoMeshes = new();

    private static CollisionBlock collisionBlockBoundary = new()
    {
        pathfindingLineKind = PathfindingLineKind.Boundary
    };

    private GeoMesh<CollisionBlock> GenerateWorldCollisionMesh()
    {
      var aPoints = new List<GeoPoint<CollisionBlock>>
      {
          new(0, 0, collisionBlockBoundary),
          new(1, 0, collisionBlockBoundary),
          new(1, 1, collisionBlockBoundary),
          new(0, 1, collisionBlockBoundary),
      };
      var bPoints = new List<GeoPoint<CollisionBlock>>
      {
          new(5, 5, collisionBlockBoundary),
          new(6, 5, collisionBlockBoundary),
          new(6, 6, collisionBlockBoundary),
          new(5, 6, collisionBlockBoundary),
      };
      var cPoints = new List<GeoPoint<CollisionBlock>>
      {
          new(-4, 4, collisionBlockBoundary),
          new(-2, 4, collisionBlockBoundary),
          new(-2, 3, collisionBlockBoundary),
          new(-3f, 3, collisionBlockBoundary),
          new(-3, 2, collisionBlockBoundary),
          new(-4, 2, collisionBlockBoundary),
      };

      var allPoints = aPoints.Concat(bPoints).Concat(cPoints).Select(p => p.Clone());

      var aTriangulation = Geometry.Triangulate(aPoints);
      var bTriangulation = Geometry.Triangulate(bPoints);
      var cTriangulation = Geometry.Triangulate(cPoints);
      var allTriangulation = Geometry.Triangulate(allPoints);

      var aGeoMesh = new GeoMesh<CollisionBlock>(aTriangulation);
      var aGeoMeshFloodedTriangles = aGeoMesh.GetContainingTriangles(aPoints);
      var aGeoMeshFlooded = new GeoMesh<CollisionBlock>(aGeoMeshFloodedTriangles);
      foreach (var geoLine in aGeoMeshFlooded.LinesInternal)
      {
        geoLine.payload.pathfindingLineKind = PathfindingLineKind.Internal;
      }
      
      var bGeoMesh = new GeoMesh<CollisionBlock>(bTriangulation);
      var bGeoMeshFloodedTriangles = bGeoMesh.GetContainingTriangles(bPoints);
      var bGeoMeshFlooded = new GeoMesh<CollisionBlock>(bGeoMeshFloodedTriangles);
      foreach (var geoLine in bGeoMeshFlooded.LinesInternal)
      {
        geoLine.payload.pathfindingLineKind = PathfindingLineKind.Internal;
      }
      
      var cGeoMesh = new GeoMesh<CollisionBlock>(cTriangulation);
      var cGeoMeshFloodedTriangles = cGeoMesh.GetContainingTriangles(cPoints);
      var cGeoMeshFlooded = new GeoMesh<CollisionBlock>(cGeoMeshFloodedTriangles);
      foreach (var geoLine in cGeoMeshFlooded.LinesInternal)
      {
        geoLine.payload.pathfindingLineKind = PathfindingLineKind.Internal;
      }
      
      var allGeoMesh = new GeoMesh<CollisionBlock>(allTriangulation);
      foreach (var geoLine in allGeoMesh.Lines)
      {
        geoLine.payload.pathfindingLineKind = PathfindingLineKind.Connection;
      }

      var mergedGeoMesh = Geometry.Merge(Geometry.Merge(aGeoMeshFlooded, Geometry.Merge(bGeoMeshFlooded, cGeoMeshFlooded)), allGeoMesh);
      mergedGeoMesh.UpdateLines();
      
      foreach (var geoLine in mergedGeoMesh.Lines)
      {
        geoLine.payload.weight = Geometry.Distance(geoLine.a, geoLine.b);
      }
      
      return mergedGeoMesh;
    }

    private List<GeoPoint<CollisionBlock>> GetPath(GeoPoint<CollisionBlock> start, GeoPoint<CollisionBlock> end)
    {
      List<GeoPoint<CollisionBlock>> ReconstructPath(Dictionary<GeoPoint<CollisionBlock>, GeoPoint<CollisionBlock>> cameFrom, GeoPoint<CollisionBlock> current)
      {
        var path = new List<GeoPoint<CollisionBlock>> { current };
        while (cameFrom.ContainsKey(current))
        {
          current = cameFrom[current];
          path.Add(current);
        }
        path.Reverse();
        return path;
      }
      
      float H(GeoPoint<CollisionBlock> node)
      {
        return Geometry.Distance(end, node);
      }
      
      var openSet = new HashSet<GeoPoint<CollisionBlock>> { start };
      var cameFrom = new Dictionary<GeoPoint<CollisionBlock>, GeoPoint<CollisionBlock>>();

      var gScore = new Dictionary<GeoPoint<CollisionBlock>, float>();
      gScore[start] = 0f;

      var fScore = new Dictionary<GeoPoint<CollisionBlock>, float>();
      fScore[start] = H(start);

      while (openSet.Any())
      {
        var current = openSet.OrderBy(p => fScore[p]).First();
        
        if (current == end) return ReconstructPath(cameFrom, current);

        openSet.Remove(current);
        foreach (var currentLine in current.lines.Where(l => l.payload.pathfindingLineKind != PathfindingLineKind.Internal))
        {
          var neighbor = currentLine.a == current ? currentLine.b : currentLine.a;
          var tentativeGScore = gScore.GetValueOrDefault(current, float.PositiveInfinity) + currentLine.payload.weight;
          if (tentativeGScore < gScore.GetValueOrDefault(neighbor, float.PositiveInfinity))
          {
            cameFrom[neighbor] = current;
            gScore[neighbor] = tentativeGScore;
            fScore[neighbor] = tentativeGScore + H(neighbor);
            if (!openSet.Contains(neighbor)) openSet.Add(neighbor);
          }
        }
      }
      return new List<GeoPoint<CollisionBlock>>();
    }


    private IEnumerable<GeoPoint<CollisionBlock>> _pathPoints;

    public void Start()
    {

      var ab = new GeoLine<int>(new GeoPoint<int>(0, 0, default), new GeoPoint<int>(2, 2, default), default, true);
      
      var testLine = new GeoLine<int>(new GeoPoint<int>(2, 2, default), new GeoPoint<int>(4, 0, default), default, true);
      
      Console.WriteLine(Geometry.DoesIntersectExcludeEndpointsAndCollinear(testLine, ab));
      Console.WriteLine(Geometry.DoesIntersect(testLine, ab));
      
      Cursor.Hide();
      var collisionMesh = GenerateWorldCollisionMesh();
      _geoMeshes.Add(collisionMesh);

      var start = collisionMesh.Points.First(p => p.Equals(new GeoPoint<CollisionBlock>(1, 0, default)));
      var end = collisionMesh.Points.First(p => p.Equals(new GeoPoint<CollisionBlock>(-4, 4, default)));
      
      _pathPoints = GetPath(start, end);

      _pathPoints = SimplifyPath(_pathPoints, collisionMesh);

      _pathLines = new List<GeoLine<CollisionBlock>>();
      var pathPoints = _pathPoints as GeoPoint<CollisionBlock>[] ?? _pathPoints.ToArray();
      for (var i = 0; i < pathPoints.Length - 1; i++)
      {
        _pathLines.Add(new GeoLine<CollisionBlock>(pathPoints[i], pathPoints[i+1], default, true));
      }
    }

    private List<GeoLine<CollisionBlock>> _pathLines;

    private IEnumerable<GeoPoint<CollisionBlock>> SimplifyPath(IEnumerable<GeoPoint<CollisionBlock>> pathPoints, GeoMesh<CollisionBlock> collisionMesh)
    {
      var collisionBoundary = collisionMesh.Lines.Where(l => l.payload.pathfindingLineKind == PathfindingLineKind.Boundary).ToList();
      var collisionInternal = collisionMesh.Lines.Where(l => l.payload.pathfindingLineKind == PathfindingLineKind.Internal).ToList();
      
      var simplifiedPathPoints = new List<GeoPoint<CollisionBlock>>();
      var originalPathPoints = pathPoints.ToArray();

      var i = 0;
      while (i < originalPathPoints.Length)
      {
        var currentPoint = originalPathPoints[i];
        simplifiedPathPoints.Add(currentPoint);
        if (i == originalPathPoints.Length - 1) break;
        i++;
        for (var c = originalPathPoints.Length - 1; c > i; c--)
        {
          var checkPoint = originalPathPoints[c];
          var checkLine = new GeoLine<CollisionBlock>(currentPoint, checkPoint, default, true);
          var lineOfSight = !Geometry.DoesIntersectExcludeEndpointsAndCollinear(checkLine, collisionBoundary) && !Geometry.DoesIntersect(checkLine, collisionInternal);
          if (lineOfSight)
          {
            i = c;
          }
        }
      }
      
      return simplifiedPathPoints;
    }



    private GeoPoint<CollisionBlock> _selectedPoint = null;
    public void Render(Surface s)
    {
      var mouseGeoPoint = s.CanvasToGeo(s.MousePosition);
      
      if (s.MouseDown)
      {
        _selectedPoint = _geoMeshes.SelectMany(m => m.Points).OrderBy(p => Geometry.Distance(mouseGeoPoint, p)).First();
      }
      if (s.MouseUp)
      {
        _selectedPoint = null;
      }

      if (_selectedPoint != null)
      {
        _selectedPoint.x = mouseGeoPoint.x;
        _selectedPoint.y = mouseGeoPoint.y;
      }
      
      s.DrawDot(Brushes.DarkSlateGray, new GeoPoint(0, 0), 1);
      s.DrawDot(Brushes.DarkSlateGray, new GeoPoint(1, 1), 1);
      s.DrawDot(Brushes.DarkSlateGray, new GeoPoint(2, 2), 1);
      s.DrawDot(Brushes.DarkSlateGray, new GeoPoint(3, 3), 1);
      s.DrawDot(Brushes.DarkSlateGray, new GeoPoint(4, 4), 1);
      s.DrawDot(Brushes.DarkSlateGray, new GeoPoint(5, 5), 1);
      s.DrawDot(Brushes.DarkSlateGray, new GeoPoint(6, 6), 1);
      s.DrawDot(Brushes.DarkSlateGray, new GeoPoint(7, 7), 1);
      s.DrawDot(Brushes.DarkSlateGray, new GeoPoint(8, 8), 1);

      foreach (var geoMesh in _geoMeshes)
      {
        DrawMesh(s, geoMesh);
      }
      
      s.DrawDot(Brushes.Chartreuse, s.MousePosition);

      var i = 0;
      foreach (var pathPoint in _pathPoints)
      {
        s.DrawDot(Brushes.Gold, pathPoint, 5);
        s.DrawText(Brushes.Gold, pathPoint + new GeoPoint<CollisionBlock>(0.2f, -0.2f, default), $"{i}");
        i++;
      }
      
      foreach (var geoLine in _pathLines)
      {
        s.DrawLine(Pens.Gold, geoLine.a, geoLine.b);
      }

      // var triLines = _cTriangulation.SelectMany(t => t.a.lines.Concat(t.b.lines).Concat(t.c.lines)).ToArray();
      // foreach (var geoLine in triLines)
      // {
      //   s.DrawLine(Pens.SpringGreen, geoLine.a, geoLine.b);
      // }
      // s.DrawText(Brushes.Chartreuse, new Point(300, 300), $"Lines: {triLines.Length}");
    }

    private IEnumerable<GeoTriangle<CollisionBlock>> _cTriangulation;

    private void DrawMesh(Surface s, GeoMesh<CollisionBlock> geoMesh)
    {
      foreach (var geoMeshTriangle in geoMesh.Triangles)
      {
        var centerGeoPoint = (geoMeshTriangle.a + geoMeshTriangle.b + geoMeshTriangle.c) / 3f;
        s.DrawDot(Brushes.BurlyWood, centerGeoPoint, 1);
        // s.DrawText(Brushes.Chartreuse, geoMeshTriangle.a + (centerGeoPoint - geoMeshTriangle.a) / 2f, "A");
        // s.DrawText(Brushes.Chartreuse, geoMeshTriangle.b + (centerGeoPoint - geoMeshTriangle.b) / 2f, "B");
        // s.DrawText(Brushes.Chartreuse, geoMeshTriangle.c + (centerGeoPoint - geoMeshTriangle.c) / 2f, "C");
      }
      foreach (var geoMeshLine in geoMesh.Lines)
      {
        var color = geoMeshLine.payload.pathfindingLineKind == PathfindingLineKind.Internal ? Pens.DeepSkyBlue : geoMeshLine.payload.pathfindingLineKind == PathfindingLineKind.Boundary ? Pens.Chocolate : Pens.LightGreen;

        // var color = new Pen(Color.FromArgb((int)(geoMeshLine.payload.weight * 20f), 128, 128));
        
        s.DrawLine(color, geoMeshLine.a, geoMeshLine.b);
      }
      foreach (var geoMeshPoint in geoMesh.Points)
      {
        s.DrawDot(Brushes.Aquamarine, geoMeshPoint);
      }
      var linesUnique = new HashSet<GeoLine<CollisionBlock>>();
      foreach (var geoLine in geoMesh.Points.SelectMany(p => p.lines))
      {
        linesUnique.Add(geoLine);
      }
      s.DrawText(Brushes.Gold, new GeoPoint(-1, -1), $"Points: {geoMesh.Points.Count}\nLines: {geoMesh.Lines.Count}/{linesUnique.Count}\nTriangles: {geoMesh.Triangles.Count}");
      if (_selectedPoint != null)
      {
        foreach (var geoLine in _selectedPoint.lines)
        {
          var color = geoLine.payload.pathfindingLineKind == PathfindingLineKind.Internal ? Pens.DeepSkyBlue : geoLine.payload.pathfindingLineKind == PathfindingLineKind.Boundary ? Pens.Chocolate : Pens.LightGreen;
          s.DrawLine(color, geoLine.a, geoLine.b);
          // s.DrawLine(Pens.Cyan, geoLine.a, geoLine.b);
        }
      }
    }
  }
}
