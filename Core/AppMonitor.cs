using Core.Interfaces;
using Core.Utils;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace Core;


public class AppMonitor : IAppMonitor, IDisposable
{
    private readonly ConcurrentDictionary<int, Process> _existingProcesses = new();
    private readonly ILogger _logger;
    private bool _disposed = false;

    public event ProcessStartedEventHandler? ProcessStarted;
    public event ProcessStopedEventHandler? ProcessStoped;

    public delegate void ProcessStartedEventHandler(object sender, Process process);
    public delegate void ProcessStopedEventHandler(object sender, Process process);

    public AppMonitor(ILogger<IAppMonitor> logger)
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

                await Task.Delay(Constants.ProcessCheckInterval, cancellationToken);
            }
        }
        catch (TaskCanceledException)
        {
            _logger.LogDebug("Process tracking canceled.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error.");
        }
    }

    protected virtual void OnProcessStarted(Process process)
    {
        ProcessStarted?.Invoke(this, process);
    }

    protected virtual void OnProcessStoped(Process process)
    {
        ProcessStoped?.Invoke(this, process);
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

        foreach (var processName in Constants.ProcessNames)
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

        if (_existingProcesses.TryRemove(process.Id, out _))
        {
            RemoveProcess(process);
            OnProcessStoped(process);
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