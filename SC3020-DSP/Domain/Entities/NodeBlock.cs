using SC3020_DSP.Domain.Common;

namespace SC3020_DSP.Domain.Entities;

public class NodeBlock : BaseBlock<Node>
{
    public override int Capacity { get; }
}