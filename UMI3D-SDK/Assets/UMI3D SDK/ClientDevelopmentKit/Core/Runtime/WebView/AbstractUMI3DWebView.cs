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

using umi3d.common;
using UnityEngine;

namespace umi3d.cdk
{
    /// <summary>
    /// 3D WebView
    /// </summary>
    public abstract class AbstractUMI3DWebView : MonoBehaviour, IEntity
    {
        #region Fields

        private bool _canInteract;

        /// <summary>
        /// Can user interact with webview.
        /// </summary>
        public bool canInteract
        {
            get => canInteract;
            set
            {
                if (_canInteract != value)
                {
                    _canInteract = value;
                    OnCanInteractChanged(value);
                }
            }
        }

        private string _url;
        /// <summary>
        /// Web view url
        /// </summary>
        public string url
        {
            get => _url;
            set
            {
                if (!canUrlBeForced)
                    return;

                if (_url != value)
                {
                    _url = value;

                    if(!string.IsNullOrEmpty(_url))
                        OnUrlChanged(value);
                }
            }
        }

        private Vector2 _size;
        /// <summary>
        /// Webview size.
        /// </summary>
        public Vector2 size
        {
            get => _size;
            set
            {
                if (_size != value)
                {
                    _size = value;
                    OnSizeChanged(value);
                }
            }
        }

        private Vector2 _textureSize;
        /// <summary>
        /// Webview texture dimension.
        /// </summary>
        public Vector2 textureSize
        {
            get => _textureSize;
            set
            {
                if (_textureSize != value)
                {
                    _textureSize = value;
                    OnTextureSizeChanged(value);
                }
            }
        }

        private bool _canUrlBeForced;
        /// <summary>
        /// Can <see cref="url"/> be forced by the server.
        /// </summary>
        public bool canUrlBeForced
        {
            get => _canUrlBeForced;
            set
            {
                if (_canUrlBeForced != value)
                {
                    _canUrlBeForced = value;
                }
            }
        }

        #endregion

        #region Methods

        public virtual void Init(UMI3DWebViewDto dto)
        {
            url = dto.url;
            size = dto.size.Struct();
            textureSize = dto.textureSize.Struct();
            canInteract = dto.canInteract;
        }

        protected abstract void OnUrlChanged(string url);

        protected abstract void OnSizeChanged(Vector2 size);

        protected abstract void OnTextureSizeChanged(Vector2 size);

        protected abstract void OnCanInteractChanged(bool canInteract);

        #endregion
    }
}