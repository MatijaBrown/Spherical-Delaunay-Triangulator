using System;

namespace Planets.Utils
{
    public struct Vector3D
    {

        public static readonly Vector3D UnitY = new(0, 1, 0);

        public double X;
        public double Y;
        public double Z;

        public Vector3D(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public override string ToString()
        {
            return "Vector3D(" + X.ToString().Replace(',', '.') + ", " + Y.ToString().Replace(',', '.') + ", " + Z.ToString().Replace(',', '.') + ")";
        }

        public static Vector3D operator +(Vector3D left, Vector3D right)
        {
            return new Vector3D()
            {
                X = left.X + right.X,
                Y = left.Y + right.Y,
                Z = left.Z + right.Z
            };
        }

        public static Vector3D operator -(Vector3D vec)
        {
            return new Vector3D()
            {
                X = -vec.X,
                Y = -vec.Y,
                Z = -vec.Z
            };
        }

        public static Vector3D operator -(Vector3D left, Vector3D right)
        {
            return left + (-right);
        }

        public static Vector3D operator *(Vector3D left, double right)
        {
            return new Vector3D()
            {
                X = left.X * right,
                Y = left.Y * right,
                Z = left.Z * right
            };
        }

        public static Vector3D operator *(double left, Vector3D right)
        {
            return right * left;
        }

        public static Vector3D operator /(Vector3D left, double right)
        {
            return new Vector3D()
            {
                X = left.X / right,
                Y = left.Y / right,
                Z = left.Z / right
            };
        }

        public double LengthSquared()
        {
            return X * X + Y * Y + Z * Z;
        }

        public double Length()
        {
            return Math.Sqrt(LengthSquared());
        }

        public static Vector3D Normalize(Vector3D vector)
        {
            double length = vector.Length();
            return new Vector3D()
            {
                X = vector.X / length,
                Y = vector.Y / length,
                Z = vector.Z / length
            };
        }

        public static double Dot(Vector3D a, Vector3D b)
        {
            return a.X * b.X + a.Y * b.Y + a.Z * b.Z;
        }

        public static Vector3D Cross(Vector3D a, Vector3D b)
        {
            return new Vector3D()
            {
                X = (a.Y * b.Z) - (a.Z * b.Y),
                Y = (a.Z * b.X) - (a.X * b.Z),
                Z = (a.X * b.Y) - (a.Y * b.X)
            };
        }

    }
}
