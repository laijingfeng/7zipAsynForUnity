﻿using System;
using System.IO;
using SevenZip.Compression.LZMA;

public class DecompressNotMono : CompressNotMonoBase
{
    private Decoder coder = null;

    public override long FinishSize
    {
        get
        {
            if (coder == null)
            {
                return 0;
            }
            return (long)coder.NowPos64;
        }
    }

    public override long TotalSize
    {
        get
        {
            if(config != null)
            {
                return config.inFileSize;
            }
            return 0;
        }
    }

    public DecompressNotMono(CompressConfig config, Action callback = null)
        : base(config, callback)
    {
        if (config.inFileSize == 0
            && File.Exists(this.config.inFile))
        {
            FileStream input = new FileStream(this.config.inFile, FileMode.Open);
            byte[] properties = new byte[5];
            input.Read(properties, 0, 5);

            byte[] fileLengthBytes = new byte[8];
            input.Read(fileLengthBytes, 0, 8);

            this.config.inFileSize = BitConverter.ToInt64(fileLengthBytes, 0);
            input.Close();
            input.Dispose();
        }
    }

    protected override void DoWork()
    {
        byte[] properties = new byte[5];
        input.Read(properties, 0, 5);

        byte[] fileLengthBytes = new byte[8];
        input.Read(fileLengthBytes, 0, 8);
        long fileLength = BitConverter.ToInt64(fileLengthBytes, 0);

        coder = new Decoder();
        coder.SetDecoderProperties(properties);
        coder.Code(input, output, input.Length, fileLength, null);
    }
}