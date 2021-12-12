namespace GeoViz
{
  public class GeoLine
  {
    public CollisionBlock payload;
    public GeoPoint a;
    public GeoPoint b;
    public GeoLine(GeoPoint a, GeoPoint b, CollisionBlock payload, bool isGhost = false)
    {
      this.a = a;
      this.b = b;
      this.payload = payload;
      if (!isGhost)
      {
        this.a.lines.Add(this);
        this.b.lines.Add(this);
      }
    }

    public GeoLine Clone()
    {
      return new GeoLine(a, b, payload);
    }
    
    public override bool Equals(object obj)
    {
      if (obj == null) return false;
      if (obj.GetType() != GetType()) return false;
      if (obj is GeoLine geoLine)
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
