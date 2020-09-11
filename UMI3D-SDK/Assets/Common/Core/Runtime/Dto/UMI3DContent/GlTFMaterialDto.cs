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

namespace umi3d.common
{
    [System.Serializable]

    /// <summary>
    /// Should follow glTF 2.0 specifications.
    /// https://github.com/KhronosGroup/glTF/tree/master/specification/2.0#materials
    /// UMI3D-based textures using ResourceDto should be defined in the umi3d extension.
    /// </summary>
    public class GlTFMaterialDto
    {
        public string alphaMode;
        public bool doubleSided = false;
        public string name;
        public PBRMaterialDto pbrMetallicRoughness = new PBRMaterialDto();
        public GlTFMaterialExtensions extensions = null;


        public SerializableVector3 emissiveFactor;
    }


    /*
    
        {
            "name" : "MatBed",
            "normalTexture" : {
                "index" : 0
            },
            "occlusionTexture" : {
                "index" : 1
            },
            "pbrMetallicRoughness" : {
                "baseColorTexture" : {
                    "index" : 2
                },
                "metallicRoughnessTexture" : {
                    "index" : 1
                }
            }
        },
        {
            "alphaMode": "BLEND",
            "doubleSided" : true,
            "name" : "MatBlend",
            "pbrMetallicRoughness" : {
                "baseColorTexture" : {
                    "index" : 3
                },
                "metallicFactor" : 0.0,
                "roughnessFactor" : 0.800000011920929
            }
        },


    TILLING 

This example utilizes only the lower left quadrant of the source image, rotated clockwise 90°.

{
  "materials": [{
    "emissiveTexture": {
      "index": 0,
      "extensions": {
        "KHR_texture_transform": {
          "offset": [0, 1],
          "rotation": 1.57079632679,
          "scale": [0.5, 0.5]
        }
      }
    }
  }]
}

This example inverts the T axis, effectively defining a bottom-left origin.

{
  "materials": [{
    "emissiveTexture": {
      "index": 0,
      "extensions": {
        "KHR_texture_transform": {
          "offset": [0, 1],
          "scale": [1, -1]
        }
      }
    }
  }]
}

    


    */
}
