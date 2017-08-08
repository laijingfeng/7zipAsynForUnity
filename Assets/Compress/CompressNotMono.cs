using System;
using System.IO;
using System.Threading;
using SevenZip.Compression.LZMA;
using UnityEngine;

/// <summary>
/// 文件压缩逻辑
/// </summary>
public class CompressNotMono
{
    private CompressConfig config;
    private Thread thread = null;
    private Encoder coder = null;
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
            return coder.NowPos64;
        }
    }

    public long TotalSize
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

    public CompressNotMono(CompressConfig config, Action callback = null)
    {
        this.config = config;
        this.finishCallback = callback;
        if (config.inFileSize == 0
            && File.Exists(this.config.inFile))
        {
            FileStream input = new FileStream(this.config.inFile, FileMode.Open);
            config.inFileSize = input.Length;
            input.Close();
            input.Dispose();
        }
    }

    public void StartCompress()
    {
        status = CompressState.Working;
        thread = new Thread(new ThreadStart(DoCompress));
        thread.IsBackground = true;
        thread.Start();
    }

    /// <summary>
    /// 使用LZMA算法压缩文件  
    /// </summary>
    private void DoCompress()
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

            coder = new Encoder();
            coder.WriteCoderProperties(output);
            
            byte[] data = BitConverter.GetBytes(input.Length);

            output.Write(data, 0, data.Length);

            coder.Code(input, output, input.Length, -1, null);
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