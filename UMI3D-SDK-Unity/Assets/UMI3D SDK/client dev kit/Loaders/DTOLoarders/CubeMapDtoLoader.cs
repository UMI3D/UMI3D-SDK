/*
Copyright 2019 Gfi Informatique

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
using umi3d.common;
using UnityEngine;

namespace umi3d.cdk
{
    public class CubeMapDtoLoader : AbstractDTOLoader<ResourceDto, Cubemap>
    {

        /// <summary>
        /// LOAD
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="callback"></param>
        public override void LoadDTO(ResourceDto dto, Action<Cubemap> callback)
        {
            //Material skyboxMat;

            HDResourceCache.Download(dto, (Texture2D loadedImage) =>
            {
                Cubemap cube;
                Color[] imageColors;

                //prerequises: 
                // 1) image is in format
                //     +y
                //  -x +z +x -z
                //     -Y
                // 2) faces are cubes

                int size = loadedImage.width / 4;
                cube = new Cubemap(size, TextureFormat.RGB24, false);

                //Need to invert y ? Oo
                var buffer = new Texture2D(loadedImage.width, loadedImage.height);
                buffer.SetPixels(loadedImage.GetPixels());
                for (int x = 0; x < loadedImage.width; x++)
                    for (int y = 0; y < loadedImage.height; y++)
                        loadedImage.SetPixel(x, y, buffer.GetPixel(x, loadedImage.height - 1 - y));

                imageColors = loadedImage.GetPixels(size, 0, size, size);
                cube.SetPixels(imageColors, CubemapFace.PositiveY);

                imageColors = loadedImage.GetPixels(0, size, size, size);
                cube.SetPixels(imageColors, CubemapFace.NegativeX);

                imageColors = loadedImage.GetPixels(size, size, size, size);
                cube.SetPixels(imageColors, CubemapFace.PositiveZ);

                imageColors = loadedImage.GetPixels(size * 2, size, size, size);
                cube.SetPixels(imageColors, CubemapFace.PositiveX);

                imageColors = loadedImage.GetPixels(size * 3, size, size, size);
                cube.SetPixels(imageColors, CubemapFace.NegativeZ);

                imageColors = loadedImage.GetPixels(size, size*2, size, size);
                cube.SetPixels(imageColors, CubemapFace.NegativeY);

                cube.Apply();
                callback(cube);

                //skyboxMat.SetTexture("_Tex", cube);
                //RenderSettings.skybox = skyboxMat;
            });



        }



        /// <summary>
        /// Update Material from DTO
        /// </summary>
        /// <param name="meshMaterial"></param>
        /// <param name="olddto"></param>
        /// <param name="newdto"></param>
        public override void UpdateFromDTO(Cubemap c, ResourceDto olddto, ResourceDto newdto)
        {
            if (newdto == null)
                return;
        }

    }
}