/*
Copyright 2019 - 2025 Inetum

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

using inetum.unityUtils.observation;
using System;
using umi3d.common.interaction;
using UnityEngine;

namespace umi3d.cdk.interaction
{
    public class InteractableHoverStateListener : MonoBehaviour
    {
        public enum State
        {
            Enter,
            Hover,
            Exit
        }
        public new Collider collider;
        public InteractableContainer interactableContainer;
        public InteractableUIController uiController;

        public State state { get; private set; }
        public HoveredDto hoveredDto { get; private set; }
        public static readonly Delegates<IInteractableHoverStateDelegate> delegates = new();

        void Awake()
        {
            NotificationHub.Default.Subscribe(this, ID.FromType<InteractableNotificationKeys.HoverStateChanged>(), (Callback)OnHoverStateChanged);
        }

        void OnHoverStateChanged(Notification notification)
        {
            if (!notification.TryGetInfoT(InteractableNotificationKeys.HoverStateChanged.State, out State state)) { return; }
            if (!notification.TryGetInfoT(InteractableNotificationKeys.HoverStateChanged.Collider, out Collider collider)) { return; }

            if (this.collider != collider) { return; }

            switch (state)
            {
                case State.Enter:
                    OnHoverEnter(notification);
                    break;

                case State.Hover:
                    OnHover(notification);
                    break;

                case State.Exit:
                    OnHoverExit(notification);
                    break;

                default:
                    break;
            }
        }

        void OnHoverEnter(Notification notification)
        {
            if (!notification.TryGetInfoT(InteractableNotificationKeys.HoverStateChanged.HoveredDto, out HoveredDto hoveredDto)) { return; }

            state = State.Enter;
            this.hoveredDto = hoveredDto;
            delegates.ForEach(@delegate =>
            {
                @delegate.OnHoverEnter(collider, interactableContainer, this);
                return Flow.Continue;
            });
        }

        void OnHoverExit(Notification notification)
        {
            if (!notification.TryGetInfoT(InteractableNotificationKeys.HoverStateChanged.HoveredDto, out HoveredDto hoveredDto)) { return; }

            state = State.Enter;
            delegates.ForEach(@delegate =>
            {
                @delegate.OnHoverExit(collider, interactableContainer, this);
                return Flow.Continue;
            });
        }

        void OnHover(Notification notification)
        {
            if (!notification.TryGetInfoT(InteractableNotificationKeys.HoverStateChanged.HoveredDto, out HoveredDto hoveredDto)) { return; }

            state = State.Enter;
            delegates.ForEach(@delegate =>
            {
                @delegate.OnHover(collider, interactableContainer, this);
                return Flow.Continue;
            });
        }
    }

    public interface IInteractableHoverStateDelegate
    {
        void OnHoverEnter(Collider collider, InteractableContainer interactableContainer, InteractableHoverStateListener hoverStateListener);
        void OnHoverExit(Collider collider, InteractableContainer interactableContainer, InteractableHoverStateListener hoverStateListener);
        void OnHover(Collider collider, InteractableContainer interactableContainer, InteractableHoverStateListener hoverStateListener);
    }
}