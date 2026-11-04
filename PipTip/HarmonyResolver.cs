using System;
using System.IO;
using System.Reflection;

namespace PipTip
{
    // Finds the 0Harmony.dll shipped in the mod folder if nothing else has loaded Harmony yet.
    internal static class HarmonyResolver
    {
        private static string _modDirectory;
        private static bool _installed;

        public static void Install(string modDirectory)
        {
            _modDirectory = modDirectory;
            if (_installed)
                return;
            AppDomain.CurrentDomain.AssemblyResolve += Resolve;
            _installed = true;
        }

        private static Assembly Resolve(object sender, ResolveEventArgs args)
        {
            if (new AssemblyName(args.Name).Name != "0Harmony" || _modDirectory == null)
                return null;
            foreach (Assembly loaded in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (loaded.GetName().Name == "0Harmony")
                    return loaded;
            }
            string path = Path.Combine(_modDirectory, "0Harmony.dll");
            return File.Exists(path) ? Assembly.LoadFrom(path) : null;
        }
    }
}
