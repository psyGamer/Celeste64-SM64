using LibSM64Sharp.LowLevel;
using System.Runtime.InteropServices;

namespace LibSM64Sharp.Impl;

public sealed partial class Sm64Context
{
    public uint TickAudio(uint numQueuedSamples, uint numDesiredSamples, short[] audioBuffer)
    {
        uint numSamples;
        {
            var audioBufferHandle = GCHandle.Alloc(audioBuffer, GCHandleType.Pinned);
            numSamples = LibSm64Interop.sm64_audio_tick(numQueuedSamples, numDesiredSamples, audioBufferHandle.AddrOfPinnedObject());
            audioBufferHandle.Free();
        }

        return numSamples;
    }
}