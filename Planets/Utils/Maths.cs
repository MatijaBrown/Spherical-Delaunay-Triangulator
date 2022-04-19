using System;

namespace Planets.Utils
{
    public static class Maths
    {

        public static double TripleProduct(Vector3D a, Vector3D b, Vector3D c)
        {
            return Vector3D.Dot(a, Vector3D.Cross(b, c));
        }

        public static double Determinant(Vector3D a, Vector3D b, Vector3D c)
        {
            return 
                (a.X * b.Y * c.Z) +
                (a.Y * b.Z * c.X) +
                (a.Z * b.X * c.Y) -
                (a.Z * b.Y * c.X) -
                (a.Y * b.X * c.Z) -
                (a.X * b.Z * c.Y);
        }

        public static bool IsCCW(Vector3D a, Vector3D b, Vector3D c)
        {
            return TripleProduct(a, b, c) >= 0;
        }

        public static bool IsLeft(Vector3D p, Vector3D firstPointOnLine, Vector3D secondPointOnLine)
        {
            return IsCCW(p, firstPointOnLine, secondPointOnLine);
        }

        public static Vector3D Circumcentre(Vector3D a, Vector3D b, Vector3D c)
        {
            return Vector3D.Cross(b - a, c - a) / Vector3D.Cross(b - a, c - a).Length();
        }

        public static double Circumradius(Vector3D a, Vector3D b, Vector3D c)
        {
            return Math.Acos(TripleProduct(a, b, c) / (Vector3D.Cross(a, b) + Vector3D.Cross(b, c) + Vector3D.Cross(c, a)).Length());
        }

        public static double ArcLength(Vector3D a, Vector3D b)
        {
            return Math.Acos(Vector3D.Dot(a, b));
        }

    }
}
