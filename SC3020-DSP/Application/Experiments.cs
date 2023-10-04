using System.Diagnostics;
using SC3020_DSP.Domain;
using SC3020_DSP.Domain.Configurations;
using SC3020_DSP.Domain.Entities;
using SC3020_DSP.Domain.Infrastructure;

namespace SC3020_DSP.Application;

public class Experiments
{
    private BPlusTreeImpl _bPlusTree;
    private Database _database;

    public Experiments()
    {
    }

    // Experiment 1
    public void Initialize()
    {
        var options = new DatabaseOptions()
        {
            RecordSizeInBytes = 73,
            BlockSizeInBytes = 400,
            DiskCapacityInBytes = 500 * 1024 * 1024
        };
        _database = new Database(options);
        var csv = new CsvService();

        var records = csv.Read("Data.csv");
        foreach (var record in records)
        {
            _database.Add(record);
        }
        Console.WriteLine("=========================== Experiment 1 ==========================");
        Console.WriteLine($"Number of records: {records.Count}");
        Console.WriteLine($"Size of record: {options.RecordSizeInBytes}");
        Console.WriteLine($"Max number of records per block: {_database.GetDataBlockCapacity()}");
        Console.WriteLine($"Number of blocks for storing the data: {_database.GetDataBlocks().Count()}");
        Console.WriteLine("===================================================================\n");
    }

    // Experiment 2
    public void BuildBPlusTree()
    {
        _bPlusTree = new BPlusTreeImpl(_database);
        foreach (var block in _database.GetDataBlocks().ToList())
        {
            var blockId = block.Id;
            var offset = 0;
            foreach (var record in block.Items)
            {
                var pointer = new Pointer(blockId, offset);
                _bPlusTree.Add(record, pointer);
                offset++;
            }
        }

        Console.WriteLine("=========================== Experiment 2 ==========================");
        Console.WriteLine($"Parameter N: "); // TODO: Set parameter N
        Console.WriteLine($"Number of nodes: {_database.GetNodeBlocks().Count()}");
        Console.WriteLine($"Number of levels: {_bPlusTree.Levels}");
        Console.WriteLine($"Root node: {string.Join(", ", _bPlusTree.Root.Keys)}");
        Console.WriteLine("===================================================================\n");
    }

    // Experiment 3
    public void FindRecords(decimal key)
    {
        var result = _bPlusTree.Find(key);
        var linearResult = _database.FindRecords(key);
        Console.WriteLine("=========================== Experiment 3 ==========================");
        Console.WriteLine("+++ B Plus Tree +++");
        Console.WriteLine($"Number of index nodes accessed: {result.IndexNodeAccessed}");
        Console.WriteLine($"Number of data blocks accessed: {result.DataBlockAccessed}");
        Console.WriteLine($"Average 'FG3_PCT_home': {result.Records.Average(x => x.Fg3PctHome)}");
        Console.WriteLine($"Ticks elapsed: {result.Ticks}");
        Console.WriteLine("\n-------------------------------------------------------------------\n");
        Console.WriteLine($"+++ Brute Force +++");
        Console.WriteLine($"Number of data blocks accessed: {linearResult.DataBlockAccessed}");
        Console.WriteLine($"Ticks elapsed: {linearResult.Ticks}");
        Console.WriteLine("===================================================================\n");
    }
}