using System;
using System.IO;
using SevenZip.Compression.LZMA;
using UnityEngine;

/// <summary>
/// 文件压缩逻辑
/// </summary>
public static class Compress
{
    /// <summary>
    /// 打包后的文件后缀名
    /// </summary>
    public const string EXTENSION = ".zip";

    /// <summary>
    /// 是否是压缩包
    /// </summary>
    public static bool IsCompressFile(string file_name)
    {
        return file_name.Contains(EXTENSION);
    }

    /// <summary>
    /// 获得文件的压缩包名
    /// </summary>
    public static string GetCompressFileName(string file_name)
    {
        return file_name + EXTENSION;
    }

    /// <summary>
    /// 获得默认文件名
    /// </summary>
    public static string GetDefaultFileName(string compress_file_name)
    {
        return compress_file_name.Replace(EXTENSION, "");
    }

    /// <summary>
    /// 压缩文件
    /// </summary>
    public static bool CompressFile(string in_file, string out_file = null, CodeProgress progress = null)
    {
        if (out_file == null)
        {
            out_file = GetCompressFileName(in_file);
        }
        return CompressFileLZMA(in_file, out_file, progress);
    }

    /// <summary>
    /// 解压文件
    /// </summary>
    public static bool DecompressFile(string in_file, string out_file = null, CodeProgress progress = null)
    {
        if (out_file == null)
        {
            out_file = GetDefaultFileName(in_file);
        }

        return DecompressFileLZMA(in_file, out_file, progress);
    }

    /// <summary>
    /// 使用LZMA算法压缩文件  
    /// </summary>
    static bool CompressFileLZMA(string inFile, string outFile, CodeProgress progress = null)
    {
        try
        {
            if (!File.Exists(inFile))
            {
                return false;
            }
            FileStream input = new FileStream(inFile, FileMode.Open);
            FileStream output = new FileStream(outFile, FileMode.OpenOrCreate);

            Encoder coder = new Encoder();
            coder.WriteCoderProperties(output);

            byte[] data = BitConverter.GetBytes(input.Length);

            output.Write(data, 0, data.Length);

            coder.Code(input, output, input.Length, -1, progress);
            output.Flush();
            output.Close();
            input.Close();

            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError(ex.Message);
        }

        return false;
    }

    /// <summary>
    /// 使用LZMA算法解压文件  
    /// </summary>
    static bool DecompressFileLZMA(string inFile, string outFile, CodeProgress progress = null)
    {
        try
        {
            if (!File.Exists(inFile))
            {
                return false;
            }
            FileStream input = new FileStream(inFile, FileMode.Open);
            FileStream output = new FileStream(outFile, FileMode.OpenOrCreate);

            byte[] properties = new byte[5];
            input.Read(properties, 0, 5);

            byte[] fileLengthBytes = new byte[8];
            input.Read(fileLengthBytes, 0, 8);
            long fileLength = BitConverter.ToInt64(fileLengthBytes, 0);

            Decoder coder = new Decoder();
            coder.SetDecoderProperties(properties);
            coder.Code(input, output, input.Length, fileLength, progress);
            output.Flush();
            output.Close();
            input.Close();
        }
        catch (System.Exception ex)
        {
            Debug.LogError(ex.Message);
        }

        return false;
    }
}

public class CodeProgress : SevenZip.ICodeProgress
{
    public delegate void ProgressDelegate(Int64 inSize, Int64 outSize);

    public ProgressDelegate m_ProgressDelegate = null;

    public CodeProgress(ProgressDelegate del)
    {
        m_ProgressDelegate = del;
    }

    public void SetProgress(Int64 inSize, Int64 outSize)
    {
        m_ProgressDelegate(inSize, outSize);
    }
}