using SC3020_DSP.Domain.Common;

namespace SC3020_DSP.Domain.Entities;

public class DataBlock : BaseBlock<Record>
{
    public override int Capacity => 10;

    public DataBlock(int id)
    {
        Id = id;
    }
    public void Add(Record record)
    {
        Items.Add(record);
    }
}