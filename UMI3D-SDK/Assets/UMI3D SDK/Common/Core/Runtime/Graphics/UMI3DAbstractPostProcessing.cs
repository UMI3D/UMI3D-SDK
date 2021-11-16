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

using UnityEngine;

namespace umi3d.common.graphics
{
    public abstract class UMI3DAbstractPostProcessing : common.Singleton<UMI3DAbstractPostProcessing>
    {
        /// <summary>
        /// Get the main camera.
        /// </summary>
        /// <returns></returns>
        public static Camera GetCamera()
        {
            return Exists ? Instance._GetCamera() : null;
        }

        /// <see cref=" UMI3DAbstractPostProcessing.GetCamera()"/>
        protected virtual Camera _GetCamera()
        {
            return Camera.main;
        }

        #region Bloom
        /// <summary>
        /// Set the bloom using parameters in GlobalBloom
        /// </summary>
        /// <returns></returns>
        public static void SetBloom(UMI3DGlobalBloom bloom)
        {
            if (Exists)
                Instance._SetBloom(bloom);
        }

        /// <see cref=" UMI3DAbstractPostProcessing.SetBloom(UMI3DGlobalBloom)"/>
        protected abstract void _SetBloom(UMI3DGlobalBloom bloom);


        /// <summary>
        /// Reset the bloom
        /// </summary>
        /// <returns></returns>
        public static void ResetBloom()
        {
            if (Exists)
                Instance._ResetBloom();
        }

        /// <see cref=" UMI3DAbstractPostProcessing.ResetBloom()"/>
        protected abstract void _ResetBloom();
        #endregion

        #region Vignette
        /// <summary>
        /// Set the vignette using parameters in GlobalVignette
        /// </summary>
        /// <returns></returns>
        public static void SetVignette(UMI3DGlobalVignette bloom)
        {
            if (Exists)
                Instance._SetVignette(bloom);
        }

        /// <see cref=" UMI3DAbstractPostProcessing.SetVignette(UMI3DGlobalVignette)"/>
        protected abstract void _SetVignette(UMI3DGlobalVignette bloom);

        /// <summary>
        /// Reset the vignette
        /// </summary>
        /// <returns></returns>
        public static void ResetVignette()
        {
            if (Exists)
                Instance._ResetVignette();
        }

        /// <see cref=" UMI3DAbstractPostProcessing.ResetVignette()"/>
        protected abstract void _ResetVignette();
        #endregion

        #region Fog
        /// <summary>
        /// Set the Fog using parameters in FogSettings
        /// </summary>
        /// <returns></returns>
        public static void SetFogSettings(UMI3DFogSettings fogSettings)
        {
            if (Exists)
                Instance._SetFog(fogSettings);
        }

        /// <see cref=" UMI3DAbstractPostProcessing.SetFogSettings(UMI3DFogSettings)"/>
        protected abstract void _SetFog(UMI3DFogSettings fogSettings);

        /// <summary>
        /// Reset the fog
        /// </summary>
        /// <returns></returns>
        public static void ResetFog()
        {
            if (Exists)
                Instance._ResetFog();
        }

        /// <see cref=" UMI3DAbstractPostProcessing.ResetFog()"/>
        protected abstract void _ResetFog();
        #endregion


        protected virtual void Start()
        {
            ResetFog();
            ResetBloom();
            ResetVignette();
        }
    }
}