using System;
using System.IO;
using System.Threading;

/// <summary>
/// Description of Logger.
/// </summary>
public class Logger
{
    public Logger()
    {
    }

    public static string prefix = "";

    static Object objLock = new object();
    public static void log(string value)
    {
        value = "[" + DateTime.Now.ToString() + "] - " + value;
        Console.WriteLine(value);
        lock (objLock)
        {
            System.IO.StreamWriter w = new StreamWriter(@"C:\bot\" + prefix + "logger.txt", true);
            w.WriteLine(value);
            w.Close();
            w.Dispose();
            w = null;
        }
    }
    public static void trade(string value)
    {
        value = "[" + DateTime.Now.ToString() + "] - " + value;
        Console.WriteLine(value);
        //lock (objLock)
        //{
        //    System.IO.StreamWriter w = new StreamWriter(@"C:\bot\" + prefix + "trade.txt", true);
        //    w.WriteLine(value);
        //    w.Close();
        //    w.Dispose();
        //    w = null;
        //}
    }
    public static void triangle(string value)
    {
        try
        {
            new Thread(() =>
            {
                _triangle(value);
            }).Start();
        }
        catch { }
    }

    public static void _triangle(string value)
    {
        try
        {
            value = "[" + DateTime.Now.ToString() + "] - " + value;
            Console.WriteLine(value);
            lock (objLock)
            {
                System.IO.StreamWriter w = new StreamWriter(@"C:\bot\" + prefix + "triangle.txt", true);
                w.WriteLine(value);
                w.Close();
                w.Dispose();
                w = null;
            }
        }
        catch { }
    }

        public static void high(string value)
    {
        value = "[" + DateTime.Now.ToString() + "] - " + value;
        Console.WriteLine(value);
        lock (objLock)
        {
            System.IO.StreamWriter w = new StreamWriter(@"C:\bot\high.txt", true);
            w.WriteLine(value);
            w.Close();
            w.Dispose();
            w = null;
        }
    }
}
