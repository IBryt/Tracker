using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace Shared;


public class ProcessTracker : IDisposable
{
    private readonly ConcurrentDictionary<int, Process> _existingProcesses = new();
    private readonly List<string> _processNames = new() { "notepad", "Telegram" };
    private readonly int _delay = 1000;
    private bool _disposed = false;
    private readonly ILogger _logger;

    public delegate void ProcessStartedEventHandler(object sender, Process process);
    public event ProcessStartedEventHandler? ProcessStarted;

    public ProcessTracker(ILogger<ProcessTracker> logger)
    {
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var processes = GetProcesses();

                foreach (var process in processes)
                {
                    _logger.LogDebug($"Process started: {process.ProcessName}, ID: {process.Id}");
                    try
                    {
                        if (_existingProcesses.TryAdd(process.Id, process))
                        {
                            OnProcessStarted(process);
                            process.EnableRaisingEvents = true;
                            process.Exited += OnProcessExited;
                        }
                        else
                        {
                            process.Dispose();
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error processing process {process.ProcessName}: {ex.Message}");
                        if (process.HasExited)
                        {
                            _existingProcesses.TryRemove(process.Id, out _);
                        }
                    }
                }

                await Task.Delay(_delay, cancellationToken);
            }
        }
        catch (TaskCanceledException)
        {
            _logger.LogInformation("Process tracking canceled.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error.");
        }
    }

    protected virtual void OnProcessStarted(Process process)
    {
        Task.Run(() => ProcessStarted?.Invoke(this, process));
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            foreach (var process in _existingProcesses.Values)
            {
                RemoveProcess(process);
            }
            _existingProcesses.Clear();
            _disposed = true;
        }
    }

    private List<Process> GetProcesses()
    {
        List<Process> processes = new();

        foreach (var processName in _processNames)
        {
            var processesByName = Process.GetProcessesByName(processName);

            foreach (var process in processesByName)
            {
                if (_existingProcesses.TryGetValue(process.Id, out _))
                {
                    RemoveProcess(process);
                }
                else
                {
                    processes.Add(process);
                }
            }
        }

        return processes;
    }

    private void OnProcessExited(object? sender, EventArgs e)
    {
        if (sender is not Process process)
        {
            return;
        }

        _logger.LogDebug($"Process {process.ProcessName} with ID {process.Id} has exited.");
        if (_existingProcesses.TryRemove(process.Id, out _))
        {
            RemoveProcess(process);
        }
    }

    private void RemoveProcess(Process process)
    {
        if (!process.HasExited)
        {
            process.Exited -= OnProcessExited;
            process.Dispose();
        }
    }
}