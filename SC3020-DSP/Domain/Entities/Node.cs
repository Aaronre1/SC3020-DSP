namespace SC3020_DSP.Domain.Entities;

public class Node
{
    public Pointer Pointer { get; set; }
    public string Key { get; set; }

    public Node(Pointer pointer, string key)
    {
        Pointer = pointer;
        Key = key;
    }
}