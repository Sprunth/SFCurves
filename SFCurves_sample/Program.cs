using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFCurves;
using SFML.Graphics;
using SFML.Window;

namespace SFCurves_sample
{
    class Program
    {
        private static RenderWindow win;

        private static Curve curve;

        static void Main(string[] args)
        {
            win = new RenderWindow(new VideoMode(1200, 800), "SFCurves Sample");
            win.Closed += win_Closed;
            win.SetFramerateLimit(60);

            curve = new Curve();
            curve.AddPoint(new BezierPoint(100, 100));
            curve.AddPoint(new BezierPoint(300, 400));
            curve.AddPoint(new BezierPoint(800, 100));
            curve.AddPoint(new BezierPoint(900, 500));
            

            while (win.IsOpen())
            {
                win.DispatchEvents();

                win.Clear(new Color(210, 190, 170));
                win.Draw(curve);
                win.Display();
            }
        }

        static void win_Closed(object sender, EventArgs e)
        {
            win.Close();
        }
    }
}
