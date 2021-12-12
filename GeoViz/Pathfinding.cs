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
    public static List<GeoPoint> GetPath(GeoMesh collisionMesh, GeoPoint start, GeoPoint goal)
    {
      var collisionMeshClone = collisionMesh.Clone();
      var startClone = start.Clone();
      var goalClone = goal.Clone();
      
      var closestPointToStart = collisionMeshClone.Points.OrderBy(p => Geometry.Distance(startClone, p)).First();
      var closestPointToGoal = collisionMeshClone.Points.OrderBy(p => Geometry.Distance(goalClone, p)).First();
      collisionMeshClone.Points.Add(startClone);
      collisionMeshClone.Points.Add(goalClone);
      collisionMeshClone.Lines.Add(new GeoLine(startClone, closestPointToStart, new CollisionBlock
      {
          pathfindingLineKind = PathfindingLineKind.Connection,
          weight = 0
      }));
      collisionMeshClone.Lines.Add(new GeoLine(goalClone, closestPointToGoal, new CollisionBlock
      {
          pathfindingLineKind = PathfindingLineKind.Connection,
          weight = 0
      }));

      var path = GetPath(startClone, goalClone);
      return SimplifyPath(path, collisionMeshClone);
    }
    
    private static List<GeoPoint> GetPath(GeoPoint start, GeoPoint goal)
    {
      var openSet = new HashSet<GeoPoint> { start };
      var cameFrom = new Dictionary<GeoPoint, GeoPoint>();

      var gScore = new Dictionary<GeoPoint, float>();
      gScore[start] = 0f;

      var fScore = new Dictionary<GeoPoint, float>();
      fScore[start] = Heuristic(start, goal);

      while (openSet.Any())
      {
        var current = openSet.OrderBy(p => fScore[p]).First();
        
        if (Equals(current, goal)) return ReconstructPath(cameFrom, current).Select(p => p.Clone()).ToList();

        openSet.Remove(current);
        foreach (var currentLine in current.lines.Where(l => l.payload.pathfindingLineKind != PathfindingLineKind.Internal))
        {
          var neighbor = Equals(currentLine.a, current) ? currentLine.b : currentLine.a;
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
      return new List<GeoPoint>();
    }
    
    private static List<GeoPoint> SimplifyPath(IEnumerable<GeoPoint> pathPoints, GeoMesh collisionMesh)
    {
      var collisionBoundary = collisionMesh.Lines.Where(l => l.payload.pathfindingLineKind == PathfindingLineKind.Boundary).ToList();
      var collisionInternal = collisionMesh.Lines.Where(l => l.payload.pathfindingLineKind == PathfindingLineKind.Internal).ToList();
      
      var simplifiedPathPoints = new List<GeoPoint>();
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
          var checkLine = new GeoLine(currentPoint, checkPoint, default, true);
          var lineOfSight = !Geometry.DoesIntersectExcludeEndpointsAndCollinear(checkLine, collisionBoundary) && !Geometry.DoesIntersect(checkLine, collisionInternal);
          if (lineOfSight)
          {
            i = c;
          }
        }
      }
      return simplifiedPathPoints;
    }
    
    private static List<GeoPoint> ReconstructPath(Dictionary<GeoPoint, GeoPoint> cameFrom, GeoPoint current)
    {
      var path = new List<GeoPoint> { current };
      while (cameFrom.ContainsKey(current))
      {
        current = cameFrom[current];
        path.Add(current);
      }
      path.Reverse();
      return path;
    }
    
    private static float Heuristic(GeoPoint current, GeoPoint goal)
    {
      return Geometry.Distance(current, goal);
    }
  }
}
