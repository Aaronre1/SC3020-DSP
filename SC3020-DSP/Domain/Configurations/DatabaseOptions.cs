namespace SC3020_DSP.Domain.Configurations;

public class DatabaseOptions
{
    public long RecordSizeInBytes => 74;

    public long PointerSizeInBytes => 8;

    public long BlockSizeInBytes => 400;

    public long DiskCapacityInBytes => 500 * 1024 * 1024;
}