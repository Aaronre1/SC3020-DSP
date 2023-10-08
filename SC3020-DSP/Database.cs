using System.Diagnostics;
using SC3020_DSP.Application.Models;
using SC3020_DSP.Domain.Common;
using SC3020_DSP.Domain.Configurations;
using SC3020_DSP.Domain.Entities;

namespace SC3020_DSP.Domain;

public class Database
{
    private readonly int _dataBlockCapacity;
    private readonly int _nodeBlockCapacity;
    private readonly int _bucketBlockCapacity;
    private readonly int _capacity;
    private readonly DatabaseOptions _options;
    public List<BaseBlock> Blocks { get; }

    public Database(DatabaseOptions options)
    {
        _options = options;
        _capacity = (int)(options.DiskCapacityInBytes / options.BlockSizeInBytes);
        Blocks = new List<BaseBlock>(_capacity);
       // Console.WriteLine($"Initialized database with {_capacity} blocks capacity.");
        
        _dataBlockCapacity = (int)(options.BlockSizeInBytes / options.RecordSizeInBytes);
        // nodeBlockCapacity = [400 - (pointer size) - (delete flag)] / (pointer size + key size)
        _nodeBlockCapacity =
            (int)((options.BlockSizeInBytes - options.PointerSizeInBytes - 1) /
                  (options.PointerSizeInBytes + sizeof(decimal)));
        // bucketBlockCapacity = [400 - (pointer size)] / (pointer size)
        _bucketBlockCapacity = (int)((options.BlockSizeInBytes - options.PointerSizeInBytes) /
                                     options.PointerSizeInBytes);
    }

    public int GetDataBlockCapacity() => _dataBlockCapacity;
    public int NodeBlockCapacity() => _nodeBlockCapacity;
    public DatabaseOptions Options => _options;

    public bool Add(Record record)
    {
        foreach (var block in GetDataBlocks())
        {
            if (block.Add(record))
            {
                return true;
            }
        }

        if (Blocks.Count == _capacity)
        {
            Console.WriteLine("Database is full");
            return false;
        }

        var newBlock = new DataBlock(Blocks.Count, _dataBlockCapacity);
        newBlock.Add(record);
        Blocks.Add(newBlock);
        return true;
    }

    public void AddToBucket(Pointer recordPtr, Pointer bucketPtr)
    {
        var bucket = FindBucketBlock(bucketPtr);
        if (bucket.Count == bucket.Capacity)
        {
            if (bucket.OverflowBucket == null)
            {
                var newBucket = AssignBucketBlock();
                bucket.OverflowBucket = newBucket.Address;
                bucket = newBucket;
            }
            else
            {
                AddToBucket(recordPtr, bucket.OverflowBucket);
                return;
            }
        }

        bucket.Pointers.Add(recordPtr);
    }

    public NodeBlock AssignNodeBlock()
    {
        if (Blocks.Count == _capacity)
        {
            throw new Exception("Database is full");
        }

        var block = new NodeBlock(Blocks.Count, _nodeBlockCapacity);
        Blocks.Add(block);
        return block;
    }

    public BucketBlock AssignBucketBlock()
    {
        if (Blocks.Count == _capacity)
        {
            throw new Exception("Database is full");
        }

        var block = new BucketBlock(Blocks.Count, _bucketBlockCapacity);
        Blocks.Add(block);
        return block;
    }


    public NodeBlock FindNodeBlock(Pointer pointer)
    {
        return (NodeBlock)Blocks[pointer.BlockId];
    }

    public BucketBlock FindBucketBlock(Pointer pointer)
    {
        return (BucketBlock)Blocks[pointer.BlockId];
    }

    public DataBlock FindDataBlock(Pointer pointer)
    {
        return (DataBlock)Blocks[pointer.BlockId];
    }

    public FindResultModel FindRecords(decimal key)
    {
        var result = new FindResultModel();
        var sw = new Stopwatch();
        sw.Start();
        foreach (var block in GetDataBlocks())
        {
            foreach (var record in block.Items)
            {
                if (record.Key == key)
                {
                    result.Records.Add(record);
                }
            }

            result.DataBlockAccessed++;
        }

        sw.Stop();
        result.Ticks = sw.ElapsedTicks;
        return result;
    }

    public FindResultModel FindRecords(decimal from, decimal to)
    {
        var result = new FindResultModel();
        var sw = new Stopwatch();
        sw.Start();
        foreach (var block in GetDataBlocks())
        {
            foreach (var record in block.Items)
            {
                if (record.Key >= from && record.Key <= to)
                {
                    result.Records.Add(record);
                }
            }

            result.DataBlockAccessed++;
        }

        sw.Stop();
        result.Ticks = sw.ElapsedTicks;
        return result;
    }

    public IEnumerable<DataBlock> GetDataBlocks()
    {
        foreach (var block in Blocks)
        {
            if (block.GetType() != typeof(DataBlock))
            {
                continue;
            }

            yield return (DataBlock)block;
        }
    }

    public IEnumerable<NodeBlock> GetNodeBlocks()
    {
        foreach (var block in Blocks)
        {
            if (block.GetType() != typeof(NodeBlock))
            {
                continue;
            }

            yield return (NodeBlock)block;
        }
    }

    public RemoveResultModel RemoveRecordsTill(decimal to)
    {
        var result = new RemoveResultModel();
        var sw = new Stopwatch();
        sw.Start();
        foreach (var block in Blocks)
        {
            result.DataBlockAccessed++;
            if (block.GetType() != typeof(DataBlock))
            {
                continue;
            }

            var dataBlock = (DataBlock)block;
            foreach (var record in dataBlock.Items)
            {
                if (record.Key <= to)
                {
                    record.Deleted = true;
                    result.RecordsRemoved++;
                }
            }
        }
        sw.Stop();
        result.Ticks = sw.ElapsedTicks;
        return result;
    }
}