using Jolt.Native;

namespace Jolt
{
    public static class JoltAutoInitialization 
    {
        private static bool initialized;

        private const uint DefaultTempAllocatorSize = 10 * 1024 * 1024; // 10MB

        public static void Initialize()
        {
            if (initialized) return;

            if (!JoltAPI.JPH_Init(DefaultTempAllocatorSize))
            {
                //Debug.LogError("JPH_Init failed");
            }

            initialized = true;

            //Application.quitting += Shutdown;
        }

        private static void Shutdown()
        {
            JoltAPI.JPH_Shutdown();

            initialized = false;
        }
    }
}
