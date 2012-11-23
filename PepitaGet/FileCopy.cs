using System;
using System.IO;

public static class FileCopy
{
    public static void Copy(string from, string to, out bool didCopy)
    {
        
        if (File.Exists(to))
        {
            didCopy = false;
            return;
        }
        try
        {
            File.Copy(from, to, true);
        }
        catch (Exception)
        {
            if (File.Exists(to))
            {
                didCopy = false;
                return;
            }
            File.Copy(from, to, true);
        } 
        didCopy = true;

    }
    public static void Copy(string from, string to)
    {
        if (File.Exists(to))
        {
            return;
        }
        try
        {
            File.Copy(from, to, true);
        }
        catch (Exception)
        {
            if (File.Exists(to))
            {
                return;
            }
            File.Copy(from, to, true);
        }
    }
}