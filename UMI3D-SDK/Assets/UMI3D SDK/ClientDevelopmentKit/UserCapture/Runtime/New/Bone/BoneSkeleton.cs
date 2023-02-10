using System.Collections;
using System.Collections.Generic;
using umi3d.cdk.userCapture;
using umi3d.common.userCapture;
using UnityEngine;

public class BoneSkeleton : MonoBehaviour
{
    public void Init(uint boneTypes, Transform node)
    {
        this.boneTypes = boneTypes;
        this.node = node;
    }

    private void Awake()
    {
        bonSkeletonInstances.TryAdd(boneTypes, this);
    }

    public static Dictionary<uint, BoneSkeleton> bonSkeletonInstances = new Dictionary<uint, BoneSkeleton>();

    public uint boneTypes;
    public Transform node;
}
