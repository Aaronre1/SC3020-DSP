using SC3020_DSP.Domain.Common;

namespace SC3020_DSP.Domain.Entities;

public class Node
{
    public static long ByteSize => 12;
    public Pointer Pointer { get; set; } // 8
    public int Key { get; set; } // 4

    public Node(Pointer pointer, int key)
    {
        Pointer = pointer;
        Key = key;
    }
}