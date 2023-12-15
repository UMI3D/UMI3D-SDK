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
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public static class ShaderFix
{
    public static void FixShadersForEditor(GameObject prefab)
    {
        var renderers = prefab.GetComponentsInChildren<Renderer>(true);
        foreach (var renderer in renderers)
        {
            ReplaceShaderForEditor(renderer.sharedMaterials);
        }

        var tmps = prefab.GetComponentsInChildren<TextMeshProUGUI>(true);
        foreach (var tmp in tmps)
        {
            ReplaceShaderForEditor(tmp.material);
            ReplaceShaderForEditor(tmp.materialForRendering);
        }

        var spritesRenderers = prefab.GetComponentsInChildren<SpriteRenderer>(true);
        foreach (var spriteRenderer in spritesRenderers)
        {
            ReplaceShaderForEditor(spriteRenderer.sharedMaterials);
        }

        var images = prefab.GetComponentsInChildren<Image>(true);
        foreach (var image in images)
        {
            ReplaceShaderForEditor(image.material);
        }

        var particleSystemRenderers = prefab.GetComponentsInChildren<ParticleSystemRenderer>(true);
        foreach (var particleSystemRenderer in particleSystemRenderers)
        {
            ReplaceShaderForEditor(particleSystemRenderer.sharedMaterials);
        }

        var particles = prefab.GetComponentsInChildren<ParticleSystem>(true);
        foreach (var particle in particles)
        {
            var renderer = particle.GetComponent<Renderer>();
            if (renderer != null) ReplaceShaderForEditor(renderer.sharedMaterials);
        }
    }

    public static void ReplaceShaderForEditor(Material[] materials)
    {
        for (int i = 0; i < materials.Length; i++)
        {
            ReplaceShaderForEditor(materials[i]);
        }
    }

    public static void ReplaceShaderForEditor(Material material)
    {
        if (material == null) return;

        var shaderName = material.shader.name;
        var shader = Shader.Find(shaderName);

        if (shader != null) material.shader = shader;
    }
}