using SC3020_DSP.Domain;
using SC3020_DSP.Domain.Domain.Enums;
using SC3020_DSP.Domain.Entities;

namespace SC3020_DSP.Application;

public class BPlusTreeImpl : IBPlusTree
{
    private Database _database;
    private NodeBlock _root;

    private int _maxKeys = 5;

    public BPlusTreeImpl(Database database)
    {
        _database = database;
    }

    public void Add(Record record, Pointer pointer)
    {
        if (record.Key == null)
        {
            return;
        }

        if (_root == null)
        {
            _root = _database.AssignNodeBlock();
            _root.NodeType = NodeType.LeafNode;
            _root.Keys[0] = (decimal)record.Key;
            // TODO: Insert to bucket
            _root.Pointers[0] = pointer;
            return;
        }

        NodeBlock cur = _root;
        //NodeBlock parent;
        Stack<NodeBlock> parents = new Stack<NodeBlock>();
        // find leaf node to insert
        while (cur.NodeType == NodeType.InternalNode)
        {
            parents.Push(cur);
            for (int i = 0; i < cur.Count; i++)
            {
                // found key < target key
                if (record.Key < cur.Keys[i])
                {
                    cur = _database.FindNodeBlock(cur.Pointers[i]);
                    break;
                }

                // found key >= target key
                // TODO: TRY IF WORKS
                cur = _database.FindNodeBlock(cur.Pointers[i + 1]);
            }
        }

        // TODO: Find duplicate key
        // find slot index to insert key
        int keyIndex = 0;
        // 0 .. 4 < 5
        while (keyIndex < _maxKeys && cur.Keys[keyIndex] != null && record.Key > cur.Keys[keyIndex])
        {
            keyIndex++;
        }

        // target key already exists
        if (record.Key == cur.Keys[keyIndex])
        {
            // TODO: insert to bucket
            return;
        }

        // leaf node has slot
        if (cur.Count < _maxKeys)
        {
            // // find slot index to insert key
            // int keyIndex = 0;
            // while (keyIndex < cur.Count && record.Key > cur.Keys[keyIndex])
            // {
            //     keyIndex++;
            // }
            // target key already exists
            // if (record.Key == cur.Keys[keyIndex])
            // {
            //     // TODO: insert to bucket
            //     return;
            // }

            // right shift keys to make space
            for (int i = cur.Count; i > keyIndex; i--)
            {
                cur.Keys[i] = cur.Keys[i - 1];
                cur.Pointers[i] = cur.Pointers[i - 1];
            }

            cur.Keys[keyIndex] = (decimal)record.Key;
            // TODO: insert to bucket
            cur.Pointers[keyIndex] = pointer;
            return;
        }
        // leaf is full
        // create temp node
        var tempKeys = new decimal?[_maxKeys + 1];
        var tempPtrs = new Pointer[_maxKeys + 1];
        for (int i = 0; i < _maxKeys; i++)
        {
            tempKeys[i] = cur.Keys[i];
            tempPtrs[i] = cur.Pointers[i];
        }

        // right shift keys
        for (int i = _maxKeys; i > keyIndex; i--)
        {
            tempKeys[i] = tempKeys[i - 1];
            tempPtrs[i] = tempPtrs[i - 1];
        }

        tempKeys[keyIndex] = record.Key;
        tempPtrs[keyIndex] = pointer;

        //insert into new leaf floor(n+1/2)
        var newLeaf = _database.AssignNodeBlock();
        newLeaf.NodeType = NodeType.LeafNode;

        int splitIndex = (_maxKeys + 1) / 2; // 5 + 1 /2 = 3  || 4+1/2 = 2
        for (int i = 0; i < _maxKeys + 1; i++) // 0..5=> 0,1,2 | 3,4,5     0..4 => 0,1,2 | 3,4 
        {
            // insert left node
            if (i < splitIndex)
            {
                cur.Keys[i] = tempKeys[i];
                cur.Pointers[i] = tempPtrs[i];
                continue;
            }

            // insert right node
            cur.Keys[i] = null;
            cur.Pointers[i] = null;
            newLeaf.Keys[i - splitIndex] = tempKeys[i];
            newLeaf.Pointers[i - splitIndex] = tempPtrs[i];
        }

        cur.Pointers[_maxKeys] = new Pointer(newLeaf.Id, 0); // link next ptr

        // create first non-leaf root node
        if (cur == _root)
        {
            var newRoot = _database.AssignNodeBlock();
            newRoot.NodeType = NodeType.InternalNode;
            newRoot.Keys[0] = newLeaf.Keys[0];
            newRoot.Pointers[0] = new Pointer(cur.Id);
            newRoot.Pointers[1] = new Pointer(newLeaf.Id);
            _root = newRoot;
            // TODO: have a stack to navigate parent nodes
            cur.ParentPointer = new Pointer(newRoot.Id);
            newLeaf.ParentPointer = new Pointer(newRoot.Id);
        }


        throw new NotImplementedException();
    }

    private void InsertInternal(decimal? key, NodeBlock cur, NodeBlock child)
    {
        var childPtr = new Pointer(child.Id);
        // Find slot to insert key
        int keyIndex = 0;
        while (keyIndex < _maxKeys && cur.Keys[keyIndex] != null && key > cur.Keys[keyIndex])
        {
            keyIndex++;
        }

        // target key already exists
        if (key == cur.Keys[keyIndex])
        {
            return;
        }

        // internal node has slot
        if (cur.Count < _maxKeys)
        {
            // right shift keys to make space
            for (int i = cur.Count; i > keyIndex; i--)
            {
                cur.Keys[i] = cur.Keys[i - 1];
                cur.Pointers[i + 1] = cur.Pointers[i];
            }

            cur.Keys[keyIndex] = (decimal)key;
            cur.Pointers[keyIndex + 1] = childPtr;
            return;
        }
        // internal node is full

        // create temp node
        var tempKeys = new decimal?[_maxKeys + 1];
        var tempPtrs = new Pointer[_maxKeys + 2];
        for (int i = 0; i < _maxKeys; i++)
        {
            tempKeys[i] = cur.Keys[i];
            tempPtrs[i] = cur.Pointers[i];
        }
        tempPtrs[_maxKeys + 1] = cur.Pointers[_maxKeys];
        
        // right shift keys
        for (int i = _maxKeys + 1; i > keyIndex; i--)
        {
            tempKeys[i] = tempKeys[i - 1];
        }
        // right shift pointers
        for (int i = _maxKeys + 2; i > keyIndex + 1; i--)
        {
            tempPtrs[i] = tempPtrs[i - 1];
        }

        tempKeys[keyIndex] = key;
        tempPtrs[keyIndex + 1] = childPtr;
        
        // insert into new leaf floor (n+1/2)
        var newInternal = _database.AssignNodeBlock();
        newInternal.NodeType = NodeType.InternalNode;

        int splitIndex = (_maxKeys + 1) / 2; //TODO: extract 
        for (int i = 0; i < _maxKeys + 1; i++)
        {
            // insert left node
            if (i < splitIndex)
            {
                cur.Keys[i] = tempKeys[i];
                // TODO: How to split internal node?
            }
        }

    }
}