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
using BrowserDesktop.Menu;
using System.Collections;
using System.Collections.Generic;
using umi3d.cdk.menu.core;
using umi3d.cdk.menu.view;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HoldableButtonDisplayer2D : AbstractDisplayer, IMenuItemAnchor
{
    /// <summary>
    /// Button menu item to display.
    /// </summary>
    protected HoldableButtonMenuItem menuItem;

    /// <summary>
    /// Button
    /// </summary>
    public Button button;

    Vector3 anchor;
    Vector3 direction;
    public RectTransform Button;
    RectTransform rect;

    Vector2 size = Vector2.zero;

    public void Set(Vector3 anchor, Vector3 direction)
    {
        this.anchor = anchor;
        this.direction = direction;
        size = Vector2.one;
        rect = GetComponent<RectTransform>();
        LayoutRebuilder.ForceRebuildLayoutImmediate(Button);
        LayoutRebuilder.MarkLayoutForRebuild(Button);
        LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
        LayoutRebuilder.MarkLayoutForRebuild(rect);
        rect.localPosition = ComputePosition(Button);
    }

    private void Update()
    {
        Vector2 size = Button.sizeDelta;
        if (size != this.size)
        {
            this.size = size;
            rect.localPosition = ComputePosition(Button);
        }
    }

    Vector3 ComputePosition(RectTransform rect)
    {
        Vector3 projection = new Vector3(Vector2.Dot(direction, Vector2.right), Vector2.Dot(direction, Vector2.up));
        if (projection.x < 0.5f && projection.x > -0.5f) projection.x = 0;
        else projection.x = Mathf.Sign(projection.x) * size.x / 2;
        if (projection.y < 0.5f && projection.y > -0.5f) projection.y = 0;
        else projection.y = Mathf.Sign(projection.y) * size.y / 2;
        projection -= new Vector3(size.x,-size.y)/2;
        return anchor + projection;
    }

    /// <summary>
    /// Notify that the button has been pressed.
    /// </summary>
    public void NotifyPressDown()
    {
        menuItem.NotifyValueChange(true);
    }
    public void NotifyPressUp()
    {
        menuItem.NotifyValueChange(false);
    }

    /// <summary>
    /// Set menu item to display and initialise the display.
    /// </summary>
    /// <param name="item">Item to display</param>
    /// <returns></returns>
    public override void SetMenuItem(AbstractMenuItem item)
    {
        if (item is HoldableButtonMenuItem)
        {
            menuItem = item as HoldableButtonMenuItem;
            button.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = item.Name;
        }
        else
            throw new System.Exception("MenuItem must be a ButtonInput");
    }

    public override int IsSuitableFor(umi3d.cdk.menu.core.AbstractMenuItem menu)
    {
        return (menu is HoldableButtonMenuItem) ? 2 : 0;
    }

    public override void Clear()
    {
    }

    public override void Display(bool forceUpdate = false)
    {
        button.gameObject.SetActive(true);
    }

    public override void Hide()
    {
        button.gameObject.SetActive(false);
    }
}
