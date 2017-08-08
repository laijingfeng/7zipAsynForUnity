using UnityEngine;
using System.Collections;

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
}