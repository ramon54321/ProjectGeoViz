using System.Collections.Generic;

namespace GeoViz
{
  public class GeoTriangle<T>
  {
    public GeoPoint<T> a;
    public GeoPoint<T> b;
    public GeoPoint<T> c;

    public readonly GeoLine<T> ab;
    public readonly GeoLine<T> bc;
    public readonly GeoLine<T> ca;

    public IEnumerable<GeoLine<T>> Lines => new[] { ab, bc, ca };

    public GeoTriangle(GeoPoint<T> a, GeoPoint<T> b, GeoPoint<T> c)
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
      
      ab = new GeoLine<T>(this.a, this.b, a.payload);
      bc = new GeoLine<T>(this.b, this.c, b.payload);
      ca = new GeoLine<T>(this.c, this.a, c.payload);
      this.a.triangles.Add(this);
      this.b.triangles.Add(this);
      this.c.triangles.Add(this);

      UpdateCircumcircle();
    }

    private GeoTriangle
    (
      GeoPoint<T> a,
      GeoPoint<T> b,
      GeoPoint<T> c,
      GeoLine<T> ab,
      GeoLine<T> bc,
      GeoLine<T> ca
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

    public GeoTriangle<T> Clone()
    {
      var na = a.Clone();
      var nb = b.Clone();
      var nc = c.Clone();
      var nab = new GeoLine<T>(na, nb, ab.payload);
      var nbc = new GeoLine<T>(nb, nc, bc.payload);
      var nca = new GeoLine<T>(nc, na, ca.payload);
      return new GeoTriangle<T>(a, b, c, nab, nbc, nca);
    }

    public void UpdateLinesPoints()
    {
      ab.a = a;
      ab.b = b;
      bc.a = b;
      bc.b = c;
      ca.a = c;
      ca.b = a;
    }

    private GeoPoint<T> _circumcenter;
    private float _radiusSquared;

    public void DisconnectPoints()
    {
      a.triangles.Remove(this);
      b.triangles.Remove(this);
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

      var center = new GeoPoint<T>(aux1 / div, aux2 / div, default);
      _circumcenter = center;
      _radiusSquared = (center.x - a.x) * (center.x - a.x) + (center.y - a.y) * (center.y - a.y);
    }
    
    public bool IsPointInsideCircumcircle<A>(GeoPoint<A> geoPoint)
    {
      var dSquared = (geoPoint.x - _circumcenter.x) * (geoPoint.x - _circumcenter.x) +
                     (geoPoint.y - _circumcenter.y) * (geoPoint.y - _circumcenter.y);
      return dSquared < _radiusSquared;
    }
    
    public bool ContainsPoint<A>(GeoPoint<A> geoPoint)
    {
      return a.Equals(geoPoint) || b.Equals(geoPoint) || c.Equals(geoPoint);
    }
    
    private static bool IsCounterClockwise(GeoPoint<T> point1, GeoPoint<T> point2, GeoPoint<T> point3)
    {
      var result = (point2.x - point1.x) * (point3.y - point1.y) -
                   (point3.x - point1.x) * (point2.y - point1.y);
      return result > 0;
    }
  }
}
