namespace Jolt
{
    public static class JoltRuntimeInitialization
    {
        private static bool initialized;

        public static bool Initialize()
        {
            if (initialized) return false;

            NativeSafetyHandle.Initialize();
            Jolt.SetAssertFailureHandler(OnAssertFailure);

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
        
        private static bool OnAssertFailure(string expr, string message, string file, uint line)
        {
            // Debug.Log($"Jolt Assertion Failed:\n{expr}\n{message}\n{file}\n{line}");
            return false;
        }
    }
}
