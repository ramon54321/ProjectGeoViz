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
  }
  
  public class Triangulation : IRenderable
  {
    private List<GeoMesh<CollisionBlock>> _geoMeshes = new();

    public static CollisionBlock collisionBlockInternal = new()
    {
        pathfindingLineKind = PathfindingLineKind.Internal
    };
    public static CollisionBlock collisionBlockBoundary = new()
    {
        pathfindingLineKind = PathfindingLineKind.Boundary
    };
    public static CollisionBlock collisionBlockConnection = new()
    {
        pathfindingLineKind = PathfindingLineKind.Connection
    };
    
    public void Start()
    {
      Cursor.Hide();

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
          new(-3, 3, collisionBlockBoundary),
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


      // var merge = Geometry.Merge(bGeoMesh, cGeoMesh);

      // _geoMeshes.Add(aGeoMeshFlooded);
      // _geoMeshes.Add(cGeoMeshFlooded);
      // _geoMeshes.Add(bGeoMesh);
      // _geoMeshes.Add(cGeoMesh);
      //
      var mergedGeoMesh = Geometry.Merge(Geometry.Merge(aGeoMeshFlooded, Geometry.Merge(bGeoMeshFlooded, cGeoMeshFlooded)), allGeoMesh);
      
      _geoMeshes.Add(mergedGeoMesh);


      // var floodMesh = new GeoMesh<CollisionBlock>(cGeoMesh.GetContainingTriangles(new List<GeoPoint<CollisionBlock>>
      // {
      //     new(-4, 4, collisionBlockBoundary),
      //     new(-2, 4, collisionBlockBoundary),
      //     new(-2, 3, collisionBlockBoundary),
      //     new(-3, 3, collisionBlockBoundary),
      //     new(-3, 2, collisionBlockBoundary),
      //     new(-4, 2, collisionBlockBoundary),
      // }));
      // _geoMeshes.Add(floodMesh);

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
    }

    private void DrawMesh(Surface s, GeoMesh<CollisionBlock> geoMesh)
    {
      foreach (var geoMeshTriangle in geoMesh.Triangles)
      {
        var centerGeoPoint = (geoMeshTriangle.a + geoMeshTriangle.b + geoMeshTriangle.c) / 3f;
        s.DrawDot(Brushes.BurlyWood, centerGeoPoint, 1);
        s.DrawText(Brushes.Chartreuse, geoMeshTriangle.a + (centerGeoPoint - geoMeshTriangle.a) / 2f, "A");
        s.DrawText(Brushes.Chartreuse, geoMeshTriangle.b + (centerGeoPoint - geoMeshTriangle.b) / 2f, "B");
        s.DrawText(Brushes.Chartreuse, geoMeshTriangle.c + (centerGeoPoint - geoMeshTriangle.c) / 2f, "C");
      }
      foreach (var geoMeshLine in geoMesh.Lines)
      {
        var color = geoMeshLine.payload.pathfindingLineKind == PathfindingLineKind.Internal ? Pens.OrangeRed : geoMeshLine.payload.pathfindingLineKind == PathfindingLineKind.Boundary ? Pens.Chocolate : Pens.LightGreen;
        s.DrawLine(color, geoMeshLine.a, geoMeshLine.b);
      }
      foreach (var geoMeshPoint in geoMesh.Points)
      {
        s.DrawDot(Brushes.Aquamarine, geoMeshPoint);
      }
      s.DrawText(Brushes.Gold, new GeoPoint(-1, -1), $"Points: {geoMesh.Points.Count}\nLines: {geoMesh.Lines.Count}\nTriangles: {geoMesh.Triangles.Count}");
    }
  }
}