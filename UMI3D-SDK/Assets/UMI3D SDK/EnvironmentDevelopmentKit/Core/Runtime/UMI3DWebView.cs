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

        #endregion

        #region Async properties

        private UMI3DAsyncProperty<bool> _objectCanInteract;

        private UMI3DAsyncProperty<Vector2> _objectSize;

        private UMI3DAsyncProperty<Vector2> _objectTextureSize;

        private UMI3DAsyncProperty<string> _objectUrl;


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

            objectCanInteract.OnValueChanged += (b) => canInteract = b;
            objectTextureSize.OnValueChanged += (s) => textureSize = s;
            objectSize.OnValueChanged += (s) => size = s;
            objectUrl.OnValueChanged += (u) => url = u;
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
                + UMI3DSerializer.Write(objectCanInteract.GetValue(user));
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