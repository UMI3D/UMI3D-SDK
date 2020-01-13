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
using UnityEngine;
using umi3d.edk;

public class RandomColor : MonoBehaviour
{

    /// <summary>
    /// Change the color of the color randomly.
    /// This have on purpose to be call by a OnTrigger Event.
    /// </summary>
    public void SetRandomColor()
    {
        CVEPrimitive prim = this.GetComponent<CVEPrimitive>();
        if (prim != null)
        {
            CVEMaterial mat = prim.Material;
            mat.AlbedoColor = Random.ColorHSV(0, 1, 0.7f, 1, 0.7f, 1, 1, 1);
            prim.Material = mat;
            prim.Material.SyncMaterialProperties();
        }
        else
        {
            CVEModel model = this.GetComponent<CVEModel>();
            if (model != null)
            {
                CVEMaterial mat = model.Material;
                mat.AlbedoColor = Random.ColorHSV(0, 1, 0.7f, 1, 0.7f, 1, 1, 1);
                model.Material = mat;
                prim.Material.SyncMaterialProperties();
            }
        }
    }
}
