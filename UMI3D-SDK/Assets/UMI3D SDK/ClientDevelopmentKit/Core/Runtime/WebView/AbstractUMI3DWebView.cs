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

using System.Collections.Generic;
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

        private bool _canInteract = true;

        /// <summary>
        /// Can user interact with webview.
        /// </summary>
        public bool canInteract
        {
            get => _canInteract || isAdmin;
            set
            {
                if (_canInteract != value)
                {
                    _canInteract = value;
                    OnCanInteractChanged(value);
                }
            }
        }

        private string _url = string.Empty;

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

                    if (!string.IsNullOrEmpty(_url))
                        OnUrlChanged(value);
                }
            }
        }

        private int _scrollOffset = 0;

        /// <summary>
        /// Web view url
        /// </summary>
        public int scrollOffset
        {
            get => _scrollOffset;
            set
            {
                if (!canUrlBeForced)
                    return;

                if (_scrollOffset != value)
                {
                    _scrollOffset = value;
                    OnScrollOffsetChanged(value);
                }
            }
        }

        private bool _isAdmin = false;

        /// <summary>
        /// Web view url
        /// </summary>
        public bool isAdmin
        {
            get => _isAdmin;
            set
            {
                if (_isAdmin != value)
                {
                    _isAdmin = value;

                    OnAdminStatusChanged(value);
                }
            }
        }

        private Vector2 _size = Vector2.zero;

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

        private Vector2 _textureSize = Vector2.zero;

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

        private bool _canUrlBeForced = true;

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

        /// <summary>
        /// if true, will use <see cref="whiteList"/> to determine which domains are allowed.
        /// </summary>
        public bool useWhiteList { get; set; } = false;

        /// <summary>
        /// Authorized domains.
        /// </summary>
        public List<string> whiteList { get; set; } = new List<string>();

        /// <summary>
        /// If true, will use <see cref="useBlackList"/> to determine which domains are prohibited.
        /// </summary>
        public bool useBlackList { get; set; } = false;

        /// <summary>
        /// Not authorized domains.
        /// </summary>
        public List<string> blackList { get; set; } = new List<string>();

        #endregion

        #region Methods

        public virtual void Init(UMI3DWebViewDto dto)
        {
            canUrlBeForced = dto.canUrlBeForced;

            useWhiteList = dto.useWhiteList;
            whiteList = dto.whiteList;
            useBlackList = dto.useBlackList;
            blackList = dto.blackList;

            url = dto.url;
            scrollOffset = dto.scrollOffset;
            size = dto.size.Struct();
            textureSize = dto.textureSize.Struct();
            canInteract = dto.canInteract;
            isAdmin = dto.isAdmin;

            OnCanInteractChanged(canInteract);
        }

        protected abstract void OnUrlChanged(string url);

        protected abstract void OnScrollOffsetChanged(int scroll);

        protected abstract void OnAdminStatusChanged(bool admin);

        protected abstract void OnSizeChanged(Vector2 size);

        protected abstract void OnTextureSizeChanged(Vector2 size);

        protected abstract void OnCanInteractChanged(bool canInteract);

        #endregion
    }
}