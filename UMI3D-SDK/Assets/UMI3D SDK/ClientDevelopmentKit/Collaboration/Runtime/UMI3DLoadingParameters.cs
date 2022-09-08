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

using inetum.unityUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using umi3d.cdk.interaction;
using umi3d.cdk.userCapture;
using umi3d.cdk.volumes;
using umi3d.common;
using umi3d.common.interaction;
using umi3d.common.userCapture;
using umi3d.common.volume;
using UnityEngine;

namespace umi3d.cdk
{

    [CreateAssetMenu(fileName = "DefaultLoadingParameters", menuName = "UMI3D/Default Loading Parameters")]
    public class UMI3DLoadingParameters : AbstractUMI3DLoadingParameters
    {
        private const DebugScope scope = DebugScope.CDK | DebugScope.Collaboration | DebugScope.Loading;

        [ConstEnum(typeof(UMI3DAssetFormat), typeof(string))]
        public List<string> supportedformats = new List<string>();
        public float maximumResolution;

        public virtual UMI3DNodeLoader nodeLoader { get; } = new UMI3DNodeLoader();
        public virtual UMI3DMeshNodeLoader meshLoader { get; } = new UMI3DMeshNodeLoader();
        public virtual UMI3DLineRendererLoader lineLoader { get; } = new UMI3DLineRendererLoader();
        public virtual UMI3DUINodeLoader UILoader { get; } = new UMI3DUINodeLoader();
        public virtual UMI3DAbstractAnchorLoader AnchorLoader { get; protected set; } = null;
        public virtual UMI3DAvatarNodeLoader avatarLoader { get; } = new UMI3DAvatarNodeLoader();
        public virtual UMI3DSubMeshNodeLoader SubMeshLoader { get; } = new UMI3DSubMeshNodeLoader();

        public NotificationLoader notificationLoader;

        [SerializeField]
        private Material _skyboxMaterial;
        public Material skyboxMaterial { get { if (_skyboxMaterial == null) { _skyboxMaterial = new Material(RenderSettings.skybox); RenderSettings.skybox = _skyboxMaterial; } return _skyboxMaterial; } }

        public List<IResourcesLoader> ResourcesLoaders { get; } = new List<IResourcesLoader>() { new ObjMeshDtoLoader(), new ImageDtoLoader(), new GlTFMeshDtoLoader(), new BundleDtoLoader(), new AudioLoader() };

        public List<AbstractUMI3DMaterialLoader> MaterialLoaders { get; } = new List<AbstractUMI3DMaterialLoader>() { new UMI3DExternalMaterialLoader(), new UMI3DPbrMaterialLoader(), new UMI3DOriginalMaterialLoader() };

        /// <summary>
        /// Load an UMI3DObject.
        /// </summary>
        /// <param name="dto">dto.</param>
        /// <param name="node">gameObject on which the abstract node will be loaded.</param>
        /// <param name="finished">Finish callback.</param>
        /// <param name="failed">error callback.</param>
        public override void ReadUMI3DExtension(UMI3DDto dto, GameObject node, Action finished, Action<Umi3dException> failed)
        {
            Action callback = () => { if (AnchorLoader != null) AnchorLoader.ReadUMI3DExtension(dto, node, finished, failed); else finished.Invoke(); };
            switch (dto)
            {
                case EntityGroupDto e:
                    EntityGroupLoader.ReadUMI3DExtension(e);
                    finished?.Invoke();
                    break;
                case UMI3DAbstractAnimationDto a:
                    UMI3DAnimationLoader.ReadUMI3DExtension(a, node, finished, failed);
                    break;
                case PreloadedSceneDto ps:
                    PreloadedSceneLoader.ReadUMI3DExtension(ps, node, finished, failed);
                    break;
                case InteractableDto i:
                    UMI3DInteractableLoader.ReadUMI3DExtension(i, node, finished, failed);
                    break;
                case GlobalToolDto t:
                    UMI3DGlobalToolLoader.ReadUMI3DExtension(t, finished, failed);
                    finished?.Invoke();
                    break;
                case UMI3DMeshNodeDto m:
                    meshLoader.ReadUMI3DExtension(dto, node, callback, failed);
                    break;
                case UMI3DLineDto m:
                    lineLoader.ReadUMI3DExtension(dto, node, callback, failed);
                    break;
                case SubModelDto s:
                    SubMeshLoader.ReadUMI3DExtension(s, node, callback, failed);
                    break;
                case AbstractVolumeDescriptorDto v:
                    UMI3DVolumeLoader.ReadUMI3DExtension(v, callback, failed);
                    break;
                case UIRectDto r:
                    UILoader.ReadUMI3DExtension(dto, node, callback, failed);
                    break;
                case UMI3DAvatarNodeDto a:
                    avatarLoader.ReadUMI3DExtension(dto, node, callback, failed);
                    break;
                case UMI3DHandPoseDto h:
                    UMI3DHandPoseLoader.Load(h);
                    finished?.Invoke();
                    break;
                case UMI3DEmotesConfigDto e:
                    UMI3DEmotesConfigLoader.Load(e);
                    finished?.Invoke();
                    break;
                case UMI3DEmoteDto e:
                    UMI3DEmoteLoader.Load(e);
                    finished?.Invoke();
                    break;
                case NotificationDto n:
                    notificationLoader.Load(n);
                    finished?.Invoke();
                    break;
                default:
                    nodeLoader.ReadUMI3DExtension(dto, node, callback, failed);
                    break;
            }
        }

        /// <summary>
        /// Update a property.
        /// </summary>
        /// <param name="entity">entity to be updated.</param>
        /// <param name="property">property containing the new value.</param>
        /// <returns></returns>
        public override bool SetUMI3DProperty(UMI3DEntityInstance entity, SetEntityPropertyDto property)
        {
            if (entity == null)
                throw new Exception($"no entity found for {property} [{property.entityId}]");
            if (EntityGroupLoader.SetUMI3DProperty(entity, property))
                return true;
            if (UMI3DEnvironmentLoader.Exists && UMI3DEnvironmentLoader.Instance.sceneLoader.SetUMI3DProperty(entity, property))
                return true;
            if (UMI3DAnimationLoader.SetUMI3DProperty(entity, property))
                return true;
            if (PreloadedSceneLoader.SetUMI3DProperty(entity, property))
                return true;
            if (UMI3DInteractableLoader.SetUMI3DProperty(entity, property))
                return true;
            if (UMI3DGlobalToolLoader.SetUMI3DProperty(entity, property))
                return true;
            if (notificationLoader != null && notificationLoader.SetUMI3DProperty(entity, property))
                return true;
            if (SubMeshLoader.SetUMI3DProperty(entity, property))
                return true;
            if (meshLoader.SetUMI3DProperty(entity, property))
                return true;
            if (UMI3DVolumeLoader.SetUMI3DProperty(entity, property))
                return true;
            if (lineLoader.SetUMI3DProperty(entity, property))
                return true;
            if (UILoader.SetUMI3DProperty(entity, property))
                return true;
            if (avatarLoader.SetUMI3DProperty(entity, property))
                return true;
            if (UMI3DHandPoseLoader.SetUMI3DProperty(entity, property))
                return true;
            if (UMI3DBodyPoseLoader.SetUMI3DProperty(entity, property))
                return true;
            if (UMI3DEmotesConfigLoader.SetUMI3DProperty(entity, property))
                return true;
            if (UMI3DEmoteLoader.SetUMI3DProperty(entity, property))
                return true;
            if (nodeLoader.SetUMI3DProperty(entity, property))
                return true;
            if (AnchorLoader != null && AnchorLoader.SetUMI3DProperty(entity, property))
                return true;
            return GlTFNodeLoader.SetUMI3DProperty(entity, property);
        }


        public override bool SetUMI3DProperty(UMI3DEntityInstance entity, uint operationId, uint propertyKey, ByteContainer container)
        {
            if (entity == null)
                throw new Exception($"Entity should not be null");
            if (EntityGroupLoader.SetUMI3DProperty(entity, operationId, propertyKey, container))
                return true;
            if (UMI3DEnvironmentLoader.Exists && UMI3DEnvironmentLoader.Instance.sceneLoader.SetUMI3DProperty(entity, operationId, propertyKey, container))
                return true;
            if (UMI3DAnimationLoader.SetUMI3DProperty(entity, operationId, propertyKey, container))
                return true;
            if (PreloadedSceneLoader.SetUMI3DProperty(entity, operationId, propertyKey, container))
                return true;
            if (UMI3DInteractableLoader.SetUMI3DProperty(entity, operationId, propertyKey, container))
                return true;
            if (UMI3DGlobalToolLoader.SetUMI3DProperty(entity, operationId, propertyKey, container))
                return true;
            if (UMI3DVolumeLoader.SetUMI3DProperty(entity, operationId, propertyKey, container))
                return true;
            if (notificationLoader != null && notificationLoader.SetUMI3DProperty(entity, operationId, propertyKey, container))
                return true;
            if (SubMeshLoader.SetUMI3DProperty(entity, operationId, propertyKey, container))
                return true;
            if (meshLoader.SetUMI3DProperty(entity, operationId, propertyKey, container))
                return true;
            if (UMI3DVolumeLoader.SetUMI3DProperty(entity, operationId, propertyKey, container))
                return true;
            if (lineLoader.SetUMI3DProperty(entity, operationId, propertyKey, container))
                return true;
            if (UILoader.SetUMI3DProperty(entity, operationId, propertyKey, container))
                return true;
            if (avatarLoader.SetUMI3DProperty(entity, operationId, propertyKey, container))
                return true;
            if (UMI3DHandPoseLoader.SetUMI3DProperty(entity, operationId, propertyKey, container))
                return true;
            if (UMI3DBodyPoseLoader.SetUMI3DProperty(entity, operationId, propertyKey, container))
                return true;
            if (UMI3DEmotesConfigLoader.SetUMI3DProperty(entity, operationId, propertyKey, container))
                return true;
            if (UMI3DEmoteLoader.SetUMI3DProperty(entity, operationId, propertyKey, container))
                return true;
            if (nodeLoader.SetUMI3DProperty(entity, operationId, propertyKey, container))
                return true;
            if (AnchorLoader != null && AnchorLoader.SetUMI3DPorperty(entity, operationId, propertyKey, container))
                return true;
            return GlTFNodeLoader.SetUMI3DProperty(entity, operationId, propertyKey, container);
        }

        public override bool ReadUMI3DProperty(ref object value, uint propertyKey, ByteContainer container)
        {
            if (UMI3DEnvironmentLoader.Exists && UMI3DEnvironmentLoader.Instance.sceneLoader.ReadUMI3DProperty(ref value, propertyKey, container))
                return true;
            if (UMI3DAnimationLoader.ReadUMI3DProperty(ref value, propertyKey, container))
                return true;
            if (PreloadedSceneLoader.ReadUMI3DProperty(ref value, propertyKey, container))
                return true;
            if (UMI3DInteractableLoader.ReadUMI3DProperty(ref value, propertyKey, container))
                return true;
            if (UMI3DGlobalToolLoader.ReadUMI3DProperty(ref value, propertyKey, container))
                return true;
            if (notificationLoader != null && notificationLoader.ReadUMI3DProperty(ref value, propertyKey, container))
                return true;
            if (SubMeshLoader.ReadUMI3DProperty(ref value, propertyKey, container))
                return true;
            if (meshLoader.ReadUMI3DProperty(ref value, propertyKey, container))
                return true;
            if (lineLoader.ReadUMI3DProperty(ref value, propertyKey, container))
                return true;
            if (UILoader.ReadUMI3DProperty(ref value, propertyKey, container))
                return true;
            if (avatarLoader.ReadUMI3DProperty(ref value, propertyKey, container))
                return true;
            if (UMI3DHandPoseLoader.ReadUMI3DProperty(ref value, propertyKey, container))
                return true;
            if (UMI3DBodyPoseLoader.ReadUMI3DProperty(ref value, propertyKey, container))
                return true;
            if (UMI3DEmotesConfigLoader.ReadUMI3DProperty(ref value, propertyKey, container))
                return true;
            if (UMI3DEmoteLoader.ReadUMI3DProperty(ref value, propertyKey, container))
                return true;
            if (nodeLoader.ReadUMI3DProperty(ref value, propertyKey, container))
                return true;
            if (AnchorLoader != null && AnchorLoader.ReadUMI3DProperty(ref value, propertyKey, container))
                return true;
            return GlTFNodeLoader.ReadUMI3DProperty(ref value, propertyKey, container);
        }

        ///<inheritdoc/>
        public override UMI3DLocalAssetDirectory ChooseVariant(AssetLibraryDto assetLibrary)
        {
            UMI3DLocalAssetDirectory res = null;
            foreach (UMI3DLocalAssetDirectory assetDir in assetLibrary.variants)
            {
                bool ok = res == null;
                if (!ok && !assetDir.formats.Any(f => !supportedformats.Contains(f)))
                {
                    if (res.formats.Any(f => !supportedformats.Contains(f)))
                        ok = true;
                    else
                        ok = Compare(assetDir.metrics.resolution, res.metrics.resolution, maximumResolution);
                }

                if (ok)
                {
                    res = assetDir;
                }
            }
            return res;
        }

        /// <summary>
        /// is "a" bigger than "b" and inferior than max
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="max">maximum, 0 mean no maximum</param>
        /// <returns></returns>
        private bool Compare(float a, float b, float max)
        {
            if (max <= 0) return a > b;
            if (b > max) return b > a;
            if (a < max) return a > b;
            return false;
        }

        ///<inheritdoc/>
        public override FileDto ChooseVariant(List<FileDto> files)
        {
            FileDto res = null;
            if (files != null)
            {
                foreach (FileDto file in files)
                {
                    bool ok = res == null;
                    if (!ok && supportedformats.Contains(file.format))
                    {
                        if (!supportedformats.Contains(res.format))
                            ok = true;
                        else
                            ok = Compare(file.metrics.resolution, res.metrics.resolution, maximumResolution);

                    }
                    if (ok)
                    {
                        res = file;
                    }
                }
            }

            return res;
        }

        ///<inheritdoc/>
        public override IResourcesLoader SelectLoader(string extension)
        {
            foreach (IResourcesLoader loader in ResourcesLoaders)
            {
                if (loader.IsSuitableFor(extension))
                    return loader;
                if (loader.IsToBeIgnored(extension))
                    return null;
            }
            UMI3DLogger.LogError("there is no compatible loader for this extention : " + extension, scope);
            return null;
        }

        ///<inheritdoc/>
        public override AbstractUMI3DMaterialLoader SelectMaterialLoader(GlTFMaterialDto gltfMatDto)
        {
            foreach (AbstractUMI3DMaterialLoader loader in MaterialLoaders)
            {
                if (loader.IsSuitableFor(gltfMatDto))
                    return loader;
            }
            UMI3DLogger.LogError("there is no compatible material loader for this material.", scope);
            return null;
        }

        ///<inheritdoc/>
        public override void loadSkybox(ResourceDto skybox)
        {
            FileDto fileToLoad = ChooseVariant(skybox.variants);
            if (fileToLoad == null) return;
            string url = fileToLoad.url;
            string ext = fileToLoad.extension;
            string authorization = fileToLoad.authorization;
            IResourcesLoader loader = SelectLoader(ext);
            if (loader != null)
            {
                UMI3DResourcesManager.LoadFile(
                    UMI3DGlobalID.EnvironementId,
                    fileToLoad,
                    loader.UrlToObject,
                    loader.ObjectFromCache,
                    (o) =>
                    {

                        var tex = (Texture2D)o;
                        if (tex != null)
                        {

                            Cubemap cube;
                            Color[] imageColors;

                            //prerequises: 
                            // 1) image is in format
                            //     +y
                            //  -x +z +x -z
                            //     -Y
                            // 2) faces are cubes

                            int size = tex.width / 4;
                            cube = new Cubemap(size, TextureFormat.RGB24, false);

                            //Need to invert y ? Oo
                            var buffer = new Texture2D(tex.width, tex.height);
                            buffer.SetPixels(tex.GetPixels());
                            for (int x = 0; x < tex.width; x++)
                            {
                                for (int y = 0; y < tex.height; y++)
                                    tex.SetPixel(x, y, buffer.GetPixel(x, tex.height - 1 - y));
                            }

                            imageColors = tex.GetPixels(size, 0, size, size);
                            cube.SetPixels(imageColors, CubemapFace.PositiveY);

                            imageColors = tex.GetPixels(0, size, size, size);
                            cube.SetPixels(imageColors, CubemapFace.NegativeX);

                            imageColors = tex.GetPixels(size, size, size, size);
                            cube.SetPixels(imageColors, CubemapFace.PositiveZ);

                            imageColors = tex.GetPixels(size * 2, size, size, size);
                            cube.SetPixels(imageColors, CubemapFace.PositiveX);

                            imageColors = tex.GetPixels(size * 3, size, size, size);
                            cube.SetPixels(imageColors, CubemapFace.NegativeZ);

                            imageColors = tex.GetPixels(size, size * 2, size, size);
                            cube.SetPixels(imageColors, CubemapFace.NegativeY);

                            cube.Apply();
                            skyboxMaterial.SetTexture("_Tex", cube);
                            RenderSettings.skybox = skyboxMaterial;
                        }
                        else
                        {
                            UMI3DLogger.LogWarning($"invalid cast from {o.GetType()} to {typeof(Texture2D)}", scope);
                        }
                    },
                    e => UMI3DLogger.LogWarning(e, scope),
                    loader.DeleteObject
                    );
            }
        }

        ///<inheritdoc/>
        public override void UnknownOperationHandler(AbstractOperationDto operation, Action performed)
        {
            switch (operation)
            {
                case SwitchToolDto switchTool:
                    AbstractInteractionMapper.Instance.SwitchTools(switchTool.toolId, switchTool.replacedToolId, switchTool.releasable, 0, new interaction.RequestedByEnvironment());
                    performed.Invoke();
                    break;
                case ProjectToolDto projection:
                    AbstractInteractionMapper.Instance.SelectTool(projection.toolId, projection.releasable, 0, new interaction.RequestedByEnvironment());
                    performed.Invoke();
                    break;
                case ReleaseToolDto release:
                    AbstractInteractionMapper.Instance.ReleaseTool(release.toolId, new interaction.RequestedByEnvironment());
                    performed.Invoke();
                    break;
                case SetTrackingTargetFPSDto setTargetFPS:
                    UMI3DClientUserTracking.Instance.SetFPSTarget(setTargetFPS.targetFPS);
                    performed.Invoke();
                    break;
                case SetStreamedBonesDto streamedBones:
                    UMI3DClientUserTracking.Instance.SetStreamedBones(streamedBones.streamedBones);
                    performed.Invoke();
                    break;
                case SetSendingCameraPropertiesDto sendingCamera:
                    UMI3DClientUserTracking.Instance.SetCameraPropertiesSending(sendingCamera.activeSending);
                    performed.Invoke();
                    break;
                case SetSendingTrackingDto sendingTracking:
                    UMI3DClientUserTracking.Instance.SetTrackingSending(sendingTracking.activeSending);
                    performed.Invoke();
                    break;
            }
        }

        public override void UnknownOperationHandler(uint operationId, ByteContainer container, Action performed)
        {
            ulong id;
            bool releasable;

            switch (operationId)
            {
                case UMI3DOperationKeys.SwitchTool:
                    id = UMI3DNetworkingHelper.Read<ulong>(container);
                    ulong oldid = UMI3DNetworkingHelper.Read<ulong>(container);
                    releasable = UMI3DNetworkingHelper.Read<bool>(container);
                    AbstractInteractionMapper.Instance.SwitchTools(id, oldid, releasable, 0, new interaction.RequestedByEnvironment());
                    performed.Invoke();
                    break;
                case UMI3DOperationKeys.ProjectTool:
                    id = UMI3DNetworkingHelper.Read<ulong>(container);
                    releasable = UMI3DNetworkingHelper.Read<bool>(container);
                    AbstractInteractionMapper.Instance.SelectTool(id, releasable, 0, new interaction.RequestedByEnvironment());
                    performed.Invoke();
                    break;
                case UMI3DOperationKeys.ReleaseTool:
                    id = UMI3DNetworkingHelper.Read<ulong>(container);
                    AbstractInteractionMapper.Instance.ReleaseTool(id, new interaction.RequestedByEnvironment());
                    performed.Invoke();
                    break;
                case UMI3DOperationKeys.SetUTSTargetFPS:
                    int target = UMI3DNetworkingHelper.Read<int>(container);
                    UMI3DClientUserTracking.Instance.SetFPSTarget(target);
                    performed.Invoke();
                    break;
                case UMI3DOperationKeys.SetStreamedBones:
                    List<uint> streamedBones = UMI3DNetworkingHelper.ReadList<uint>(container);
                    UMI3DClientUserTracking.Instance.SetStreamedBones(streamedBones);
                    performed.Invoke();
                    break;
                case UMI3DOperationKeys.SetSendingCameraProperty:
                    bool sendCamera = UMI3DNetworkingHelper.Read<bool>(container);
                    UMI3DClientUserTracking.Instance.SetCameraPropertiesSending(sendCamera);
                    performed.Invoke();
                    break;
                case UMI3DOperationKeys.SetSendingTracking:
                    bool sendTracking = UMI3DNetworkingHelper.Read<bool>(container);
                    UMI3DClientUserTracking.Instance.SetTrackingSending(sendTracking);
                    performed.Invoke();
                    break;
            }
        }
    }
}