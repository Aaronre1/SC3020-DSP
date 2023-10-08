namespace SC3020_DSP.Application.Models;

public class RemoveResultModel
{
    public int IndexNodeAccessed { get; set; }
    public int DataBlockAccessed { get; set; }
    public long Ticks { get; set; }
    public int RecordsRemoved { get; set; }
}