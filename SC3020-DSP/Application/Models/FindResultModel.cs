using SC3020_DSP.Domain.Entities;

namespace SC3020_DSP.Application.Models;

public class FindResultModel
{
    public int IndexNodeAccessed { get; set; }
    public int DataBlockAccessed { get; set; }
    public int BucketBlockAccessed { get; set; }
    public List<Record> Records { get; set; } = new List<Record>();
    public long Ticks { get; set; }
}