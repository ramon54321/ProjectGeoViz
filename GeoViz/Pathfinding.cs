using System.Collections.Generic;
using System.Linq;

namespace GeoViz
{
  public enum PathfindingLineKind
  {
    Internal,
    Boundary,
    Connection,
  }

  public class Pathfinding
  {
    public static IEnumerable<GeoPoint<CollisionBlock>> GetPath(GeoMesh<CollisionBlock> collisionMesh, GeoPoint start, GeoPoint goal)
    {
      var startGeoPoint = collisionMesh.Points.First(p => p.Equals(start));
      var goalGeoPoint = collisionMesh.Points.First(p => p.Equals(goal));
      var path = GetPath(startGeoPoint, goalGeoPoint);
      return SimplifyPath(path, collisionMesh);
    }
    
    private static IEnumerable<GeoPoint<CollisionBlock>> GetPath(GeoPoint<CollisionBlock> start, GeoPoint<CollisionBlock> goal)
    {
      var openSet = new HashSet<GeoPoint<CollisionBlock>> { start };
      var cameFrom = new Dictionary<GeoPoint<CollisionBlock>, GeoPoint<CollisionBlock>>();

      var gScore = new Dictionary<GeoPoint<CollisionBlock>, float>();
      gScore[start] = 0f;

      var fScore = new Dictionary<GeoPoint<CollisionBlock>, float>();
      fScore[start] = Heuristic(start, goal);

      while (openSet.Any())
      {
        var current = openSet.OrderBy(p => fScore[p]).First();
        
        if (ReferenceEquals(current, goal)) return ReconstructPath(cameFrom, current);

        openSet.Remove(current);
        foreach (var currentLine in current.lines.Where(l => l.payload.pathfindingLineKind != PathfindingLineKind.Internal))
        {
          var neighbor = ReferenceEquals(currentLine.a, current) ? currentLine.b : currentLine.a;
          var tentativeGScore = gScore.GetValueOrDefault(current, float.PositiveInfinity) + currentLine.payload.weight;
          if (tentativeGScore < gScore.GetValueOrDefault(neighbor, float.PositiveInfinity))
          {
            cameFrom[neighbor] = current;
            gScore[neighbor] = tentativeGScore;
            fScore[neighbor] = tentativeGScore + Heuristic(neighbor, goal);
            if (!openSet.Contains(neighbor)) openSet.Add(neighbor);
          }
        }
      }
      return new List<GeoPoint<CollisionBlock>>();
    }
    
    private static IEnumerable<GeoPoint<CollisionBlock>> SimplifyPath(IEnumerable<GeoPoint<CollisionBlock>> pathPoints, GeoMesh<CollisionBlock> collisionMesh)
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
    
    private static List<GeoPoint<CollisionBlock>> ReconstructPath(Dictionary<GeoPoint<CollisionBlock>, GeoPoint<CollisionBlock>> cameFrom, GeoPoint<CollisionBlock> current)
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
    
    private static float Heuristic(GeoPoint<CollisionBlock> current, GeoPoint<CollisionBlock> goal)
    {
      return Geometry.Distance(current, goal);
    }
  }
}
