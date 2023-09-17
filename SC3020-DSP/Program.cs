// See https://aka.ms/new-console-template for more information


using SC3020_DSP.Domain;
using SC3020_DSP.Domain.Configurations;
using SC3020_DSP.Domain.Entities;

var options = new DatabaseOptions();
var db = new Database(options)
{
    
};


for (var i = 0; i < 1000; i++)
{
    var record = new Record()
    {
        GameDate = DateTime.Now,
        TeamId = i * 123,
        PtsHome = i
    };
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
// Datetime // 8
Console.WriteLine(sizeof(int)); //4
Console.WriteLine(sizeof(decimal)); //16
Console.WriteLine(sizeof(bool)); //1