using System.Collections.Generic;
using Model;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace PipTip
{
    // Added to each car pip: hover shows the info callout, right-click shows the car menu.
    internal sealed class PipHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        private TrainBrakeDisplay _display;
        private int _index;

        public void Bind(TrainBrakeDisplay display, int index)
        {
            _display = display;
            _index = index;
        }

        public List<Car> Cars => BrakeDisplayAccess.Cars(_display);

        public int Index => _index;

        public Car Car
        {
            get
            {
                List<Car> cars = Cars;
                return cars != null && _index >= 0 && _index < cars.Count ? cars[_index] : null;
            }
        }

        public void OnPointerEnter(PointerEventData eventData) => PipTooltip.Show(this);

        public void OnPointerExit(PointerEventData eventData) => PipTooltip.Hide(this);

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Right)
            {
                Car car = Car;
                if (car != null)
                {
                    PipTooltip.Hide(this);
                    PipMenu.Show(car, Cars, _index);
                }
                return;
            }
            // Leave other clicks to whatever handled them before this mod.
            if (transform.parent != null)
                ExecuteEvents.ExecuteHierarchy(transform.parent.gameObject, eventData, ExecuteEvents.pointerClickHandler);
        }

        private void OnDisable() => PipTooltip.Hide(this);

        public static void RemoveAll()
        {
            foreach (PipHover hover in FindObjectsByType<PipHover>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                Destroy(hover);
        }
    }
}
