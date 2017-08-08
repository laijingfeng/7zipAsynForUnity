using System;
using System.IO;
using System.Threading;
using SevenZip.Compression.LZMA;
using UnityEngine;

/// <summary>
/// 文件压缩逻辑
/// </summary>
public class DecompressNotMono
{
    private CompressConfig config;
    private Thread thread = null;
    private Decoder coder = null;
    private Action finishCallback = null;

    private CompressState status = CompressState.None;
    public CompressState Status
    {
        get
        {
            return status;
        }
    }

    public long FinishSize
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

    public long TotalSize
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
    {
        this.config = config;
        this.finishCallback = callback;
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

    public void StartCompress()
    {
        status = CompressState.Working;
        thread = new Thread(new ThreadStart(DoDecompress));
        thread.IsBackground = true;
        thread.Start();
    }

    /// <summary>
    /// 使用LZMA算法压缩文件  
    /// </summary>
    private void DoDecompress()
    {
        FileStream input = null;
        FileStream output = null;
        try
        {
            if (!File.Exists(this.config.inFile))
            {
                status = CompressState.Error;
                if (finishCallback != null)
                {
                    finishCallback();
                }
                return;
            }

            input = new FileStream(this.config.inFile, FileMode.Open);
            output = new FileStream(this.config.outFile, FileMode.OpenOrCreate);

            byte[] properties = new byte[5];
            input.Read(properties, 0, 5);

            byte[] fileLengthBytes = new byte[8];
            input.Read(fileLengthBytes, 0, 8);
            long fileLength = BitConverter.ToInt64(fileLengthBytes, 0);

            coder = new Decoder();
            coder.SetDecoderProperties(properties);
            coder.Code(input, output, input.Length, fileLength, null);

            output.Flush();
            output.Close();
            output.Dispose();
            input.Close();
            input.Dispose();
        }
        catch (System.Exception ex)
        {
            Debug.LogError(ex.Message);
        }

        if (input != null)
        {
            input.Close();
            input.Dispose();
        }

        if (output != null)
        {
            output.Close();
            output.Dispose();
        }

        status = CompressState.Finish;
        if (finishCallback != null)
        {
            finishCallback();
        }
    }
}