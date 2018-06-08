using Jerry;
using SevenZip.Compression.LZMA;
using System;
using System.Collections;
using System.IO;
using System.Threading;
using UnityEngine;

/// <summary>
/// 二进制压缩
/// </summary>
public class CompressBytesMono : SingletonMono<CompressBytesMono>
{
    #region Compress

    private bool compressBytesLZMAFinish = true;
    private Encoder coder = null;
    private byte[] inBytes;
    private byte[] outBytes;

    public void CompressBytes(byte[] in_bytes, Action<Int64, Int64> progress = null, Action<byte[]> finish = null)
    {
        compressBytesLZMAFinish = false;
        coder = null;
        inBytes = in_bytes;
        outBytes = null;

        Thread decompressThread = new Thread(new ThreadStart(DoCompressBytesLZMA));
        decompressThread.Start();

        if (progress != null || finish != null)
        {
            this.StartCoroutine(IE_WaitCompressBytesLZMA(progress, finish));
        }
    }

    private IEnumerator IE_WaitCompressBytesLZMA(Action<Int64, Int64> progress, Action<byte[]> finish)
    {
        if (progress != null)
        {
            while (!compressBytesLZMAFinish)
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
            yield return new WaitUntil(() => compressBytesLZMAFinish == true);
        }

        if (finish != null)
        {
            finish(outBytes);
        }

        inBytes = null;
        outBytes = null;
    }

    private void DoCompressBytesLZMA()
    {
        try
        {
            MemoryStream input = new MemoryStream(inBytes);
            MemoryStream output = new MemoryStream();
            
            coder = new Encoder();
            coder.WriteCoderProperties(output);

            byte[] data = BitConverter.GetBytes(input.Length);

            output.Write(data, 0, data.Length);

            coder.Code(input, output, input.Length, -1, null);
            output.Flush();

            outBytes = output.ToArray();

            output.Close();
            input.Close();
        }
        catch (System.Exception ex)
        {
            Debug.LogError(ex.Message + "\n" + ex.StackTrace);
        }
        compressBytesLZMAFinish = true;
    }
    
    #endregion Compress

    #region Decompress

    private bool decompressBytesLZMAFinish = true;
    private Decoder deCoder = null;
    private byte[] deInBytes;
    private byte[] deOutBytes;

    public void DecompressBytesLZMA(byte[] in_bytes, string outFile, Action<UInt64, UInt64> progress = null, Action<byte[]> finish = null)
    {
        decompressBytesLZMAFinish = false;
        deCoder = null;
        deInBytes = in_bytes;
        deOutBytes = null;

        Thread decompressThread = new Thread(new ThreadStart(DoDecompressFileLZMA));
        decompressThread.Start();

        if (progress != null || finish != null)
        {
            this.StartCoroutine(IE_WaitDecompressBytesLZMA(progress, finish));
        }
    }

    private IEnumerator IE_WaitDecompressBytesLZMA(Action<UInt64, UInt64> progress, Action<byte[]> finish)
    {
        if (progress != null)
        {
            while (!decompressBytesLZMAFinish)
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
            yield return new WaitUntil(() => decompressBytesLZMAFinish == true);
        }

        if (finish != null)
        {
            finish(deOutBytes);
        }

        deInBytes = null;
        deOutBytes = null;
    }

    private void DoDecompressFileLZMA()
    {
        try
        {
            MemoryStream input = new MemoryStream(deInBytes);
            MemoryStream output = new MemoryStream();

            byte[] properties = new byte[5];
            input.Read(properties, 0, 5);

            byte[] fileLengthBytes = new byte[8];
            input.Read(fileLengthBytes, 0, 8);
            long fileLength = BitConverter.ToInt64(fileLengthBytes, 0);

            deCoder = new Decoder();
            deCoder.SetDecoderProperties(properties);
            deCoder.Code(input, output, input.Length, fileLength, null);

            output.Flush();
            
            deOutBytes = output.ToArray();

            output.Close();
            input.Close();
        }
        catch (System.Exception ex)
        {
            Debug.LogError(ex.Message + "\n" + ex.StackTrace);
        }
        decompressBytesLZMAFinish = true;
    }

    #endregion Decompress
}