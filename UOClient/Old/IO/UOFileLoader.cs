using System;
using System.Threading.Tasks;

namespace UOClient.Old.IO
{
    internal abstract class UOFileLoader : IDisposable
    {
        public UOFileIndex[] Entries;

        public bool IsDisposed { get; private set; }

        public virtual void Dispose()
        {
            if (IsDisposed)
                return;

            IsDisposed = true;
            ClearResources();
        }

        public abstract Task Load();

        public virtual void ClearResources()
        { }

        public ref UOFileIndex GetValidRefEntry(int index)
        {
            if (index < 0 || Entries is null || index >= Entries.Length)
                return ref UOFileIndex.Invalid;

            ref UOFileIndex entry = ref Entries[index];

            if (entry.Offset < 0 || entry.Length <= 0 || entry.Offset == 0x0000_0000_FFFF_FFFF)
                return ref UOFileIndex.Invalid;

            return ref entry;
        }
    }
}