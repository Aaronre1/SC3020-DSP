using SC3020_DSP.Domain.Common;
using SC3020_DSP.Domain.Domain.Enums;

namespace SC3020_DSP.Domain.Entities;

public class NodeBlock : BaseBlock<NodeBlock>
{
    public override int Capacity { get; }

    public Pointer ParentPointer { get; set; }

    public NodeType NodeType { get; set; }

    public bool IsFull => Capacity == Keys.Count();
// 3 keys -> 3 ptr + 1 ptr
    public decimal?[] Keys { get; }
    
    public override int Count => Keys.Count(x => x != null);

    public Pointer[] Pointers { get; }
    public NodeBlock(int id, int capacity)
    {
        Id = id;
        Capacity = capacity;
        Items = new List<NodeBlock>(capacity);
        Keys = new decimal?[capacity];
        Pointers = new Pointer[capacity+1];
    }

    public NodeBlock()
    {
        
    }

    
    
    
}