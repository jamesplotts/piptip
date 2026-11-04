using System.Collections.Generic;
using Model;

namespace PipTip
{
    internal static class Consist
    {
        // The neighbor of cars[index] on the side of the nearest engine. False for an engine,
        // for its own tender, or when the strip has no engine.
        public static bool TryGetEngineSideNeighbor(List<Car> cars, int index, out Car neighbor)
        {
            neighbor = null;
            if (cars == null || index < 0 || index >= cars.Count)
                return false;
            Car car = cars[index];
            if (car.IsLocomotive)
                return false;

            int engine = -1;
            for (int distance = 1; distance < cars.Count && engine < 0; distance++)
            {
                if (index - distance >= 0 && cars[index - distance].IsLocomotive)
                    engine = index - distance;
                else if (index + distance < cars.Count && cars[index + distance].IsLocomotive)
                    engine = index + distance;
            }
            if (engine < 0)
                return false;

            neighbor = cars[engine < index ? index - 1 : index + 1];
            if (neighbor.IsLocomotive && IsTenderOf(car, neighbor))
            {
                neighbor = null;
                return false;
            }
            return EndFacing(car, neighbor, out _);
        }

        // For an engine (or a steam engine's tender): the coupling between the engine/tender pair
        // and its cars. Empty when there are cars on both sides, since then it isn't clear which
        // to drop. Another engine is left coupled, so an MU lash-up stays together.
        public static List<(Car car, Car neighbor)> CarsCoupledToEngine(Car car)
        {
            List<(Car, Car)> cuts = new List<(Car, Car)>();
            Car partner = Partner(car);
            if (!car.IsLocomotive && !(partner != null && partner.IsLocomotive))
                return cuts;
            foreach (Car unit in partner != null ? new[] { car, partner } : new[] { car })
            {
                foreach (Car.LogicalEnd end in new[] { Car.LogicalEnd.A, Car.LogicalEnd.B })
                {
                    Car other = unit.CoupledTo(end);
                    if (other != null && other != car && other != partner && !other.IsLocomotive)
                        cuts.Add((unit, other));
                }
            }
            if (cuts.Count > 1)
                cuts.Clear();
            return cuts;
        }

        // Pull the cut lever between the two cars, closing both angle cocks first so neither
        // side dumps its air (as the game does with the smart-air modifier held).
        public static void Disconnect(Car car, Car neighbor)
        {
            if (!EndFacing(car, neighbor, out Car.LogicalEnd end) || !EndFacing(neighbor, car, out Car.LogicalEnd neighborEnd))
                return;
            car.ApplyEndGearChange(end, Car.EndGearStateKey.Anglecock, 0f);
            neighbor.ApplyEndGearChange(neighborEnd, Car.EndGearStateKey.Anglecock, 0f);
            car.ApplyEndGearChange(end, Car.EndGearStateKey.CutLever, 1f);
        }

        public static bool EndFacing(Car car, Car other, out Car.LogicalEnd end)
        {
            end = Car.LogicalEnd.A;
            if (car.CoupledTo(Car.LogicalEnd.A) == other)
                return true;
            end = Car.LogicalEnd.B;
            return car.CoupledTo(Car.LogicalEnd.B) == other;
        }

        // A steam engine's tender: the tender car coupled directly to it.
        public static bool IsTenderOf(Car tender, Car engine)
        {
            return tender != null && engine is SteamLocomotive && tender.CarType.IsTender
                && (engine.CoupledTo(Car.LogicalEnd.A) == tender || engine.CoupledTo(Car.LogicalEnd.B) == tender);
        }

        // The engine/tender partner of a car, or null.
        public static Car Partner(Car car)
        {
            foreach (Car.LogicalEnd end in new[] { Car.LogicalEnd.A, Car.LogicalEnd.B })
            {
                Car other = car.CoupledTo(end);
                if (other == null)
                    continue;
                if (IsTenderOf(other, car) || IsTenderOf(car, other))
                    return other;
            }
            return null;
        }
    }
}
