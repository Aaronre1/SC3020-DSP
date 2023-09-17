using SC3020_DSP.Domain.Common;

namespace SC3020_DSP.Domain.Entities;

public class DataBlock : BaseBlock<Record>
{
    public override int Capacity { get; }

    public DataBlock(int id, int capacity)
    {
        Id = id;
        Capacity = capacity;
        Items = new List<Record>(capacity);
    }
    public bool Add(Record record)
    {
        if (Items.Count == Capacity)
        {
            return false;
        }
        Items.Add(record);
        return true;
    }
}