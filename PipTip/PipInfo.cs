using System.Collections.Generic;
using System.Linq;
using System.Text;
using Helpers;
using Model;
using Model.Definition.Data;
using Model.Ops;
using Model.Ops.Definition;
using UI.Tags;
using UnityEngine;

namespace PipTip
{
    // What the callout says about one car: title, body text and the Tab-tag color.
    internal static class PipInfo
    {
        private const string Rule = "<alpha=#40>───<alpha=#ff>";

        public static string Title(Car car) => car.DisplayName.NoParse();

        public static Color TagColor(Car car) => CarTagColor.For(car, OpsController.Shared);

        public static string Text(Car car)
        {
            List<string> lines = new List<string>();
            Car partner = Consist.Partner(car);
            // Show an engine/tender pair as one: the engine's info, then the tender's fuel.
            Car engine = partner != null && partner.IsLocomotive ? partner : car;
            Car tender = engine == car ? partner : car;

            OpsController ops = OpsController.Shared;
            if (ops != null && ops.TryGetDestinationInfo(car, out string destinationName, out bool isAtDestination, out _, out _))
                lines.Add((isAtDestination ? "<sprite name=\"Spotted\">" : "<sprite name=\"Destination\">") + " " + destinationName);
            else if (car.TryGetTrainName(out string trainName))
                lines.Add(trainName);

            if (partner != null)
            {
                AddLoads(lines, engine);
                AddLoads(lines, tender);
            }
            else if (!AddLoads(lines, car) && (car.Definition.LoadSlots.Count > 0 || car.IsPassengerCar()))
            {
                lines.Add(TextSprites.PiePercent(0f, 1f) + " Empty");
            }

            if (lines.Count > 0)
                lines.Add(Rule);
            if (partner != null)
            {
                lines.Add("Condition " + Percent(engine.Condition) + " engine, " + Percent(tender.Condition) + " tender");
                if (engine.EnableOiling || tender.EnableOiling)
                    lines.Add("Wheel oil " + Percent(engine.Oiled) + " engine, " + Percent(tender.Oiled) + " tender");
            }
            else
            {
                lines.Add("Condition " + Percent(car.Condition));
                if (car.EnableOiling)
                    lines.Add("Wheel oil " + Percent(car.Oiled));
            }

            if (car.air.handbrakeApplied)
                lines.Add("<sprite name=\"HandbrakeWheel\"> Handbrake");
            if (car.HasHotbox)
                lines.Add("<sprite name=\"Flame\"> Hotbox");
            if (car.IsDerailed)
                lines.Add("Derailed");
            return string.Join("\n", lines);
        }

        // Load slots (freight, coal, water, fuel) and passengers. False if nothing is aboard.
        private static bool AddLoads(List<string> lines, Car car)
        {
            bool any = false;
            foreach ((LoadSlot slot, int index) in car.Definition.DisplayOrderLoadSlots())
            {
                CarLoadInfo? info = car.GetLoadInfo(index);
                if (!info.HasValue)
                    continue;
                Load load = CarLoadLibrary.GetLoad(info.Value.LoadId);
                if (load == null)
                    continue;
                float capacity = slot.CalculateMaximumCapacity(load, car);
                lines.Add(TextSprites.PiePercent(info.Value.Quantity, capacity) + " " + info.Value.LoadString(load));
                any = true;
            }
            if (car.IsPassengerCar())
            {
                PassengerMarker? marker = car.GetPassengerMarker();
                if (marker.HasValue)
                {
                    lines.Add(Passengers(car, marker.Value));
                    any = true;
                }
            }
            return any;
        }

        private static string Passengers(Car car, PassengerMarker marker)
        {
            StringBuilder text = new StringBuilder(car.PassengerCountString(marker) + " Passengers");
            HashSet<string> stops = marker.Groups.Where(g => g.Count > 0)
                .Select(g => PassengerStop.ShortNameForIdentifier(g.Destination)).ToHashSet();
            if (stops.Count > 0)
                text.Append(" to " + string.Join(", ", stops));
            return text.ToString();
        }

        // Red under 40%, amber under 70%.
        private static string Percent(float fraction)
        {
            string text = Mathf.RoundToInt(Mathf.Clamp01(fraction) * 100f) + "%";
            if (fraction < 0.4f)
                return "<color=#FF6A5C>" + text + "</color>";
            if (fraction < 0.7f)
                return "<color=#FFC65C>" + text + "</color>";
            return text;
        }
    }
}
