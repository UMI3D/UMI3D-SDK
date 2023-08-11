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
using System.Threading.Tasks;
using umi3d.cdk.volumes;
using umi3d.cdk.binding;
using umi3d.common;
using UnityEngine;

namespace umi3d.cdk
{
    /// <summary>
    /// Loading helper.
    /// </summary>
    [CreateAssetMenu(fileName = "DefaultLoadingParameters", menuName = "UMI3D/Default Loading Parameters")]
    public class UMI3DLoadingParameters : AbstractUMI3DLoadingParameters, IUMI3DLoadingParameters
    {

        private const DebugScope scope = DebugScope.CDK | DebugScope.Collaboration | DebugScope.Loading;

        /// <summary>
        /// Loader for KHR Light.
        /// </summary>
        public virtual KHR_lights_punctualLoader khr_lights_punctualLoader { get; } = new KHR_lights_punctualLoader();

        [ConstEnum(typeof(UMI3DAssetFormat), typeof(string))]
        public List<string> supportedformats = new List<string>();
        public float maximumResolution;

        public virtual UMI3DNodeLoader nodeLoader { get; protected set; }
        public virtual UMI3DAbstractAnchorLoader AnchorLoader { get; protected set; } = null;

        public NotificationLoader notificationLoader;

        //[SerializeField, Tooltip("Material to use when material is unavailable or inexistent.")]
        //public Material baseMaterial;

        [SerializeField]
        private Material _skyboxMaterial;
        public Material skyboxMaterial
        {
            get
            {
                if (defaultMat == null)
                    defaultMat = new Material(RenderSettings.skybox);
                if (_skyboxMaterial == null)
                {
                    _skyboxMaterial = new Material(RenderSettings.skybox);
                    RenderSettings.skybox = _skyboxMaterial;
                }

                return _skyboxMaterial;
            }
        }

        public Material defaultMat = null;

        public override Material GetDefaultMaterial() => defaultMat;
        

        [SerializeField]
        private Material _skyboxEquirectangularMaterial;

        public Material skyboxEquirectangularMaterial
        {
            get
            {
                if (defaultMat == null)
                    defaultMat = new Material(RenderSettings.skybox);
                if (_skyboxEquirectangularMaterial == null)
                {
                    _skyboxEquirectangularMaterial = new Material(RenderSettings.skybox);
                    RenderSettings.skybox = _skyboxEquirectangularMaterial;
                }

                return _skyboxEquirectangularMaterial;
            }
        }

        public List<IResourcesLoader> ResourcesLoaders { get; } = new List<IResourcesLoader>() { new ObjMeshDtoLoader(), new ImageDtoLoader(), new GlTFMeshDtoLoader(), new BundleDtoLoader(), new AudioLoader() };

        public List<AbstractUMI3DMaterialLoader> MaterialLoaders { get; } = new List<AbstractUMI3DMaterialLoader>() { new UMI3DExternalMaterialLoader(), new UMI3DPbrMaterialLoader(), new UMI3DOriginalMaterialLoader() };

        protected IBindingBrowserService bindingService;

        protected AbstractLoader loader;


        public virtual void Init()
        {
            nodeLoader = new UMI3DNodeLoader();

            (loader = new EntityGroupLoader())
            .SetNext(new UMI3DAnimationLoader())
            .SetNext(new PreloadedSceneLoader())
            .SetNext(new UMI3DMeshNodeLoader())
            .SetNext(new UMI3DLineRendererLoader())
            .SetNext(new UMI3DSubMeshNodeLoader())
            .SetNext(new UMI3DVolumeLoader())
            .SetNext(new UMI3DUINodeLoader())
            .SetNext(new BindingLoader())
            .SetNext(notificationLoader.GetNotificationLoader())
            .SetNext(new WebViewLoader())
            .SetNext(new UMI3DNodeLoader())
            .SetNext(UMI3DEnvironmentLoader.Instance.nodeLoader)
            ;
        }

        protected AbstractLoader GetLoader()
        {
            if (loader == null)
                Init();
            return loader;
        }

        /// <summary>
        /// Load an UMI3D Object.
        /// </summary>
        public override async Task ReadUMI3DExtension(ReadUMI3DExtensionData data)
        {
            await GetLoader().Handle(data);
            if (AnchorLoader != null)
                await AnchorLoader.Handle(data);
        }

        /// <inheritdoc/>
        public override async Task<bool> SetUMI3DProperty(SetUMI3DPropertyData data)
        {
            var b = await GetLoader().Handle(data);
            if (AnchorLoader != null)
                await AnchorLoader.Handle(data);
            return b;
        }

        /// <inheritdoc/>
        public override async Task<bool> SetUMI3DProperty(SetUMI3DPropertyContainerData data)
        {
            var b = await GetLoader().Handle(data);
            if (AnchorLoader != null)
                await AnchorLoader.Handle(data);
            return b;
        }

        /// <inheritdoc/>
        public override async Task<bool> ReadUMI3DProperty(ReadUMI3DPropertyData data)
        {
            var b = await GetLoader().Handle(data);
            if (AnchorLoader != null)
                await AnchorLoader.Handle(data);
            return b;
        }

        /// <inheritdoc/>
        public override UMI3DLocalAssetDirectoryDto ChooseVariant(AssetLibraryDto assetLibrary)
        {
            UMI3DLocalAssetDirectoryDto res = null;
            foreach (var assetDir in assetLibrary.variants)
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
        /// Is <paramref name="a"/> bigger than <paramref name="b"/> and inferior than <paramref name="max"/>?
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

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public override IResourcesLoader SelectLoader(string extension)
        {
            foreach (IResourcesLoader loader in ResourcesLoaders)
            {
                if (loader.IsSuitableFor(extension))
                    return loader;
                if (loader.IsToBeIgnored(extension))
                    return null;
            }
            throw new Umi3dException("there is no compatible loader for this extention : " + extension);
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public override async void LoadSkybox(ResourceDto skybox, SkyboxType type, float skyboxRotation, float skyboxExposure)
        {
            try
            {
                if (skybox == null)
                {
                    RenderSettings.skybox = defaultMat;
                    return;
                }

                FileDto fileToLoad = ChooseVariant(skybox.variants);
                if (fileToLoad == null) return;
                string ext = fileToLoad.extension;
                IResourcesLoader loader = SelectLoader(ext);
                if (loader != null)
                {
                    var o = await UMI3DResourcesManager.LoadFile(UMI3DGlobalID.EnvironementId, fileToLoad, loader);
                    var tex = (Texture2D)o;
                    if (tex != null)
                    {
                        switch (type)
                        {
                            case SkyboxType.Equirectangular:
                                LoadEquirectangularSkybox(tex, skyboxRotation, skyboxExposure);
                                break;
                            case SkyboxType.Cubemap:
                                LoadCubeMapSkybox(tex);
                                break;
                            default:
                                break;
                        }
                    }
                    else
                    {
                        UMI3DLogger.LogWarning($"invalid cast from {o.GetType()} to {typeof(Texture2D)}", scope);
                    }
                }
            }
            catch (Exception e)
            {
                UMI3DLogger.LogException(e, scope);
            }
        }

        private void LoadCubeMapSkybox(Texture2D tex)
        {
            try
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
            catch (Exception e)
            {
                UMI3DLogger.LogException(e, scope);
            }
        }

        private void LoadEquirectangularSkybox(Texture2D texture, float rotation, float exposure)
        {
            skyboxEquirectangularMaterial.SetTexture("_Tex", texture);

            SetSkyboxProperties(SkyboxType.Equirectangular, rotation, exposure);

            RenderSettings.skybox = skyboxEquirectangularMaterial;
        }

        public override bool SetSkyboxProperties(SkyboxType type, float skyboxRotation, float skyboxExposure)
        {
            if (type == SkyboxType.Equirectangular)
            {
                if (skyboxEquirectangularMaterial.HasProperty("_Rotation"))
                {
                    skyboxEquirectangularMaterial.SetFloat("_Rotation", skyboxRotation);
                }

                if (skyboxEquirectangularMaterial.HasProperty("_Exposure"))
                {
                    if (RenderSettings.ambientMode == UnityEngine.Rendering.AmbientMode.Skybox)
                        skyboxEquirectangularMaterial.SetFloat("_Exposure", skyboxExposure);
                    else
                        skyboxEquirectangularMaterial.SetFloat("_Exposure", 1f);
                }
            }

            return true;
        }

        /// <inheritdoc/>
        public override Task UnknownOperationHandler(DtoContainer operation)
        {
            return Task.CompletedTask;
        }

        public override Task UnknownOperationHandler(uint operationId, ByteContainer container)
        {
            return Task.CompletedTask;
        }
    }
}