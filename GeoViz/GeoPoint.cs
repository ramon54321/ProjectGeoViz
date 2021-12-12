using System;
using System.Collections.Generic;

namespace GeoViz
{
  // public class GeoPoint : GeoPoint<int>
  // {
  //   public GeoPoint(float x, float y) : base(x, y, default) {}
  // }

  public class GeoPoint
  {
    public readonly Guid guid = Guid.NewGuid();
    public CollisionBlock payload;
    public float x;
    public float y;
    public readonly HashSet<GeoTriangle> triangles;
    public readonly HashSet<GeoLine> lines;
    
    public GeoPoint(float x, float y, CollisionBlock payload)
    {
      this.x = x;
      this.y = y;
      this.payload = payload;
      triangles = new HashSet<GeoTriangle>();
      lines = new HashSet<GeoLine>();
    }
    
    public GeoPoint(float x, float y)
    {
      this.x = x;
      this.y = y;
      this.payload = default;
      triangles = new HashSet<GeoTriangle>();
      lines = new HashSet<GeoLine>();
    }
    
    public GeoPoint Clone()
    {
      return new GeoPoint(x, y, payload);
    }

    public static GeoPoint operator *(GeoPoint geoPoint, float a)
    {
      return new GeoPoint(geoPoint.x * a, geoPoint.y * a, geoPoint.payload);
    }
    
    public static GeoPoint operator /(GeoPoint geoPoint, float a)
    {
      return new GeoPoint(geoPoint.x / a, geoPoint.y / a, geoPoint.payload);
    }
    
    public static GeoPoint operator +(GeoPoint geoPoint, float a)
    {
      return new GeoPoint(geoPoint.x + a, geoPoint.y + a, geoPoint.payload);
    }

    public static GeoPoint operator +(GeoPoint a, GeoPoint b)
    {
      return new GeoPoint(a.x + b.x, a.y + b.y, a.payload);
    }
    
    public static GeoPoint operator -(GeoPoint geoPoint, float a)
    {
      return new GeoPoint(geoPoint.x - a, geoPoint.y - a, geoPoint.payload);
    }
    
    public static GeoPoint operator -(GeoPoint a, GeoPoint b)
    {
      return new GeoPoint(a.x - b.x, a.y - b.y, a.payload);
    }

    public override bool Equals(object obj)
    {
      const float tolerance = 0.001f;
      if (obj is GeoPoint geoPoint)
      {
        return Math.Abs(geoPoint.x - x) < tolerance && Math.Abs(geoPoint.y - y) < tolerance;
      }
      return false;
    }

    public override int GetHashCode()
    {
      var hCode = (int)x ^ (int)y;
      return hCode.GetHashCode();
    }

    public override string ToString() => $"{guid} {x}, {y} Lines: {lines.Count} Triangles: {triangles.Count}";
  }
  
  // public class GeoPoint : GeoPoint<int>
  // {
  //   public GeoPoint(float x, float y) : base(x, y, default) {}
  // }
  //
  // public class GeoPoint<T>
  // {
  //   public readonly T payload;
  //   public float x;
  //   public float y;
  //   public readonly HashSet<GeoTriangle<T>> triangles = new();
  //   public readonly HashSet<GeoLine<T>> lines = new();
  //   public GeoPoint(float x, float y, T payload)
  //   {
  //     this.x = x;
  //     this.y = y;
  //     this.payload = payload;
  //   }
  //
  //   public GeoPoint<T> Clone()
  //   {
  //     return new GeoPoint<T>(x, y, payload);
  //   }
  //   
  //   public static GeoPoint<T> operator *(GeoPoint<T> geoPoint, float a)
  //   {
  //     return new GeoPoint<T>(geoPoint.x * a, geoPoint.y * a, geoPoint.payload);
  //   }
  //   
  //   public static GeoPoint<T> operator /(GeoPoint<T> geoPoint, float a)
  //   {
  //     return new GeoPoint<T>(geoPoint.x / a, geoPoint.y / a, geoPoint.payload);
  //   }
  //   
  //   public static GeoPoint<T> operator +(GeoPoint<T> geoPoint, float a)
  //   {
  //     return new GeoPoint<T>(geoPoint.x + a, geoPoint.y + a, geoPoint.payload);
  //   }
  //
  //   public static GeoPoint<T> operator +(GeoPoint<T> a, GeoPoint<T> b)
  //   {
  //     return new GeoPoint<T>(a.x + b.x, a.y + b.y, a.payload);
  //   }
  //   
  //   public static GeoPoint<T> operator -(GeoPoint<T> geoPoint, float a)
  //   {
  //     return new GeoPoint<T>(geoPoint.x - a, geoPoint.y - a, geoPoint.payload);
  //   }
  //   
  //   public static GeoPoint<T> operator -(GeoPoint<T> a, GeoPoint<T> b)
  //   {
  //     return new GeoPoint<T>(a.x - b.x, a.y - b.y, a.payload);
  //   }
  //   
  //   public override bool Equals(object obj)
  //   {
  //     const float tolerance = 0.001f;
  //     if (obj is GeoPoint<T> geoPointA)
  //     {
  //       return Math.Abs(geoPointA.x - x) < tolerance && Math.Abs(geoPointA.y - y) < tolerance;
  //     }
  //     if (obj is GeoPoint geoPointB)
  //     {
  //       return Math.Abs(geoPointB.x - x) < tolerance && Math.Abs(geoPointB.y - y) < tolerance;
  //     }
  //     return false;
  //   }
  //
  //   public override int GetHashCode()
  //   {
  //     var hCode = (int)x ^ (int)y;
  //     return hCode.GetHashCode();
  //   }
  //
  //   public override string ToString() => $"{x}, {y}";
  // }
}
