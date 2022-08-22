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
using umi3d.common;
using UnityEngine;

namespace umi3d.cdk
{
    /// <summary>
    /// Constraint the object's rotation to make it face the main camera.
    /// </summary>
    public class Billboard : MonoBehaviour
    {

        public bool X;
        public bool Y;
        public GlTFNodeDto glTFNodeDto;

        private void Start()
        {
            ComputeOrientation();
        }

        private void LateUpdate()
        {
            ComputeOrientation();
        }

        private void ComputeOrientation()
        {
            if (glTFNodeDto == null)
                return;

            Vector3 pos = Camera.main.transform.position - transform.position;

            if (!X) { pos -= Vector3.up * Vector3.Dot(Vector3.up, pos); }
            if (!Y) { pos -= Vector3.right * Vector3.Dot(Vector3.right, pos); }

            if (pos != Vector3.zero)
                transform.rotation = Quaternion.LookRotation(-pos) * glTFNodeDto.rotation;
            else
                transform.rotation = glTFNodeDto.rotation;
        }
    }
}