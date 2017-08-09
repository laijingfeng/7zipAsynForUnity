using System;
using System.IO;
using SevenZip.Compression.LZMA;

public class CompressNotMono : CompressNotMonoBase
{
    private Encoder coder = null;

    public override long FinishSize
    {
        get
        {
            if (coder == null)
            {
                return 0;
            }
            return coder.NowPos64;
        }
    }

    public override long TotalSize
    {
        get
        {
            if (config != null)
            {
                return config.inFileSize;
            }
            return 0;
        }
    }

    public CompressNotMono()
        : base()
    {

    }
    
    public static CompressConfig CalInFileSize(CompressConfig config)
    {
        if (config.inFileSize == 0
            && File.Exists(config.inFile))
        {
            FileStream input = new FileStream(config.inFile, FileMode.Open);
            config.inFileSize = input.Length;
            input.Close();
            input.Dispose();
        }
        return config;
    }

    public override void SetConfig(CompressConfig config, Action callback = null)
    {
        base.SetConfig(config, callback);
        this.config = CalInFileSize(this.config);
    }

    protected override void DoWork()
    {
        coder = new Encoder();
        coder.WriteCoderProperties(output);

        byte[] data = BitConverter.GetBytes(input.Length);
        output.Write(data, 0, data.Length);
        coder.Code(input, output, input.Length, -1, null);
    }
}