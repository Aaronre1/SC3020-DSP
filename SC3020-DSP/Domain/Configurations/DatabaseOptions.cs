namespace SC3020_DSP.Domain.Configurations;

public class DatabaseOptions
{
    public long RecordSizeInBytes { get; set; } = 73;

    public long BlockSizeInBytes { get; set; } = 400;

    public long DiskCapacityInBytes { get; set; } = 500 * 1024 * 1024;
    

}