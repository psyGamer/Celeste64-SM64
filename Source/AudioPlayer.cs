using System.Runtime.InteropServices;
using FMOD;
using static LibSM64.Native;

namespace Celeste64.Mod.SuperMario64;

public static class AudioPlayer
{
    private const int NumChannels = 2;
    private const int SampleRate = 32000;
    private const int AudioBufferSize = 544 * 2;
    private const int QueueSize = 32767;
    
    private static readonly short[] audioBuffer = new short[AudioBufferSize * NumChannels];
    private static readonly CircularQueue<short> audioQueue = new(QueueSize);
    
    private static FMOD.Sound sound;
    
    public static void Create()
    {
        // Create FMOD audio stream to play back libsm64 data
        Audio.Check(Audio.system.getCoreSystem(out var coreSystem));
        CREATESOUNDEXINFO exinfo = default;
        exinfo.cbsize = Marshal.SizeOf(typeof(CREATESOUNDEXINFO));
        exinfo.numchannels = NumChannels;
        exinfo.decodebuffersize = (uint)(AudioBufferSize * Marshal.SizeOf<short>());
        exinfo.format = SOUND_FORMAT.PCM16;
        exinfo.defaultfrequency = SampleRate;
        exinfo.pcmreadcallback = LibSM64Playback;
        exinfo.length = (uint)(SampleRate * NumChannels * Marshal.SizeOf<short>());
        exinfo.length = (uint)(AudioBufferSize * NumChannels * Marshal.SizeOf<short>());
        
        Audio.Check(coreSystem.createStream("libsm64 playback", MODE.OPENUSER | MODE.LOOP_NORMAL, ref exinfo, out sound));
        Audio.Check(coreSystem.playSound(sound, new ChannelGroup(0), false, out _));
    }
    
    public static unsafe void Update()
    {
        if (SuperMario64Mod.IsOddFrame)
            return;
        
        fixed (short* pBuf = audioBuffer)
        {
            uint writtenSamples = sm64_audio_tick(QueueSize, (uint)audioBuffer.Length, pBuf);
            audioQueue.Enqueue(pBuf, writtenSamples * 2 * 2);
        }
    }
    
    public static void Dispose()
    {
        Audio.Check(sound.release());
    }
    
    private static unsafe RESULT LibSM64Playback(IntPtr _, IntPtr data, uint length)
    {
        var len = Math.Min((int)(length / Marshal.SizeOf<short>()), audioQueue.Size);
        audioQueue.Dequeue((short*)data, (uint)len);
        
        // Fill reset with 0
        var filled = len * Marshal.SizeOf<short>();
        NativeMemory.Fill((void*)(data + filled), (UIntPtr)(length - filled), 0);
        
        return RESULT.OK;
    }
}