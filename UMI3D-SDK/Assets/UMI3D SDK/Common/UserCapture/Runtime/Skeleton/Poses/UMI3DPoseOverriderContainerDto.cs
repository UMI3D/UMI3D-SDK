namespace umi3d.common.userCapture.pose
{
    public class UMI3DPoseOverriderContainerDto : UMI3DDto, IEntity
    {
        /// <summary>
        /// The id of the entity
        /// </summary>
        public ulong id { get; set; }

        public ulong relatedNodeId { get; set; }

        /// <summary>
        /// All the pose ovveriders of the linked container
        /// </summary>
        public PoseOverriderDto[] poseOverriderDtos { get; set; }
    }
}