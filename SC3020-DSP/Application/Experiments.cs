using System.Diagnostics;
using SC3020_DSP.Domain;
using SC3020_DSP.Domain.Configurations;
using SC3020_DSP.Domain.Entities;
using SC3020_DSP.Domain.Infrastructure;

namespace SC3020_DSP.Application;

public class Experiments
{
    private BPlusTree _bPlusTree;
    private Database _database;
    private List<Record> _records;
    private readonly DatabaseOptions _options;

    public Experiments()
    {
        _options = new DatabaseOptions()
        {
            RecordSizeInBytes = 74,
            BlockSizeInBytes = 400,
            DiskCapacityInBytes = 500 * 1024 * 1024
        };
    }

    // Experiment 1
    public void Initialize()
    {
        _database = new Database(_options);
        var csv = new CsvService();
        Console.WriteLine("Seeding data..."); 
        _records = csv.Read("Data.csv");
        foreach (var record in _records)
        {
            _database.Add(record);
        }

        Console.WriteLine("=========================== Experiment 1 ==========================");
        Console.WriteLine($"Number of records: {_records.Count}");
        Console.WriteLine($"Size of record: {_options.RecordSizeInBytes}");
        Console.WriteLine($"Max number of records per block: {_database.GetDataBlockCapacity()}");
        Console.WriteLine($"Number of blocks for storing the data: {_database.GetDataBlocks().Count()}");
        Console.WriteLine("===================================================================\n");
    }

    // Experiment 2
    public void BuildBPlusTree()
    {
        _bPlusTree = new BPlusTree(_database);
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
        Console.WriteLine($"Parameter N: {_database.NodeBlockCapacity()}");
        Console.WriteLine($"Number of nodes: {_database.GetNodeBlocks().Count()}");
        Console.WriteLine($"Number of levels: {_bPlusTree.Levels}");
        Console.WriteLine($"Root node: {string.Join(", ", _bPlusTree.Root.Keys.Where(i => i != null))}");
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
        Console.WriteLine("-------------------------------------------------------------------");
        Console.WriteLine("+++ Brute Force +++");
        Console.WriteLine($"Number of data blocks accessed: {linearResult.DataBlockAccessed}");
        Console.WriteLine($"Average 'FG3_PCT_home': {linearResult.Records.Average(x => x.Fg3PctHome)}");
        Console.WriteLine($"Ticks elapsed: {linearResult.Ticks}");
        Console.WriteLine("===================================================================\n");
    }

    // Experiment 4
    public void FindRecords(decimal from, decimal to)
    {
        var result = _bPlusTree.Find(from, to);
        var linearResult = _database.FindRecords(from, to);
        Console.WriteLine("=========================== Experiment 4 ==========================");
        Console.WriteLine("+++ B Plus Tree +++");
        Console.WriteLine($"Number of index nodes accessed: {result.IndexNodeAccessed}");
        Console.WriteLine($"Number of data blocks accessed: {result.DataBlockAccessed}");
        Console.WriteLine($"Number of records found: {result.Records.Count}");
        Console.WriteLine($"Average 'FG3_PCT_home': {result.Records.Average(x => x.Fg3PctHome)}");
        Console.WriteLine($"Ticks elapsed: {result.Ticks}");
        Console.WriteLine("-------------------------------------------------------------------");
        Console.WriteLine("+++ Brute Force +++");
        Console.WriteLine($"Number of data blocks accessed: {linearResult.DataBlockAccessed}");
        Console.WriteLine($"Number of records found: {linearResult.Records.Count}");
        Console.WriteLine($"Average 'FG3_PCT_home': {linearResult.Records.Average(x => x.Fg3PctHome)}");
        Console.WriteLine($"Ticks elapsed: {linearResult.Ticks}");
        Console.WriteLine("===================================================================\n");
    }

    // Experiment 5
    public void Experiment5(decimal to)
    {
        // BP tree remove
        var result = _bPlusTree.RemoveRecordsTill(to);

        // Rebuild tree
        _bPlusTree = new BPlusTree(_database);
        var sw = new Stopwatch();
        sw.Start();
        // Delete existing nodes
        foreach (var node in _database.GetNodeBlocks())
        {
            node.Deleted = true;
        }
        
        foreach (var block in _database.GetDataBlocks().ToList())
        {
            var blockId = block.Id;
            var offset = 0;
            foreach (var record in block.Items)
            {
                if (!record.Deleted)
                {
                    var pointer = new Pointer(blockId, offset);
                    _bPlusTree.Add(record, pointer);
                }
                offset++;
            }
        }
        sw.Stop();
        var newNodesCount = _database.GetNodeBlocks().Count(x => !x.Deleted);
        var total = result.Ticks + sw.ElapsedTicks;
        
        // Reseed data
        _database = new Database(_options);
        foreach (var record in _records)
        {
            _database.Add(record);
        }
        
        // Brute Force
        var linearResult = _database.RemoveRecordsTill(to);

        Console.WriteLine("=========================== Experiment 5 ==========================");
        Console.WriteLine($"Number of nodes: {newNodesCount}");
        Console.WriteLine($"Number of levels: {_bPlusTree.Levels}");
        Console.WriteLine($"Root node: {string.Join(", ", _bPlusTree.Root.Keys.Where(i => i != null))}");
        Console.WriteLine($"Ticks elapsed (Delete): {result.Ticks}");
        Console.WriteLine($"Ticks elapsed (Build): {sw.ElapsedTicks}");
        Console.WriteLine($"Ticks elapsed (Total): {total}");
        Console.WriteLine("-------------------------------------------------------------------");
        Console.WriteLine("+++ Brute Force +++");
        Console.WriteLine($"Number of data blocks accessed: {linearResult.DataBlockAccessed}");
        Console.WriteLine($"Ticks elapsed: {linearResult.Ticks}");
        Console.WriteLine("===================================================================\n");
        _bPlusTree = new BPlusTree(_database);
    }
}