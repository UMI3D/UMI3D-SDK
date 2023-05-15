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

using inetum.unityUtils;
using System.Linq;
using System.Threading.Tasks;
using umi3d.common;
using umi3d.common.userCapture;
using UnityEngine;

namespace umi3d.cdk.userCapture
{
    /// <summary>
    /// Loader called to load <see cref="UMI3DSkeletonNodeDto"/>.
    /// </summary>
    /// It is based upon the <see cref="UMI3DMeshNodeDto"/> as the animation ressources are packaged in a bundle just like in a model.
    public class UMI3DSkeletonNodeLoader : UMI3DMeshNodeLoader
    {
        #region Dependency Injection
        private readonly ISkeletonManager personnalSkeletonService;
        public UMI3DSkeletonNodeLoader() : base()
        {
            personnalSkeletonService = PersonalSkeletonManager.Instance;
        }
        #endregion Dependency Injection

        /// <inheritdoc/>
        public override bool CanReadUMI3DExtension(ReadUMI3DExtensionData data)
        {
            return data.dto is UMI3DSkeletonNodeDto && base.CanReadUMI3DExtension(data);
        }

        /// <inheritdoc/>
        public override async Task ReadUMI3DExtension(ReadUMI3DExtensionData data)
        {
            if (data.dto is not UMI3DSkeletonNodeDto nodeDto)
                throw new Umi3dException("DTO should be an UM3DSkeletonNodeDto");

            await base.ReadUMI3DExtension(data);

            UMI3DNodeInstance nodeInstance = UMI3DEnvironmentLoader.GetNode(nodeDto.id);

            var go = nodeInstance.gameObject;

            var modelTracker = data.node.GetOrAddComponent<ModelTracker>();

            SkeletonMapper skeletonMapper = go.GetComponentInChildren<SkeletonMapper>(); //? should it come with the bundle ??
            if (skeletonMapper == null) //? hopefully not necessary because it would imply to rebind everything
            {
                skeletonMapper = go.AddComponent<SkeletonMapper>();
                if (go.TryGetComponent(out TrackedSkeletonBone bone))
                    skeletonMapper.BoneAnchor = new BonePoseDto() { bone = bone.boneType, Position = bone.transform.position.Dto(), Rotation = bone.transform.rotation.Dto() };
                else
                    throw new Umi3dException("No bone found to attach skeleton.");
            }

            AnimatedSkeleton animationSkeleton = new(skeletonMapper);
            personnalSkeletonService.personalSkeleton.Skeletons.Add(animationSkeleton);


            foreach (var renderer in go.GetComponentsInChildren<Renderer>())
                renderer.gameObject.layer = LayerMask.NameToLayer("Invisible");

            //if (go.TryGetComponent(out Animator animator)) //! may delete that if already done in base class
            //    modelTracker.animatorsToRebind.Add(animator);
        }
    }
}