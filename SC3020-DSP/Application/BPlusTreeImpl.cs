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

    public Record Find(decimal? key)
    {
        NodeBlock cur = _root;
        while (cur.NodeType == NodeType.InternalNode)
        {
            for (int i = 0; i < cur.Count; i++)
            {
                // found key <= target key
                if (key <= cur.Keys[i])
                {
                    cur = _database.FindNodeBlock(cur.Pointers[i]);
                    break;
                }

                // found key > target key && last node
                if (i == cur.Count - 1)
                {
                    cur = _database.FindNodeBlock(cur.Pointers[i + 1]);
                    break;
                }
            }
        }
        
        int keyIndex = 0;
        while (keyIndex < _maxKeys && cur.Keys[keyIndex] != null && key > cur.Keys[keyIndex])
        {
            keyIndex++;
        }

        if (cur.Keys[keyIndex] != key)
        {
            throw new Exception($"{key} not found!");
        }

        var pointer = cur.Pointers[keyIndex];
        var dataBlock = _database.FindDataBlock(pointer);
        return dataBlock.Items[pointer.Offset];

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

                if (record.Key == cur.Keys[i])
                {
                    cur = _database.FindNodeBlock(cur.Pointers[i + 1]);
                    break;
                }
                
                // found key > target key && last node
                if (i == cur.Count - 1)
                {
                    cur = _database.FindNodeBlock(cur.Pointers[i + 1]);
                    break;
                }
            }
        }

        // TODO: Find duplicate key
        // find slot index to insert key
        int keyIndex = 0;
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
            return;
        }

        // push key up to parent
        while (parents.Any())
        {
            var parent = parents.Pop();
            var newChild = InsertInternal(record.Key, parent, newLeaf);
            if (newChild == null)
            {
                return;
            }
        }
    }

    private NodeBlock? InsertInternal(decimal? key, NodeBlock cur, NodeBlock child)
    {
        var childPtr = new Pointer(child.Id);
        // Find slot to insert key
        int keyIndex = 0;
        while (keyIndex < _maxKeys && cur.Keys[keyIndex] != null && key > cur.Keys[keyIndex])
        {
            keyIndex++;
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
            return null;
        }

        // internal node is full, split node and return false(continue push up)
        // insert into new leaf floor (n+1/2)
        var newInternal = _database.AssignNodeBlock();
        newInternal.NodeType = NodeType.InternalNode;
        newInternal.Pointers[0] = childPtr;

        int splitIndex = (_maxKeys + 1) / 2; //TODO: extract method

        for (int i = splitIndex; i < _maxKeys; i++) // 3  | 2  split
        {
            newInternal.Keys[i - splitIndex] = cur.Keys[i]; // 3-3 = 3   0 = 3
            newInternal.Pointers[i - splitIndex + 1] = cur.Pointers[i + 1]; // 1 = 4
            cur.Keys[i] = null;
            cur.Pointers[i + 1] = null;
        }

        return newInternal;
    }
}