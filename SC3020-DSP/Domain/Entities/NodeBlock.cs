using SC3020_DSP.Domain.Common;

namespace SC3020_DSP.Domain.Entities;

public class NodeBlock : BaseBlock<Node>
{
    public override int Capacity { get; }
    
    public Pointer Pointer { get; set; }

    public NodeBlock(int id, int capacity)
    {
        Id = id;
        Capacity = capacity;
        Items = new List<Node>(capacity);
    }

    public bool Add(Node node)
    {
        if (Items.Count == Capacity)
        {
            return false;
        }
        
        Items.Add(node);
        return true;
    }
}