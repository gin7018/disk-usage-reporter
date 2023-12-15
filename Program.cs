// @author Ghislaine Nyagatare

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


/// <summary>
/// The DiskUsageReporter reports the disk usage
/// of a directory in a parallel or sequential manner (or both)
/// </summary>
public class DiskUsageReporter
{
    private static string PARALLEL = "-p";
    private static string SEQUENTIAL = "-s";
    private static string BOTH = "-b";
    private static string[] cases = {PARALLEL, SEQUENTIAL, BOTH};

    private DiskUsageReporter(string path, string scenario)
    {
        var pathError = false;
        try
        {
            var files = Directory.GetFiles(path);
        }
        catch (DirectoryNotFoundException)
        {
            PrintHelpMessage();
            pathError = true;
        }

        if (pathError) return;
        if (scenario.Equals(PARALLEL) || scenario.Equals(SEQUENTIAL))
        {
            GetDirectoryInfo(path, scenario);
        }
        else if (scenario.Equals(BOTH))
        {
            GetDirectoryInfo(path, PARALLEL);
            GetDirectoryInfo(path, SEQUENTIAL);
        }
    }

    private void RunParallel(List<string> files, long dirCount)
    {
        var fileCount = 0L;
        var usage = 0L;
        var sw = new Stopwatch();
        sw.Start();
        Parallel.ForEach(files, file =>
        {
            try
            {
                Interlocked.Add(ref usage, new FileInfo(file).Length);
                Interlocked.Increment(ref fileCount);
            }
            catch (FileNotFoundException)
            {
            }
        });
        sw.Stop();
        Console.WriteLine();
        Console.WriteLine("Parallel Calculated in: {0}s ", sw.ElapsedMilliseconds/1000);
        Console.WriteLine("{0:N0} folders, {1:N0} files, {2:N0} bytes", dirCount, fileCount, usage);
    }
    
    private void RunSequential(List<string> files, long dirCount)
    {
        var fileCount = 0L;
        var usage = 0L;
        var sw = new Stopwatch();
        sw.Start();
        foreach (string file in files)
        {
            try
            {
                var info = new FileInfo(file);
                usage += info.Length;
                fileCount++;
            }
            catch (FileNotFoundException)
            {
                continue;
            }
        }
        sw.Stop();
        Console.WriteLine();
        Console.WriteLine("Sequential Calculated in: {0}s ", sw.ElapsedMilliseconds/1000);
        Console.WriteLine("{0:N0} folders, {1:N0} files, {2:N0} bytes", dirCount, fileCount, usage);
        
    }

    private static void PrintHelpMessage()
    {
        const string help = "Usage: du [-s] [-p] [-b] <path> \n" +
                            "Summarize disk usage of the set of FILES, recursively for directories.\n\n" + 
                            "You MUST specify one of the parameters, -s, -p, or -b \n" +
                            "-s \t Run in single threaded mode\n" + 
                            "-p \t Run in parallel mode (uses all available processors)\n" + 
                            "-b \t Run in both parallel and single threaded mode.\n " +
                            "\t Runs parallel followed by sequential mode\n";
        Console.WriteLine(help);
    }

    
    /// <summary>
    /// Recursively gets all the files in a directory (and all its sub-directories)
    /// and adds up the disk usage of those files either in a sequential or a parallel manner
    /// </summary>
    /// <param name="path">the path to the directory</param>
    /// <param name="scenario">sequential or parallel flag</param>
    public void GetDirectoryInfo(string path, string scenario)
    {
        long dirCount = 0L;
        Stack<string> directoriesToTraverse = new Stack<string>();
        directoriesToTraverse.Push(path);
        List<string> bigFileTable = new List<string>();

        while (directoriesToTraverse.Count > 0)
        {
            string currentDir = directoriesToTraverse.Pop();
            string[] subDirectories = {};

            // try to get the sub directories and their count
            try
            {
                subDirectories = Directory.GetDirectories(currentDir);
                dirCount += subDirectories.Length;
            }
            catch (UnauthorizedAccessException)
            {
                continue;
            }
            
            // try to get the directories files
            try
            {
                bigFileTable.AddRange(Directory.GetFiles(currentDir));
            }
            catch (UnauthorizedAccessException)
            {
                continue;
            }
            
            foreach (var things in subDirectories)
            {
                directoriesToTraverse.Push(things);
            }
        }
        
        if (scenario.Equals(SEQUENTIAL))
        {
            RunSequential(bigFileTable, dirCount);
        }
        else if (scenario.Equals(PARALLEL))
        {
            RunParallel(bigFileTable, dirCount);
        }
    }
    
    public static void Main(string[] args)
    {
        if (args.Length != 2)
        {
            PrintHelpMessage();
        }
        else
        {
            var scenario = args[0];
            var path = args[1];
            if (!cases.Contains(scenario))
            {
                PrintHelpMessage();
            }
            else
            {
                Console.WriteLine("Directory \'{0}\':", path);
                var report = new DiskUsageReporter(path, scenario);
            }
        }
    }
}
