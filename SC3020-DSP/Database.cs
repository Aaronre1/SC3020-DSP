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
    private readonly int _capacity;
    private readonly DatabaseOptions _options;
    private List<BaseBlock> Blocks;

    public Database(DatabaseOptions options)
    {
        _options = options;
        _capacity = (int)(options.DiskCapacityInBytes / options.BlockSizeInBytes);
        Blocks = new List<BaseBlock>(_capacity);
        Console.WriteLine($"Initialized database with {_capacity} blocks capacity.");

        // nodeBlockCapacity = 400 - (pointer size) / (pointer size + key size)
        _nodeBlockCapacity =
            (int)((options.BlockSizeInBytes - options.PointerSizeInBytes) /
                  (options.PointerSizeInBytes + sizeof(decimal)));
        _dataBlockCapacity = (int)(options.BlockSizeInBytes / options.RecordSizeInBytes);
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
            throw new Exception("Database is full"); //TODO: Extract method
        }

        var block = new BucketBlock(Blocks.Count, _nodeBlockCapacity); //TODO: Bucket capacity
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

    public long BytesUsed()
    {
        long result = 0;

        foreach (var block in Blocks)
        {
            if (block.GetType() == typeof(DataBlock))
            {
                var dataBytes = block.Count * _options.RecordSizeInBytes;
                result += dataBytes;
            }

            // if (block.GetType() == typeof(NodeBlock))
            // {
            //     var blockBytes = block.Count * Node.ByteSize;
            //     blockBytes += Pointer.ByteSize;
            //     result += blockBytes;
            // }
        }

        return result;
    }
}