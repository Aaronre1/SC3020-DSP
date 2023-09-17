using SC3020_DSP.Domain.Common;

namespace SC3020_DSP.Domain.Entities;

public class Pointer : BaseRecord
{
    public override long Size => 8;
    
    public int BlockId { get; set; }
    
    public int Offset { get; set; }
}