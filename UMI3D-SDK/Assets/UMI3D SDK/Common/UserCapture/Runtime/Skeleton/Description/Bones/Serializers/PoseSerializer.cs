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

using System;

namespace umi3d.common.userCapture.description
{
    /// <summary>
    /// Serializer for <see cref="PoseDto"/> and <see cref="PoseAnchorDto"/>.
    /// </summary>
    public class PoseSerializerModule : UMI3DSerializerModule
    {
        /// <summary>
        /// Used to serialize inherited types.
        /// </summary>
        public enum PoseAnchorSerializingIndex : int
        {
            DEFAULT,
            BONE_ANCHORED,
            NODE_ANCHORED,
            FLOOR_ANCHORED
        }

        /// <inheritdoc/>
        public bool? IsCountable<T>()
        {
            return true switch
            {
                true when typeof(T) == typeof(PoseDto) => true,
                true when typeof(T) == typeof(PoseAnchorDto) => true,
                true when typeof(T) == typeof(BonePoseAnchorDto) => true,
                true when typeof(T) == typeof(NodePoseAnchorDto) => true,
                true when typeof(T) == typeof(FloorPoseAnchorDto) => true,

                _ => null
            };
        }

        /// <inheritdoc/>
        public bool Read<T>(ByteContainer container, out bool readable, out T result)
        {
            switch (true)
            {
                case true when typeof(T) == typeof(PoseDto):
                    {
                        var bones = UMI3DSerializer.ReadList<BoneDto>(container);
                        readable = UMI3DSerializer.TryRead(container, out PoseAnchorDto poseAnchor);


                        if (readable)
                        {
                            PoseDto poseDto = new()
                            {
                                anchor = poseAnchor,
                                bones = bones,
                            };

                            result = (T)Convert.ChangeType(poseDto, typeof(PoseDto));
                            return true;
                        }
                        break;
                    }

                case true when typeof(T) == typeof(PoseAnchorDto):
                    {
                        readable = UMI3DSerializer.TryRead(container, out int index);

                        if (readable)
                        {
                            PoseAnchorDto bonePoseDto = null;

                            switch (index)
                            {
                                case (int)PoseAnchorSerializingIndex.DEFAULT:
                                    ReadPoseAnchorDto(container, out readable, out bonePoseDto);
                                    break;

                                case (int)PoseAnchorSerializingIndex.BONE_ANCHORED:
                                    ReadPoseAnchorDto(container, out readable, out BonePoseAnchorDto anchorBonePoseDto);
                                    bonePoseDto = anchorBonePoseDto;
                                    break;

                                case (int)PoseAnchorSerializingIndex.NODE_ANCHORED:
                                    ReadPoseAnchorDto(container, out readable, out NodePoseAnchorDto nodePositionAnchoredBonePoseDto);
                                    bonePoseDto = nodePositionAnchoredBonePoseDto;
                                    break;

                                case (int)PoseAnchorSerializingIndex.FLOOR_ANCHORED:
                                    ReadPoseAnchorDto(container, out readable, out FloorPoseAnchorDto floorAnchoredBonePoseDto);
                                    bonePoseDto = floorAnchoredBonePoseDto;
                                    break;
                            }

                            if (bonePoseDto != null)
                            {
                                result = (T)(object)bonePoseDto;
                                return true;
                            }
                        }
                        break;
                    }



            }
            result = default(T);
            readable = false;
            return false;
        }

        /// <inheritdoc/>
        public bool ReadPoseAnchorDto<T>(ByteContainer container, out bool readable, out T result)
        {
            switch (true)
            {
                case true when typeof(T) == typeof(FloorPoseAnchorDto):
                    {
                        ReadPoseAnchorDto(container, out readable, out PoseAnchorDto bonePoseDto);

                        if (readable)
                        {
                            FloorPoseAnchorDto nodePositionAnchoredBonePoseDto = new FloorPoseAnchorDto()
                            {
                                bone = bonePoseDto.bone,
                                position = bonePoseDto.position,
                                rotation = bonePoseDto.rotation,
                            };

                            result = (T)Convert.ChangeType(nodePositionAnchoredBonePoseDto, typeof(FloorPoseAnchorDto));
                            return true;
                        }
                        break;
                    }

                case true when typeof(T) == typeof(NodePoseAnchorDto):
                    {
                        ReadPoseAnchorDto(container, out readable, out PoseAnchorDto bonePoseDto);

                        readable &= UMI3DSerializer.TryRead(container, out ulong node);

                        if (readable)
                        {
                            NodePoseAnchorDto nodeAnchoredBonePoseDto = new NodePoseAnchorDto()
                            {
                                bone = bonePoseDto.bone,
                                position = bonePoseDto.position,
                                rotation = bonePoseDto.rotation,
                                node = node
                            };

                            result = (T)Convert.ChangeType(nodeAnchoredBonePoseDto, typeof(NodePoseAnchorDto));
                            return true;
                        }
                        break;
                    }

                case true when typeof(T) == typeof(BonePoseAnchorDto):
                    {
                        ReadPoseAnchorDto(container, out readable, out PoseAnchorDto bonePoseDto);

                        readable &= UMI3DSerializer.TryRead(container, out uint otherBone);

                        if (readable)
                        {
                            BonePoseAnchorDto anchorBonePoseDto = new BonePoseAnchorDto()
                            {
                                bone = bonePoseDto.bone,
                                position = bonePoseDto.position,
                                rotation = bonePoseDto.rotation,
                                otherBone = otherBone
                            };

                            result = (T)Convert.ChangeType(anchorBonePoseDto, typeof(BonePoseAnchorDto));
                            return true;
                        }
                        break;
                    }

                case true when typeof(T) == typeof(PoseAnchorDto):
                    {
                        readable = UMI3DSerializer.TryRead(container, out uint bone);
                        readable &= UMI3DSerializer.TryRead(container, out Vector3Dto position);
                        readable &= UMI3DSerializer.TryRead(container, out Vector4Dto rotation);

                        if (readable)
                        {
                            PoseAnchorDto bonePoseDto = new PoseAnchorDto()
                            {
                                bone = bone,
                                position = position,
                                rotation = rotation,
                            };

                            result = (T)Convert.ChangeType(bonePoseDto, typeof(PoseAnchorDto));
                            return true;
                        }
                        break;
                    }
            }
            result = default(T);
            readable = false;
            return false;
        }

        /// <inheritdoc/>
        public bool Write<T>(T value, out Bytable bytable, params object[] parameters)
        {
            switch (value)
            {
                case PoseDto poseDto:
                    bytable = UMI3DSerializer.WriteCollection(poseDto.bones)
                        + UMI3DSerializer.Write(poseDto.anchor);
                    break;

                case BonePoseAnchorDto anchorBonePoseDto:
                    bytable = UMI3DSerializer.Write((int)PoseAnchorSerializingIndex.BONE_ANCHORED)
                        + UMI3DSerializer.Write(anchorBonePoseDto.bone)
                        + UMI3DSerializer.Write(anchorBonePoseDto.position)
                        + UMI3DSerializer.Write(anchorBonePoseDto.rotation)
                        + UMI3DSerializer.Write(anchorBonePoseDto.otherBone);
                    break;

                case NodePoseAnchorDto nodePositionAnchoredBonePoseDto:
                    bytable = UMI3DSerializer.Write((int)PoseAnchorSerializingIndex.NODE_ANCHORED)
                        + UMI3DSerializer.Write(nodePositionAnchoredBonePoseDto.bone)
                        + UMI3DSerializer.Write(nodePositionAnchoredBonePoseDto.position)
                        + UMI3DSerializer.Write(nodePositionAnchoredBonePoseDto.rotation)
                        + UMI3DSerializer.Write(nodePositionAnchoredBonePoseDto.node);
                    break;

                case FloorPoseAnchorDto floorAnchoredBonePoseDto:
                    bytable = UMI3DSerializer.Write((int)PoseAnchorSerializingIndex.FLOOR_ANCHORED)
                        + UMI3DSerializer.Write(floorAnchoredBonePoseDto.bone)
                        + UMI3DSerializer.Write(floorAnchoredBonePoseDto.position)
                        + UMI3DSerializer.Write(floorAnchoredBonePoseDto.rotation);
                    break;

                case PoseAnchorDto bonePoseDto:
                    bytable = UMI3DSerializer.Write((int)PoseAnchorSerializingIndex.DEFAULT)
                        + UMI3DSerializer.Write(bonePoseDto.bone)
                        + UMI3DSerializer.Write(bonePoseDto.position)
                        + UMI3DSerializer.Write(bonePoseDto.rotation);
                    break;

                default:
                    bytable = null;
                    return false;
            }
            return true;
        }
    }
}