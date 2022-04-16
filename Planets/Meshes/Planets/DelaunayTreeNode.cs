using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Planets.Meshes.Planets
{
    public class DelaunayTreeNode : IEnumerable<DelaunayTreeNode>
    {

        public uint A { get; private set; }

        public uint B { get; private set; }

        public uint C { get; private set; }

        public DelaunayTreeNode TA { get; set; }

        public DelaunayTreeNode TB { get; set; }

        public DelaunayTreeNode TC { get; set; }

        public DelaunayTreeNode[] Children { get; }

        public int ChildCount { get; private set; }

        public DelaunayTreeNode(uint a, uint b, uint c, DelaunayTreeNode ta, DelaunayTreeNode tb, DelaunayTreeNode tc)
        {
            A = a;
            B = b;
            C = c;
            TA = ta;
            TB = tb;
            TC = tc;
            Children = new DelaunayTreeNode[3];
            ChildCount = 0;
        }

        public DelaunayTreeNode(uint a, uint b, uint c)
            : this(a, b, c, null, null, null) { }

        public DelaunayTreeNode this[int index]
        {
            get
            {
                if ((index < 0) || (index >= ChildCount))
                {
                    return null;
                }
                return Children[index];
            }
        }

        public void AddChild(DelaunayTreeNode child)
        {
            if (ChildCount == Children.Length)
            {
                throw new IndexOutOfRangeException();
            }
            Children[ChildCount++] = child;
        }

        public void AddChild(uint a, uint b, uint c, DelaunayTreeNode ta, DelaunayTreeNode tb, DelaunayTreeNode tc)
        {
            AddChild(new DelaunayTreeNode(a, b, c, ta, tb, tc));
        }

        public void AddChild(uint a, uint b, uint c)
        {
            AddChild(new DelaunayTreeNode(a, b, c));
        }

        public void FlipVertexOrder()
        {
            uint cacheIndex = A;
            DelaunayTreeNode cacheAdjacent = TA;

            A = C;
            TA = TC;

            C = cacheIndex;
            TA = cacheAdjacent;
        }

        public DelaunayTreeNode Opposite(uint point)
        {
            if      (point == A) return TA;
            else if (point == B) return TB;
            else if (point == C) return TC;

            throw new Exception(point + " is not part of this triangle!");
        }

        public void SetOpposite(uint point, DelaunayTreeNode newNode)
        {
            if      (point == A) TA = newNode;
            else if (point == B) TB = newNode;
            else if (point == C) TC = newNode;
            else throw new Exception(point + " is not part of this triangle!");
        }

        public void ReplaceOpposite(DelaunayTreeNode oldNode, DelaunayTreeNode newNode)
        {
            if      (oldNode == TA) TA = newNode;
            else if (oldNode == TB) TB = newNode;
            else if (oldNode == TC) TC = newNode;
            else throw new Exception(oldNode + " is not an opposite to any point of triangle!");
        }

        public uint ThirdPoint(uint a, uint b)
        {
            if      (((a == A) && (b == B)) || ((b == A) && (a == B))) return C;
            else if (((a == B) && (b == C)) || ((b == B) && (a == C))) return A;
            else if (((a == C) && (b == A)) || ((b == C) && (a == A))) return B;

            throw new Exception(a + " or " + b + " is not part of this triangle!");
        }

        public void Clear()
        {
            ChildCount = -1;
        }

        private class DelaunayTreeEnumerator : IEnumerator<DelaunayTreeNode>
        {
            private readonly DelaunayTreeNode _node;

            private int _index;

            public DelaunayTreeEnumerator(DelaunayTreeNode node)
            {
                _node = node;
                _index = -1;
            }

            public DelaunayTreeNode Current => _node[_index];

            object IEnumerator.Current => throw new NotImplementedException();

            public bool MoveNext()
            {
                return ++_index < _node.ChildCount;
            }

            public void Reset()
            {
                _index = 0;
            }

            public void Dispose() { }
        }

        public IEnumerator<DelaunayTreeNode> GetEnumerator()
        {
            return new DelaunayTreeEnumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

    }
}
