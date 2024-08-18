using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.NamingConventionBinder;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

class SyncFolder
{
    private static CancellationTokenSource cts = new CancellationTokenSource();
    static async Task<int> Main(string[] args)
    {
        var rootCommand = new RootCommand
        {
            new Argument<string>("source", "The source folder e.g c:/path/to/source"),
            new Argument<string>("replica", "Replica folder path e.g c:/path/to/replica"),
            new Argument<int>("interval", "Sync interval in seconds e.g 60"),
            new Argument<string>("logFile", "Log file path e.g c:/path/to/logfile.log")
        };
        
        rootCommand.Description = "Synchronize two folders.";
        
        // Register command handler
        rootCommand.Handler = CommandHandler.Create<string, string, int, string>(SyncFolders);

		// Graceful shutdown
        Console.CancelKeyPress += (sender, e) =>
        {
            e.Cancel = true; // Prevent the process from terminating.
            cts.Cancel(); // Signal cancellation to the sync loop.
        };

        return await rootCommand.InvokeAsync(args);
    }

     /// <summary>
    /// Synchronizes the source folder with the replica folder.
    /// </summary>
    static void Sync(string source, string replica, string logFile)
    {
        var sourceDir = new DirectoryInfo(source);
        var replicaDir = new DirectoryInfo(replica);

        // logs when there are no directories
        if (!sourceDir.Exists)
        {
            Log($"Source dir does not exist: {source}", logFile);
            return;
        }
        
        if (!replicaDir.Exists)
        {
            Directory.CreateDirectory(replica);
            Log($"Created directory {replica}", logFile);
            return;
        }
        
        // sync from source to replica
        foreach (var file in sourceDir.GetFiles("*", SearchOption.AllDirectories))
        {
            var relativePath = Path.GetRelativePath(source, file.FullName);
            var replicaFilePath = Path.Combine(replica, relativePath);
            var replicaFile = new FileInfo(replicaFilePath);

            if (!replicaFile.Exists || !FilesAreEqual(file, replicaFile))
            {
                Directory.CreateDirectory(replicaFile.DirectoryName);
                file.CopyTo(replicaFile.FullName, true);
                Log($"Copied file {file.FullName} to {replicaFile.FullName}", logFile);
            }
        }
        
        // remove files that are not in the source destination
        foreach(var file in replicaDir.GetFiles("*", SearchOption.AllDirectories))
        {
            var relativePath = Path.GetRelativePath(replica, file.FullName);
            var sourceFilePath = Path.Combine(source, relativePath);

            if (!File.Exists(sourceFilePath))
            {
                file.Delete();
                Log($"Removed file {file.FullName}", logFile);
            }  
        }

        // remove directories that are not in the source destination
        foreach (var dir in replicaDir.GetDirectories("*",SearchOption.AllDirectories))
        {
            var relativePath = Path.GetRelativePath(replica, dir.FullName);
            var sourceDirPath = Path.Combine(source, relativePath);

            if (!Directory.Exists(sourceDirPath))
            {
                dir.Delete(true);
                Log($"Removed directory {dir.FullName}", logFile);
            }
        }
    }

    static bool FilesAreEqual(FileInfo first, FileInfo second)
    {
        if (first.Length != second.Length)
            return false;

        const int BYTES_TO_READ = sizeof(long);
        
        using (FileStream firstStream = first.OpenRead())
        using (FileStream secondStream = second.OpenRead())
        {
            byte[] one = new Byte[BYTES_TO_READ];
            byte[] two = new Byte[BYTES_TO_READ];

            while (firstStream.Read(one, 0, BYTES_TO_READ) > 0)
            {
                secondStream.Read(two, 0, BYTES_TO_READ);
                if (BitConverter.ToInt64(one, 0) != BitConverter.ToInt64(two, 0) )
                    return false;
            }
        }
        return true;
    }

    static void Log(string message, string logFile)
    {
        var logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}";
        Console.WriteLine(logMessage);
        File.AppendAllText(logFile, logMessage + Environment.NewLine);
    }
    
    static void SyncFolders(string source, string replica, int interval, string logFile)
    {
        while (!cts.Token.IsCancellationRequested)
        {
            try
            {
                Sync(source, replica, logFile);

            }
            catch (Exception exception)
            {
                Log($"Error: {exception.Message}", logFile);
            }
            
            Thread.Sleep(interval * 1000);
        }
    }
}