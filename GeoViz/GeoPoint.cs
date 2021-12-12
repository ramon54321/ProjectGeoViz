using System;
using System.Collections.Generic;

namespace GeoViz
{
  public class GeoPoint : GeoPoint<int>
  {
    public GeoPoint(float x, float y) : base(x, y, default) {}
  }

  public class GeoPoint<T>
  {
    public readonly T payload;
    public float x;
    public float y;
    public readonly HashSet<GeoTriangle<T>> triangles = new();
    public readonly HashSet<GeoLine<T>> lines = new();
    public GeoPoint(float x, float y, T payload)
    {
      this.x = x;
      this.y = y;
      this.payload = payload;
    }

    public GeoPoint<T> Clone()
    {
      return new GeoPoint<T>(x, y, payload);
    }
    
    public static GeoPoint<T> operator *(GeoPoint<T> geoPoint, float a)
    {
      return new GeoPoint<T>(geoPoint.x * a, geoPoint.y * a, geoPoint.payload);
    }
    
    public static GeoPoint<T> operator /(GeoPoint<T> geoPoint, float a)
    {
      return new GeoPoint<T>(geoPoint.x / a, geoPoint.y / a, geoPoint.payload);
    }
    
    public static GeoPoint<T> operator +(GeoPoint<T> geoPoint, float a)
    {
      return new GeoPoint<T>(geoPoint.x + a, geoPoint.y + a, geoPoint.payload);
    }

    public static GeoPoint<T> operator +(GeoPoint<T> a, GeoPoint<T> b)
    {
      return new GeoPoint<T>(a.x + b.x, a.y + b.y, a.payload);
    }
    
    public static GeoPoint<T> operator -(GeoPoint<T> geoPoint, float a)
    {
      return new GeoPoint<T>(geoPoint.x - a, geoPoint.y - a, geoPoint.payload);
    }
    
    public static GeoPoint<T> operator -(GeoPoint<T> a, GeoPoint<T> b)
    {
      return new GeoPoint<T>(a.x - b.x, a.y - b.y, a.payload);
    }
    
    public override bool Equals(object obj)
    {
      if (obj is not GeoPoint<T> geoPoint) return false;
      const float tolerance = 0.001f;
      return Math.Abs(geoPoint.x - x) < tolerance && Math.Abs(geoPoint.y - y) < tolerance;
    }

    public override int GetHashCode()
    {
      var hCode = (int)x ^ (int)y;
      return hCode.GetHashCode();
    }

    public override string ToString() => $"{x}, {y}";
  }
}
