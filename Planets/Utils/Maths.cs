using System;
using System.Numerics;

namespace Planets.Utils
{
    public static class Maths
    {

        public static float AngleFromPointOnUnitCircle(Vector2 point)
        {
            float x = point.X;
            float y = point.Y;

            x = MathF.Max(x, -1.0f);
            x = MathF.Min(x, 1.0f);

            float angle = float.NaN;

            if (y >= 0.0f)
            {
                angle = MathF.Acos(x);
            }
            else if (y <= 0.0f)
            {
                angle = MathF.PI + (MathF.PI - MathF.Acos(x));
            }

            return angle;
        }

    }
}
