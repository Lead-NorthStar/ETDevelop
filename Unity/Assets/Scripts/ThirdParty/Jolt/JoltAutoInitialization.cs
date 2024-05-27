namespace Jolt
{
    public static class JoltAutoInitialization
    {
        private static bool initialized;

        private const uint DefaultTempAllocatorSize = 10 * 1024 * 1024; // 10MB

        public static bool Initialize()
        {
            if (initialized) return false;

            NativeSafetyHandle.Initialize();

            if (!SafeBindings.JPH_Init(DefaultTempAllocatorSize))
                return false;

            SafeBindings.JPH_SetAssertFailureHandler(OnAssertFailure);
            initialized = true;

            return true;
        }
        
        private static void OnAssertFailure(string expr, string message, string file, uint line)
        {
            // Log.Error($"Jolt Assertion Failed:\n{expr}\n{message}\n{file}\n{line}");
        }

        public static void Destroy()
        {
            if (!initialized)
                return;
            
            SafeBindings.JPH_Shutdown();
            NativeSafetyHandle.Deinitialize();

            initialized = false;
        }
    }
}