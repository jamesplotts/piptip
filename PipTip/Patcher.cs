using System;
using System.Collections.Generic;
using HarmonyLib;
using Model;
using UI;
using UnityEngine.UI;

namespace PipTip
{
    internal static class Patcher
    {
        private static Harmony _harmony;

        public static void Patch(string id)
        {
            _harmony = new Harmony(id);
            try
            {
                _harmony.PatchAll(typeof(Patcher).Assembly);
                ModEntry.Log?.Info("PipTip: patched TrainBrakeDisplay.");
            }
            catch (Exception ex)
            {
                // A game update renamed something: leave the game untouched.
                _harmony.UnpatchAll(id);
                _harmony = null;
                ModEntry.Log?.Error("PipTip: patching failed, mod inactive: " + ex.Message);
            }
        }

        public static void Unpatch()
        {
            _harmony?.UnpatchAll(_harmony.Id);
            _harmony = null;
        }
    }

    // The strip of car pips under the selected engine's name. Each pip is an Image made
    // by GetCarImage(index, ...) and stands for _displayCars[index].
    [HarmonyPatch(typeof(TrainBrakeDisplay), "GetCarImage")]
    internal static class GetCarImagePatch
    {
        private static void Postfix(TrainBrakeDisplay __instance, int index, Image __result)
        {
            PipHover hover = __result.GetComponent<PipHover>();
            if (hover == null)
                hover = __result.gameObject.AddComponent<PipHover>();
            hover.Bind(__instance, index);
        }
    }

    // The air-joint markers overlap the top of the pips; keep them from eating the hover.
    [HarmonyPatch(typeof(TrainBrakeDisplay), "GetAirImage")]
    internal static class GetAirImagePatch
    {
        private static void Postfix(Image __result)
        {
            __result.raycastTarget = false;
        }
    }

    internal static class BrakeDisplayAccess
    {
        private static readonly AccessTools.FieldRef<TrainBrakeDisplay, List<Car>> DisplayCars =
            AccessTools.FieldRefAccess<TrainBrakeDisplay, List<Car>>("_displayCars");

        public static List<Car> Cars(TrainBrakeDisplay display) => display == null ? null : DisplayCars(display);
    }
}
