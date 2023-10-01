using SC3020_DSP.Domain.Common;

namespace SC3020_DSP.Domain.Entities;

public class DataBlock : BaseBlock
{
    public override int Capacity { get; }
    public List<Record> Items { get; set; }
    public override int Count => Items.Count;
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