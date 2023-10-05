using System.Diagnostics;
using SC3020_DSP.Application.Models;
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

    public int N { get; private set; }
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
            foreach (var recordPtr in bucketBlock.Pointers)
            {
                var dataBlock = _database.FindDataBlock(recordPtr);
                // TODO: does multiple access to same data block count as separate access?
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
        while (keyIndex < _maxKeys && cur.Keys[keyIndex] != null && record.Key > cur.Keys[keyIndex])
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
            cur.Keys[i] = null;
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
            Levels++;
            return (null, null);
        }

        return (splitKey, newInternal);
    }

    public void Remove(decimal? key)
    {
        if (!key.HasValue)
        {
            return;
        }

        if (_root == null)
        {
            return;
        }

        var cur = _root;
        var parents = new Stack<NodeBlock>();
        int left = 0, right = 0;
        while (cur.NodeType == NodeType.InternalNode)
        {
            for (int i = 0; i < cur.Count; i++)
            {
                parents.Push(cur);
                left = i - 1;
                right = i + 1;
                if (key < cur.Keys[i])
                {
                    cur = _database.FindNodeBlock(cur.Pointers[i + 1]);
                }

                if (i == cur.Count - 1)
                {
                    left = i;
                    right = i + 2;
                    cur = _database.FindNodeBlock(cur.Pointers[i + 1]);
                    break;
                }
            }
        }

        var found = false;
        int keyIndex;
        for (keyIndex = 0; keyIndex < cur.Count; keyIndex++)
        {
            if (cur.Keys[keyIndex] == key)
            {
                found = true;
                break;
            }
        }

        if (!found)
        {
            Console.WriteLine($"Key to remove is not found: {key}");
            return;
        }

        //shift keys
        for (int i = keyIndex; i < cur.Count; i++)
        {
            cur.Keys[i] = cur.Keys[i + 1];
        }

        if (cur == _root)
        {
            for (int i = 0; i < _maxKeys + 1; i++)
            {
                cur.Pointers[i] = null;
            }

            if (cur.Count == 0)
            {
                // TODO: delete dead tree
                _root = null;
            }

            return;
        }

        cur.Pointers[cur.Count] = cur.Pointers[cur.Count + 1];
        cur.Pointers[cur.Count + 1] = null;
        if (cur.Count >= (_maxKeys + 1) / 2)
        {
            return;
        }


        if (left >= 0)
        {
            var leftNode = _database.FindNodeBlock(parents.Peek().Pointers[left]);
            if (leftNode.Count >= (_maxKeys + 1 / 2 + 1))
            {
                for (int i = cur.Count; i > 0; i--)
                {
                    cur.Keys[i] = cur.Keys[i - 1];
                }

                cur.Pointers[cur.Count + 1] = cur.Pointers[cur.Count];
                cur.Pointers[cur.Count] = null;
                cur.Keys[0] = leftNode.Keys[leftNode.Count - 1];

                leftNode.Pointers[leftNode.Count - 1] = cur.Address;
                leftNode.Pointers[leftNode.Count] = null;
                parents.Peek().Keys[left] = cur.Keys[0];
                return;
            }
        }

        // share from right sibling
        if (right <= parents.Peek().Count)
        {
            var rightNode = _database.FindNodeBlock(parents.Peek().Pointers[right]);
            if (rightNode.Count >= (_maxKeys + 1) / 2 + 1)
            {
                cur.Pointers[cur.Count + 1] = cur.Pointers[cur.Count];
                cur.Pointers[cur.Count] = null;

                cur.Keys[cur.Count - 1] = rightNode.Keys[0];

                rightNode.Pointers[rightNode.Count - 1] = rightNode.Pointers[rightNode.Count];
                rightNode.Pointers[rightNode.Count] = null;

                for (var i = 0; i < rightNode.Count; i++)
                {
                    rightNode.Keys[i] = rightNode.Keys[i + 1];
                }

                parents.Peek().Keys[right - 1] = rightNode.Keys[0];
                return;
            }
        }

        if (left >= 0)
        {
            var leftNode = _database.FindNodeBlock(parents.Peek().Pointers[left]);
            for (int i = leftNode.Count, j = 0; j < cur.Count; i++, j++)
            {
                leftNode.Keys[i] = cur.Keys[j];
            }

            leftNode.Pointers[leftNode.Count] = null;
            leftNode.Pointers[leftNode.Count] = cur.Pointers[cur.Count];

            //TODO: RemoveInternal()
            //TODO: Delete cur
        }
        else if (right <= parents.Peek().Count)
        {
            var rightNode = _database.FindNodeBlock(parents.Peek().Pointers[right]);
            for (int i = cur.Count, j = 0; j < rightNode.Count; i++, j++)
            {
                cur.Keys[i] = rightNode.Keys[j];
            }

            cur.Pointers[cur.Count] = null;
            cur.Pointers[cur.Count] = rightNode.Pointers[rightNode.Count];
            //TODO: RemoveInternal()
            //TODO: Delte cur
        }
    }

    private void RemoveInternal(int key, NodeBlock cur, NodeBlock child)
    {
        if (cur == _root)
        {
            if (cur.Count == 1)
            {
                if (cur.Pointers[1] == child.Address)
                {
                    _root = _database.FindNodeBlock(cur.Pointers[0]);
                    return;
                }
                else if (cur.Pointers[0] == child.Address)
                {
                    _root = _database.FindNodeBlock(cur.Pointers[1]);
                }
            }
        }

        int pos = 0;
        for (pos = 0; pos < cur.Count; pos++)
        {
            if (cur.Keys[pos] == key)
            {
                break;
            }
        }

        for (int i = pos; i < cur.Count; i++)
        {
            cur.Keys[i] = cur.Keys[i + 1];
        }

        for (pos = 0; pos < cur.Count + 1; pos++)
        {
            if (cur.Pointers[pos] == child.Address)
            {
                break;
            }
        }

        for (int i = pos; i < cur.Count + 1; i++)
        {
            cur.Pointers[i] = cur.Pointers[i + 1];
        }

        if (cur.Count - 1 >= (_maxKeys + 1) / 2 - 1)
        {
            return;
        }

        if (cur == _root)
        {
            return;
        }

        // TODO: Parent stack 
    }
}