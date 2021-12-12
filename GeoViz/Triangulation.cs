using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace GeoViz
{
  public class Triangulation : IRenderable
  {
    private readonly List<GeoMesh> _geoMeshes = new();

    private static CollisionBlock collisionBlockBoundary = new()
    {
        pathfindingLineKind = PathfindingLineKind.Boundary
    };

    public void Start()
    {
      Cursor.Hide();
      
      var borderA = new List<GeoPoint>
      {
          new(0, 0, collisionBlockBoundary),
          new(1, 0, collisionBlockBoundary),
          new(1, 1, collisionBlockBoundary),
          new(0, 1, collisionBlockBoundary),
      };
      var borderB = new List<GeoPoint>
      {
          new(5, 5, collisionBlockBoundary),
          new(6, 5, collisionBlockBoundary),
          new(6, 6, collisionBlockBoundary),
          new(5, 6, collisionBlockBoundary),
      };
      var borderC = new List<GeoPoint>
      {
          new(-4, 4, collisionBlockBoundary),
          new(-2, 4, collisionBlockBoundary),
          new(-2, 3, collisionBlockBoundary),
          new(-3, 3, collisionBlockBoundary),
          new(-3, 2, collisionBlockBoundary),
          new(-4, 2, collisionBlockBoundary),
      };
      var collidableBorders = new List<List<GeoPoint>> { borderA, borderB, borderC };
      
      _collisionMesh = Collision.CollisionGeoMeshFromBorders(collidableBorders);
      _geoMeshes.Add(_collisionMesh);
      
      UpdatePath();
    }

    private void UpdatePath()
    {
      _pathPoints = Pathfinding.GetPath(_collisionMesh, _startPoint, _goalPoint);
      _pathLines = new List<GeoLine>();
      var pathPoints = _pathPoints.ToArray();
      for (var i = 0; i < pathPoints.Length - 1; i++)
      {
        _pathLines.Add(new GeoLine(pathPoints[i], pathPoints[i+1], default, true));
      }
    }

    private GeoPoint _startPoint = new GeoPoint(-1, 2);
    private GeoPoint _goalPoint = new GeoPoint(3, 2);

    private GeoMesh _collisionMesh;
    private IEnumerable<GeoPoint> _pathPoints;
    private List<GeoLine> _pathLines;
    private GeoPoint _selectedPoint = null;
    
    public void Render(Surface s)
    {
      var mouseGeoPoint = s.CanvasToGeo(s.MousePosition);
      
      if (s.MouseDown)
      {
        // var pointsByDistance = _geoMeshes.SelectMany(m => m.Points).OrderBy(p => Geometry.Distance(mouseGeoPoint, p)).ToList();
        // if (pointsByDistance.Any()) _selectedPoint = pointsByDistance.First();
        if (Geometry.Distance(mouseGeoPoint, _startPoint) < Geometry.Distance(mouseGeoPoint, _goalPoint))
        {
          _selectedPoint = _startPoint;
        }
        else
        {
          _selectedPoint = _goalPoint;
        }
      }
      if (s.MouseUp)
      {
        _selectedPoint = null;
      }
      if (_selectedPoint != null)
      {
        _selectedPoint.x = mouseGeoPoint.x;
        _selectedPoint.y = mouseGeoPoint.y;
        UpdatePath();
      }

      foreach (var geoMesh in _geoMeshes)
      {
        DrawMesh(s, geoMesh);
      }

      if (_pathPoints != null)
      {
        var i = 0;
        foreach (var pathPoint in _pathPoints)
        {
          s.DrawDot(Brushes.Gold, pathPoint, 5);
          s.DrawText(Brushes.Gold, pathPoint + new GeoPoint(0.2f, -0.2f), $"{i}");
          i++;
        }
      }
      if (_pathLines != null)
      {
        foreach (var geoLine in _pathLines)
        {
          s.DrawLine(Pens.Gold, geoLine.a, geoLine.b);
        }
      }
      
      s.DrawDot(Brushes.Chartreuse, s.MousePosition, 2);
    }

    private void DrawMesh(Surface s, GeoMesh geoMesh)
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
      var linesUnique = new HashSet<GeoLine>();
      foreach (var geoLine in geoMesh.Points.SelectMany(p => p.lines))
      {
        linesUnique.Add(geoLine);
      }
      s.DrawText(Brushes.Gold, new GeoPoint(-1, -1), $"Points: {geoMesh.Points.Count}\nLines: {geoMesh.Lines.Count}/{linesUnique.Count}\nTriangles: {geoMesh.Triangles.Count}");
      // if (_selectedPoint != null)
      // {
      //   foreach (var geoLine in _selectedPoint.lines)
      //   {
      //     var color = geoLine.payload.pathfindingLineKind == PathfindingLineKind.Internal ? Pens.DeepSkyBlue : geoLine.payload.pathfindingLineKind == PathfindingLineKind.Boundary ? Pens.Chocolate : Pens.LightGreen;
      //     s.DrawLine(color, geoLine.a, geoLine.b);
      //     // s.DrawLine(Pens.Cyan, geoLine.a, geoLine.b);
      //   }
      // }
    }
  }
}
