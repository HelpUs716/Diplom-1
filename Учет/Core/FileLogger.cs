using Microsoft.Extensions.Logging;

namespace Учет.Core
{
    public class FileLogger : ILogger
    {
        private readonly string _path; private readonly object _lock = new();
        public FileLogger(string path) => _path = path;
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => default!;

        public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel)) return;
            var msg = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{logLevel}] {formatter(state, exception)}\n";
            lock (_lock) try { System.IO.File.AppendAllText(_path, msg); } catch { }
        }
    }
    public class FileLoggerProvider : ILoggerProvider { private readonly string _path; public FileLoggerProvider(string p) => _path = p; public ILogger CreateLogger(string n) => new FileLogger(_path); public void Dispose() { } }
}