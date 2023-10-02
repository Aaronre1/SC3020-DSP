// See https://aka.ms/new-console-template for more information


using SC3020_DSP.Application;
using SC3020_DSP.Domain;
using SC3020_DSP.Domain.Configurations;
using SC3020_DSP.Domain.Entities;
using SC3020_DSP.Domain.Infrastructure;

// Requirements on on project
// RecordSizeInBytes are determined by inspecting the data manually 
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
// records are packed in unspanned method  

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

Console.WriteLine("*********************Experiment 1*****************************");
Console.WriteLine("Number of records: "+ records.Count);
Console.WriteLine("Size of a record: "+ options.RecordSizeInBytes); 
Console.WriteLine("Number of records stored in a block: "+ db.GetDataBlockCapacity());
Console.WriteLine("Number of blocks for storing the data: "+ db.GetDataBlocks().Count());
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
        //Console.WriteLine($"Insert:{record.Key}");
        bptree.Add(record,pointer);
        offset++;
        //bptree.Print();
        //Console.WriteLine("===");
    }
}

bptree.Print();

foreach (var block in db.GetDataBlocks())
{
    foreach (var record in block.Items)
    {
        if (record.Key == null)
        {
            continue;
        }
        var result = bptree.Find(record.Key);
        
      //  Console.WriteLine($"Found {result.Count} records with key {record.Key}");
    }
}

Console.WriteLine($"Actual # of records with key 0.431 : {records.Where(i=>i.Key==0.431M).Count()}");


