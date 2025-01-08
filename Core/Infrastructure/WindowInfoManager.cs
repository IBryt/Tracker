using Core.Entities;
using Core.Interfaces.Infrastructure;
using System.Collections.Concurrent;

namespace Core.Infrastructure;

public class WindowInfoManager : IWindowInfoManager
{
    private ConcurrentDictionary<nint, WindowInfo> _windowInfos = new();

    public bool TryGetValue(nint key, out WindowInfo? value)
    {
        return _windowInfos.TryGetValue(key, out value);
    }

    public bool TryAdd(nint key, WindowInfo value)
    {
        return _windowInfos.TryAdd(key, value);
    }

    public bool TryUpdate(nint key, WindowInfo newValue, WindowInfo currentValue)
    {
        return _windowInfos.TryUpdate(key, newValue, currentValue);
    }

    public WindowInfo AddOrUpdate(nint key, WindowInfo addValue, Func<nint, WindowInfo, WindowInfo> updateValue)
    {
        return _windowInfos.AddOrUpdate(key, addValue, updateValue);
    }

    public bool TryRemove(nint key, out WindowInfo? value)
    {
        return _windowInfos.TryRemove(key, out value);
    }
}
