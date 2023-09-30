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

       // _nodeBlockCapacity = (int)(options.BlockSizeInBytes / Node.ByteSize);
       _nodeBlockCapacity = 10;
        Console.WriteLine(_nodeBlockCapacity);
        _dataBlockCapacity = (int)(options.BlockSizeInBytes / options.RecordSizeInBytes);
    }

    public int GetDataBlockCapacity() => _dataBlockCapacity;
    
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


    public NodeBlock FindNodeBlock(Pointer pointer)
    {
        return (NodeBlock)Blocks[pointer.BlockId];
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

    public DataBlock FindDataBlock(Pointer pointer)
    {
        return (DataBlock)Blocks[pointer.BlockId];
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