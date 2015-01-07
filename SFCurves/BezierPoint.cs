using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFML.Window;

namespace SFCurves
{
    public class BezierPoint
    {
        private Vector2f _vec;
        public Vector2f Vector { get { return _vec; } }
        public float X { get { return _vec.X; } set { _vec.X = value; } }
        public float Y { get { return _vec.Y; } set { _vec.Y = value; } }

        public BezierPoint(float x, float y) : this(new Vector2f(x, y))
        { }

        public BezierPoint(Vector2f vec)
        {
            _vec = vec;
        }
    }
}
