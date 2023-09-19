

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