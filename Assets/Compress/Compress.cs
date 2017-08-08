using System;
using System.Collections;
using System.IO;
using System.Threading;
using Jerry;
using SevenZip.Compression.LZMA;
using UnityEngine;

/// <summary>
/// 文件压缩逻辑
/// </summary>
public class Compress : SingletonMono<Compress>
{
    private bool compressFileLZMAFinish = true;
    private Encoder coder = null;
    private string inFile;
    private string outFile;

    /// <summary>
    /// 压缩文件
    /// </summary>
    public void CompressFile(string in_file, string out_file = null, Action<Int64, Int64> progress = null, Action<bool> finish = null)
    {
        if (out_file == null)
        {
            out_file = CompressUtil.GetCompressFileName(in_file);
        }
        compressFileLZMAFinish = false;
        coder = null;
        inFile = in_file;
        outFile = out_file;

        Thread decompressThread = new Thread(new ThreadStart(DoCompressFileLZMA));
        decompressThread.Start();

        if (progress != null || finish != null)
        {
            this.StartCoroutine(IE_WaitCompressFileLZMA(progress, finish));
        }
    }

    private IEnumerator IE_WaitCompressFileLZMA(Action<Int64, Int64> progress, Action<bool> finish)
    {
        if (progress != null)
        {
            while (!compressFileLZMAFinish)
            {
                if (coder != null)
                {
                    progress(coder.NowPos64, coder.TargetPos64);
                }
                yield return new WaitForEndOfFrame();
            }
            if (coder != null)
            {
                progress(coder.NowPos64, coder.TargetPos64);
            }
        }
        else
        {
            yield return new WaitUntil(() => compressFileLZMAFinish == true);
        }

        if (finish != null)
        {
            //if (coder == null || coder.NowPos64 < coder.TargetPos64)
            //{
            //    finish(false);
            //}
            //else
            {
                finish(true);
            }
        }
    }

    /// <summary>
    /// 使用LZMA算法压缩文件  
    /// </summary>
    private void DoCompressFileLZMA()
    {
        try
        {
            if (!File.Exists(inFile))
            {
                compressFileLZMAFinish = true;
                return;
            }
            FileStream input = new FileStream(inFile, FileMode.Open);
            FileStream output = new FileStream(outFile, FileMode.OpenOrCreate);

            coder = new Encoder();
            coder.WriteCoderProperties(output);

            byte[] data = BitConverter.GetBytes(input.Length);

            output.Write(data, 0, data.Length);

            Debug.LogWarning(input.Length + " ");
            
            coder.Code(input, output, input.Length, -1, null);
            output.Flush();
            output.Close();
            input.Close();
        }
        catch (System.Exception ex)
        {
            Debug.LogError(ex.Message);
        }
        compressFileLZMAFinish = true;
    }

    private bool decompressFileLZMAFinish = true;
    private Decoder deCoder = null;
    private string deInFile;
    private string deOutFile;

    public void DecompressFileLZMA(string inFile, string outFile, Action<UInt64, UInt64> progress = null, Action<bool> finish = null)
    {
        if (outFile == null)
        {
            outFile = CompressUtil.GetDefaultFileName(inFile);
        }
        decompressFileLZMAFinish = false;
        deCoder = null;
        deInFile = inFile;
        deOutFile = outFile;

        Thread decompressThread = new Thread(new ThreadStart(DoDecompressFileLZMA));
        decompressThread.Start();

        if (progress != null || finish != null)
        {
            this.StartCoroutine(IE_WaitDecompressFileLZMA(progress, finish));
        }
    }

    private IEnumerator IE_WaitDecompressFileLZMA(Action<UInt64, UInt64> progress, Action<bool> finish)
    {
        if (progress != null)
        {
            while (!decompressFileLZMAFinish)
            {
                if (deCoder != null)
                {
                    progress(deCoder.NowPos64, deCoder.TargetPos64);
                }
                yield return new WaitForEndOfFrame();
            }
            if (deCoder != null)
            {
                progress(deCoder.NowPos64, deCoder.TargetPos64);
            }
        }
        else
        {
            yield return new WaitUntil(() => decompressFileLZMAFinish == true);
        }

        if (finish != null)
        {
            if (deCoder == null || deCoder.NowPos64 < deCoder.TargetPos64)
            {
                finish(false);
            }
            else
            {
                finish(true);
            }
        }
    }

    private void DoDecompressFileLZMA()
    {
        try
        {
            if (!File.Exists(deInFile))
            {
                decompressFileLZMAFinish = true;
                return;
            }
            FileStream input = new FileStream(deInFile, FileMode.Open);
            FileStream output = new FileStream(deOutFile, FileMode.OpenOrCreate);

            byte[] properties = new byte[5];
            input.Read(properties, 0, 5);

            byte[] fileLengthBytes = new byte[8];
            input.Read(fileLengthBytes, 0, 8);
            long fileLength = BitConverter.ToInt64(fileLengthBytes, 0);

            deCoder = new Decoder();
            deCoder.SetDecoderProperties(properties);
            Debug.LogWarning(input.Length + " " + fileLength);
            deCoder.Code(input, output, input.Length, fileLength, null);

            output.Flush();
            output.Close();
            input.Close();
        }
        catch (System.Exception ex)
        {
            Debug.LogError(ex.Message);
        }
        decompressFileLZMAFinish = true;
    }
}