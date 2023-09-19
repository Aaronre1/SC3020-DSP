using SC3020_DSP.Domain.Domain.Enums;

namespace SC3020_DSP.Domain.Entities;

public class Node1
{
    public NodeType NodeType { get; set; } 
    
    public int Capacity { get; set; }

    public List<Pointer> Pointers { get; } = new List<Pointer>();
    
    public List<decimal> Keys { get; } = new List<decimal>();

    public int Count => Pointers.Count;
}


public abstract class Nodee
{
    public abstract NodeType NodeType { get; }
    
    public int Capacity { get; set; }
    
    public abstract int Count { get; }
    
    public Pointer Pointer { get; set; }
}

/// <summary>
/// Internal Node is a sparse index
/// </summary>
public class InternalNode : Nodee
{
    public override NodeType NodeType => NodeType.InternalNode;

    //3 keys
    public List<decimal> Keys { get; set; }
    // Cound be internal or leaf nodes
    //4 Pointers
    public List<Nodee> Items { get; set; }

    public override int Count => Items.Count;
}
/// <summary>
/// Leaf Node is a dense index
/// </summary>
public class LeafNode : Nodee
{
    public override NodeType NodeType => NodeType.LeafNode;
    public override int Count => Items.Count;
    public List<NodeItem> Items { get; set; }
    
    public LeafNode? Sibling { get; set; }
    
    public void Add(Record record)
    {
        var item = new NodeItem()
        {
            Key = record.Key,
            Record = record
        };
        Items.Add(item);
        Items = Items.OrderBy(i => i.Key).ToList();
    }
}

public class NodeItem
{
    public decimal? Key { get; set; }

    public Record Record { get; set; }

    // Support duplicate keys
    public IList<Record> Records { get; set; }
}