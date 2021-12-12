using System.Collections.Generic;
using System.Linq;

namespace GeoViz
{
  public struct CollisionBlock
  {
    public PathfindingLineKind pathfindingLineKind;
    public float weight;
  }

  public static class Collision
  {
    public static GeoMesh<CollisionBlock> CollisionGeoMeshFromBorders(IEnumerable<IEnumerable<GeoPoint<CollisionBlock>>> collidableBorders)
    {
      var collidableBordersArray = collidableBorders.ToArray();
      var triangulations = collidableBordersArray.Select(Geometry.Triangulate);
      
      var allPoints = collidableBordersArray.SelectMany(border => border);
      var allPointsTriangulation = Geometry.Triangulate(allPoints);

      var geoMeshes = triangulations.Select(triangulation => new GeoMesh<CollisionBlock>(triangulation)).ToArray();
      var geoMeshesFlooded = geoMeshes.Select((mesh, i) =>
      {
        var points = collidableBordersArray[i];
        var geoMesh = geoMeshes[i];
        var geoMeshFlooded = new GeoMesh<CollisionBlock>(geoMesh.GetContainingTriangles(points));
        foreach (var geoLine in geoMeshFlooded.LinesInternal)
        {
          geoLine.payload.pathfindingLineKind = PathfindingLineKind.Internal;
        }
        return geoMeshFlooded;
      });
      
      var allPointsGeoMesh = new GeoMesh<CollisionBlock>(allPointsTriangulation);
      foreach (var geoLine in allPointsGeoMesh.Lines)
      {
        geoLine.payload.pathfindingLineKind = PathfindingLineKind.Connection;
      }

      var mergedGeoMesh = new GeoMesh<CollisionBlock>();
      foreach (var geoMeshFlooded in geoMeshesFlooded)
      {
        mergedGeoMesh = Geometry.Merge(mergedGeoMesh, geoMeshFlooded);
      }
      mergedGeoMesh = Geometry.Merge(mergedGeoMesh, allPointsGeoMesh);
      mergedGeoMesh.UpdateLines();
      
      foreach (var geoLine in mergedGeoMesh.Lines)
      {
        geoLine.payload.weight = Geometry.Distance(geoLine.a, geoLine.b);
      }
      
      return mergedGeoMesh;
    }
  }
}
