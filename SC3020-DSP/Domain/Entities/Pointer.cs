using SC3020_DSP.Domain.Common;

namespace SC3020_DSP.Domain.Entities;

public class Pointer
{
    public static int ByteSize => 8;

    public Pointer(int blockId, int offset = 0)
    {
        BlockId = blockId;
        Offset = offset;
    }
    
    public int BlockId { get; set; } // 4
    
    public int Offset { get; set; } // 4
}