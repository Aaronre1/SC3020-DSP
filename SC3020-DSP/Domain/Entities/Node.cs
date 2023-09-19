using SC3020_DSP.Domain.Common;

namespace SC3020_DSP.Domain.Entities;

public class Node
{
    public static long ByteSize => 12;
    public Pointer Pointer { get; set; } // 8
    public decimal? Key { get; set; } // 8

    public Node(Pointer pointer, decimal? key)
    {
        Pointer = pointer;
        Key = key;
    }
}