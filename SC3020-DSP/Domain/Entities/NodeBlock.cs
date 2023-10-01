using SC3020_DSP.Domain.Common;
using SC3020_DSP.Domain.Domain.Enums;

namespace SC3020_DSP.Domain.Entities;

public class NodeBlock : BaseBlock
{
    public override int Capacity { get; }

    public NodeType NodeType { get; set; }

    public decimal?[] Keys { get; }
    
    public override int Count => Keys.Count(x => x != null);

    public Pointer[] Pointers { get; }
    
    public NodeBlock(int id, int capacity)
    {
        Id = id;
        Capacity = capacity;
        Keys = new decimal?[capacity];
        Pointers = new Pointer[capacity+1];
    }
}