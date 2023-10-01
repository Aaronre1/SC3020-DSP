
using SC3020_DSP.Domain.Entities;

namespace SC3020_DSP.Domain.Common;

public abstract class BaseBlock
{
    public int Id { get; init; }

    public abstract int Capacity { get; }

    public abstract int Count { get; }

    public Pointer Address => new Pointer(Id);
}