using Silk.NET.Windowing;

namespace Planets
{
    class Program
    {
        static void Main()
        {
            var ops = WindowOptions.Default;
            ops.Size = new Silk.NET.Maths.Vector2D<int>(1280, 720);
            ops.Title = "Planets Rendering Project";
            var window = Window.Create(ops);

            using Planets planets = new(window);
            planets.Run();
        }
    }
}
