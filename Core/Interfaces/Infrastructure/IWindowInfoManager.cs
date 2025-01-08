using Core.Entities;

namespace Core.Interfaces.Infrastructure;

public interface IWindowInfoManager
{
    public bool TryGetValue(nint key, out WindowInfo? value);
    public bool TryAdd(nint key, WindowInfo value);
    public bool TryUpdate(nint key, WindowInfo newValue, WindowInfo currentValue);
    public WindowInfo AddOrUpdate(nint key, WindowInfo addValue, Func<nint, WindowInfo, WindowInfo> updateValue);
    public bool TryRemove(nint key, out WindowInfo? value);
}
