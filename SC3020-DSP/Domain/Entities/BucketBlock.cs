using SC3020_DSP.Domain.Common;

namespace SC3020_DSP.Domain.Entities;

public class BucketBlock : BaseBlock
{
    public List<Pointer> Pointers { get; set; }
    public override int Capacity { get; }
    public override int Count => Pointers.Count;
    public Pointer? OverflowBucket { get; set; }

    public BucketBlock(int id, int capacity)
    {
        Id = id;
        Capacity = capacity;
        Pointers = new List<Pointer>();
        OverflowBucket = null;
    }
}