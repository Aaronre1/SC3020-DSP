using System.Drawing;
using SC3020_DSP.Domain.Common;
using SC3020_DSP.Domain.Configurations;
using SC3020_DSP.Domain.Entities;

namespace SC3020_DSP.Domain;

public class Database
{
    public long BytesUsed { get; set; }

    public long Capacity { get; set; }
    
    public List<BaseBlock> Blocks { get; set; } = new List<BaseBlock>();

    public Database(DatabaseOptions options)
    {
        Capacity = options.DiskCapacityInBytes / options.BlockSizeInBytes;
        Blocks = new List<BaseBlock>((int)Capacity);
        
        Console.WriteLine($"Initialized database with {Capacity} blocks capacity.");
    }
    public bool Add(Record record)
    {
        foreach (var block in Blocks)
        {
            if (block.GetType() != typeof(DataBlock))
            {
                continue;
            }

            if (block.Current == block.Capacity)
            {
                continue;
            }

            var dataBlock = (DataBlock)block;
            dataBlock.Add(record);
            return true;
        }

        if (Blocks.Count == Capacity)
        {
            // Database is full
            Console.WriteLine("Database is full");
            return false; 
        }

        var newBlock = new DataBlock(Blocks.Count);
        newBlock.Add(record);
        Blocks.Add(newBlock);
        return true;
    }

    public IEnumerable<DataBlock> GetDataBlocks()
    {
        foreach (var block in Blocks)
        {
            if (block.GetType() != typeof(DataBlock))
            {
                continue;
            }

            var dataBlock = (DataBlock)block;
            yield return dataBlock;
        }
    }
}