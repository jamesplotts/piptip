using System.Collections.Generic;
using Model;
using UI;
using UI.CarInspector;
using UI.ContextMenu;

namespace PipTip
{
    // The game's own car right-click menu, plus Disconnect from Train.
    internal static class PipMenu
    {
        public static void Show(Car car, List<Car> cars, int index)
        {
            UI.ContextMenu.ContextMenu menu = UI.ContextMenu.ContextMenu.Shared;
            TrainController trainController = TrainController.Shared;
            if (menu == null || trainController == null)
                return;
            if (UI.ContextMenu.ContextMenu.IsShown)
                menu.Hide();
            menu.Clear();

            menu.AddButton(ContextMenuQuadrant.General, "Inspect", SpriteName.Inspect, () => CarInspector.Show(car));
            menu.AddButton(ContextMenuQuadrant.General, trainController.SelectedCar == car ? "Deselect" : "Select", SpriteName.Select, () =>
            {
                trainController.SelectedCar = trainController.SelectedCar == car ? null : car;
            });
            if (car.SupportsBleed())
                menu.AddButton(ContextMenuQuadrant.Brakes, "Bleed", SpriteName.Bleed, car.SetBleed);
            menu.AddButton(ContextMenuQuadrant.Brakes, car.air.handbrakeApplied ? "Release Handbrake" : "Apply Handbrake", SpriteName.Handbrake, () =>
            {
                car.SetHandbrake(!car.air.handbrakeApplied);
            });
            if (Consist.TryGetEngineSideNeighbor(cars, index, out Car neighbor))
                menu.AddButton(ContextMenuQuadrant.Unused2, "Disconnect from Train", () => Consist.Disconnect(car, neighbor));
            List<(Car car, Car neighbor)> engineCuts = Consist.CarsCoupledToEngine(car);
            if (engineCuts.Count > 0)
                menu.AddButton(ContextMenuQuadrant.Unused2, "Disconnect Cars from Engine", () =>
                {
                    // Set the handbrake on the first car of each cut so the cars stay put.
                    foreach ((Car unit, Car other) in engineCuts)
                    {
                        other.SetHandbrake(true);
                        Consist.Disconnect(unit, other);
                    }
                });

            menu.Show(car.DisplayName);
        }
    }
}
