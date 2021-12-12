namespace GeoViz
{
  public class GeoLine<T>
  {
    public T payload;
    public GeoPoint<T> a;
    public GeoPoint<T> b;
    public GeoLine(GeoPoint<T> a, GeoPoint<T> b, T payload)
    {
      this.a = a;
      this.b = b;
      this.payload = payload;
    }

    public GeoLine<T> Clone()
    {
      return new GeoLine<T>(a.Clone(), b.Clone(), payload);
    }
    
    public override bool Equals(object obj)
    {
      if (obj == null) return false;
      if (obj.GetType() != GetType()) return false;
      if (obj is GeoLine<T> geoLine)
      {
        var samePoints = a.Equals(geoLine.a) && b.Equals(geoLine.b);
        var samePointsReversed = a.Equals(geoLine.b) && b.Equals(geoLine.a);
        return samePoints || samePointsReversed;
      }
      return false;
    }

    public override int GetHashCode()
    {
      int hCode = (int)a.x ^ (int)a.y ^ (int)b.x ^ (int)b.y;
      return hCode.GetHashCode();
    }
  }
}
