using SC3020_DSP.Domain.Application;
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
    private readonly List<BaseBlock> _blocks;

    public Database(DatabaseOptions options)
    {
        _options = options;
        _capacity = (int)(options.DiskCapacityInBytes / options.BlockSizeInBytes);
        _blocks = new List<BaseBlock>(_capacity);
         Console.WriteLine($"Initialized database with {_capacity} blocks capacity.");

        _nodeBlockCapacity = (int)(options.BlockSizeInBytes / Node.ByteSize);
        Console.WriteLine(_nodeBlockCapacity);
        _dataBlockCapacity = (int)(options.BlockSizeInBytes / options.RecordSizeInBytes);
    }

    public int GetDataBlockCapacity() => _dataBlockCapacity;
    
    // Adding Record  
    public bool Add(Record record)
    {
        foreach (var block in GetDataBlocks())
        {
            if (block.Add(record))
            {
                // Still can access the block entries 
                // Manipulate for b+ tree 
                return true;
            }
        }

        if (_blocks.Count == _capacity)
        {
            Console.WriteLine("Database is full");
            return false;
        }

        var newBlock = new DataBlock(_blocks.Count, _dataBlockCapacity);
        newBlock.Add(record);
        _blocks.Add(newBlock);
        return true;
    }

    // Add Node
    public bool Add(Node node)
    {
        foreach (var block in GetNodeBlocks())
        {
            if (block.Add(node))
            {
                return true;
            }
        }

        if (_blocks.Count == _capacity)
        {
            Console.WriteLine("Database is full");
            return false;
        }

        var newBlock = new NodeBlock(_blocks.Count, _nodeBlockCapacity);
        newBlock.Add(node);
        _blocks.Add(newBlock);
        return true;
    }

    public IEnumerable<NodeBlock> GetNodeBlocks()
    {
        foreach (var block in _blocks)
        {
            if (block.GetType() != typeof(NodeBlock))
            {
                continue;
            }

            yield return (NodeBlock)block;
        }
    }

    public IEnumerable<DataBlock> GetDataBlocks()
    {
        // run 1000 times 
        foreach (var block in _blocks)
        {
            if (block.GetType() != typeof(DataBlock))
            {
                continue;
            }
            // 500 data block variables 
            yield return (DataBlock)block;
        }
    }

    public long BytesUsed()
    {
        long result = 0;

        foreach (var block in _blocks)
        {
            if (block.GetType() == typeof(DataBlock))
            {
                var dataBytes = block.Current * _options.RecordSizeInBytes;
                result += dataBytes;
            }

            if (block.GetType() == typeof(NodeBlock))
            {
                var blockBytes = block.Current * Node.ByteSize;
                blockBytes += Pointer.ByteSize;
                result += blockBytes;
            }
        }

        return result;
    }
    
    
    public BPTree createBPTree()
    {
        var nodeBlock = new NodeBlock(_blocks.Count, _nodeBlockCapacity);
        var BPTree = new BPTree(nodeBlock, this);
        return BPTree;
    }
}