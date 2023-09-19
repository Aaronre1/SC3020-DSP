using SC3020_DSP.Domain.Common;
using SC3020_DSP.Domain.Domain.Enums;

namespace SC3020_DSP.Domain.Entities;

public class NodeBlock : BaseBlock<NodeBlock>
{
    public override int Capacity { get; }

    public Pointer Pointer { get; set; } = new Pointer();

    public NodeType NodeType { get; set; }

    public bool IsFull => Capacity == Items.Count;

    public List<decimal?> Keys { get; } = new List<decimal?>();

    public NodeBlock(int id, int capacity)
    {
        Id = id;
        Capacity = capacity;
        Items = new List<NodeBlock>(capacity);
    }

    public NodeBlock()
    {
        
    }

    // public bool Add(Node node)
    // {
    //     if (Items.Count == Capacity)
    //     {
    //         return false;
    //     }
    //     
    //     Items.Add(node);
    //     Items = Items.OrderBy(i => i.Key).ToList();
    //     return true;
    // }
    
    
}