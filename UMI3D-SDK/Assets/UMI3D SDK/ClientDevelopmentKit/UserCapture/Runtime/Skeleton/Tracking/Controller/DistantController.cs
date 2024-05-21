/*
Copyright 2019 - 2023 Inetum

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

using umi3d.common.core;
using UnityEngine;

namespace umi3d.cdk.userCapture.tracking
{
    /// <summary>
    /// Representation of a controller not bound to the current browser.
    /// </summary>
    public class DistantController : IController
    {
        public uint boneType { get; set; }

        public PureTransformation Transformation { get; set; } = new();

        public Vector3 position
        {
            get => Transformation.Position;
            set => Transformation.Position = value;
        }

        public Quaternion rotation
        {
            get => Transformation.Rotation;
            set => Transformation.Rotation = value;
        }

        public Vector3 scale
        {
            get => Transformation.Scale;
            set => Transformation.Scale = value;
        }

        public bool isActive { get; set; }

        public bool isOverrider { get; set; }

        public event System.Action Destroyed;

        public void Destroy()
        {
            Destroyed?.Invoke();
        }
    }
}