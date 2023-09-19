using SC3020_DSP.Domain;
using SC3020_DSP.Domain.Domain.Enums;
using SC3020_DSP.Domain.Entities;

namespace SC3020_DSP.Application;

public class BPlusTree
{

    private Database _database;
    
    private NodeBlock _root;

    private NodeBlock _cur;
    private NodeBlock _parent;
    
    

    public BPlusTree(Database database)
    {
        _root = database.AssignNodeBlock();
    }

    public void Insert(Record record, Pointer address)
    {
        Node1 cur = _root;
        Node1 parent;

        while (cur.NodeType == NodeType.InternalNode)
        {
            parent = cur;
            for (int i = 0; i < cur.Count; i++)
            {
                if (record.Key < cur.Keys[i])
                {
                    cur = cur.Pointers[i];
                    break;
                }

                if (i == curInternal.Count - 1)
                {
                    cur = curInternal.Items[i + 1];
                    break;
                }
            }
        }

        if (cur.Count < cur.Capacity)
        {
            int i = 0;
            var curLeaf = (LeafNode)cur;
            while(record.Key > cur)
        }
    }
}