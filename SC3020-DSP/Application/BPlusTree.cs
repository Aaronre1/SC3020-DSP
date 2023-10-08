using System.Diagnostics;
using SC3020_DSP.Application.Models;
using SC3020_DSP.Domain;
using SC3020_DSP.Domain.Domain.Enums;
using SC3020_DSP.Domain.Entities;

namespace SC3020_DSP.Application;

public class BPlusTree
{
    private readonly Database _database;
    private readonly int _maxKeys;
    private NodeBlock _root = null!;

    public BPlusTree(Database database)
    {
        _database = database;
        _maxKeys = _database.NodeBlockCapacity();
        
    }

    public int Levels { get; private set; }
    public NodeBlock Root => _root;

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

    public FindResultModel Find(decimal? key)
    {
        var sw = new Stopwatch();
        sw.Start();
        var result = new FindResultModel();

        NodeBlock cur = _root;
        result.IndexNodeAccessed++;
        while (cur.NodeType == NodeType.InternalNode)
        {
            for (int i = 0; i < cur.Count; i++)
            {
                // found key <= target key
                if (key < cur.Keys[i])
                {
                    cur = _database.FindNodeBlock(cur.Pointers[i]);
                    result.IndexNodeAccessed++;
                    break;
                }

                // found key > target key && last node
                if (i == cur.Count - 1)
                {
                    cur = _database.FindNodeBlock(cur.Pointers[i + 1]);
                    result.IndexNodeAccessed++;
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
            Console.WriteLine(string.Join("|", cur.Keys));
            throw new Exception($"{key} not found!");
        }

        var pointer = cur.Pointers[keyIndex];
        var bucketBlock = _database.FindBucketBlock(pointer);
        while (bucketBlock != null)
        {
            result.BucketBlockAccessed++;
            foreach (var recordPtr in bucketBlock.Pointers)
            {
                var dataBlock = _database.FindDataBlock(recordPtr);
                var record = dataBlock.Items[recordPtr.Offset];
                result.Records.Add(record);
                result.DataBlockAccessed++;
            }

            if (bucketBlock.OverflowBucket != null)
            {
                bucketBlock = _database.FindBucketBlock(bucketBlock.OverflowBucket);
            }
            else
            {
                bucketBlock = null;
            }
        }

        sw.Stop();
        result.Ticks = sw.ElapsedTicks;
        return result;
    }

    public FindResultModel Find(decimal? from, decimal to)
    {
        var sw = new Stopwatch();
        sw.Start();
        var result = new FindResultModel();

        NodeBlock cur = _root;
        result.IndexNodeAccessed++;
        while (cur.NodeType == NodeType.InternalNode)
        {
            for (int i = 0; i < cur.Count; i++)
            {
                // found key <= target key
                if (from < cur.Keys[i])
                {
                    cur = _database.FindNodeBlock(cur.Pointers[i]);
                    result.IndexNodeAccessed++;
                    break;
                }

                // found key > target key && last node
                if (i == cur.Count - 1)
                {
                    cur = _database.FindNodeBlock(cur.Pointers[i + 1]);
                    result.IndexNodeAccessed++;
                    break;
                }
            }
        }

        int keyIndex = 0;
        while (keyIndex < _maxKeys && cur.Keys[keyIndex] != null && from > cur.Keys[keyIndex])
        {
            keyIndex++;
        }

        var pointer = cur.Pointers[keyIndex];
        while (pointer != null)
        {
            var bucketBlock = _database.FindBucketBlock(pointer);
            while (bucketBlock != null)
            {
                result.BucketBlockAccessed++;
                foreach (var recordPtr in bucketBlock.Pointers)
                {
                    var dataBlock = _database.FindDataBlock(recordPtr);
                    var record = dataBlock.Items[recordPtr.Offset];
                    result.Records.Add(record);
                    result.DataBlockAccessed++;
                }

                if (bucketBlock.OverflowBucket != null)
                {
                    bucketBlock = _database.FindBucketBlock(bucketBlock.OverflowBucket);
                }
                else
                {
                    bucketBlock = null;
                }
            }

            // search next pointer or continue next leaf node
            keyIndex++;
            // continue next leaf node
            if (keyIndex == _maxKeys || cur.Pointers[keyIndex] == null)
            {
                if (cur.Pointers[_maxKeys] == null)
                {
                    break;
                }
                cur = _database.FindNodeBlock(cur.Pointers[_maxKeys]);
                result.IndexNodeAccessed++;
                keyIndex = 0;
            }
            if (to < cur.Keys[keyIndex])
            {
                break;
            }

            pointer = cur.Pointers[keyIndex];
        }

        sw.Stop();
        result.Ticks = sw.ElapsedTicks;
        return result;
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
            // Insert to bucket
            var rootBucket = _database.AssignBucketBlock();
            rootBucket.Pointers.Add(pointer);
            _root.Pointers[0] = rootBucket.Address;
            Levels++;
            return;
        }

        NodeBlock cur = _root;
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

        // find slot index to insert key
        int keyIndex = 0;
        while (keyIndex < _maxKeys-1 && cur.Keys[keyIndex] != null && record.Key > cur.Keys[keyIndex])
        {
            keyIndex++;
        }

        // target key already exists
        if (record.Key == cur.Keys[keyIndex])
        {
            // insert to bucket
            _database.AddToBucket(pointer, cur.Pointers[keyIndex]);
            return;
        }

        var bucket = _database.AssignBucketBlock();
        bucket.Pointers.Add(pointer);
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
            // insert to bucket
            cur.Pointers[keyIndex] = bucket.Address;
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
        // insert bucket
        tempPtrs[keyIndex] = bucket.Address;

        //insert into new leaf floor(n+1/2)
        var newLeaf = _database.AssignNodeBlock();
        newLeaf.NodeType = NodeType.LeafNode;

        var nextPtr = cur.Pointers[_maxKeys];
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
            if (i < _maxKeys)
            {
                cur.Keys[i] = null;
            }
            cur.Pointers[i] = null;
            newLeaf.Keys[i - splitIndex] = tempKeys[i];
            newLeaf.Pointers[i - splitIndex] = tempPtrs[i];
        }

        cur.Pointers[_maxKeys] = newLeaf.Address; // link next ptr
        newLeaf.Pointers[_maxKeys] = nextPtr;
        // create new non-leaf root node
        if (cur == _root)
        {
            var newRoot = _database.AssignNodeBlock();
            newRoot.NodeType = NodeType.InternalNode;
            newRoot.Keys[0] = newLeaf.Keys[0];
            newRoot.Pointers[0] = cur.Address;
            newRoot.Pointers[1] = newLeaf.Address;
            _root = newRoot;
            Levels++;
            return;
        }

        // push key up to parent
        var newChild = newLeaf;
        var newKey = newLeaf.Keys[0];

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

    private (decimal?, NodeBlock?) InsertInternal(decimal? key, NodeBlock cur, NodeBlock child)
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

        // insert into new internal node floor (n+1/2)
        var newInternal = _database.AssignNodeBlock();
        newInternal.NodeType = NodeType.InternalNode;

        int splitIndex = (_maxKeys + 1) / 2;
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
            if (i < _maxKeys)
            {
                cur.Keys[i] = null;
                cur.Pointers[i + 1] = null;
            }
            
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
            Levels++;
            return (null, null);
        }

        return (splitKey, newInternal);
    }
    
    public RemoveResultModel RemoveRecordsTill(decimal to)
    {
        var cur = _root;
        var result = new RemoveResultModel();
        var sw = new Stopwatch();
        sw.Start();
        
        result.IndexNodeAccessed++;
        while (cur.NodeType == NodeType.InternalNode)
        {
            cur = _database.FindNodeBlock(cur.Pointers[0]);
            result.IndexNodeAccessed++;
        }

        while (cur != null)
        {
            for (var i = 0; i < cur.Count; i++)
            {
                if (cur.Keys[i] <= to)
                {
                    // delete records in bucket
                    var bucket = _database.FindBucketBlock(cur.Pointers[i]);
                    while (bucket != null)
                    {
                        foreach (var pointer in bucket.Pointers)
                        {
                            var dataBlock = _database.FindDataBlock(pointer);
                            dataBlock.Items[pointer.Offset].Deleted = true;
                            result.RecordsRemoved++;
                        }

                        if (bucket.OverflowBucket != null)
                        {
                            bucket = _database.FindBucketBlock(bucket.OverflowBucket);
                        }
                        else
                        {
                            bucket = null;
                        }
                    }
                }
                else
                {
                    sw.Stop();
                    result.Ticks = sw.ElapsedTicks;
                    return result;
                }
            }

            if (cur.Pointers[_maxKeys] != null)
            {
                cur = _database.FindNodeBlock(cur.Pointers[_maxKeys]);
                result.IndexNodeAccessed++;
            }
            else
            {
                cur = null;
            }
        }

        sw.Stop();
        result.Ticks = sw.ElapsedTicks;
        return result;
    }
}