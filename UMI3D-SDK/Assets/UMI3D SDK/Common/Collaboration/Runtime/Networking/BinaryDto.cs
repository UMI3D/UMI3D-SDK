using umi3d.common;

public class BinaryDto : UMI3DDto
{
    public byte[] data { get; set; }
    public int groupId { get; set; }

    public ulong timestep { get; set; }
}
