using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFML.Graphics;
using SFML.Window;

namespace SFCurves
{
    public class Curve : Drawable
    {
        private readonly List<BezierPoint> _points  = new List<BezierPoint>();
        private readonly List<Curve> _curves = new List<Curve>();
        private bool NeedsToSplit {get { return _points.Count != 0; }} // whether to look at _points list and/or _curves list (and recurse)
        private VertexArray _array = new VertexArray(PrimitiveType.LinesStrip);
        private VertexArray _array2 = new VertexArray(PrimitiveType.LinesStrip);
        private bool _isDirty = false;
        
        public Curve()
        {
            
        }

        public void AddPoint(BezierPoint bp)
        {
            _isDirty = true;

            _points.Add(bp);
        }

        private VertexArray RegenerateVertexArray()
        {
            // Optimize instead of clear/restart?

            var array = new VertexArray(PrimitiveType.LinesStrip);

            if (NeedsToSplit && IsFlat())
            {
                _points.ForEach(point => array.Append(new Vertex(point.Vector)));
                return array;
            }
            if (NeedsToSplit) // and is not flat
            {
                Console.WriteLine("Splitting");
                // split 4 points into 2 curves of 4 points each
                var curveLeft = new Curve();
                var curveRight = new Curve();

                var a0 = Midpoint(_points[0].Vector, _points[1].Vector);
                var a1 = Midpoint(_points[1].Vector, _points[2].Vector);
                var a2 = Midpoint(_points[2].Vector, _points[3].Vector);

                var b0 = Midpoint(a0, a1);
                var b1 = Midpoint(a1, a2);

                var c0 = Midpoint(b0, b1);

                curveLeft.AddPoint(new BezierPoint(_points[0].Vector));
                curveLeft.AddPoint(new BezierPoint(a0));
                curveLeft.AddPoint(new BezierPoint(b0));
                curveLeft.AddPoint(new BezierPoint(c0));

                curveRight.AddPoint(new BezierPoint(c0));
                curveRight.AddPoint(new BezierPoint(b1));
                curveRight.AddPoint(new BezierPoint(a2));
                curveRight.AddPoint(new BezierPoint(_points[3].Vector));

                _curves.Add(curveLeft);
                _curves.Add(curveRight);

                _points.RemoveRange(0, 4);
                
            }
            _curves.ForEach(curve =>
            {
                var l = curve.RegenerateVertexArray();
                for (uint i = 0; i < l.VertexCount; i++)
                    array.Append(l[i]);
            });
            

            return array;
        }

        private VertexArray OffSetArray()
        {
            var va = new VertexArray(PrimitiveType.LinesStrip);

            // Loop through every line segment
            for (uint i = 0; i < _array.VertexCount - 1; i++)
            {
                var point1 = _array[i].Position;
                var point2 = _array[i + 1].Position;

                var segmentAngle = Math.Atan2(point2.Y - point1.Y, point2.X - point1.X);
                var angle1 = segmentAngle - Math.PI/2;
                var angle2 = segmentAngle + Math.PI/2;

                var distance = 25f;

                var newPoint1a = new Vector2f(
                    (float) Math.Round(point1.X + distance*Math.Cos(angle1)),
                    (float) Math.Round(point1.Y + distance*Math.Sin(angle1))
                    );
                var newPoint1b = new Vector2f(
                    (float)Math.Round(point2.X + distance * Math.Cos(angle1)),
                    (float)Math.Round(point2.Y + distance * Math.Sin(angle1))
                    );
                var newPoint2a = new Vector2f(
                    (float)Math.Round(point1.X + distance * Math.Cos(angle2)),
                    (float)Math.Round(point1.Y + distance * Math.Sin(angle2))
                    );
                var newPoint2b = new Vector2f(
                    (float)Math.Round(point2.X + distance * Math.Cos(angle2)),
                    (float)Math.Round(point2.Y + distance * Math.Sin(angle2))
                    );

                // no clipping or stitching of new points
                va.Append(new Vertex(newPoint1a));
                va.Append(new Vertex(newPoint2a));
                va.Append(new Vertex(newPoint2b));
                va.Append(new Vertex(newPoint1b));
            }
            return va;
        }

        private bool IsFlat()
        {
            //http://jeremykun.com/2013/05/11/bezier-curves-and-picasso/
            //function isFlat(curve) {
            //   var tol = 10; // anything below 50 is roughly good-looking
            //
            //   var ax = 3.0*curve[1][0] - 2.0*curve[0][0] - curve[3][0]; ax *= ax;
            //   var ay = 3.0*curve[1][1] - 2.0*curve[0][1] - curve[3][1]; ay *= ay;
            //   var bx = 3.0*curve[2][0] - curve[0][0] - 2.0*curve[3][0]; bx *= bx;
            //   var by = 3.0*curve[2][1] - curve[0][1] - 2.0*curve[3][1]; by *= by;
            //
            //   return (Math.max(ax, bx) + Math.max(ay, by) <= tol);
            //}

            const int tol = 50;
            var ax = 3.0 * _points[1].X - 2.0 * _points[0].X - _points[3].X;
            ax *= ax;
            var ay = 3.0 * _points[1].Y - 2.0 * _points[0].Y - _points[3].Y;
            ay *= ay;
            var bx = 3.0 * _points[2].X - _points[0].X - 2.0 * _points[3].X;
            bx *= bx;
            var by = 3.0 * _points[2].Y - _points[0].Y - 2.0 * _points[3].Y;
            by *= by;

            return (Math.Max(ax, bx) + Math.Max(ay, by) <= tol);
        }

        public void Draw(RenderTarget target, RenderStates states)
        {
            
            if (_isDirty)
            {
                _isDirty = false;
                _array = RegenerateVertexArray();
                _array2 = OffSetArray();
            }

            target.Draw(_array2);
            target.Draw(_array);
            

            //return;
            _points.ForEach(point =>
            {
                target.Draw(new CircleShape(4)
                {
                    FillColor = Color.Red,
                    OutlineColor = Color.Blue,
                    OutlineThickness = 1,
                    Origin = new Vector2f(2,2),
                    Position = point.Vector,
                });
            });
            
            //_curves.ForEach(curve => curve.Draw(target, states));
        }

        Vector2f Midpoint(Vector2f v1, Vector2f v2)
        {
            return new Vector2f((v1.X + v2.X)/2f, (v1.Y + v2.Y)/2f);
        }
    }
}
