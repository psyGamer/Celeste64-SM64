using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SuperMario64;

public sealed unsafe class CircularQueue<T> where T: unmanaged
{
    private T[] internalBuffer;
    private uint internalBufferSize;
    
    private uint headIdx = 0, tailIdx = 0;
    private uint size;
    
    public uint Size => size;
    
    public CircularQueue(uint bufferSize)
    {
        internalBuffer = new T[bufferSize];
        internalBufferSize = bufferSize;
    }
    
    public void Clear()
    {
        headIdx = tailIdx = size = 0;
    }
    
    public void Enqueue(T* srcBuffer, uint count)
    {
        Debug.Assert(count <= internalBufferSize); // TODO
        
        fixed (T* pInternal = internalBuffer)
        {
            uint untilCut = internalBufferSize - headIdx;
            if (count <= untilCut)
            {
                // Fits into remaining space
                NativeMemory.Copy(srcBuffer, pInternal + headIdx, (UIntPtr)(count * Marshal.SizeOf<T>()));
                headIdx += count;
            }
            else
            {
                // Needs to be cut in half
                uint wrappedSize = count - untilCut;
                NativeMemory.Copy(srcBuffer, pInternal + headIdx, (UIntPtr)(untilCut * Marshal.SizeOf<T>()));
                NativeMemory.Copy(srcBuffer + untilCut, pInternal, (UIntPtr)(wrappedSize * Marshal.SizeOf<T>()));
                headIdx = wrappedSize;
            }
        }
        
        size += count;
    }
    
    public void Dequeue(T* dstBuffer, uint count)
    {
        Debug.Assert(count <= internalBufferSize); // TODO
        
        fixed (T* pInternal = internalBuffer)
        {
            uint untilCut = internalBufferSize - tailIdx;
            if (count <= untilCut)
            {
                // Fits into remaining space
                NativeMemory.Copy(pInternal + tailIdx, dstBuffer, (UIntPtr)(count * Marshal.SizeOf<T>()));
                tailIdx += count;
            }
            else
            {
                // Needs to be cut in half
                uint wrappedSize = count - untilCut;
                NativeMemory.Copy(pInternal + tailIdx, dstBuffer, (UIntPtr)(untilCut * Marshal.SizeOf<T>()));
                NativeMemory.Copy(pInternal, dstBuffer + untilCut, (UIntPtr)(wrappedSize * Marshal.SizeOf<T>()));
                tailIdx = wrappedSize;
            }
        }
        
        size -= count;
    }
}
