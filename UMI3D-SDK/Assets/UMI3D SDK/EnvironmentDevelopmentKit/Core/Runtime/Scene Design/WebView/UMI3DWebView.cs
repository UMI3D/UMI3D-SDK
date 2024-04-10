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

namespace umi3d.edk
{
    /// <summary>
    /// 3D WebView support in UMI3D.
    /// </summary>
    public class UMI3DWebView : UMI3DNode
    {
        #region Fields

        /// <summary>
        /// Can users interact with the webview ?
        /// </summary>
        [SerializeField, Tooltip("Can users interact with the webview ?")]
        private bool canInteract = true;

        /// <summary>
        /// Webview size.
        /// </summary>
        [SerializeField, Tooltip("Webview size")]
        private Vector2 size = new Vector2(1.280f, .720f);

        /// <summary>
        /// Webview texture dimensions.
        /// </summary>
        [SerializeField, Tooltip("Webview texture dimensions")]
        private Vector2 textureSize = new Vector2(1280, 720);

        /// <summary>
        /// Url to load on clients.
        /// </summary>
        [SerializeField, Tooltip("Url to load on clients")]
        private string url = string.Empty;

        /// <summary>
        /// If set to false, when <see cref="url"/> is set, the value will be ignored by the browser.
        /// </summary>
        [SerializeField, Tooltip("Url to load on clients")]
        private bool canUrlBeForced = true;

        /// <summary>
        /// If true, will use <see cref="whiteList"/> to determine which domains are allowed.
        /// </summary>
        [SerializeField, Tooltip("Use a white list ?")]
        private bool useWhiteList = false;

        /// <summary>
        /// If true, will use <see cref="whiteList"/> to determine which domains are allowed.
        /// </summary>
        [SerializeField, Tooltip("If useWhiteList true, list of authorized domains")]
        private List<string> whiteList = new();

        /// <summary>
        /// If true, will use <see cref="blackList"/> to determine which domains are not allowed.
        /// </summary>
        [SerializeField, Tooltip("Use a black list ?")]
        private bool useBlackList = false;

        /// <summary>
        /// If true, will use <see cref="whiteList"/> to determine which domains are not allowed.
        /// </summary>
        [SerializeField, Tooltip("If useBlackList true, list of non authorized domains")]
        private List<string> blackList = new();

        #endregion

        #region Async properties

        private UMI3DAsyncProperty<bool> _objectCanInteract;

        private UMI3DAsyncProperty<Vector2> _objectSize;

        private UMI3DAsyncProperty<Vector2> _objectTextureSize;

        private UMI3DAsyncProperty<string> _objectUrl;

        private UMI3DAsyncProperty<bool> _objectCanUrlBeForced;

        private UMI3DAsyncProperty<bool> _objectUseWhiteList;

        private UMI3DAsyncProperty<List<string>> _objectWhiteList;

        private UMI3DAsyncProperty<bool> _objectUseBlackList;

        private UMI3DAsyncProperty<List<string>> _objectBlackList;


        /// <summary>
        /// Async property to change <see cref="canInteract"/> property.
        /// </summary>
        public UMI3DAsyncProperty<bool> objectCanInteract { get { Register(); return _objectCanInteract; } protected set => _objectCanInteract = value; }

        /// <summary>
        /// Async property to change <see cref="size"/> property.
        /// </summary>
        public UMI3DAsyncProperty<Vector2> objectSize { get { Register(); return _objectSize; } protected set => _objectSize = value; }

        /// <summary>
        /// Async property to change <see cref="textureSize"/> property.
        /// </summary>
        public UMI3DAsyncProperty<Vector2> objectTextureSize { get { Register(); return _objectTextureSize; } protected set => _objectTextureSize = value; }

        /// <summary>
        /// Async property to change <see cref="url"/> property.
        /// </summary>
        public UMI3DAsyncProperty<string> objectUrl { get { Register(); return _objectUrl; } protected set => _objectUrl = value; }

        /// <summary>
        /// Async property to change <see cref="canUrlBeForced"/> property.
        /// </summary>
        public UMI3DAsyncProperty<bool> objectCanUrlBeForced { get { Register(); return _objectCanUrlBeForced; } protected set => _objectCanUrlBeForced = value; }


        /// <summary>
        /// Async property to change <see cref="useWhiteList"> property.
        /// </summary>
        public UMI3DAsyncProperty<bool> objectUseWhiteList { get { Register(); return _objectUseWhiteList; } protected set => _objectUseWhiteList = value; }

        /// <summary>
        /// Async property to change <see cref="whiteList"/> property.
        /// </summary>
        public UMI3DAsyncProperty<List<string>> objectWhiteList { get { Register(); return _objectWhiteList; } protected set => _objectWhiteList = value; }

        /// <summary>
        /// Async property to change <see cref="useBlackList"> property.
        /// </summary>
        public UMI3DAsyncProperty<bool> objectUseBlackList { get { Register(); return _objectUseBlackList; } protected set => _objectUseBlackList = value; }

        /// <summary>
        /// Async property to change <see cref="blackList"/> property.
        /// </summary>
        public UMI3DAsyncProperty<List<string>> objectBlackList { get { Register(); return _objectBlackList; } protected set => _objectBlackList = value; }

        #endregion

        #region Methods

        /// <summary>
        /// Initialize object's properties.
        /// </summary>
        protected override void InitDefinition(ulong id)
        {
            base.InitDefinition(id);

            objectCanInteract = new UMI3DAsyncProperty<bool>(id, UMI3DPropertyKeys.WebViewCanInteract, canInteract);
            objectSize = new UMI3DAsyncProperty<Vector2>(id, UMI3DPropertyKeys.WebViewSize, size, ToUMI3DSerializable.ToSerializableVector2, new UMI3DAsyncPropertyEquality { epsilon = 0.000001f }.Vector2Equality);
            objectTextureSize = new UMI3DAsyncProperty<Vector2>(id, UMI3DPropertyKeys.WebViewTextureSize, textureSize, ToUMI3DSerializable.ToSerializableVector2, new UMI3DAsyncPropertyEquality { epsilon = 0.000001f }.Vector2Equality);
            objectUrl = new UMI3DAsyncProperty<string>(id, UMI3DPropertyKeys.WebViewUrl, url);
            objectCanUrlBeForced = new UMI3DAsyncProperty<bool>(id, UMI3DPropertyKeys.WebViewCanUrlBeForced, canUrlBeForced);
            objectUseWhiteList = new UMI3DAsyncProperty<bool>(id, UMI3DPropertyKeys.WebViewUseWhileList, useWhiteList);
            objectWhiteList = new UMI3DAsyncProperty<List<string>>(id, UMI3DPropertyKeys.WebViewWhileList, whiteList);
            objectUseBlackList = new UMI3DAsyncProperty<bool>(id, UMI3DPropertyKeys.WebViewUseBlackList, useBlackList);
            objectBlackList = new UMI3DAsyncProperty<List<string>>(id, UMI3DPropertyKeys.WebViewBlackList, blackList);

            objectCanInteract.OnValueChanged += (b) => canInteract = b;
            objectTextureSize.OnValueChanged += (s) => textureSize = s;
            objectSize.OnValueChanged += (s) => size = s;
            objectUrl.OnValueChanged += (u) => url = u;
            objectCanUrlBeForced.OnValueChanged += (b) => canUrlBeForced = b;
            objectUseWhiteList.OnValueChanged += (u) => useWhiteList = u;
            objectWhiteList.OnValueChanged += (b) => whiteList = b;
            objectUseBlackList.OnValueChanged += (u) => useBlackList = u;
            objectBlackList.OnValueChanged += (b) => blackList = b;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override Bytable ToBytes(UMI3DUser user)
        {
            return base.ToBytes(user)
                + UMI3DSerializer.Write(objectCanInteract.GetValue(user))
                + UMI3DSerializer.Write(objectSize.GetValue(user))
                + UMI3DSerializer.Write(objectTextureSize.GetValue(user))
                + UMI3DSerializer.Write(objectUrl.GetValue(user))
                + UMI3DSerializer.Write(objectCanInteract.GetValue(user))
                + UMI3DSerializer.Write(objectCanUrlBeForced.GetValue(user))
                + UMI3DSerializer.Write(objectUseWhiteList.GetValue(user))
                + UMI3DSerializer.Write(objectWhiteList.GetValue(user))
                + UMI3DSerializer.Write(objectUseBlackList.GetValue(user))
                + UMI3DSerializer.Write(objectBlackList.GetValue(user));
        }

        /// <summary>
        /// Create an empty Dto.
        /// </summary>
        /// <returns></returns>
        protected override UMI3DNodeDto CreateDto()
        {
            return new UMI3DWebViewDto();
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        protected override void WriteProperties(UMI3DAbstractNodeDto dto, UMI3DUser user)
        {
            base.WriteProperties(dto, user);

            var webViewDto = dto as UMI3DWebViewDto;
            webViewDto.canInteract = objectCanInteract.GetValue(user);
            webViewDto.size = objectSize.GetValue(user).Dto();
            webViewDto.textureSize = objectTextureSize.GetValue(user).Dto();
            webViewDto.url = objectUrl.GetValue(user);
            webViewDto.canUrlBeForced = objectCanUrlBeForced.GetValue(user);
            webViewDto.useWhiteList = objectUseWhiteList.GetValue(user);
            webViewDto.whiteList = objectWhiteList.GetValue(user);
            webViewDto.useBlackList = objectUseBlackList.GetValue(user);
            webViewDto.blackList = objectBlackList.GetValue(user);
        }

        #endregion

        #region Gizmos

        private void OnDrawGizmos()
        {
            float halfHeight = transform.lossyScale.y * size.y / 2f;
            float halfWidth = transform.lossyScale.x * size.x / 2f;

            Color prev = Gizmos.color;
            Gizmos.color = Color.yellow;

            Gizmos.DrawLine(transform.position + transform.up * halfHeight - transform.right * halfWidth,
                transform.position + transform.up * halfHeight + transform.right * halfWidth);
            Gizmos.DrawLine(transform.position + transform.up * halfHeight + transform.right * halfWidth,
                transform.position - transform.up * halfHeight + transform.right * halfWidth);
            Gizmos.DrawLine(transform.position - transform.up * halfHeight + transform.right * halfWidth,
                transform.position - transform.up * halfHeight - transform.right * halfWidth);
            Gizmos.DrawLine(transform.position + transform.up * halfHeight - transform.right * halfWidth,
                transform.position - transform.up * halfHeight - transform.right * halfWidth);

            Gizmos.color = new Color(236 / 255f, 229 / 255f, 199 / 255f, .7f);
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(Vector3.zero, new Vector3(size.x, size.y, 0.001f));

#if UNITY_EDITOR

            var style = new GUIStyle { alignment = TextAnchor.MiddleCenter };
            style.normal.textColor = Color.yellow;
            UnityEditor.Handles.Label(transform.position, "UMI3DWebView \n (" + textureSize.x + "x" + textureSize.y + ")", style);
#endif

            Gizmos.matrix = default;
            Gizmos.color = prev;
        }

        #endregion
    }
}