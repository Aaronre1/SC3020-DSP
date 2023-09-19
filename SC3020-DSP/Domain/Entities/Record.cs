using SC3020_DSP.Domain.Common;

namespace SC3020_DSP.Domain.Entities;

public class Record
{
    public decimal? Key => FgPctHome;
    public DateTime GameDate { get; set; } // 8
    public int TeamId { get; set; } // 4
    public int? PtsHome { get; set; } // 4

    public decimal? FgPctHome { get; set; } // 16

    public decimal? FtPctHome { get; set; } // 16

    public decimal? Fg3PctHome { get; set; } // 16

    public int? AstHome { get; set; } // 4

    public int? RebHome { get; set; } // 4

    public bool HomeTeamWins { get; set; } // 1

    public override string ToString()
    {
        return $"{GameDate} {TeamId} {PtsHome} ...";
    }
}