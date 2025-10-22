using System;
using System.IO;
using UnityEngine;

public static class LogPathUtil
{
    public static readonly string Root =
        Path.Combine(Application.persistentDataPath, "Logs");

    public static string NewSessionId()
    {
        var ts = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        var rand = UnityEngine.Random.Range(0, 0xFFFFFF);
        return $"{ts}_{rand:X6}";
    }

    public static string GetUniquePath(string dir, string baseNameNoExt, string ext = ".jsonl")
    {
        Directory.CreateDirectory(dir);
        string path = Path.Combine(dir, baseNameNoExt + ext);
        if (!File.Exists(path)) return path;

        for (int i = 1; i < 1000; i++)
        {
            string p = Path.Combine(dir, $"{baseNameNoExt}_{i:D3}{ext}");
            if (!File.Exists(p)) return p;
        }
        return Path.Combine(dir, $"{baseNameNoExt}_{Guid.NewGuid():N}{ext}");
    }
}