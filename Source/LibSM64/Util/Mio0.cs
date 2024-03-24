using System.Runtime.InteropServices;

namespace LibSM64.Util;

public static class Mio0
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Header
    {
        public uint dest_size;
        public uint comp_offset;
        public uint uncomp_offset;
    }
    
    public static unsafe Header DecodeHeader(byte* inBuffer)
    {
        // Check magic
        if (inBuffer[0] != 'M' || inBuffer[1] != 'I' || inBuffer[2] != 'O' || inBuffer[3] != '0') 
            throw new InvalidDataException("Expected 'MIO0' header");
        
        return new Header
        {
            dest_size = *(uint*)(inBuffer + 4),
            comp_offset = *(uint*)(inBuffer + 8),
            uncomp_offset = *(uint*)(inBuffer + 12),
        };
    }
    
    private static unsafe bool GetBit(byte* buffer, int bit)
    { 
        byte bitMask = (byte)(1u << 7 - (bit % 8));
        return (buffer[(bit) / 8] & bitMask) == 1;
    }

    private const int MIO0_HEADER_LENGTH = 16;
    
    public static unsafe void Decode(Header header, byte* inBuffer, byte* outBuffer)
    {
        uint bytesWritten = 0;
        int bitIdx = 0;
        int compIdx = 0;
        int uncompIdx = 0;
        
        // Decode data
        while (bytesWritten < header.dest_size) 
        {
            if (GetBit(&inBuffer[MIO0_HEADER_LENGTH], bitIdx)) 
            {
                // 1 - Pull uncompressed data
                outBuffer[bytesWritten] = inBuffer[header.uncomp_offset + uncompIdx];
                bytesWritten++;
                uncompIdx++;
            } 
            else 
            {
                // 0 - Read compressed data
                byte* vals = &inBuffer[header.comp_offset + compIdx];
                compIdx += 2;

                int length = ((vals[0] & 0xF0) >> 4) + 3;
                int idx = ((vals[0] & 0x0F) << 8) + vals[1] + 1;
                for (int i = 0; i < length; i++) {
                    outBuffer[bytesWritten] = outBuffer[bytesWritten - idx];
                    bytesWritten++;
                }
            }
            bitIdx++;
        }
    }
}