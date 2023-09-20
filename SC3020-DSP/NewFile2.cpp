

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
        Nodeparent;

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

            //
            cursor->size = (MAX + 1) / 2;
            newLeaf->size = MAX + 1 - (MAX + 1) / 2;
            cursor->ptr[cursor->size] = newLeaf;
            newLeaf->ptr[newLeaf->size] = cursor->ptr[MAX];
            cursor->ptr[MAX] = NULL;

            
            for (i = 0; i < cursor->size; i++) {
                cursor->key[i] = virtualNode[i];
            }
            for (i = 0, j = cursor->size; i < newLeaf->size; i++, j++) {
                newLeaf->key[i] = virtualNode[j];
            }
            if (cursor == root) {
                Node *newRoot = new Node;
                newRoot->key[0] = newLeaf->key[0];
                newRoot->ptr[0] = cursor;
                newRoot->ptr[1] = newLeaf;
                newRoot->IS_LEAF = false;
                newRoot->size = 1;
                root = newRoot;
            } else {
                insertInternal(newLeaf->key[0], parent, newLeaf);
            }
        }
    }
}