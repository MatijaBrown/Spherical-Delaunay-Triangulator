using System;
using System.Numerics;

namespace Planets.Utils
{
    public static class Maths
    {

        public const float SMALL = 1E-6F;

        public static float AngleFromPointOnUnitCircle(Vector2 point)
        {
            float x = point.X;
            float y = point.Y;

            x = MathF.Max(x, -1.0f);
            x = MathF.Min(x, 1.0f);

            float angle = float.NaN;

            if (y >= 0.0f)
            {
                angle = (float)Math.Acos((double)x);
            }
            else if (y <= 0.0f)
            {
                angle = MathF.PI + (MathF.PI - MathF.Acos(x));
            }

            angle = MathF.Max(angle, 0.0f);
            angle = MathF.Min(angle, 2.0f * MathF.PI - SMALL); // Make sure we get nothing on the border.

            return angle;
        }

        public static float TripleProduct(Vector3 a, Vector3 b, Vector3 c)
        {
            return Vector3.Dot(a, Vector3.Cross(b, c));
        }

        public static float Determinant(Vector3 a, Vector3 b, Vector3 c)
        {
            return 
                (a.X * b.Y * c.Z) +
                (a.Y * b.Z * c.X) +
                (a.Z * b.X * c.Y) -
                (a.Z * b.Y * c.X) -
                (a.Y * b.X * c.Z) -
                (a.X * b.Z * c.Y);
        }

        public static bool IsCCW(Vector3 a, Vector3 b, Vector3 c)
        {
            float det = Determinant(a, b, c);
            float trip = TripleProduct(a, b, c);
            return Determinant(a, b, c) >= 0.0f;
        }

        public static bool IsLeft(Vector3 p, Vector3 firstPointOnLine, Vector3 secondPointOnLine)
        {
            return IsCCW(p, firstPointOnLine, secondPointOnLine);
        }

        public static Vector3 Circumcentre(Vector3 a, Vector3 b, Vector3 c)
        {
            return Vector3.Cross(b - a, c - a) / Vector3.Cross(b - a, c - a).Length();
        }

        public static float Circumradius(Vector3 a, Vector3 b, Vector3 c)
        {
            return MathF.Acos(TripleProduct(a, b, c) / (Vector3.Cross(a, b) + Vector3.Cross(b, c) + Vector3.Cross(c, a)).Length());
        }

        public static float ArcLength(Vector3 a, Vector3 b)
        {
            return MathF.Acos(Vector3.Dot(a, b));
        }

    }
}
