namespace UOClient.ECS.Events
{
    internal readonly record struct CurrentSectorChanged(ushort X, ushort Y);
    internal readonly record struct SectorAdded(ushort X, ushort Y);
    internal readonly record struct SectorRemoved(ushort X, ushort Y);
}
