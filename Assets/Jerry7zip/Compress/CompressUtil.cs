using System;

public class CompressUtil
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

    static public string Bytes2String(byte[] msg)
    {
        return BitConverter.ToString(msg).Replace("-", "");
    }

    static public byte[] String2Bytes(string msg)
    {
        msg = msg.Trim();
        if (string.IsNullOrEmpty(msg))
        {
            return null;
        }
        int len = msg.Length;
        if (len % 2 == 1)
        {
            return null;
        }
        byte[] data = new byte[len / 2];
        for (int i = 0, j = 0; i < len; i += 2, j++)
        {
            data[j] = (byte)(CharToHex(msg[i]) * 16 + CharToHex(msg[i + 1]));
        }
        return data;
    }

    static private byte CharToHex(char ch)
    {
        if (ch >= '0' && ch <= '9')
        {
            return (byte)(ch - '0');
        }
        else if (ch >= 'A' && ch <= 'Z')
        {
            return (byte)(ch - 'A' + 10);
        }
        return 0;
    }
}

public class CompressConfig
{
    public string inFile;
    public string outFile;
    public long inFileSize = 0;
}

public enum CompressState
{
    None = 0,
    Working,
    Finish,
    Error,
}

public delegate void CompressCallback(long finishSize, long totalSize, CompressState status);