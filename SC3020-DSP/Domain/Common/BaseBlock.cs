namespace SC3020_DSP.Domain.Common;

// public abstract class BaseBlock<T> : BaseBlock
// {
//     public List<T> Items { get; set; } = new List<T>();
//
//     public override int Count => Items.Count;
// }

public abstract class BaseBlock
{
    public int Id { get; init; }

    public abstract int Capacity { get; }

    public abstract int Count { get; }
}