
public class MultiCompress : MultiCompressBase<CompressNotMono>
{
    public MultiCompress()
        : base()
    {
    }

    public override CompressConfig CalInFileSize(CompressConfig config)
    {
        return CompressNotMono.CalInFileSize(config);
    }
}