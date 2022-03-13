using Silk.NET.Maths;
using System;

namespace Planets.Meshes.Planets
{
    public struct Simplex
    {

        // Vertex pointers to the simplex's points
        public uint A;
        public uint B;
        public uint C;

        // Pointers to the simplexes opposite each point.
        public int TA;
        public int TB;
        public int TC;

        public Simplex(uint a, int ta, uint b, int tb, uint c, int tc, bool cIsLeft)
        {
            A = a;
            TA = ta;
            if (cIsLeft)
            {
                B = b;
                TB = tb;
                C = c;
                TC = tc;
            }
            else
            {
                C = b;
                TC = tb;
                B = c;
                TB = tc;
            }
        }

        public Simplex(uint a, uint b, uint c, int tc, bool cIsLeft)
            : this(a, -1, b, -1, c, tc, cIsLeft) { }

        public int Opposite(uint p)
        {
            if (p == A)
            {
                return TA;
            }
            else if (p == B)
            {
                return TB;
            }
            else if (p == C)
            {
                return TC;
            }
            else
            {
                throw new ArgumentException(p + " is not a point of this triangle!");
            }
        }
        public void Opposite(uint p, int opp)
        {
            if (p == A)
            {
                TA = opp;
            }
            else if (p == B)
            {
                TB = opp;
            }
            else if (p == C)
            {
                TC = opp;
            }
            else
            {
                throw new ArgumentException(p + " is not a point of this triangle!");
            }
        }

        public uint GetThirdPoint(uint a, uint b)
        {
            if (((a == A) && (b == B)) || ((a == B) && (b == A)))
            {
                return C;
            }
            else if (((a == B) && (b == C)) || ((a == C) && (b == B)))
            {
                return A;
            }
            else if (((a == A) && (b == C)) || ((a == C) && (b == A)))
            {
                return B;
            }

            throw new ArgumentException(a + " and " + b + " are not points of this triangle!");
        }

        public (uint, uint) GetOthers(uint p)
        {
            if (p == A)
            {
                return (B, C);
            }
            else if (p == B)
            {
                return (A, C);
            }
            else if (p == C)
            {
                return (A, B);
            }

            throw new ArgumentException(p + " is not a point of this triangle!");
        }

    }
}
