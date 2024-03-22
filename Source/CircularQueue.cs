using System.Runtime.InteropServices;

namespace Celeste64.Mod.SuperMario64;

public sealed unsafe class CircularQueue<T>(uint bufferSize)
    where T : unmanaged
{
    private const float ResizeFactor = 2.0f;
    
    private T[] internalBuffer = new T[bufferSize];

    private uint headIdx = 0, tailIdx = 0;
    private uint size;

    public uint Size => size;

    public void Clear()
    {
        headIdx = tailIdx = size = 0;
    }
    
    public void Resize(uint newBufferSize)
    {
        var newBuffer = new T[newBufferSize];
        
        if (tailIdx <= headIdx)
        {
            // Data is currently not wrapped
            // Change indices, so that tail is at 0
            Array.Copy(internalBuffer, tailIdx, newBuffer, 0, headIdx - tailIdx);
        }
        else
        {
            // Data is currently wrapped
            long untilCut = internalBuffer.Length - tailIdx;
            Array.Copy(internalBuffer, tailIdx, newBuffer, 0, untilCut);
            Array.Copy(internalBuffer, 0, newBuffer, untilCut, headIdx);
        }
        
        internalBuffer = newBuffer;
    }
    
    public void Enqueue(T* srcBuffer, uint count)
    {
        lock (this)
        {
            if (count > internalBuffer.Length)
            {
                Resize(Math.Max(count, (uint)(internalBuffer.Length * ResizeFactor)));
            }
        
            fixed (T* pInternal = internalBuffer)
            {
                long untilCut = internalBuffer.Length - headIdx;
                if (count <= untilCut)
                {
                    // Fits into remaining space
                    NativeMemory.Copy(srcBuffer, pInternal + headIdx, (UIntPtr)(count * Marshal.SizeOf<T>()));
                    headIdx += count;
                }
                else
                {
                    // Needs to be cut in half
                    long wrappedSize = count - untilCut;
                    NativeMemory.Copy(srcBuffer, pInternal + headIdx, (UIntPtr)(untilCut * Marshal.SizeOf<T>()));
                    NativeMemory.Copy(srcBuffer + untilCut, pInternal, (UIntPtr)(wrappedSize * Marshal.SizeOf<T>()));
                    headIdx = (uint)wrappedSize;
                }
            }
        
            size += count;
        }
    }
    
    public void Dequeue(T* dstBuffer, uint count)
    {
        lock (this)
        {
            if (count > internalBuffer.Length)
            {
                Resize(Math.Max(count, (uint)(internalBuffer.Length * ResizeFactor)));
            }
        
            fixed (T* pInternal = internalBuffer)
            {
                long untilCut = internalBuffer.Length - tailIdx;
                if (count <= untilCut)
                {
                    // Fits into remaining space
                    NativeMemory.Copy(pInternal + tailIdx, dstBuffer, (UIntPtr)(count * Marshal.SizeOf<T>()));
                    tailIdx += count;
                }
                else
                {
                    // Needs to be cut in half
                    long wrappedSize = count - untilCut;
                    NativeMemory.Copy(pInternal + tailIdx, dstBuffer, (UIntPtr)(untilCut * Marshal.SizeOf<T>()));
                    NativeMemory.Copy(pInternal, dstBuffer + untilCut, (UIntPtr)(wrappedSize * Marshal.SizeOf<T>()));
                    tailIdx = (uint)wrappedSize;
                }
            }
        
            size -= count;
        }
    }
}
