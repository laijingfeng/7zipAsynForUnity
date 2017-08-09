
public class MultiDecompress : MultiCompressBase<DecompressNotMono>
{
    public MultiDecompress()
        : base()
    {
    }

    public override CompressConfig CalInFileSize(CompressConfig config)
    {
        return DecompressNotMono.CalInFileSize(config);
    }
}