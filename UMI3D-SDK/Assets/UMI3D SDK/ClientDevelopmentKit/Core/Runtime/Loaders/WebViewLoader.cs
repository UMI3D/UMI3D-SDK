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

using System.Threading.Tasks;
using umi3d.common;

namespace umi3d.cdk
{
    /// <summary>
    /// Loader to load <see cref="AbstractUMI3DWebView"/>.
    /// </summary>
    public class WebViewLoader : UMI3DNodeLoader
    {
        private const DebugScope scope = DebugScope.CDK | DebugScope.Core | DebugScope.Loading;

        public override bool CanReadUMI3DExtension(ReadUMI3DExtensionData data)
        {
            return data?.dto is UMI3DWebViewDto && base.CanReadUMI3DExtension(data);
        }

        public override async Task ReadUMI3DExtension(ReadUMI3DExtensionData data)
        {
            await base.ReadUMI3DExtension(data);

            if (AbstractWebViewFactory.Exists)
            {
                AbstractUMI3DWebView webView = await AbstractWebViewFactory.Instance.CreateWebView();
                webView.Init(data.dto as UMI3DWebViewDto);
                webView.transform.SetParent(data.node.transform);
                webView.transform.localPosition = UnityEngine.Vector3.zero;
                webView.transform.localRotation = UnityEngine.Quaternion.identity;
            } else
            {
                UMI3DLogger.LogError("Impossible to load WebView, not implemented on this browser", scope);
            }
        }

        public override async Task<bool> SetUMI3DProperty(SetUMI3DPropertyData data)
        {
            var node = data.entity as UMI3DNodeInstance;
            if (node == null) return false;
            var dto = (node.dto as GlTFNodeDto)?.extensions?.umi3d as UMI3DWebViewDto;
            if (dto == null) return false;

            AbstractUMI3DWebView webView = node.gameObject.GetComponentInChildren<AbstractUMI3DWebView>();
            if (webView == null)
            {
                UMI3DLogger.Log("Webview should not be null on " + node.transform.name, scope);
                return false;
            }

            switch (data.property.property)
            {
                case UMI3DPropertyKeys.WebViewCanInteract:
                    webView.canInteract = dto.canInteract = (bool)data.property.value;
                    break;
                case UMI3DPropertyKeys.WebViewSize:
                    dto.size = (Vector2Dto)data.property.value;
                    webView.size = dto.size.Struct();
                    break;
                case UMI3DPropertyKeys.WebViewTextureSize:
                    dto.textureSize = (Vector2Dto)data.property.value;
                    webView.textureSize = dto.textureSize.Struct();
                    break;
                case UMI3DPropertyKeys.WebViewUrl:
                    webView.url = dto.url = (string)data.property.value;
                    break;
                case UMI3DPropertyKeys.WebViewCanUrlBeForced:
                    webView.canUrlBeForced = dto.canUrlBeForced = (bool)data.property.value;
                    break;
                default:
                    return false;
            }

            return true;
        }

        public override async Task<bool> SetUMI3DProperty(SetUMI3DPropertyContainerData data)
        {
            var node = data.entity as UMI3DNodeInstance;
            if (node == null) return false;
            var dto = (node.dto as GlTFNodeDto)?.extensions?.umi3d as UMI3DWebViewDto;
            if (dto == null) return false;

            AbstractUMI3DWebView webView = node.gameObject.GetComponentInChildren<AbstractUMI3DWebView>();
            if (webView == null)
            {
                UMI3DLogger.Log("Webview should not be null on " + node.transform.name, scope);
                return false;
            }

            switch (data.propertyKey)
            {
                case UMI3DPropertyKeys.WebViewCanInteract:
                    webView.canInteract = dto.canInteract = UMI3DSerializer.Read<bool>(data.container);
                    break;
                case UMI3DPropertyKeys.WebViewSize:
                    dto.size = UMI3DSerializer.Read<Vector2Dto>(data.container);
                    webView.size = dto.size.Struct();
                    break;
                case UMI3DPropertyKeys.WebViewTextureSize:
                    dto.textureSize = UMI3DSerializer.Read<Vector2Dto>(data.container);
                    webView.textureSize = dto.textureSize.Struct();
                    break;
                case UMI3DPropertyKeys.WebViewUrl:
                    webView.url = dto.url = UMI3DSerializer.Read<string>(data.container);
                    break;
                case UMI3DPropertyKeys.WebViewCanUrlBeForced:
                    webView.canUrlBeForced = dto.canUrlBeForced = UMI3DSerializer.Read<bool>(data.container);
                    break;
                default:
                    return false;
            }

            return true;
        }
    }
}