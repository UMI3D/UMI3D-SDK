/*
Copyright 2019 - 2024 Inetum

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


using UnityEngine;

namespace umi3d.common.core
{
    public interface ITransformation
    {
        /// <summary>
        /// World position (global space).
        /// </summary>
        public Vector3 Position { get; set; }

        /// <summary>
        /// World rotation (global space).
        /// </summary>
        public Quaternion Rotation { get; set; }

        /// <summary>
        /// Rotation relative to parent.
        /// </summary>
        public Quaternion LocalRotation { get; set; }

        /// <summary>
        /// Position relative to parent.
        /// </summary>
        public Vector3 LocalPosition { get; set; }

        /// <summary>
        /// Scale relative to parent.
        /// </summary>
        public Vector3 Scale { get; set; }
    }

    /// <summary>
    /// Transformation container without a gameobject.
    /// </summary>
    /// Storage only. No logic is ensured.
    public class PureTransformation : ITransformation
    {
        public Vector3 Position { get; set; } = Vector3.zero;

        public Vector3 LocalPosition { get; set; } = Vector3.zero;

        public Quaternion Rotation { get; set; } = Quaternion.identity;

        public Quaternion LocalRotation { get; set; } = Quaternion.identity;

        public Vector3 Scale { get; set; } = Vector3.one;

        public static PureTransformation CopyTransform(Transform transform)
        {
            return new PureTransformation()
            {
                Position = transform.position,
                Rotation = transform.rotation,
                LocalPosition = transform.localPosition,
                LocalRotation = transform.localRotation,
                Scale = transform.localScale
            };
        }
    }

    /// <summary>
    /// Transformation wrapper for a Unity tranform.
    /// </summary>
    public class UnityTransformation : ITransformation
    {
        public Vector3 Position
        {
            get => Transform.position;
            set => Transform.position = value;
        }

        public Quaternion Rotation
        {
            get => Transform.rotation;
            set => Transform.rotation = value;
        }

        public Vector3 LocalPosition
        {
            get => Transform.localPosition;
            set => Transform.localPosition = value;
        }

        public Quaternion LocalRotation
        {
            get => Transform.localRotation;
            set => Transform.localRotation = value;
        }

        public Vector3 Scale
        {
            get => Transform.localScale;
            set => Transform.localScale = value;
        }

        public Transform Transform { get; }

        public UnityTransformation(Transform transform)
        {
            if (transform == null)
                throw new System.ArgumentNullException(nameof(transform));

            Transform = transform;
        }
    }
}
