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

using System;
using System.Collections.Generic;
using umi3d.common.interaction;
using UnityEngine;

namespace umi3d.cdk.interaction
{
    public interface HasManipulationControlData
    {
        ManipulationControlData ManipulationControlData { get; set; }
    }

    [Serializable]
    public sealed class ManipulationControlData
    {
        public EnumeratorMessageSender messageSender;
        [Tooltip("Translation multiplicative strength")]
        /// <summary>
        /// Input multiplicative strength.
        /// </summary>
        public float translationStrength = 1;
        [Tooltip("Rotation multiplicative strength")]
        /// <summary>
        /// Input multiplicative strength.
        /// </summary>
        public float rotationStrength = 200;
        public List<DofGroupEnum> compatibleDofGroup = new();

        /// <summary>
        /// World position of on activation.
        /// </summary>
        [HideInInspector] public Vector3 initialPosition;
        /// <summary>
        /// World rotation of on activation.
        /// </summary>
        [HideInInspector] public Quaternion initialRotation;
        /// <summary>
        /// Frame of reference of the associated interaction.
        /// </summary>
        [HideInInspector] public Transform frameOfReference;

        public void SetRequestTranslationAndRotation(
            DofGroupEnum dofs, 
            ManipulationRequestDto request,
            Transform currentTracking
        )
        {
            switch (dofs)
            {
                case DofGroupEnum.ALL:
                    request.translation 
                        = GetTranslation3Axis(currentTracking.position)
                        .Dto();
                    request.rotation = GetRotation3Axis(currentTracking.rotation)
                        .Dto();
                    break;
                case DofGroupEnum.X:
                case DofGroupEnum.Y:
                case DofGroupEnum.Z:
                    request.translation 
                        = GetTranslation1Axis(
                            dofs, 
                            currentTracking.position
                        ).Dto();
                    break;
                case DofGroupEnum.XY:
                case DofGroupEnum.XZ:
                case DofGroupEnum.YZ:
                    request.translation = GetTranslation2Axis(
                        dofs, 
                        currentTracking.position
                    ).Dto();
                    break;
                case DofGroupEnum.XYZ:
                    request.translation = GetTranslation3Axis(currentTracking.position)
                        .Dto();
                    break;
                case DofGroupEnum.RX:
                case DofGroupEnum.RY:
                case DofGroupEnum.RZ:
                    request.rotation = GetRotation1Axis(
                        dofs, 
                        currentTracking.rotation
                    ).Dto();
                    break;
                case DofGroupEnum.RX_RY:
                case DofGroupEnum.RX_RZ:
                case DofGroupEnum.RY_RZ:
                    request.rotation = GetRotation2Axis(
                        dofs, 
                        currentTracking.rotation
                    ).Dto();
                    break;
                case DofGroupEnum.RX_RY_RZ:
                    request.rotation = GetRotation3Axis(currentTracking.rotation)
                        .Dto();
                    break;
                case DofGroupEnum.X_RX:
                    request.translation = GetTranslation1Axis(
                        DofGroupEnum.X, 
                        currentTracking.position
                    ).Dto();
                    request.rotation = GetRotation1Axis(
                        DofGroupEnum.RX, 
                        currentTracking.rotation
                    ).Dto();
                    break;
                case DofGroupEnum.Y_RY:
                    request.translation = GetTranslation1Axis(
                        DofGroupEnum.Y, 
                        currentTracking.position
                    ).Dto();
                    request.rotation = GetRotation1Axis(
                        DofGroupEnum.RY,
                        currentTracking.rotation
                    ).Dto();
                    break;
                case DofGroupEnum.Z_RZ:
                    request.translation = GetTranslation1Axis(
                        DofGroupEnum.Z, 
                        currentTracking.position
                    ).Dto();
                    request.rotation = GetRotation1Axis(
                        DofGroupEnum.RZ, 
                        currentTracking.rotation
                    ).Dto();
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Returns translation movement along 1 axis.
        /// </summary>
        /// <param name="dofs"></param>
        /// <returns></returns>
        Vector3 GetTranslation1Axis(DofGroupEnum dofs, Vector3 currentPosition)
        {
            Vector3 axis = new Vector3(
                x: dofs == DofGroupEnum.X ? 1f : 0f,
                y: dofs == DofGroupEnum.Y ? 1f : 0f,
                z: dofs == DofGroupEnum.Z ? 1f : 0f
            );

            Vector3 distanceInWorld = currentPosition - initialPosition;
            Vector3 distanceInFrame = 
                frameOfReference.InverseTransformDirection(
                    Vector3.Project(
                        distanceInWorld, 
                        frameOfReference.TransformDirection(axis)
                    )
                );

            return distanceInFrame * translationStrength;
        }

        /// <summary>
        /// Returns translation movement along 2 axis.
        /// </summary>
        /// <param name="dofs"></param>
        /// <returns></returns>
        Vector3 GetTranslation2Axis(DofGroupEnum dofs, Vector3 currentPosition)
        {
            Vector3 normal = new Vector3(
                x: dofs == DofGroupEnum.YZ ? 1f : 0f,
                y: dofs == DofGroupEnum.XZ ? 1f : 0f,
                z: dofs == DofGroupEnum.XY ? 1f : 0f
            );

            Vector3 distanceInWorld = currentPosition - initialPosition;
            Vector3 distanceInFrame = 
                frameOfReference.InverseTransformDirection(
                    Vector3.ProjectOnPlane(
                        distanceInWorld, 
                        frameOfReference.TransformDirection(normal)
                    ));

            return distanceInFrame * translationStrength;
        }

        /// <summary>
        /// Returns translation movement along 3 axis.
        /// </summary>
        /// <returns></returns>
        private Vector3 GetTranslation3Axis(Vector3 currentPosition)
        {
            Vector3 distanceInWorld = currentPosition - initialPosition;
            Vector3 distanceInFrame = frameOfReference.InverseTransformDirection(distanceInWorld);

            return distanceInFrame * translationStrength;
        }

        /// <summary>
        /// Returns rotation movement along 1 axis.
        /// </summary>
        /// <param name="dofs"></param>
        /// <returns></returns>
        Quaternion GetRotation1Axis(DofGroupEnum dofs, Quaternion currentRotation)
        {
            Quaternion rotationInWorld = currentRotation * Quaternion.Inverse(initialRotation);
            var rotationInWorldRemapped = new Vector3(
                (rotationInWorld.x > 180) ? rotationInWorld.x - 360 : rotationInWorld.x,
                (rotationInWorld.y > 180) ? rotationInWorld.y - 360 : rotationInWorld.y,
                (rotationInWorld.z > 180) ? rotationInWorld.z - 360 : rotationInWorld.z
            );

            Vector3 filtre = new Vector3(
                x: dofs == DofGroupEnum.RX ? 1f : 0f,
                y: dofs == DofGroupEnum.RY ? 1f : 0f,
                z: dofs == DofGroupEnum.RZ ? 1f : 0f
            );

            return Quaternion.Euler(
                Vector3.Scale(
                        frameOfReference.InverseTransformDirection(rotationInWorldRemapped),
                        filtre
                    ) * rotationStrength
            );
        }

        /// <summary>
        /// Returns rotation movement along 2 axis.
        /// </summary>
        /// <param name="dofs"></param>
        /// <returns></returns>
        Quaternion GetRotation2Axis(DofGroupEnum dofs, Quaternion currentRotation)
        {
            Quaternion rotationInWorld = currentRotation * Quaternion.Inverse(initialRotation);
            var rotationInWorldRemapped = new Vector3(
                (rotationInWorld.x > 180) ? rotationInWorld.x - 360 : rotationInWorld.x,
                (rotationInWorld.y > 180) ? rotationInWorld.y - 360 : rotationInWorld.y,
                (rotationInWorld.z > 180) ? rotationInWorld.z - 360 : rotationInWorld.z);

            Vector3 filtre = new Vector3(
                x: dofs == DofGroupEnum.RX_RY || dofs == DofGroupEnum.RX_RZ ? 1f : 0f,
                y: dofs == DofGroupEnum.RX_RY || dofs == DofGroupEnum.RY_RZ ? 1f : 0f,
                z: dofs == DofGroupEnum.RX_RZ || dofs == DofGroupEnum.RY_RZ ? 1f : 0f
            );

            return Quaternion.Euler(
                Vector3.Scale(
                    frameOfReference.InverseTransformDirection(rotationInWorldRemapped),
                    filtre
                ) * rotationStrength
            );
        }

        /// <summary>
        /// Returns rotation movement along 3 axis.
        /// </summary>
        /// <returns></returns>
        Quaternion GetRotation3Axis(Quaternion currentRotation)
        {
            Quaternion rotationInWorld = currentRotation * Quaternion.Inverse(initialRotation);
            var rotationInWorldRemapped = new Vector3(
                (rotationInWorld.x > 180) ? rotationInWorld.x - 360 : rotationInWorld.x,
                (rotationInWorld.y > 180) ? rotationInWorld.y - 360 : rotationInWorld.y,
                (rotationInWorld.z > 180) ? rotationInWorld.z - 360 : rotationInWorld.z);

            return Quaternion.Euler(
                frameOfReference
                    .InverseTransformDirection(rotationInWorldRemapped) 
                    * rotationStrength
                );
        }
    }
}