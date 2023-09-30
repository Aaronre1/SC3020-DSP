using SC3020_DSP.Domain.Entities;

namespace SC3020_DSP.Application;

public interface IBPlusTree
{
    public void Add(Record record, Pointer ptr);
}