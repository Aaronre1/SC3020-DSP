// See https://aka.ms/new-console-template for more information


using SC3020_DSP.Domain;
using SC3020_DSP.Domain.Configurations;
using SC3020_DSP.Domain.Infrastructure;

var options = new DatabaseOptions()
{
    RecordSizeInBytes = 73,
    BlockSizeInBytes = 400,
    DiskCapacityInBytes = 500 * 1024 * 1024
};
var db = new Database(options);
var csv = new CsvService();

var records = csv.Read("Data.csv");

foreach (var record in records)
{
    db.Add(record);
}

foreach (var block in db.GetDataBlocks())
{
    Console.WriteLine($"Reading block # {block.Id}");

    foreach (var record in block.Items)
    {
        Console.WriteLine(record);
    }
}

Console.WriteLine(db.BytesUsed());