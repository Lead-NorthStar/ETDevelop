using System;
using Jolt.Native;

namespace Jolt
{
    public static class JoltAutoInitialization 
    {
        private static bool initialized;

        private const uint DefaultTempAllocatorSize = 10 * 1024 * 1024; // 10MB

        public static bool Initialize()
        {
            if (initialized) return false;

            if (!JoltAPI.JPH_Init(DefaultTempAllocatorSize))
                return false;

            initialized = true;

            return true;
        }

        private static void Shutdown()
        {
            JoltAPI.JPH_Shutdown();

            initialized = false;
        }
    }
}
