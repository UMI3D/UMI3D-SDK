using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using umi3d.cdk;
using umi3d.cdk.userCapture;
using umi3d.cdk.utils.extrapolation;
using umi3d.common;
using umi3d.common.userCapture;
using UnityEngine;

public abstract class AbstractSkeleton : ISkeleton
{
    public Dictionary<uint, ISkeleton.s_Transform> Bones { get => bones; set => bones = value; }
    protected Dictionary<uint, ISkeleton.s_Transform> bones;
    public List<ISubSkeleton> Skeletons { get => skeletons; set => skeletons = value; }
    protected List<ISubSkeleton> skeletons;
    protected const DebugScope scope = DebugScope.CDK | DebugScope.UserCapture;

    protected AbstractSkeleton()
    {

    }

    /// <summary>
    /// Has the user currently active bindings?
    /// </summary>
    public bool activeUserBindings { get; protected set; }
    /// <summary>
    /// User's registered id
    /// </summary>
    public ulong userId { get; protected set; }

    /// <summary>
    /// Extrapolator for the avatar position.
    /// </summary>
    protected Vector3LinearDelayedExtrapolator nodePositionExtrapolator { get; set; }

    /// <summary>
    /// Extrapolator for the avatar rotation.
    /// </summary>
    protected QuaternionLinearDelayedExtrapolator nodeRotationExtrapolator { get; set; }

    protected List<Bound> bounds { get; set; }

    public List<Transform> boundRigs { get; set; }
    /// <summary>
    /// List of currently applied <see cref="BoneBindingDto"/> to the user's skeleton.
    /// </summary>
    public List<BoneBindingDto> userBindings { get; protected set; }
    /// <summary>
    /// Saves of the transform of objects before they had been bound to a user's bone.
    /// </summary>
    protected Dictionary<BoundObject, SavedTransform> savedTransforms { get; set; }

    #region Data Structure
    /// <summary>
    /// Represents a binding.
    /// </summary>
    /// Used mainly for computational purposes.
    protected struct Bound
    {
        public uint bonetype;
        public Transform obj;
        public Vector3 offsetPosition;
        public Quaternion offsetRotation;
        public Vector3 offsetScale;
        public bool syncPos;
        public bool syncRot;
        public bool freezeWorldScale;
        public Vector3 frozenLossyScale;
        public Quaternion anchorRelativeRot;
    }

    protected struct SavedTransform
    {
        public Transform obj;
        public Vector3 savedPosition;
        public Quaternion savedRotation;
        public Vector3 savedLocalScale;
        public Vector3 savedLossyScale;
    }

    /// <summary>
    /// Object that has been bound through a bone binding.
    /// </summary>
    protected struct BoundObject
    {
        public ulong objectId;
        public string rigname;
    }
    #endregion

    public abstract void UpdateFrame(UserTrackingFrameDto frame);

    #region bindings
    #region Add/Remove
    /// <summary>
    /// Set the binding activation from the given bool.
    /// </summary>
    /// <param name="b">the activation value</param>
    public void SetActiveBindings(bool b)
    {
        activeUserBindings = b;

        if (userBindings != null)
        {
            if (activeUserBindings)
            {
                foreach (BoneBindingDto dto in userBindings)
                {
                    if (dto.active)
                        UpdateBindingPosition(dto);
                }
            }
            else
            {
                foreach (BoneBindingDto dto in userBindings)
                {
                    if (savedTransforms.ContainsKey(new BoundObject() { objectId = dto.objectId, rigname = dto.rigName }))
                        ResetObjectBindings(dto);
                }
            }
        }
    }

    /// <summary>
    /// Set a new list of BoneBindingDto.
    /// </summary>
    /// <param name="newBindings">a list of dto containing binding data</param>
    public void SetBindings(List<BoneBindingDto> newBindings)
    {
        foreach (BoneBindingDto dto in userBindings)
        {
            if (savedTransforms.ContainsKey(new BoundObject() { objectId = dto.objectId, rigname = dto.rigName }))
                ResetObjectBindings(dto);
        }

        userBindings = newBindings;

        if (activeUserBindings && userBindings != null)
        {
            foreach (BoneBindingDto dto in userBindings)
            {
                if (dto.active)
                    UpdateBindingPosition(dto);
            }
        }
    }

    /// <summary>
    /// Add a new BoneBindingDto at the given index.
    /// </summary>
    /// <param name="index">the index in the list of bindings</param>
    /// <param name="dto"></param>
    public void AddBinding(int index, BoneBindingDto boneBinding)
    {
        if (boneBinding == null) return;

        if (userBindings == null) userBindings = new List<BoneBindingDto>();

        if (index <= userBindings.Count - 1)
        {
            BoneBindingDto dtoAtIndex = userBindings[index];

            if (!boneBinding.bindingId.Contains(dtoAtIndex.bindingId) && !dtoAtIndex.bindingId.Contains(boneBinding.bindingId))
            {
                AddBinding_(index, boneBinding);
            }
        }
        else
        {
            AddBinding_(index, boneBinding);
        }
    }

    private void AddBinding_(int index, BoneBindingDto boneBinding)
    {
        userBindings.Insert(index, boneBinding);
        if (activeUserBindings && boneBinding.active)
        {
            UpdateBindingPosition(boneBinding);
        }
    }

    /// <summary>
    /// Remove the BoneBindingDto at the given index.
    /// </summary>
    /// <param name="index">the index in the list of bindings</param>
    /// <param name="dto"></param>
    public void RemoveBinding(int index)
    {
        if (userBindings == null) return;
        BoneBindingDto dto = userBindings[index];
        userBindings.RemoveAt(index);
        ResetObjectBindings(dto);
    }

    #endregion

    #region Update/Await
    /// <summary>
    /// Update the BoneBindingDto at the given index.
    /// </summary>
    /// <param name="index">the index in the list of bindings</param>
    /// <param name="dto"></param>
    public void UpdateBinding(int index, BoneBindingDto dto)
    {
        userBindings[index] = dto;
        if (activeUserBindings)
        {
            if (dto.active)
                UpdateBindingPosition(dto);
            else if (savedTransforms.ContainsKey(new BoundObject() { objectId = dto.objectId, rigname = dto.rigName }))
                ResetObjectBindings(dto);
        }
    }

    protected void UpdateBindingPosition(BoneBindingDto dto)
    {
        if (userId == UMI3DClientServer.Instance.GetUserId())
        {
            if (UMI3DClientUserTrackingBone.instances.TryGetValue(dto.boneType, out UMI3DClientUserTrackingBone bone))
            {
                WaitForRig(dto, bone);
            }
            else
            {
                UMI3DLogger.LogWarning(dto.boneType + "not found in bones instances", scope);
            }
        }
        else
        {
            WaitForOtherRig(dto);
        }
    }

    protected Transform InspectBoundRigs(BoneBindingDto dto)
    {
        Transform obj = null;
        foreach (Transform rig in boundRigs)
        {
            if ((obj = rig.GetComponentsInChildren<Transform>().FirstOrDefault(t => t.name == dto.rigName)) != null)
                return obj;
        }
        return obj;
    }

    /// <summary>
    /// Replace an object associated with a binding to its saved state.
    /// </summary>
    /// <param name="dto"></param>
    protected void ResetObjectBindings(BoneBindingDto dto)
    {
        UMI3DNodeInstance node = UMI3DEnvironmentLoader.GetNode(dto.objectId);


        if (node != null)
        {
            if (savedTransforms.TryGetValue(new BoundObject() { objectId = dto.objectId, rigname = dto.rigName }, out SavedTransform savedTransform))
            {

                ResetSavedTansform(dto, savedTransform, node);

                if (dto.rigName == "" && node != null)
                    node.updatePose = true;
                else
                    boundRigs.Remove(savedTransform.obj);

                savedTransforms.Remove(new BoundObject() { objectId = dto.objectId, rigname = dto.rigName });
            }
            else
            {
                GameObject.Destroy(node.gameObject);
            }
        }

        if (!dto.active)
        {
            if (savedTransforms == null) return;

            savedTransforms.Remove(new BoundObject() { objectId = dto.objectId, rigname = dto.rigName });
        }
    }

    protected void ResetSavedTansform(BoneBindingDto dto, SavedTransform savedTransform, UMI3DNodeInstance node)
    {
        if (savedTransform.obj != null)
        {
            int index = bounds.FindIndex(b => b.obj == savedTransform.obj);

            Bound bd = bounds[index];
            bounds.Remove(bd);

            if (dto.rigName == "")
            {
                if (node.dto is GlTFNodeDto)
                {
                    savedTransform.obj.localPosition = (node.dto as GlTFNodeDto).position;
                    savedTransform.obj.localRotation = (node.dto as GlTFNodeDto).rotation;
                    savedTransform.obj.localScale = (node.dto as GlTFNodeDto).scale;
                }
                else if (node.dto is GlTFSceneDto)
                {
                    savedTransform.obj.localPosition = (node.dto as GlTFSceneDto).extensions.umi3d.position;
                    savedTransform.obj.localRotation = (node.dto as GlTFSceneDto).extensions.umi3d.rotation;
                    savedTransform.obj.localScale = (node.dto as GlTFSceneDto).extensions.umi3d.scale;
                }
            }
            else
            {
                savedTransform.obj.localPosition = savedTransform.savedPosition;
                savedTransform.obj.localRotation = savedTransform.savedRotation;
                savedTransform.obj.localScale = savedTransform.savedLocalScale;
            }
        }
    }

    /// <summary>
    /// Called to reset the extrapolators. 
    /// </summary>
    /// E.g. when entering in a vehicle.
    public void ResetExtrapolators()
    {
        nodePositionExtrapolator = null;
        nodeRotationExtrapolator = null;
    }
    #endregion
    #region Waiting
    protected void WaitForRig(BoneBindingDto dto, UMI3DClientUserTrackingBone bone)
    {
        UMI3DEnvironmentLoader.WaitForAnEntityToBeLoaded(dto.objectId, (entityInstance) =>
        {
            if (entityInstance is UMI3DNodeInstance node)
            {
                WaitForRig(node, dto, bone);
            }
        }
        );
    }

    protected void WaitForOtherRig(BoneBindingDto dto)
    {
        UMI3DEnvironmentLoader.WaitForAnEntityToBeLoaded(dto.objectId, async (entityInstance) =>
        {
            if (entityInstance is UMI3DNodeInstance node)
            {
                await WaitForFrame();
                WaitingForOtherEntityRig(node, dto);
            }
        });
    }
    protected void WaitingForOtherEntityRig(UMI3DNodeInstance node, BoneBindingDto dto)
    {
        Transform obj = null;
        if (dto.rigName != "")
        {
            obj = node.transform.GetComponentsInChildren<Transform>().FirstOrDefault(t => t.name == dto.rigName);
        }
        else
        {
            obj = node.transform;
        }

        AddANewBound(dto, obj, node);

        if (!savedTransforms.ContainsKey(new BoundObject() { objectId = dto.objectId, rigname = dto.rigName }))
        {
            SaveTransform(dto, obj);
        }
    }

    protected async Task WaitForRig(UMI3DNodeInstance node, BoneBindingDto dto, UMI3DClientUserTrackingBone bone)
    {
        Transform obj = null;
        if (dto.rigName != "")
        {
            while ((obj = node.transform.GetComponentsInChildren<Transform>().FirstOrDefault(t => t.name == dto.rigName)) == null && (obj = InspectBoundRigs(dto)) == null)
            {
                await WaitForFrame();
            }

            if (!boundRigs.Contains(obj))
                boundRigs.Add(obj);
        }
        else
        {
            obj = node.transform;
        }

        if (savedTransforms.TryGetValue(new BoundObject() { objectId = dto.objectId, rigname = dto.rigName }, out SavedTransform savedTransform))
        {
            int index = bounds.FindIndex(b => b.obj == savedTransform.obj && b.bonetype == dto.boneType);

            if (index >= 0)
            {
                Bound bound = bounds[index];
                bound.offsetPosition = dto.offsetPosition;
                bound.offsetRotation = dto.offsetRotation;
                bound.offsetScale = dto.offsetScale;
                bound.syncPos = dto.syncPosition;
                bound.syncRot = dto.syncRotation;
                bound.freezeWorldScale = dto.freezeWorldScale;
                bounds[index] = bound;
            }
            else
            {
                AddANewBound(dto, obj, node);
            }
        }
        else
        {
            SaveTransform(dto, obj);
            AddANewBound(dto, obj, node);

            if (dto.rigName == "")
                node.updatePose = false;
        }
    }

    void AddANewBound(BoneBindingDto dto, Transform obj, UMI3DNodeInstance node)
    {
        bounds.Add(new Bound()
        {
            bonetype = dto.boneType,
            obj = obj,
            offsetPosition = dto.offsetPosition,
            offsetRotation = dto.offsetRotation,
            offsetScale = dto.offsetScale,
            syncPos = dto.syncPosition,
            syncRot = dto.syncRotation,
            freezeWorldScale = dto.freezeWorldScale,
            frozenLossyScale = obj.lossyScale,
            anchorRelativeRot = dto.rigName == "" ? Quaternion.identity : Quaternion.Inverse(node.transform.rotation) * obj.rotation
        });
    }

    void SaveTransform(BoneBindingDto dto, Transform obj)
    {
        var savedTransform = new SavedTransform
        {
            obj = obj,
            savedPosition = obj.localPosition,
            savedRotation = obj.localRotation,
            savedLocalScale = obj.localScale,
            savedLossyScale = obj.lossyScale,
        };

        savedTransforms.Add(new BoundObject() { objectId = dto.objectId, rigname = dto.rigName }, savedTransform);
    }

    #region Tasks
    private async Task WaitForFrame()
    {
        var actual = Time.frameCount;

        while (actual == Time.frameCount)
        {
            await Task.Yield();
        }
    }
    #endregion

    #endregion
    #endregion


}
