/*
Copyright 2019 - 2021 Inetum
Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at
    http://www.apache.org/licenses/LICENSE-2.0
Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/

using umi3d.cdk.interaction.selection.zoneselection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace umi3d.cdk.interaction.selection.projector
{
    /// <summary>
    /// Projector for Selectable.
    /// Identifies the Selectable type and interact with it
    /// </summary>
    public class SelectableProjector : IProjector<Selectable>
    {
        /// <summary>
        /// Makes interactons available on the controller
        /// </summary>
        /// <param name="selectable"></param>
        /// <param name="controller"></param>
        public void Project(Selectable selectable, AbstractController controller)
        {
            var pointerEventData = new PointerEventData(EventSystem.current);
            selectable.SendMessage("OnPointerEnter", pointerEventData, SendMessageOptions.DontRequireReceiver);
        }

        /// <summary>
        /// Deselect an object
        /// </summary>
        /// <param name="selectable"></param>
        public void Release(Selectable selectable)
        {
            var pointerEventData = new PointerEventData(EventSystem.current);
            if (selectable != null) //happens when UI element is destroyed but not deselected
                selectable.SendMessage("OnPointerExit", pointerEventData, SendMessageOptions.DontRequireReceiver);
        }

        /// <summary>
        /// Triggers the interaction associated to the selectable for a pick interaction
        /// </summary>
        /// <param name="selectable"></param>
        /// <param name="controller"></param>
        public void Pick(Selectable selectable, AbstractController controller)
        {
            switch (selectable.GetType().Name)
            {
                case "Button":
                    Button button = (Button)selectable;
                    button.Interact();
                    break;

                case "Toggle":
                    Toggle toggle = (Toggle)selectable;
                    toggle.Interact();
                    break;

                case "Dropdown":
                    Dropdown dropdown = (Dropdown)selectable;
                    dropdown.Interact();
                    break;

                case "InputField":
                    InputField inputfield = (InputField)selectable;
                    inputfield.Interact();
                    break;

                case "Scrollbar":
                    Scrollbar scrollbar = (Scrollbar)selectable;
                    scrollbar.Interact(controller.transform);
                    break;

                case "Slider":
                    Slider slider = (Slider)selectable;
                    slider.Interact(controller.transform);
                    break;
            }
        }
    }

    public static class UIProjection
    {
        public static void Interact(this Button button)
        {
            ExecuteEvents.Execute(button.gameObject, new PointerEventData(EventSystem.current), ExecuteEvents.submitHandler);
        }

        public static void Interact(this Toggle toggle)
        {
            toggle.isOn = !toggle.isOn;
        }

        public static void Interact(this Dropdown dropdown)
        {
            dropdown.Show();
        }

        public static void Interact(this InputField inputField)
        {
            inputField.Select();
        }

        public static void Interact(this Scrollbar scrollbar, Transform controllerTransform)
        {
            RaySelectionZone<Selectable> raycastHelper = new RaySelectionZone<Selectable>(controllerTransform);
            var closestAndRaycastHit = raycastHelper.GetClosestAndRaycastHit();

            Vector3[] corners = new Vector3[4];
            scrollbar.transform.GetComponent<RectTransform>().GetWorldCorners(corners);

            Vector3 upPosition = 0.5f * (corners[1] + corners[2]);
            Vector3 downPosition = 0.5f * (corners[0] + corners[3]);

            float Maxdist = Vector3.Distance(upPosition, downPosition);
            float Hitdistance = Vector3.Distance(upPosition, closestAndRaycastHit.raycastHit.point);

            scrollbar.value = Mathf.InverseLerp(Maxdist * 0.9f, 0.1f, Hitdistance);
        }

        public static void Interact(this Slider slider, Transform controllerTransform)
        {
            RaySelectionZone<Selectable> raycastHelper = new RaySelectionZone<Selectable>(controllerTransform);
            var closestRaycastHit = raycastHelper.GetClosestAndRaycastHit();

            Vector3[] localCorners = new Vector3[4];
            RectTransform sliderRectTransform = slider.transform.GetComponent<RectTransform>();

            sliderRectTransform.GetLocalCorners(localCorners);
            Vector3 localHitPoint = sliderRectTransform.InverseTransformPoint(closestRaycastHit.raycastHit.point);
            localHitPoint.x = Mathf.Clamp(localHitPoint.x, localCorners[0].x, localCorners[3].x);

            float newValue = (float)System.Math.Round(slider.minValue + ((localHitPoint.x + localCorners[3].x) / (localCorners[3].x - localCorners[0].x)) * (slider.maxValue - slider.minValue), 1);
            slider.value = newValue;
        }
    }
}