using Korn.Shared;
using Korn.Utils;
using System;

namespace Korn.Utils.Algorithms 
{
    public unsafe class StateCollection : IDisposable
    {
        const int LONG_SIZE = 64;
        const int LONG_HALF_SIZE = LONG_SIZE / 2;

        public StateCollection(int states)
        {
            stateCount = states;
            longCount = (states + LONG_SIZE - 1) / LONG_SIZE;

            longs = Memory.Alloc<ulong>(longCount);
        }

        int lastFreeIndex;
        readonly int stateCount;
        readonly int longCount;
        readonly ulong* longs;

        public int TopHoldedIndex { get; private set; } = -1;
        public int HoldedStateCount { get; private set; }

        public bool HasFreeEntry => HoldedStateCount != stateCount;

        public bool this[int index]
        {
            get => (longs[index / 64] & (1UL << (index % 64))) != 0;
            private set 
            {
                var pointer = longs + (index / 64);
                var mask = 1UL << (index % 64);
                if (value)
                    *pointer |= mask;
                else *pointer &= ~mask;
            }
        }

        public void FreeEntry(int index)
        {
            if (this[index])
            {
                this[index] = false;
                HoldedStateCount--;

                if (TopHoldedIndex == index)
                    TopHoldedIndex = GetTopHoldedIndex();
            }
            else
            {
                KornShared.Logger.WriteWarning(
                    $"Korn.Utils.Algorithms.StateCollection->FreeEntry:",
                    "Trying to free an entry that is already free."
                );
            }
        }

        public int HoldEntry()
        {
            var freeIndex = GetFreeEntryIndex();

            if (freeIndex != -1)
            {
                this[freeIndex] = true;
                HoldedStateCount++;
                if (freeIndex > TopHoldedIndex)
                    TopHoldedIndex = freeIndex;
            }

            return freeIndex;
        }

        int GetFreeEntryIndex()
        {
            if (!HasFreeEntry)
                return -1;

            if (lastFreeIndex != -1)
            {
                var index = lastFreeIndex;
                lastFreeIndex = -1;
                return index;
            }

            for (var longIndex = 0; longIndex < longCount; longIndex++)
            {
                var longState = longs[longIndex];
                if (longState == 0xFFFFFFFFFFFFFFFF)
                    continue;

                var lowerState = (uint)(longState & 0xFFFFFFFF);
                var index = FreeIndexInInt(lowerState);
                if (index != -1)
                    return longIndex * LONG_SIZE + index;

                var highState = (uint)(longState >> 32);
                index = FreeIndexInInt(highState);
                if (index != -1)
                    return longIndex * LONG_SIZE + LONG_HALF_SIZE + index;

                int FreeIndexInInt(uint value)
                {
                    if (value != 0xFFFFFFFF)
                        for (int bitIndex = 0; bitIndex < 32; bitIndex++)
                            if ((value & (1 << bitIndex)) == 0)
                                return bitIndex;

                    return -1;
                }
            }

            return -1;
        }

        public int GetTopHoldedIndex()
        {
            for (var longIndex = longCount - 1; longIndex >= 0; longIndex--)
            {
                var longState = longs[longIndex];
                if (longState == 0)
                    continue;

                var lowerState = (uint)(longState & 0xFFFFFFFF);
                var index = FreeIndexInInt(lowerState);
                if (index != -1)
                    return longIndex * LONG_SIZE + index;

                var highState = (uint)(longState >> 32);
                index = FreeIndexInInt(highState);
                if (index != -1)
                    return longIndex * LONG_SIZE + LONG_HALF_SIZE + index;

                int FreeIndexInInt(uint value)
                {
                    if (value != 0)
                        for (int bitIndex = 31; bitIndex >= 0; bitIndex--)
                            if ((value & (1 << bitIndex)) != 0)
                                return bitIndex;

                    return -1;
                }
            }

            return 0;
        }

        #region IDisposable
        bool disposed;
        public void Dispose()
        {
            if (disposed)
                return;
            disposed = true;

            Memory.Free(longs);
        }

        ~StateCollection() => Dispose();
        #endregion
    }
}