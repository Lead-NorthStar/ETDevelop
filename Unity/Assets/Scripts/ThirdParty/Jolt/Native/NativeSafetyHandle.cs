using System;
using System.Runtime.CompilerServices;
using NativeCollection;

namespace Jolt
{
    #if !JOLT_DISABLE_SAFETY_CHECkS
    
    /// <summary>
    /// A safety handle for detecting use-after-free access of native objects.
    /// </summary>
    public struct NativeSafetyHandle
    {
        // Initially I tried to reuse AtomicSafetyHandle to offload complexity out of the lib, but
        // AtomicSafetyHandle is tightly coupled to the ENABLE_UNITY_COLLECTIONS_CHECKS scripting
        // define, and ideally the Jolt safety checks can be enabled independently.

        // TODO investigate more sophisticated use-after-free safety checks

        public uint Index;

        private static uint nextHandleIndex;

        private static HashSet<uint> disposed;

        //[RuntimeInitializeOnLoadMethod]
        public static void Initialize()
        {
            if (disposed != null && !disposed.IsDisposed) disposed.Dispose();

            disposed = new HashSet<uint>(1024);
        }

        /// <summary>
        /// Dispose the internal safety handle state.
        /// </summary>
        public static void Deinitialize()
        {
            if (disposed.IsDisposed) return;

            // TODO check for unreleased safety handles?

            disposed.Dispose();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeSafetyHandle Create()
        {
            return new NativeSafetyHandle { Index = nextHandleIndex++ };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Release(in NativeSafetyHandle handle)
        {
            if (disposed.Contains(handle.Index))
            {
                //Debug.LogWarning("A NativeSafetyHandle is being released for a handle index that was already released.");
            }

            disposed.Add(handle.Index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AssertExists(in NativeSafetyHandle handle)
        {
            // TODO handle threading

            if (disposed.Contains(handle.Index))
            {
                throw new ObjectDisposedException("The native resource has been disposed.");
            }
        }
    }

    #endif
}
