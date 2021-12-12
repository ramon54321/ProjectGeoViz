using System.Collections.Generic;

namespace GeoViz
{
  public class GeoTriangle
  {
    public GeoPoint a;
    public GeoPoint b;
    public GeoPoint c;

    public readonly GeoLine ab;
    public readonly GeoLine bc;
    public readonly GeoLine ca;

    public IEnumerable<GeoLine> Lines => new[] { ab, bc, ca };

    public GeoTriangle(GeoPoint a, GeoPoint b, GeoPoint c)
    {
      if (!IsCounterClockwise(a, b, c))
      {
        this.a = a;
        this.b = c;
        this.c = b;
      }
      else
      {
        this.a = a;
        this.b = b;
        this.c = c;
      }
      
      ab = new GeoLine(this.a, this.b, a.payload);
      bc = new GeoLine(this.b, this.c, b.payload);
      ca = new GeoLine(this.c, this.a, c.payload);
      this.a.triangles.Add(this);
      this.b.triangles.Add(this);
      this.c.triangles.Add(this);

      UpdateCircumcircle();
    }

    private GeoTriangle
    (
      GeoPoint a,
      GeoPoint b,
      GeoPoint c,
      GeoLine ab,
      GeoLine bc,
      GeoLine ca
    )
    {
      this.a = a;
      this.b = b;
      this.c = c;
      this.ab = ab;
      this.bc = bc;
      this.ca = ca;
      this.a.triangles.Add(this);
      this.b.triangles.Add(this);
      this.c.triangles.Add(this);
    }

    public GeoTriangle Clone()
    {
      var na = a;
      var nb = b;
      var nc = c;
      var nab = new GeoLine(na, nb, ab.payload);
      var nbc = new GeoLine(nb, nc, bc.payload);
      var nca = new GeoLine(nc, na, ca.payload);
      return new GeoTriangle(a, b, c, nab, nbc, nca);
    }

    public void UpdateLinesPoints()
    {
      ab.a.lines.Remove(ab);
      ab.a = a;
      ab.a.lines.Add(ab);
      
      ab.b.lines.Remove(ab);
      ab.b = b;
      ab.b.lines.Add(ab);
      
      bc.a.lines.Remove(bc);
      bc.a = b;
      bc.a.lines.Add(bc);
      
      bc.b.lines.Remove(bc);
      bc.b = c;
      bc.b.lines.Add(bc);
      
      ca.a.lines.Remove(ca);
      ca.a = c;
      ca.a.lines.Add(ca);
      
      ca.b.lines.Remove(ca);
      ca.b = a;
      ca.b.lines.Add(ca);
    }

    private GeoPoint _circumcenter;
    private float _radiusSquared;

    public void DisconnectPoints()
    {
      a.lines.Clear();
      a.triangles.Remove(this);
      b.lines.Clear();
      b.triangles.Remove(this);
      c.lines.Clear();
      c.triangles.Remove(this);
    }

    private void UpdateCircumcircle()
    {
      // https://codefound.wordpress.com/2013/02/21/how-to-compute-a-circumcircle/#more-58
      // https://en.wikipedia.org/wiki/Circumscribed_circle
      var dA = a.x * a.x + a.y * a.y;
      var dB = b.x * b.x + b.y * b.y;
      var dC = c.x * c.x + c.y * c.y;

      var aux1 = dA * (c.y - b.y) + dB * (a.y - c.y) + dC * (b.y - a.y);
      var aux2 = -(dA * (c.x - b.x) + dB * (a.x - c.x) + dC * (b.x - a.x));
      var div = 2 * (a.x * (c.y - b.y) + b.x * (a.y - c.y) + c.x * (b.y - a.y));

      if (div == 0) return;

      var center = new GeoPoint(aux1 / div, aux2 / div, default);
      _circumcenter = center;
      _radiusSquared = (center.x - a.x) * (center.x - a.x) + (center.y - a.y) * (center.y - a.y);
    }
    
    public bool IsPointInsideCircumcircle(GeoPoint geoPoint)
    {
      var dSquared = (geoPoint.x - _circumcenter.x) * (geoPoint.x - _circumcenter.x) +
                     (geoPoint.y - _circumcenter.y) * (geoPoint.y - _circumcenter.y);
      return dSquared < _radiusSquared;
    }
    
    public bool ContainsPoint(GeoPoint geoPoint)
    {
      return a.Equals(geoPoint) || b.Equals(geoPoint) || c.Equals(geoPoint);
    }
    
    private static bool IsCounterClockwise(GeoPoint point1, GeoPoint point2, GeoPoint point3)
    {
      var result = (point2.x - point1.x) * (point3.y - point1.y) -
                   (point3.x - point1.x) * (point2.y - point1.y);
      return result > 0;
    }
  }
}
