﻿using System;

namespace Jolt
{
    public struct BodyID : IEquatable<BodyID>
    {
        public uint Value;

        #region IEquatable

        public static bool operator ==(BodyID lhs, BodyID rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator !=(BodyID lhs, BodyID rhs)
        {
            return !lhs.Equals(rhs);
        }

        public bool Equals(BodyID other)
        {
            return Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            return obj is BodyID other && Equals(other);
        }

        public override int GetHashCode()
        {
            return (int) Value;
        }

        #endregion
    }
}
