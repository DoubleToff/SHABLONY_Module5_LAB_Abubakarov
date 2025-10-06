using System;
using System.IO;
using System.Threading;

public enum LogLevel
{
    INFO = 1,
    WARNING = 2,
    ERROR = 3
}

public sealed class Logger
{
    private static Logger _instance = null;
    private static readonly object _lock = new object();

    private string _logFilePath = "log.txt";
    private LogLevel _currentLevel = LogLevel.INFO;

    private Logger() { }

    public static Logger GetInstance()
    {
        if (_instance == null)
        {
            lock (_lock)
            {
                if (_instance == null)
                {
                    _instance = new Logger();
                }
            }
        }
        return _instance;
    }

    public void SetLogLevel(LogLevel level)
    {
        _currentLevel = level;
        Log($"Log level changed to: {level}", LogLevel.INFO);
    }

    public void SetLogFilePath(string path)
    {
        lock (_lock)
        {
            _logFilePath = path;
            Log($"Log file path changed to: {_logFilePath}", LogLevel.INFO);
        }
    }

    public void Log(string message, LogLevel level)
    {
        if (level < _currentLevel)
            return;

        lock (_lock)
        {
            using (StreamWriter writer = new StreamWriter(_logFilePath, true))
            {
                string logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} [{level}] {message}";
                writer.WriteLine(logMessage);
            }
        }
    }

    public void ReadLogs()
    {
        lock (_lock)
        {
            if (File.Exists(_logFilePath))
            {
                Console.WriteLine("=== LOG CONTENT ===");
                Console.WriteLine(File.ReadAllText(_logFilePath));
                Console.WriteLine("===================");
            }
            else
            {
                Console.WriteLine("Log file not found.");
            }
        }
    }
}

class Program
{
    static void Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Logger logger = Logger.GetInstance();

        logger.SetLogFilePath("app_log.txt");
        logger.SetLogLevel(LogLevel.INFO);

        Thread t1 = new Thread(() =>
        {
            for (int i = 0; i < 5; i++)
            {
                logger.Log($"Thread 1 — info message {i + 1}", LogLevel.INFO);
                Thread.Sleep(50);
            }
        });

        Thread t2 = new Thread(() =>
        {
            for (int i = 0; i < 5; i++)
            {
                logger.Log($"Thread 2 — warning message {i + 1}", LogLevel.WARNING);
                Thread.Sleep(50);
            }
        });

        Thread t3 = new Thread(() =>
        {
            for (int i = 0; i < 5; i++)
            {
                logger.Log($"Thread 3 — error message {i + 1}", LogLevel.ERROR);
                Thread.Sleep(50);
            }
        });

        t1.Start();
        t2.Start();
        t3.Start();

        t1.Join();
        t2.Join();
        t3.Join();

        Console.WriteLine("Логирование завершено.\n");
        logger.ReadLogs();
    }
}
