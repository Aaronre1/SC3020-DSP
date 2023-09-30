// See https://aka.ms/new-console-template for more information


using SC3020_DSP.Application;
using SC3020_DSP.Domain;
using SC3020_DSP.Domain.Configurations;
using SC3020_DSP.Domain.Entities;
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

// Store records into blocks  (Done first) 
// Build an index 
// Index will then point to the respective blocks 
// Assuming all the data are non sequential 
// Create my B+ Tree 

// Initilise BPTree Object 
foreach (var record in records)
{
    db.Add(record);
}

foreach (var block in db.GetDataBlocks())
{
    // Console.WriteLine($"Reading block # {block.Id}");
    // insert b+ tree
    foreach (var record in block.Items)
    {
        // Console.WriteLine(record);
    }
}

Console.WriteLine("Experiment 1.");
Console.WriteLine("Number of records"+ records.Count);
Console.WriteLine("Size of a record"+ options.RecordSizeInBytes);
Console.WriteLine("Number of records stored in a block"+ db.GetDataBlocks().Count());
Console.WriteLine("Number of blocks for storing the data"+ db.GetDataBlockCapacity());
Console.WriteLine("################################################################");
// Console.WriteLine(db.BytesUsed());

BPlusTreeImpl bptree = new BPlusTreeImpl(db);

foreach (var block in db.GetDataBlocks().ToList())
{
    var blockId = block.Id;
    var offset = 0;
    foreach (var record in block.Items)
    {
        var pointer = new Pointer(blockId, offset);
        
        bptree.Add(record,pointer);
        offset++;
    }
}

foreach (var block in db.GetDataBlocks())
{
    foreach (var record in block.Items)
    {
        if (record.Key == null)
        {
            continue;
        }
        var found = bptree.Find(record.Key);
        if (found.Key != record.Key)
        {
            Console.WriteLine("error");
        }
        Console.WriteLine($"found key {found.Key}");
    }
}

