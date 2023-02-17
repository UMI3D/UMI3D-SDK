/*
Copyright 2019 - 2021 Inetum

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


using inetum.unityUtils;
using System.Collections;
using System.Collections.Generic;
using umi3d.edk.userCapture;
using umi3d.edk;
using UnityEngine;

public class BindingManager : Singleton<BindingManager> 
{
    #region Bindings

    /// <summary>
    /// Set the activation of Bindings of an Avatar Node.
    /// </summary>
    /// <param name="obj">the avatar node</param>
    /// <param name="b">the activation value</param>
    /// <returns>The associated SetEntityProperty</returns>
    public SetEntityProperty UpdateBindingActivation(UMI3DTrackedUser obj, bool b)
    {
        UMI3DTrackedUser.onActivationValueChanged.Invoke(obj.Id(), b);
        return obj.activeBindings.SetValue(b);
    }

    /// <summary>
    /// Set the activation of Bindings of an Avatar Node for a given user.
    /// </summary>
    /// <param name="user">the user</param>
    /// <param name="obj">the avatar node</param>
    /// <param name="b">the activation value</param>
    /// <returns>The associated SetEntityProperty</returns>
    public SetEntityProperty UpdateBindingActivation(UMI3DUser user, UMI3DTrackedUser obj, bool b)
    {
        UMI3DTrackedUser.onActivationValueChanged.Invoke(obj.Id(), b);
        return obj.activeBindings.SetValue(user, b);
    }

    /// <summary>
    /// Set the list of Bindings of an Avatar Node.
    /// </summary>
    /// <param name="obj">the avatar node</param>
    /// <param name="bindings">the list of bindings</param>
    /// <returns>The associated SetEntityProperty</returns>
    public SetEntityProperty UpdateBindingList(UMI3DTrackedUser obj, List<UMI3DBinding> bindings)
    {
        return obj.bindings.SetValue(bindings);
    }

    /// <summary>
    /// Set the list of Bindings of an Avatar Node for a given user.
    /// </summary>
    /// <param name="user">the user</param>
    /// <param name="obj">the avatar node</param>
    /// <param name="bindings">the list of bindings</param>
    /// <returns>The associated SetEntityProperty</returns>
    public SetEntityProperty UpdateBindingList(UMI3DUser user, UMI3DTrackedUser obj, List<UMI3DBinding> bindings)
    {
        return obj.bindings.SetValue(user, bindings);
    }

    /// <summary>
    /// Update the Binding value of an Avatar Node at a given index.
    /// </summary>
    /// <param name="obj">the avatar node</param>
    /// <param name="index">the given index</param>
    /// <param name="binding">the new binding value</param>
    /// <returns>The associated SetEntityProperty</returns>
    public SetEntityProperty UpdateBinding(UMI3DTrackedUser obj, int index, UMI3DBinding binding)
    {
        return obj.bindings.SetValue(index, binding);
    }

    /// <summary>
    /// Update the Binding value of an Avatar Node at a given index for a given user.
    /// </summary>
    /// <param name="user">the user</param>
    /// <param name="obj">the avatar node</param>
    /// <param name="index">the given index</param>
    /// <param name="binding">the new binding value</param>
    /// <returns>The associated SetEntityProperty</returns>
    public SetEntityProperty UpdateBinding(UMI3DUser user, UMI3DTrackedUser obj, int index, UMI3DBinding binding)
    {
        return obj.bindings.SetValue(user, index, binding);
    }

    /// <summary>
    /// Add a new Binding for an Avatar Node.
    /// </summary>
    /// <param name="obj">the avatar node</param>
    /// <param name="binding">the new binding value</param>
    /// <returns>The associated SetEntityProperty</returns>
    public SetEntityProperty AddBinding(UMI3DTrackedUser obj, UMI3DBinding binding)
    {
        return obj.bindings.Add(binding);
    }

    /// <summary>
    /// Add a new Binding for an Avatar Node for a given user.
    /// </summary>
    /// <param name="user">the user</param>
    /// <param name="obj">the avatar node</param>
    /// <param name="binding">the new binding value</param>
    /// <returns>The associated SetEntityProperty</returns>
    public SetEntityProperty AddBinding(UMI3DUser user, UMI3DTrackedUser obj, UMI3DBinding binding)
    {
        return obj.bindings.Add(user, binding);
    }

    /// <summary>
    /// Remove a Binding for an Avatar Node.
    /// </summary>
    /// <param name="obj">the avatar node</param>
    /// <param name="binding">the new binding value</param>
    /// <param name="keepWorldPosition">the boolean to freeze the object in the world</param>
    /// <param name="newparent">a transform intended to be the new parent. If keepWorldPosition is true, newparent must be specified.</param>
    /// <returns>The list of associated SetEntityProperty.</returns>
    public List<SetEntityProperty> RemoveBinding(UMI3DTrackedUser obj, UMI3DBinding binding, bool keepWorldPosition = false, UMI3DAbstractNode newparent = null)
    {
        var operations = new List<SetEntityProperty>();

        if (keepWorldPosition && newparent != null)
        {
            binding.node.transform.SetParent(newparent.transform, true);
            operations.Add(binding.node.objectParentId.SetValue(newparent.GetComponent<UMI3DAbstractNode>()));

            operations.Add(binding.node.objectPosition.SetValue(binding.node.transform.localPosition));
            operations.Add(binding.node.objectRotation.SetValue(binding.node.transform.localRotation));
        }

        operations.Insert(0, obj.bindings.Remove(binding));
        return operations;
    }

    /// <summary>
    /// Remove a Binding for an Avatar Node for a given user.
    /// </summary>
    /// <param name="user">the user</param>
    /// <param name="obj">the avatar node</param>
    /// <param name="binding">the new binding value</param>
    /// <param name="keepWorldPosition">the boolean to freeze the object in the world</param>
    /// <param name="newparent">a transform intended to be the new parent. If keepWorldPosition is true, newparent must be specified.</param>
    /// <returns>The list of associated SetEntityProperty</returns>
    public List<SetEntityProperty> RemoveBinding(UMI3DUser user, UMI3DTrackedUser obj, UMI3DBinding binding, bool keepWorldPosition = false, UMI3DAbstractNode newparent = null)
    {
        var operations = new List<SetEntityProperty>();

        if (keepWorldPosition && newparent != null)
        {
            binding.node.transform.SetParent(newparent.transform, true);
            operations.Add(binding.node.objectParentId.SetValue(user, newparent.GetComponent<UMI3DAbstractNode>()));

            operations.Add(binding.node.objectPosition.SetValue(user, binding.node.transform.localPosition));
            operations.Add(binding.node.objectRotation.SetValue(user, binding.node.transform.localRotation));
        }

        operations.Insert(0, obj.bindings.Remove(user, binding));
        return operations;
    }

    /// <summary>
    /// Remove a Binding at a given index for an Avatar Node.
    /// </summary>
    /// <param name="obj">the avatar node</param>
    /// <param name="index">the given index</param>
    /// <param name="keepWorldPosition">the boolean to freeze the object in the world</param>
    /// <param name="newparent">a transform intended to be the new parent. If keepWorldPosition is true, newparent must be specified.</param>
    /// <returns>The list of associated SetEntityProperty</returns>
    public List<SetEntityProperty> RemoveBinding(UMI3DAvatarNode obj, int index, bool keepWorldPosition = false, UMI3DAbstractNode newparent = null)
    {
        var operations = new List<SetEntityProperty>();

        if (keepWorldPosition && newparent != null)
        {
            UMI3DBinding binding = obj.bindings.GetValue(index);

            binding.node.transform.SetParent(newparent.transform, true);
            SetEntityProperty op = binding.node.objectParentId.SetValue(newparent.GetComponent<UMI3DAbstractNode>());

            operations.Add(op);

            operations.Add(binding.node.objectPosition.SetValue(binding.node.transform.localPosition));
            operations.Add(binding.node.objectRotation.SetValue(binding.node.transform.localRotation));
        }

        operations.Insert(0, obj.bindings.RemoveAt(index));
        return operations;
    }

    /// <summary>
    /// Remove a Binding at a given index for an Avatar Node for a given user.
    /// </summary>
    /// <param name="user">the user</param>
    /// <param name="obj">the avatar node</param>
    /// <param name="index">the given index</param>
    /// <param name="keepWorldPosition">the boolean to freeze the object in the world</param>
    /// <param name="newparent">a transform intended to be the new parent. If keepWorldPosition is true, newparent must be specified.</param>
    /// <returns>The list of associated SetEntityProperty</returns>
    public List<SetEntityProperty> RemoveBinding(UMI3DUser user, UMI3DAvatarNode obj, int index, bool keepWorldPosition = false, UMI3DAbstractNode newparent = null)
    {
        var operations = new List<SetEntityProperty>();

        if (keepWorldPosition && newparent != null)
        {
            UMI3DBinding binding = obj.bindings.GetValue(index);

            binding.node.transform.SetParent(newparent.transform, true);
            operations.Add(binding.node.objectParentId.SetValue(user, newparent.GetComponent<UMI3DAbstractNode>()));

            operations.Add(binding.node.objectPosition.SetValue(user, binding.node.transform.localPosition));
            operations.Add(binding.node.objectRotation.SetValue(user, binding.node.transform.localRotation));
        }

        operations.Insert(0, obj.bindings.RemoveAt(user, index));
        return operations;
    }

    #endregion
}
