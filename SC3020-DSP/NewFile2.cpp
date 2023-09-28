

// Insert Operation
void BPTree::insert(int x) {
    //There is no NodeBlock being initialise for B+ Tree 
    if (root == NULL) {
        // initialise a new NodeBlock as root
        root = new Node;
        // Set current NodeBlock Key to Record ID
        root->key[0] = x;
        // The first initialisation of a NodeBlock will DEFINITELY be a leaf node
        root->IS_LEAF = true;
        // set NodeBlock Capacity(amt of keys) to 1
        root->size = 1;
    } 
    // 
    else 
    {
        // Initialise a NodeBlock and assign the existing root to it
        Node cursor = root;
        // Initialise another NodeBlock
        Node *parent;

        //Checking if cursor is not a LEAF NodeBlock !!!!!!
        while (cursor->IS_LEAF == false) {
            // To use parent as a tracker for "cursor"(root node) 
            // so when the "Cursor" is pointing to the next 
            // location, it can still keep track where the root is
            parent = cursor;
            // For loop to iterate thru Cursor NodeBlock capacity
            for (int i = 0; i < cursor->size; i++) {
                // Check if the Record ID < Cursor Key ID 
                if (x < cursor->key[i]) {
                    // If yes, set the cursor to a NodeBlock that
                    // is smaller than Record ID 
                    // eg. If this is at the root
                    // it will point to the 2nd level Node Block
                    cursor = cursor->ptr[i];
                    // Exit of out this for loop 
                    // Return back to while loop. 
                    break;
                }
                // If the existing Cursor NodeBlock ID is not smaller than Record ID 
                // and there is a space(hence the size -1) 
                // (meaning it is bigger than the current NodeBlock)

                // Set the cursor NodeBlock into the current NodeBlock pointer 
                if (i == cursor->size - 1) {
                    cursor = cursor->ptr[i + 1];
                    break;
                }
            }
        }

        // Meaning the Cursor NodeBlock will be a LeafBlock 
        // Check if the LeafBlock size is still within the MAX Key (the number we declaring for maximum numbers of keys inside the node block) 
        // (basically check if the NodeBlock got space la) 
        if (cursor->size < MAX) {
            // Counter for while loop  
            int i = 0; 
            // 'check the x(Record)'s ID if is LARGER than LeafBlock's key
            // and if counter is smaller than LeafBlock capacity (so can fit in stuff lor) 
            while (x > cursor->key[i] && i < cursor->size)
                i++;
            // To handle placing of the key inbetween nodes
            for (int j = cursor->size; j > i; j--) {
                // assign the key that is larger than x(Record)'s to the next slot on the right of LeafBlock)
                cursor->key[j] = cursor->key[j - 1];
            }
            // Insert x(Record)'s ID into the new slot in LeafBlock
            cursor->key[i] = x;
            // Increment LeafBlock Capacity
            cursor->size++;
            // Assign the New Node Pointer to the Last Node Pointer 
            cursor->ptr[cursor->size] = cursor->ptr[cursor->size - 1];
            // Set the Last Pointer to Null
            cursor->ptr[cursor->size - 1] = NULL;
        }
        
        else {
            // Cursor pointing to root right now and the NodeBlock is a LeafBlock 
            // Will reach here meaning LeafBlock is FULL  and need to start the splitting
            // Initialise a new NodeBlock Object
            Node *newLeaf = new Node;
            // Creating an array of float(since our ID is in decimal)
            // This is to act as a placeholder to store our keys
            int virtualNode[MAX + 1];

            // Assign the LeafBlock's keys into the new array 
            for (int i = 0; i < MAX; i++) {
                virtualNode[i] = cursor->key[i];
            }

            // Counter for while loop and KEEP THE "J" 
            int i = 0, j;
            // This is performing the same as the above but now instead of handling the LeafBlock Capacity
            // we are checking with the MAX Capacity of how much a LeafNode can hold 
            while (x > virtualNode[i] && i < MAX)
                i++;
            // To handle placing of the key inbetween current existing keys 
            for (int j = MAX + 1; j > i; j--) {
                // Doing shifting 
                virtualNode[j] = virtualNode[j - 1];
            }
            // Insert x(Record)'s ID into the new slot in the new array  
            virtualNode[i] = x;
            // Set our NodeBlock as LeafBlock 
            newLeaf->IS_LEAF = true;

            // Basically splitting the keys up la (u should aga aga understand) 
            // E.G. MAX = 3
            // = 2   
            cursor->size = (MAX + 1) / 2;
            // 4 - 4/2 = 2 
            newLeaf->size = MAX + 1 - (MAX + 1) / 2;

            // ** Handling of pointers when splitting ** 
            // Assign the available pointer in Cursor LeafBlock to the NewLeaf LeafBlock 
            // eg. Imagine this is a LeafBLock
            // [x] |[KEY]| [x]| [KEY]| [x]| [KEY]| [x]
            // ########Used##########  ^ use this as the pointer to the NewLeaf LeafBlock (Capacity(size) is 2) 
            cursor->ptr[cursor->size] = newLeaf;
            // Same logic as above, but this time round, since the Cursor LeafBlock was full (aka there is one more pointer at the end)
            // Need to initalise it back to the new split NewLeaf LeafBlock
            newLeaf->ptr[newLeaf->size] = cursor->ptr[MAX];
            // Assign Cursor LeafNode last pointer to NULL 
            cursor->ptr[MAX] = NULL;

            // ** Handling of keys when splitting **
            // Since we have split the Cursor LeafBlock size to a minimum number (refer to line 113)
            // and our virtualNode (refer to line 86) has all the sorted keys 
            for (i = 0; i < cursor->size; i++) {
                // Assign the keys (self-explanatory) 
                cursor->key[i] = virtualNode[i];
            }
            // Similar to the above, based on how many Keys has been assigned to Cursor LeafBlock,
            // the remaining Keys will be populated into the NewLeaf LeafBlock 
            for (i = 0, j = cursor->size; i < newLeaf->size; i++, j++) {
                // Assign the keys (self-explanatory) 
                newLeaf->key[i] = virtualNode[j];
            }
            
            // Checking if the current LeafNode is the root NodeBlock a not
            // If yes, need to create into a two two level tree liao
            // In a nutshell, the first NodeBlock that is inside the tree is full and just nice the only NodeBlock is the root
            // so need to split into two LeafBlock
            // Create one Root and assign it to the respective pointers 
            if (cursor == root) {
                // Initialise a new NodeBlock Object(to act as the new tmp Root NodeBlock) 
                Node *newRoot = new Node;
                // Set the first key of new tmp Root NodeBlock =  Second NodeBlock first key
                newRoot->key[0] = newLeaf->key[0];
                // Set the new tmp Root NodeBlock first pointer to the original cursor NodeBlock 
                newRoot->ptr[0] = cursor;
                // Set the new tmp Root NodeBlock second pointer to the Second NodeBlock 
                newRoot->ptr[1] = newLeaf;
                // Self explanatory
                newRoot->IS_LEAF = false;
                // Update new tmp Root NodeBlock NodeBlock Capacity 
                newRoot->size = 1;
                // Set new tmp Root NodeBlock into Root NodeBlock 
                root = newRoot;
            }
            
            else {
                // Parse over the new(right) Leaf NodeBlock key (so can build on the internal node key)
                // for parent NodeBlock (basically to keep track of the root node), refer to line 29 
                // new LeafNode for the details 
                insertInternal(newLeaf->key[0], parent, newLeaf);
            }
        }
    }
}

// Insert Operation
void BPTree::insertInternal(int x, Node *cursor, Node *child) {
    // Check if the cursor(internal) NodeBlock size is still within the MAX Key (the number we declaring for maximum numbers of keys inside the node block) 
    // Check if cursor(internal) NodeBlock got space 
    if (cursor->size < MAX) {
        // Counter for while loop  
        int i = 0;
        // 'check the x(Record)'s ID if is LARGER than cursor(internal) NodeBlock's key
        // and if counter is smaller than cursor(internal) NodeBlock capacity (so can fit in stuff lor)
        while (x > cursor->key[i] && i < cursor->size)
            i++;
        // To handle placing of the key inbetween nodes
        for (int j = cursor->size; j > i; j--) {
            // assign the key that is larger than x(Record)'s to the next slot on the right of cursor(internal) NodeBlock)
            cursor->key[j] = cursor->key[j - 1];
        }
        // To handle pointer on the NodeBlock 
        for (int j = cursor->size + 1; j > i + 1; j--) {
            // Same concept as above, just reshuffling pointers around the cursor(internal) NodeBlock
            cursor->ptr[j] = cursor->ptr[j - 1];
        }
        
        // Insert x(Record)'s ID into the new slot in cursor(internal) NodeBlock
        cursor->key[i] = x;
        // Increment LeafBlock Capacity
        cursor->size++;
        // Set the next available pointer index of cursor(internal) NodeBlock to the Child NodeBlock(Leaf)
        cursor->ptr[i + 1] = child;
    }

    else {
        // To handle banana split of internal Nodes
        // Initialise a new Internal NodeBlock 
        Node *newInternal = new Node;
        // Creating an array of float(since our ID is in decimal)
        // This is to act as a placeholder to store our keys
        int virtualKey[MAX + 1];

        // Creating an array of Node Block 
        Node *virtualPtr[MAX + 2];

        // Assign the internal NodeBlock's keys into the new tmp key array 
        for (int i = 0; i < MAX; i++) {
            virtualKey[i] = cursor->key[i];
        }

        // Assign the internal NodeBlock's pointers into the new tmp pointer NodeBlock
        for (int i = 0; i < MAX + 1; i++) {
            virtualPtr[i] = cursor->ptr[i];
        }
        // Counter for while loop and KEEP THE "J" 
        int i = 0, j;
        // Doing comparison between keys to see where to store the key in the new tmp key array
        while (x > virtualKey[i] && i < MAX)
            i++;
        // To handle placing of the key inbetween current existing keys 
        for (int j = MAX + 1; j > i; j--) {
            virtualKey[j] = virtualKey[j - 1];
        }
        // Insert x(Record)'s ID into the new tmp key array  
        virtualKey[i] = x;
        // Similarly, handle placing of new key's pointers inbetween current pointers
        // (shuffling in the tmp pointer NodeBlock) 
        for (int j = MAX + 2; j > i + 1; j--) {
            virtualPtr[j] = virtualPtr[j - 1];
        }

        // Assign the new key's pointer that is inside tmp Pointer NodeBlock to the child LeafNode
        virtualPtr[i + 1] = child;
        // Self explanatory 
        newInternal->IS_LEAF = false;
        // Banana splitting 
        cursor->size = (MAX + 1) / 2;
        // To determine how much nodes will be going to the right side  
        newInternal->size = MAX - (MAX + 1) / 2;

        // set the new Internal NodeBlock key from the new tmp key array   
        for (i = 0, j = cursor->size + 1; i < newInternal->size; i++, j++) {
            newInternal->key[i] = virtualKey[j];
        }
        // Similarly, set the pointers of the new Internal NodeBlock from the new tmp pointer NodeBlock 
        for (i = 0, j = cursor->size + 1; i < newInternal->size + 1; i++, j++) {
            newInternal->ptr[i] = virtualPtr[j];
        }

        // Check if the current cursor Node Block is set as Root
        // If yes, Build another level for the internal NodeBlock since the internal Nodeblock is oversized
        // Same as insert function
        if (cursor == root) {
            Node *newRoot = new Node;
            // The difference is this, set the first key to the cursor Internal NodeBlock 
            newRoot->key[0] = cursor->key[cursor->size];
            newRoot->ptr[0] = cursor;
            newRoot->ptr[1] = newInternal;
            newRoot->IS_LEAF = false;
            newRoot->size = 1;
            root = newRoot;
        } else {
            // Have to recursive call to cater for building like 4 levels
            // Like 2 levels of Internal NodeBlock (middle one la)
            // So need to update the parents NodeBlock with the keys and etc
            insertInternal(cursor->key[cursor->size], findParent(root, cursor), newInternal);
        }
    }
}

// Find the parent
Node *BPTree::findParent(Node *cursor, Node *child) {
    // parent NodeBlock as tmp
    Node *parent;
    // Base Case
    // Check if new (could be) parent Node Block is a leaf Node
    // Check if new (could be) parent Node Block is a internal pointer
    // by checking the first ptr is pointing to a Leaf NodeBlock 
    if (cursor->IS_LEAF || (cursor->ptr[0])->IS_LEAF) {
        return NULL;
    }
    // Meaning it is a root node
    // will happen when its only 3 level 
    for (int i = 0; i < cursor->size + 1; i++) {
        // check if the cursor first ptr NodeBlock(parent) is pointing to the new NodeBlock(child) 
        if (cursor->ptr[i] == child) {
            parent = cursor;
            return parent;
        } else {
            // Recursive to check next ptr of NodeBlock(parent) 
            parent = findParent(cursor->ptr[i], child);
            if (parent != NULL)
                return parent;
        }
    }
    return parent;
}

void BPTree::remove(int x) {
  if (root == NULL) {
    cout << "Tree empty\n";
  } else {
    Node *cursor = root;
    Node *parent;
    int leftSibling, rightSibling;
    // shuffling thru all the NodeBlock to find the key(x) 
    while (cursor->IS_LEAF == false) {
      // Look for the key by comparing all the level of NodeBlock  
      for (int i = 0; i < cursor->size; i++) {
        parent = cursor;
        // set which index is left NodeBlock ptr at  
        leftSibling = i - 1;
        // set which index is right NodeBlock ptr at  
        rightSibling = i + 1;
        // Checking of key, if less than
        // set cursor Nodeblock to the respective pointers  
        if (x < cursor->key[i]) {
          cursor = cursor->ptr[i];
          break;
        }
        // Reached the end of the NodeBlock key  
        if (i == cursor->size - 1) {
          leftSibling = i;
          rightSibling = i + 2;
          cursor = cursor->ptr[i + 1];
          break;
        }
      }
    }

      
    bool found = false;
    int pos;
    // The while loop will update the cursor to the correct NodeBlock
    // where is key is stored, then iterate thru to find where the key  
    for (pos = 0; pos < cursor->size; pos++) {
      // self explanatory   
      if (cursor->key[pos] == x) {
        found = true;
        break;
      }
    }
    // self explanatory   
    if (!found) {
      cout << "Not found\n";
      return;
    }
    // Move the keys to the left   
    for (int i = pos; i < cursor->size; i++) {
      cursor->key[i] = cursor->key[i + 1];
    }
    // Decrement the capacity
    cursor->size--;
    // if cursor is root, aka only one key inside
    // delete 
    if (cursor == root) {
      for (int i = 0; i < MAX + 1; i++) {
        cursor->ptr[i] = NULL;
      }
      if (cursor->size == 0) {
        cout << "Tree died\n";
        delete[] cursor->key;
        delete[] cursor->ptr;
        delete cursor;
        root = NULL;
      }
      return;
    }
    // Move the cursor pointer to the left
    cursor->ptr[cursor->size] = cursor->ptr[cursor->size + 1];
    // set next available pointer to null 
    cursor->ptr[cursor->size + 1] = NULL;
    // check if the leaf NodeBlock number of keys fits the formula of n+1/2
    // if is not, need to do sharing of sibling keys 
    if (cursor->size >= (MAX + 1) / 2) {
      return;
    }
    // Sharing of sibiling keys
    // read line 332 to understand the if statement 
    if (leftSibling >= 0) {
      Node *leftNode = parent->ptr[leftSibling];
      // check if leaf node fulfill n/2 + 1 formula   
      if (leftNode->size >= (MAX + 1) / 2 + 1) {
        // if yes, move all the keys to the right first   
        for (int i = cursor->size; i > 0; i--) {
          cursor->key[i] = cursor->key[i - 1];
        }
        cursor->size++;
        cursor->ptr[cursor->size] = cursor->ptr[cursor->size - 1];
        cursor->ptr[cursor->size - 1] = NULL;
        cursor->key[0] = leftNode->key[leftNode->size - 1];
        leftNode->size--;
        leftNode->ptr[leftNode->size] = cursor;
        leftNode->ptr[leftNode->size + 1] = NULL;
        parent->key[leftSibling] = cursor->key[0];
        return;
      }
    }
    if (rightSibling <= parent->size) {
      Node *rightNode = parent->ptr[rightSibling];
      if (rightNode->size >= (MAX + 1) / 2 + 1) {
        cursor->size++;
        cursor->ptr[cursor->size] = cursor->ptr[cursor->size - 1];
        cursor->ptr[cursor->size - 1] = NULL;
        cursor->key[cursor->size - 1] = rightNode->key[0];
        rightNode->size--;
        rightNode->ptr[rightNode->size] = rightNode->ptr[rightNode->size + 1];
        rightNode->ptr[rightNode->size + 1] = NULL;
        for (int i = 0; i < rightNode->size; i++) {
          rightNode->key[i] = rightNode->key[i + 1];
        }
        parent->key[rightSibling - 1] = rightNode->key[0];
        return;
      }
    }
    if (leftSibling >= 0) {
      Node *leftNode = parent->ptr[leftSibling];
      for (int i = leftNode->size, j = 0; j < cursor->size; i++, j++) {
        leftNode->key[i] = cursor->key[j];
      }
      leftNode->ptr[leftNode->size] = NULL;
      leftNode->size += cursor->size;
      leftNode->ptr[leftNode->size] = cursor->ptr[cursor->size];
      removeInternal(parent->key[leftSibling], parent, cursor);
      delete[] cursor->key;
      delete[] cursor->ptr;
      delete cursor;
    } else if (rightSibling <= parent->size) {
      Node *rightNode = parent->ptr[rightSibling];
      for (int i = cursor->size, j = 0; j < rightNode->size; i++, j++) {
        cursor->key[i] = rightNode->key[j];
      }
      cursor->ptr[cursor->size] = NULL;
      cursor->size += rightNode->size;
      cursor->ptr[cursor->size] = rightNode->ptr[rightNode->size];
      cout << "Merging two leaf nodes\n";
      removeInternal(parent->key[rightSibling - 1], parent, rightNode);
      delete[] rightNode->key;
      delete[] rightNode->ptr;
      delete rightNode;
    }
  }
}
void BPTree::removeInternal(int x, Node *cursor, Node *child) {
  if (cursor == root) {
    if (cursor->size == 1) {
      if (cursor->ptr[1] == child) {
        delete[] child->key;
        delete[] child->ptr;
        delete child;
        root = cursor->ptr[0];
        delete[] cursor->key;
        delete[] cursor->ptr;
        delete cursor;
        cout << "Changed root node\n";
        return;
      } else if (cursor->ptr[0] == child) {
        delete[] child->key;
        delete[] child->ptr;
        delete child;
        root = cursor->ptr[1];
        delete[] cursor->key;
        delete[] cursor->ptr;
        delete cursor;
        cout << "Changed root node\n";
        return;
      }
    }
  }
  int pos;
  for (pos = 0; pos < cursor->size; pos++) {
    if (cursor->key[pos] == x) {
      break;
    }
  }
  for (int i = pos; i < cursor->size; i++) {
    cursor->key[i] = cursor->key[i + 1];
  }
  for (pos = 0; pos < cursor->size + 1; pos++) {
    if (cursor->ptr[pos] == child) {
      break;
    }
  }
  for (int i = pos; i < cursor->size + 1; i++) {
    cursor->ptr[i] = cursor->ptr[i + 1];
  }
  cursor->size--;
  if (cursor->size >= (MAX + 1) / 2 - 1) {
    return;
  }
  if (cursor == root)
    return;
  Node *parent = findParent(root, cursor);
  int leftSibling, rightSibling;
  for (pos = 0; pos < parent->size + 1; pos++) {
    if (parent->ptr[pos] == cursor) {
      leftSibling = pos - 1;
      rightSibling = pos + 1;
      break;
    }
  }
  if (leftSibling >= 0) {
    Node *leftNode = parent->ptr[leftSibling];
    if (leftNode->size >= (MAX + 1) / 2) {
      for (int i = cursor->size; i > 0; i--) {
        cursor->key[i] = cursor->key[i - 1];
      }
      cursor->key[0] = parent->key[leftSibling];
      parent->key[leftSibling] = leftNode->key[leftNode->size - 1];
      for (int i = cursor->size + 1; i > 0; i--) {
        cursor->ptr[i] = cursor->ptr[i - 1];
      }
      cursor->ptr[0] = leftNode->ptr[leftNode->size];
      cursor->size++;
      leftNode->size--;
      return;
    }
  }
  if (rightSibling <= parent->size) {
    Node *rightNode = parent->ptr[rightSibling];
    if (rightNode->size >= (MAX + 1) / 2) {
      cursor->key[cursor->size] = parent->key[pos];
      parent->key[pos] = rightNode->key[0];
      for (int i = 0; i < rightNode->size - 1; i++) {
        rightNode->key[i] = rightNode->key[i + 1];
      }
      cursor->ptr[cursor->size + 1] = rightNode->ptr[0];
      for (int i = 0; i < rightNode->size; ++i) {
        rightNode->ptr[i] = rightNode->ptr[i + 1];
      }
      cursor->size++;
      rightNode->size--;
      return;
    }
  }
  if (leftSibling >= 0) {
    Node *leftNode = parent->ptr[leftSibling];
    leftNode->key[leftNode->size] = parent->key[leftSibling];
    for (int i = leftNode->size + 1, j = 0; j < cursor->size; j++) {
      leftNode->key[i] = cursor->key[j];
    }
    for (int i = leftNode->size + 1, j = 0; j < cursor->size + 1; j++) {
      leftNode->ptr[i] = cursor->ptr[j];
      cursor->ptr[j] = NULL;
    }
    leftNode->size += cursor->size + 1;
    cursor->size = 0;
    removeInternal(parent->key[leftSibling], parent, cursor);
  } else if (rightSibling <= parent->size) {
    Node *rightNode = parent->ptr[rightSibling];
    cursor->key[cursor->size] = parent->key[rightSibling - 1];
    for (int i = cursor->size + 1, j = 0; j < rightNode->size; j++) {
      cursor->key[i] = rightNode->key[j];
    }
    for (int i = cursor->size + 1, j = 0; j < rightNode->size + 1; j++) {
      cursor->ptr[i] = rightNode->ptr[j];
      rightNode->ptr[j] = NULL;
    }
    cursor->size += rightNode->size + 1;
    rightNode->size = 0;
    removeInternal(parent->key[rightSibling - 1], parent, rightNode);
  }
}