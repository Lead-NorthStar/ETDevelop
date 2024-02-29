﻿using System;

namespace Jolt
{
    public struct ObjectVsBroadPhaseLayerFilter : IEquatable<ObjectVsBroadPhaseLayerFilter>
    {
        internal NativeOwnedHandle<JPH_ObjectVsBroadPhaseLayerFilter> Handle;

        internal ObjectVsBroadPhaseLayerFilter(NativeOwnedHandle<JPH_ObjectVsBroadPhaseLayerFilter> handle)
        {
            Handle = handle;
        }

        #region IEquatable

        public bool Equals(ObjectVsBroadPhaseLayerFilter other)
        {
            return Handle.Equals(other.Handle);
        }

        public override bool Equals(object obj)
        {
            return obj is ObjectVsBroadPhaseLayerFilter other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Handle.GetHashCode();
        }

        #endregion
    }
}
