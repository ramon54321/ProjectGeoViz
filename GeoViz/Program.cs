using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace GeoViz
{
  static class Program
  {
    [STAThread]
    static void Main()
    {
      AttachConsole(-1);
      Application.SetHighDpiMode(HighDpiMode.SystemAware);
      Application.EnableVisualStyles();
      Application.SetCompatibleTextRenderingDefault(false);
      Application.Run(new MainForm());
    }
    
    [DllImport("kernel32.dll")]
    static extern bool AttachConsole(int dwProcessId);
  }

  public class MainForm : Form
  {
    private readonly IEnumerable<IRenderable> _renderables = new List<IRenderable>
    {
        new Triangulation(),
    };
    
    public MainForm()
    {
      Width = 1041;
      Height = 1065;
    }

    private int _ticks;
    private int _ticksLastFrame;

    protected override void OnLoad(EventArgs e)
    {
      base.OnLoad(e);
      
      BackColor = Color.Black;
      DoubleBuffered = true;

      MouseDown += (sender, args) => _isMouseHold = true;
      MouseUp += (sender, args) => _isMouseHold = false;
      
      foreach (var renderable in _renderables)
      {
        renderable.Start();
      }
      
      var tickTimer = new Timer();
      tickTimer.Interval = (int) (1000f / 120f);
      tickTimer.Tick += (sender, args) =>
      {
        Refresh();
        _ticks++;
      };
      tickTimer.Start();

      var fpsTimer = new Timer();
      fpsTimer.Interval = 1000;
      fpsTimer.Tick += (sender, args) =>
      {
        _ticksLastFrame = _ticks;
        _ticks = 0;
      };
      fpsTimer.Start();
    }
    
    private bool _wasMouseHold;
    private bool _isMouseHold;
    
    protected override void OnPaint(PaintEventArgs e)
    {
      base.OnPaint(e);
      var graphics = e.Graphics;
      graphics.Clear(Color.Black);
      graphics.DrawString($"FPS: {_ticksLastFrame}", SystemFonts.DefaultFont, Brushes.Gray, 10, 10);
      var mouseUp = _wasMouseHold && !_isMouseHold;
      var mouseDown = !_wasMouseHold && _isMouseHold;
      var surface = new Surface(this, graphics, 8, mouseUp, mouseDown, _isMouseHold);
      foreach (var renderable in _renderables)
      {
        renderable.Render(surface);
      }
      _wasMouseHold = _isMouseHold;
    }
  }
  
  public class Surface
  {
    private const int WIDTH_PX = 1024;
    private const int HEIGHT_PX = 1024;
      
    private readonly float _logicMinX;
    private readonly float _logicMinY;
    private readonly float _logicMaxX;
    private readonly float _logicMaxY;

    private readonly float _spanX;
    private readonly float _spanY;
    private readonly int _spanPxX;
    private readonly int _spanPxY;

    private readonly Control _control;
    private readonly Graphics _graphics;
    
    public Surface(Control control, Graphics graphics, float span, bool mouseUp, bool mouseDown, bool mouseHold)
    {
      _logicMinX = -span;
      _logicMinY = span;
      _logicMaxX = span;
      _logicMaxY = -span;
      _spanX = _logicMaxX - _logicMinX;
      _spanY = _logicMaxY - _logicMinY;
      _spanPxX = (int) (WIDTH_PX / _spanX);
      _spanPxY = (int) (HEIGHT_PX / _spanY);
      MouseUp = mouseUp;
      MouseDown = mouseDown;
      MouseHold = mouseHold;
      _control = control;
      _graphics = graphics;
      _graphics.SmoothingMode = SmoothingMode.HighQuality;
    }

    public Point MousePosition => _control.PointToClient(Cursor.Position);
    public bool MouseUp { get; }
    public bool MouseDown { get; }
    public bool MouseHold { get; }

    public void DrawLine<A, B>(Pen pen, GeoPoint<A> a, GeoPoint<B> b)
    {
      DrawLine(pen, GeoToCanvas(a), GeoToCanvas(b));
    }
    
    public void DrawLine(Pen pen, Point a, Point b)
    {
      _graphics.DrawLine(pen, a, b);
    }
    
    public void DrawDot<A>(Brush brush, GeoPoint<A> a, float radius = 4f)
    {
      DrawDot(brush, GeoToCanvas(a), radius);
    }
    
    public void DrawDot(Brush brush, Point a, float radius = 4f)
    {
      _graphics.FillEllipse(brush, a.X - radius, a.Y - radius, radius * 2, radius * 2);
    }
    
    public void DrawTriangle<A>(Brush brush, GeoTriangle<A> geoTriangle)
    {
      _graphics.FillPolygon(brush, new []
      {
          GeoToCanvas(geoTriangle.a),
          GeoToCanvas(geoTriangle.b),
          GeoToCanvas(geoTriangle.c),
      });
    }
    
    public void DrawText<A>(Brush brush, GeoPoint<A> geoPoint, string text)
    {
      DrawText(brush, GeoToCanvas(geoPoint), text);
    }

    public void DrawText(Brush brush, Point point, string text)
    {
      var stringFormat = new StringFormat();
      stringFormat.Alignment = StringAlignment.Center;
      stringFormat.LineAlignment = StringAlignment.Center;
      _graphics.DrawString(text, SystemFonts.DefaultFont, brush, point, stringFormat);
    }

    public Point GeoToCanvas<A>(GeoPoint<A> geoPoint)
    {
      return new Point((int)((geoPoint.x - _logicMinX) * _spanPxX), (int)((geoPoint.y - _logicMinY) * _spanPxY));
    }
    
    public GeoPoint<int> CanvasToGeo(Point point)
    {
      return new GeoPoint<int>((float)point.X / _spanPxX + _logicMinX, (float)point.Y / _spanPxY + _logicMinY, 0);
    }
  }
}
