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
        _root.NodeType = NodeType.LeafNode;
    }

    public void Insert(Record record, Pointer address)
    {
        NodeBlock cur = _root;
        NodeBlock parent;

        //
        while (cur.NodeType == NodeType.InternalNode)
        {
            parent = cur;
            for (int i = 0; i < cur.Count; i++)
            {
                if (record.Key < cur.Keys[i])
                {
                    cur = cur.Items[i];
                    break;
                }

                if (i == cur.Count - 1)
                {
                    cur = cur.Items[i + 1];
                    break;
                }
            }
        }

        if (cur.Count < cur.Capacity)
        {
            int i = 0;
            while (record.Key > cur.Keys[i] && i < cur.Count)
            {
                i++;
            }

            for (int j = cur.Count; j > i; j--)
            {
                cur.Keys[j] = cur.Keys[j - 1];
            }

            cur.Keys[i] = record.Key;
            cur.Items[cur.Count] = cur.Items[cur.Count - 1];
            cur.Items[cur.Count - 1] = null;
        }
        else
        {
            // doing banana split
            var newLeaf = _database.AssignNodeBlock();
            newLeaf.NodeType = NodeType.LeafNode;
            // virtual node???
            var virtualNode = new decimal?[cur.Capacity + 1];
            for (int i = 0; i < newLeaf.Capacity; i++)
            {
                virtualNode[i] = cur.Keys[i];
            }

            int ii = 0;
            while (record.Key > virtualNode[ii] && ii < cur.Capacity)
            {
                ii++;
            }

            for (int j = cur.Capacity + 1; j > ii; j--)
            {
                virtualNode[j] = virtualNode[j - 1];
            }

            virtualNode[ii] = record.Key;
            cur.Items[cur.Count] = newLeaf;
            newLeaf.Items[newLeaf.Count] = cur.Items[cur.Capacity]; // ???
            cur.Items[cur.Capacity] = null; // ????

            for (int i = 0; i < cur.Count; i++)
            {
                cur.Keys[i] = virtualNode[i];
            }

            for (int i = 0, j = cur.Count; i < newLeaf.Count; i++, j++)
            {
                newLeaf.Keys[i] = virtualNode[j];
            }

            if (cur == _root)
            {
                var newRoot = new NodeBlock();
                newRoot.NodeType = NodeType.InternalNode;
                newRoot.Keys[0] = newLeaf.Keys[0];
                newRoot.Items[0] = cur;
                newRoot.Items[1] = newLeaf;
                _root = newRoot;
            }
            else
            {
                InsertInternal(newLeaf.Keys[0], parent, newLeaf);
            }
        }
    }

    private void InsertInternal(decimal? key, NodeBlock cur, NodeBlock child)
    {
        if (cur.Count < cur.Capacity)
        {
            int i = 0;
            while (key > cur.Keys[i] && i < cur.Count)
            {
                i++;
            }

            for (int j = cur.Count; j > i; j--)
            {
                cur.Keys[j] = cur.Keys[j - 1];
            }

            for (int j = cur.Count + 1; j > i + 1; j--)
            {
                cur.Items[j] = cur.Items[j - 1];
            }

            cur.Keys[i] = key;
            cur.Items[i + 1] = child;
        }
        else
        {
            var newInternal = new NodeBlock();
            var virtualKey = new decimal?[cur.Capacity + 1];
            for (int i = 0; i < cur.Capacity; i++)
            {
                virtualKey[i] = cur.Keys[i];
            }

            var virtualItems = new NodeBlock[cur.Capacity + 2];
            for (int i = 0; i < cur.Capacity; i++)
            {
                // TODO:
            }
        }
    }
}