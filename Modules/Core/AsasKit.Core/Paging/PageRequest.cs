namespace AsasKit.Core;

public sealed record PageRequest(int Page = 1, int Size = 20)
{
    public int Skip => Math.Max(0, (Math.Max(1, Page) - 1) * Math.Clamp(Size, 1, 200));
    public int Take => Math.Clamp(Size, 1, 200);
}
