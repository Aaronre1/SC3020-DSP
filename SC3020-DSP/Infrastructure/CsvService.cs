using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using SC3020_DSP.Domain.Entities;

namespace SC3020_DSP.Domain.Infrastructure;

public class CsvService
{

    public List<Record> Read(string filePath)
    {
        using (var reader = new StreamReader(filePath))
        using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
        {
            csv.Context.RegisterClassMap<RecordMap>();
            var records = csv.GetRecords<Record>();
            return records.ToList();
        }
    }
}