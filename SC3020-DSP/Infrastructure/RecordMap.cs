using CsvHelper.Configuration;
using SC3020_DSP.Domain.Entities;

namespace SC3020_DSP.Domain.Infrastructure;

public class RecordMap : ClassMap<Record>
{
    public RecordMap()
    {
        Map(m => m.GameDate).Name("GAME_DATE_EST")
            .TypeConverterOption.Format("d/M/yyyy");
        
        Map(m => m.TeamId).Name("TEAM_ID_home");
        
        Map(m => m.PtsHome).Name("PTS_home");
        
        Map(m => m.FgPctHome).Name("FG_PCT_home");
        
        Map(m => m.FtPctHome).Name("FT_PCT_home");
        
        Map(m => m.Fg3PctHome).Name("FG3_PCT_home");
        
        Map(m => m.AstHome).Name("AST_home");
        
        Map(m => m.RebHome).Name("REB_home");
        
        Map(m => m.HomeTeamWins).Name("HOME_TEAM_WINS");
    }
}