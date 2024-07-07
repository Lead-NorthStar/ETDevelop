namespace Jolt
{
    public static class JoltRuntimeInitialization
    {
        private static bool initialized;

        private const uint DefaultTempAllocatorSize = 10 * 1024 * 1024; // 10MB

        public static bool Initialize()
        {
            if (initialized) return false;

            NativeSafetyHandle.Initialize();

            if (!Jolt.Initialize())
                return false;

            initialized = true;

            return true;
        }

        public static void Destroy()
        {
            if (!initialized)
                return;
            
            Jolt.Shutdown();
            // NativeSafetyHandle.Deinitialize();

            initialized = false;
        }
    }
}
