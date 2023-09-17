using SC3020_DSP.Domain.Entities;

namespace SC3020_DSP.Domain.Application;

public class BPTree
{
    private readonly Database _database;
    private NodeBlock _root;
    public BPTree(NodeBlock nodeBlock, Database database)
    {
        _database = database;
        _root = nodeBlock;
    }
    
    public void insertNode(DataBlock dataBlock)
    {
        // check if is empty 
        // .any = if there is any item inside
        // without any nodes it will be false 
        // foreach (var data in dataBlock.Items)
        // {
        //     var node = new Node();
        // }
        //
        // if (!_root.Items.Any())
        // {
        //     var node = new Node();
        // }
        // Check if node is null or not exist 
        // i will then initi
    }
}