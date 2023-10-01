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

    public void Print()
    {
        Print(_root);
    }

    private void Print(NodeBlock cur)
    {
        if (cur != null)
        {
            for (int i = 0; i < cur.Count; i++)
            {
                Console.Write($" {cur.Keys[i]} ");
            }

            Console.WriteLine();
            if (cur.NodeType == NodeType.InternalNode)
            {
                for (int i = 0; i < cur.Count + 1; i++)
                {
                    Print(_database.FindNodeBlock(cur.Pointers[i]));
                }
            }
        }
    }
    public Record Find(decimal? key)
    {
        NodeBlock cur = _root;
        while (cur.NodeType == NodeType.InternalNode)
        {
            for (int i = 0; i < cur.Count; i++)
            {
                // found key <= target key
                if (key < cur.Keys[i])
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
            Console.WriteLine(string.Join("|",cur.Keys));
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

        int splitIndex = (_maxKeys + 1) / 2;
        for (int i = 0; i < _maxKeys + 1; i++) 
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

        // create new non-leaf root node
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
        var newChild = newLeaf;
        var newKey = newLeaf.Keys[0];
        //InsertInternal2(newLeaf.Keys[0], parents.Pop(), newLeaf);
        
        while (parents.Any())
        {
            var parent = parents.Pop();
            var result = InsertInternal(newKey, parent, newChild);
            if (result.Item1 == null)
            {
                return;
            }
        
            newKey = result.Item1;
            newChild = result.Item2;
        }
    }

    
    private void InsertInternal2(decimal? key, NodeBlock cur, NodeBlock child)
    {
        var childPtr = new Pointer(child.Id);
        // Find slot to insert key
        int keyIndex = 0;
        while (keyIndex < _maxKeys && cur.Keys[keyIndex] != null && key > cur.Keys[keyIndex])
        {
            keyIndex++;
        }
        // key already exists
        if (key == cur.Keys[keyIndex])
        {
            // TODO: insert to bucket
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

        // internal node is full, split node and push up middle key
        var tempKeys = new decimal?[_maxKeys + 1];
        var tempPtrs = new Pointer[_maxKeys + 2];
        for (int i = 0; i < _maxKeys; i++)
        {
            tempKeys[i] = cur.Keys[i];
            tempPtrs[i] = cur.Pointers[i];
        }
        tempPtrs[_maxKeys] = cur.Pointers[_maxKeys];
        
        // right shift till index slot
        for (int i = _maxKeys; i > keyIndex; i--)
        {
            tempKeys[i] = tempKeys[i - 1]; 
            tempPtrs[i + 1] = tempPtrs[i];
        }
        // insert into temp node
        tempKeys[keyIndex] = key;
        tempPtrs[keyIndex + 1] = childPtr;
        
        // insert into new leaf floor (n/2)
        var newInternal = _database.AssignNodeBlock();
        newInternal.NodeType = NodeType.InternalNode;

        int splitIndex = (_maxKeys + 1) / 2; // n=3 => 2
        decimal? splitKey = tempKeys[splitIndex];
        cur.Pointers[0] = tempPtrs[0];
        for (int i = 0; i < _maxKeys + 1; i++)
        {
            // insert left node
            if (i < splitIndex)
            {
                cur.Keys[i] = tempKeys[i];
                cur.Pointers[i + 1] = tempPtrs[i + 1];
                continue;
            }
            // key to push up
            if (i == splitIndex)
            {
                newInternal.Pointers[0] = tempPtrs[splitIndex];
                continue;
            }
            // insert right node
            cur.Keys[i] = null;
            cur.Pointers[i + 1] = null;
            newInternal.Keys[i - splitIndex - 1] = tempKeys[i];
            newInternal.Pointers[i - splitIndex] = tempPtrs[i + 1];
        }

        if (cur == _root)
        {
            var newRoot = _database.AssignNodeBlock();
            newRoot.NodeType = NodeType.InternalNode;
            newRoot.Keys[0] = splitKey;
            newRoot.Pointers[0] = new Pointer(cur.Id);
            newRoot.Pointers[1] = new Pointer(newInternal.Id);
            _root = newRoot;
            return;
        }
        InsertInternal2(splitKey,_database.FindNodeBlock(FindParent(new Pointer(_root.Id),new Pointer(child.Id))) ,newInternal);
    }
    private (decimal?,NodeBlock?) InsertInternal(decimal? key, NodeBlock cur, NodeBlock child)
    {
        var childPtr = new Pointer(child.Id);
        // Find slot to insert key
        int keyIndex = 0;
        while (keyIndex < _maxKeys && cur.Keys[keyIndex] != null && key > cur.Keys[keyIndex])
        {
            keyIndex++;
        }
        // key already exists
        if (key == cur.Keys[keyIndex])
        {
            // TODO: insert to bucket
            return (null, null);
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
            return (null, null);
        }

        // internal node is full, split node and push up middle key
        var tempKeys = new decimal?[_maxKeys + 1];
        var tempPtrs = new Pointer[_maxKeys + 2];
        for (int i = 0; i < _maxKeys; i++)
        {
            tempKeys[i] = cur.Keys[i];
            tempPtrs[i] = cur.Pointers[i];
        }
        tempPtrs[_maxKeys] = cur.Pointers[_maxKeys];
        
        // right shift till index slot
        for (int i = _maxKeys; i > keyIndex; i--)
        {
            tempKeys[i] = tempKeys[i - 1]; 
            tempPtrs[i + 1] = tempPtrs[i];
        }
        // insert into temp node
        tempKeys[keyIndex] = key;
        tempPtrs[keyIndex + 1] = childPtr;
        
        // insert into new leaf floor (n/2)
        var newInternal = _database.AssignNodeBlock();
        newInternal.NodeType = NodeType.InternalNode;

        int splitIndex = (_maxKeys + 1) / 2; // n=3 => 2
        decimal? splitKey = tempKeys[splitIndex];
        cur.Pointers[0] = tempPtrs[0];
        for (int i = 0; i < _maxKeys + 1; i++)
        {
            // insert left node
            if (i < splitIndex)
            {
                cur.Keys[i] = tempKeys[i];
                cur.Pointers[i + 1] = tempPtrs[i + 1];
                continue;
            }
            // key to push up
            if (i == splitIndex)
            {
                newInternal.Pointers[0] = tempPtrs[splitIndex + 1];
                cur.Keys[i] = null;
                cur.Pointers[i + 1] = null;
                continue;
            }
            // insert right node
            cur.Keys[i] = null;
            cur.Pointers[i + 1] = null;
            newInternal.Keys[i - splitIndex - 1] = tempKeys[i];
            newInternal.Pointers[i - splitIndex] = tempPtrs[i + 1];
        }

        if (cur == _root)
        {
            var newRoot = _database.AssignNodeBlock();
            newRoot.NodeType = NodeType.InternalNode;
            newRoot.Keys[0] = splitKey;
            newRoot.Pointers[0] = new Pointer(cur.Id);
            newRoot.Pointers[1] = new Pointer(newInternal.Id);
            _root = newRoot;
            return (null, null);
        }
        return (splitKey, newInternal);
    }

    private Pointer FindParent(Pointer cur, Pointer child)
    {
        Pointer parent = null;
        var curNode = _database.FindNodeBlock(cur);
        if (curNode.NodeType == NodeType.LeafNode ||
            _database.FindNodeBlock(curNode.Pointers[0]).NodeType == NodeType.LeafNode)
        {
            return null;
        }

        for (int i = 0; i < curNode.Count + 1; i++)
        {
            if (curNode.Pointers[i] == child)
            {
                parent = cur;
                return parent;
            }

            parent = FindParent(curNode.Pointers[i], child);
            if (parent != null)
            {
                return parent;
            }
        }

        return parent;
    }
}