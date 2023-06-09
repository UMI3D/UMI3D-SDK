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
using UnityEngine;

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
            return data?.dto is UMI3DWebViewDto;
        }

        public override async Task ReadUMI3DExtension(ReadUMI3DExtensionData data)
        {
            await base.ReadUMI3DExtension(data);

            if (AbstractWebViewFactory.Exists)
            {
                AbstractUMI3DWebView webView = AbstractWebViewFactory.Instance.GetWebView();
                webView.Init(data.dto as UMI3DWebViewDto);
            } else
            {
                UMI3DLogger.LogError("Impossible to load WebView, not implemented on this browser", scope);
            }
        }

        public override async Task<bool> SetUMI3DProperty(SetUMI3DPropertyData value)
        {
            if (await base.SetUMI3DProperty(value))
                return true;

            var node = value.entity as UMI3DNodeInstance;
            if (node == null) return false;

            switch (value.property.property)
            {
            }

            return true;
        }

        public override async Task<bool> SetUMI3DProperty(SetUMI3DPropertyContainerData data)
        {
            if (await base.SetUMI3DProperty(data))
                return true;

            var node = data.entity as UMI3DNodeInstance;
            if (node == null) return false;

            switch (data.propertyKey)
            {
            }

            return true;
        }
    }
}