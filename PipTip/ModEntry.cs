using Railroader.Modding;

namespace PipTip
{
    // Entry point for Railroader's built-in mod loader (0.6+). This class must not
    // touch Harmony types: they only resolve after HarmonyResolver is installed.
    public sealed class ModEntry : IModEntryPoint, IWorldLifecycle
    {
        internal static IModLogger Log;

        public void OnModLoad(IModContext context)
        {
            Log = context.Log;
            HarmonyResolver.Install(context.ModDirectory);
            Patcher.Patch(context.ModIdentifier);
        }

        public void OnModUnload()
        {
            PipTooltip.Shutdown();
            PipHover.RemoveAll();
            Patcher.Unpatch();
            Log = null;
        }

        public void OnWorldLoad()
        {
        }

        public void OnWorldUnload()
        {
            PipTooltip.Shutdown();
        }
    }
}
