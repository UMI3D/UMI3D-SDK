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
using umi3d.cdk;
using umi3d.common;
using UnityEngine;

[System.Serializable]
public class KeyMenuInput : AbstractUMI3DInput
{

    /// <summary>
    /// Associtated interaction (if any).
    /// </summary>
    public EventDto associatedInteraction { get; protected set; }
    /// <summary>
    /// Avatar bone linked to this input.
    /// </summary>
    public BoneType bone = BoneType.Hand_Right;

    protected BoneDto boneDto;
    bool risingEdgeEventSent;

    HoldableButtonMenuItem menuItem;

    public override void Associate(AbstractInteractionDto interaction)
    {
        if (associatedInteraction != null)
        {
            throw new System.Exception("This input is already binded to a interaction ! (" + associatedInteraction + ")");
        }

        if (IsCompatibleWith(interaction))
        {
            associatedInteraction = interaction as EventDto;
            menuItem = new HoldableButtonMenuItem
            {
                Name = associatedInteraction.Name,
                Holdable = associatedInteraction.Hold
            };
            menuItem.Subscribe(Pressed);
            if (CircleMenu.Exist)
            {
                CircleMenu.Instance.MenuDisplayManager.menu.Add(menuItem);
            }
        }
        else
        {
            throw new System.Exception("Trying to associate an uncompatible interaction !");
        }
    }

    public override void Associate(ManipulationDto manipulation, DofGroupEnum dofs)
    {
        throw new System.Exception("This input is can not be associated with a manipulation");
    }

    public override AbstractInteractionDto CurrentInteraction()
    {
        return associatedInteraction;
    }

    public override void Dissociate()
    {
        associatedInteraction = null;
        if (CircleMenu.Exist && menuItem != null)
        {
            CircleMenu.Instance.MenuDisplayManager.menu.Remove(menuItem);
        }
        menuItem.UnSubscribe(Pressed);
        menuItem = null;
    }

    public override bool IsAvailable()
    {
        return associatedInteraction == null;
    }

    public override bool IsCompatibleWith(AbstractInteractionDto interaction)
    {
        return interaction is EventDto;
    }

    void Pressed(bool down)
    {
        if (boneDto == null)
            boneDto = UMI3DBrowserAvatar.Instance.avatar.boneList.Find(b => b.type == bone);
        if (down)
        {
            onInputDown.Invoke();
            if ((associatedInteraction).Hold)
            {
                UMI3DHttpClient.Interact(associatedInteraction.Id, new object[2] { true, boneDto.Id });
                risingEdgeEventSent = true;
            }
            else
            {
                UMI3DHttpClient.Interact(associatedInteraction.Id, new object[2] { true, boneDto.Id });
            }
        }
        else
        {
            onInputUp.Invoke();
            if ((associatedInteraction).Hold)
            {
                if (risingEdgeEventSent)
                {
                    UMI3DHttpClient.Interact(associatedInteraction.Id, new object[2] { false, boneDto.Id });
                    risingEdgeEventSent = false;
                }
            }
        }
    }
}
