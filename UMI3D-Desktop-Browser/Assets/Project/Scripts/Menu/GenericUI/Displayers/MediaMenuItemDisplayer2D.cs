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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using umi3d.cdk;
using umi3d.cdk.menu.view;
using TMPro;

namespace BrowserDesktop.Menu
{
    /// <summary>
    /// Displayer for media menu item.
    /// </summary>
    public class MediaMenuItemDisplayer2D : AbstractMediaMenuItemDisplayer
    {

        private bool isDisplaying = false;

        public Texture2D DefaultIcon;
        public Image LoadingBackgroud;
        public Image Icon;

        public float loadingRotationSpeed = 1f;

        public TextMeshProUGUI Label;

        /// <summary>
        /// Event raised when the model has been fully loaded in the scene on display.
        /// </summary>
        public UnityEvent onIConLoaded = new UnityEvent();

        Coroutine LoadingAnimation;

        public override void Clear()
        {
            isDisplaying = false;
        }


        public override void Display(bool forceUpdate = false)
        {
            if (LoadingAnimation != null)
            {
                StopCoroutine(LoadingAnimation);
                LoadingAnimation = null;
            }

            if (!isDisplaying || forceUpdate)
            {
                Label.text = mediaMenuItem.Name;

                if (mediaMenuItem.media.Icon != null)
                {
                    LoadingBackgroud.enabled = true;
                    LoadingAnimation = StartCoroutine(_LoadingAnimation());
                    HDResourceCache.Download(mediaMenuItem.media.Icon,
                        Texture2D =>
                        {
                            SetIcon(Texture2D);
                        },
                        WWW => 
                        {
                            SetIcon(null);
                        }
                    );

                }
                else
                {
                    SetIcon(null);
                }
                    //displayer.GetComponentInChildren<Image>().sprite = mediaMenuItem.media.Icon;

                isDisplaying = true;
            }
            else
            {
                gameObject.SetActive(true);
            }
        }

        void SetIcon(Texture2D texture)
        {
            if (texture != null)
            {
                Icon.sprite = Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100.0f);
            }
            else
            {
                Icon.sprite = Sprite.Create(DefaultIcon, new Rect(0.0f, 0.0f, DefaultIcon.width, DefaultIcon.height), new Vector2(0.5f, 0.5f), 100.0f);
            }
            if (LoadingAnimation != null)
            {
                StopCoroutine(LoadingAnimation);
                LoadingAnimation = null;
            }
            LoadingBackgroud.enabled = false;
        }


        public override void Hide()
        {
            gameObject.SetActive(false);
            isDisplaying = false;
        }

        public override int IsSuitableFor(umi3d.cdk.menu.core.AbstractMenuItem menu) { 
            return (menu is MediaMenuItem) ? 500 : 0; 
        }

        public IEnumerator _LoadingAnimation()
        {
            var Wait = new WaitForFixedUpdate();
            while (true)
            {
                LoadingBackgroud.transform.Rotate(0, 0, loadingRotationSpeed * Time.fixedDeltaTime);
                yield return Wait;
            }
        }

        private void OnDestroy()
        {
            StopAllCoroutines();
            if (LoadingAnimation != null)
            {
                StopCoroutine(LoadingAnimation);
                LoadingAnimation = null;
            }
        }

    }
}
