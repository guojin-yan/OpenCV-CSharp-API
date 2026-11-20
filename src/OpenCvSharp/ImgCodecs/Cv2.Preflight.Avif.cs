using System;

namespace JYPPX.OpenCvSharp.ImgCodecs
{
    public static partial class Cv2
    {
        private struct AvifAssociationCursor
        {
            public int Offset;
            public int End;
            public int ItemIdBytes;
            public int IndexBytes;
            public uint RemainingEntries;
            public uint ItemId;
            public int AssociationOffset;
            public int AssociationCount;
            public bool HasEntry;
        }

        private static bool IsAvifSignature(byte[] data)
        {
            int next;
            int payloadOffset;
            int payloadLength;
            return TryReadAvifBox(data, 0, data.Length, out next, out payloadOffset, out payloadLength) &&
                IsAvifBoxType(data, 4, 'f', 't', 'y', 'p') &&
                TryReadAvifFileTypePayload(data, payloadOffset, payloadLength);
        }

        private static bool TryReadAvifHeader(byte[] data, out int width, out int height, out PixelFacts pixelFacts)
        {
            width = 0;
            height = 0;
            pixelFacts = new PixelFacts();
            int offset = 0;
            bool sawFileType = false;
            bool sawMeta = false;
            int primaryWidth = 0;
            int primaryHeight = 0;
            PixelFacts primaryPixels = new PixelFacts();

            while (offset < data.Length)
            {
                int next;
                int payloadOffset;
                int payloadLength;
                if (!TryReadAvifBox(data, offset, data.Length, out next, out payloadOffset, out payloadLength)) return false;
                if (!sawFileType)
                {
                    if (!IsAvifBoxType(data, offset + 4, 'f', 't', 'y', 'p') ||
                        !TryReadAvifFileTypePayload(data, payloadOffset, payloadLength)) return false;
                    sawFileType = true;
                }
                else if (IsAvifBoxType(data, offset + 4, 'f', 't', 'y', 'p'))
                {
                    return false;
                }
                else if (IsAvifBoxType(data, offset + 4, 'm', 'e', 't', 'a'))
                {
                    if (sawMeta || !TryReadAvifMeta(data, payloadOffset, payloadLength,
                        out primaryWidth, out primaryHeight, out primaryPixels)) return false;
                    sawMeta = true;
                }
                offset = next;
            }

            if (!sawFileType || !sawMeta) return false;
            width = primaryWidth;
            height = primaryHeight;
            pixelFacts = primaryPixels;
            return true;
        }

        private static bool TryReadAvifFileTypePayload(byte[] data, int payloadOffset, int payloadLength)
        {
            if (payloadLength < 8 || (payloadLength & 3) != 0) return false;
            if (IsAvifBrand(data, payloadOffset)) return true;
            int end = payloadOffset + payloadLength;
            for (int offset = payloadOffset + 8; end - offset >= 4; offset += 4)
            {
                if (IsAvifBrand(data, offset)) return true;
            }
            return false;
        }

        private static bool TryReadAvifMeta(byte[] data, int payloadOffset, int payloadLength,
            out int width, out int height, out PixelFacts pixelFacts)
        {
            width = 0;
            height = 0;
            pixelFacts = new PixelFacts();
            if (payloadLength < 4 || ReadBe32(data, payloadOffset) != 0) return false;
            int offset = payloadOffset + 4;
            int end = payloadOffset + payloadLength;
            uint primaryItemId = 0;
            int iprpOffset = 0;
            int iprpLength = 0;
            bool sawIprp = false;
            while (offset < end)
            {
                int next;
                int childPayloadOffset;
                int childPayloadLength;
                if (!TryReadAvifBox(data, offset, end, out next, out childPayloadOffset, out childPayloadLength)) return false;
                if (IsAvifBoxType(data, offset + 4, 'p', 'i', 't', 'm'))
                {
                    if (primaryItemId != 0 || childPayloadLength < 4) return false;
                    uint versionAndFlags = ReadBe32(data, childPayloadOffset);
                    if (versionAndFlags == 0 && childPayloadLength == 6)
                    {
                        primaryItemId = (uint)ReadJpeg2000Be16(data, childPayloadOffset + 4);
                    }
                    else if (versionAndFlags == 0x01000000 && childPayloadLength == 8)
                    {
                        primaryItemId = ReadBe32(data, childPayloadOffset + 4);
                    }
                    else return false;
                    if (primaryItemId == 0) return false;
                }
                else if (IsAvifBoxType(data, offset + 4, 'i', 'p', 'r', 'p'))
                {
                    if (sawIprp) return false;
                    iprpOffset = childPayloadOffset;
                    iprpLength = childPayloadLength;
                    sawIprp = true;
                }
                offset = next;
            }
            return primaryItemId != 0 && sawIprp &&
                TryReadAvifIprp(data, iprpOffset, iprpLength, primaryItemId, out width, out height, out pixelFacts);
        }

        private static bool TryReadAvifIprp(byte[] data, int payloadOffset, int payloadLength, uint primaryItemId,
            out int width, out int height, out PixelFacts pixelFacts)
        {
            width = 0;
            height = 0;
            pixelFacts = new PixelFacts();
            int end = payloadOffset + payloadLength;
            int offset;
            int ipcoOffset;
            int ipcoLength;
            if (!TryReadAvifBox(data, payloadOffset, end, out offset, out ipcoOffset, out ipcoLength) ||
                !IsAvifBoxType(data, payloadOffset + 4, 'i', 'p', 'c', 'o')) return false;

            int propertyCount = 0;
            int propertyOffset = ipcoOffset;
            int ipcoEnd = ipcoOffset + ipcoLength;
            while (propertyOffset < ipcoEnd)
            {
                int next;
                int ignoredOffset;
                int ignoredLength;
                if (!TryReadAvifBox(data, propertyOffset, ipcoEnd, out next, out ignoredOffset, out ignoredLength)) return false;
                ++propertyCount;
                propertyOffset = next;
            }

            // Four supported (version, flags) pairs; no storage uses declared entry/property counts.
            AvifAssociationCursor[] cursors = new AvifAssociationCursor[4];
            int cursorCount = 0;
            int versionFlagsSeen = 0;
            while (offset < end)
            {
                int next;
                int childOffset;
                int childLength;
                if (!TryReadAvifBox(data, offset, end, out next, out childOffset, out childLength)) return false;
                if (IsAvifBoxType(data, offset + 4, 'i', 'p', 'c', 'o')) return false;
                if (IsAvifBoxType(data, offset + 4, 'i', 'p', 'm', 'a'))
                {
                    if (childLength < 8 || cursorCount == cursors.Length) return false;
                    uint versionAndFlags = ReadBe32(data, childOffset);
                    int version = data[childOffset];
                    int flags = data[childOffset + 3];
                    if (version > 1 || flags > 1 || (versionAndFlags & 0x00FFFF00) != 0) return false;
                    int versionFlagsBit = 1 << (version * 2 + flags);
                    if ((versionFlagsSeen & versionFlagsBit) != 0) return false;
                    versionFlagsSeen |= versionFlagsBit;
                    int itemIdBytes = version == 0 ? 2 : 4;
                    uint entryCount = ReadBe32(data, childOffset + 4);
                    if (entryCount > (uint)((childLength - 8) / (itemIdBytes + 1))) return false;
                    cursors[cursorCount] = new AvifAssociationCursor
                    {
                        Offset = childOffset + 8,
                        End = childOffset + childLength,
                        ItemIdBytes = itemIdBytes,
                        IndexBytes = flags + 1,
                        RemainingEntries = entryCount
                    };
                    if (!TryAdvanceAvifAssociation(data, ref cursors[cursorCount])) return false;
                    ++cursorCount;
                }
                offset = next;
            }

            int[] primaryIndices = new int[255];
            int primaryCount = 0;
            while (true)
            {
                int selected = -1;
                for (int index = 0; index < cursorCount; ++index)
                {
                    if (cursors[index].HasEntry && (selected < 0 || cursors[index].ItemId < cursors[selected].ItemId)) selected = index;
                }
                if (selected < 0) break;
                AvifAssociationCursor current = cursors[selected];
                for (int index = 0; index < cursorCount; ++index)
                {
                    if (index != selected && cursors[index].HasEntry && cursors[index].ItemId == current.ItemId) return false;
                }
                int associationOffset = current.AssociationOffset;
                for (int association = 0; association < current.AssociationCount; ++association)
                {
                    int rawIndex = current.IndexBytes == 1 ? data[associationOffset] : ReadJpeg2000Be16(data, associationOffset);
                    associationOffset += current.IndexBytes;
                    int propertyIndex = rawIndex & (current.IndexBytes == 1 ? 0x7F : 0x7FFF);
                    if (propertyIndex == 0)
                    {
                        if (rawIndex != 0) return false;
                        continue;
                    }
                    if (propertyIndex > propertyCount) return false;
                    if (current.ItemId == primaryItemId) primaryIndices[primaryCount++] = propertyIndex;
                }
                if (!TryAdvanceAvifAssociation(data, ref cursors[selected])) return false;
            }
            if (primaryCount == 0) return false;
            Array.Sort(primaryIndices, 0, primaryCount);
            for (int index = 1; index < primaryCount; ++index)
            {
                if (primaryIndices[index] == primaryIndices[index - 1]) return false;
            }
            return TryReadAvifPrimaryProperties(data, ipcoOffset, ipcoEnd, primaryIndices, primaryCount,
                out width, out height, out pixelFacts);
        }

        private static bool TryAdvanceAvifAssociation(byte[] data, ref AvifAssociationCursor cursor)
        {
            cursor.HasEntry = false;
            if (cursor.RemainingEntries == 0) return cursor.Offset == cursor.End;
            if (cursor.End - cursor.Offset < cursor.ItemIdBytes + 1) return false;
            uint itemId = cursor.ItemIdBytes == 2 ? (uint)ReadJpeg2000Be16(data, cursor.Offset) : ReadBe32(data, cursor.Offset);
            if (itemId <= cursor.ItemId) return false;
            cursor.ItemId = itemId;
            cursor.Offset += cursor.ItemIdBytes;
            cursor.AssociationCount = data[cursor.Offset++];
            if (cursor.AssociationCount > (cursor.End - cursor.Offset) / cursor.IndexBytes) return false;
            cursor.AssociationOffset = cursor.Offset;
            cursor.Offset += cursor.AssociationCount * cursor.IndexBytes;
            --cursor.RemainingEntries;
            cursor.HasEntry = true;
            return true;
        }

        private static bool TryReadAvifPrimaryProperties(byte[] data, int offset, int end, int[] indices, int count,
            out int width, out int height, out PixelFacts pixelFacts)
        {
            width = 0;
            height = 0;
            pixelFacts = new PixelFacts();
            int propertyIndex = 0;
            int selectedIndex = 0;
            bool sawDimensions = false;
            bool sawPixels = false;
            while (offset < end && selectedIndex < count)
            {
                int next;
                int payloadOffset;
                int payloadLength;
                if (!TryReadAvifBox(data, offset, end, out next, out payloadOffset, out payloadLength)) return false;
                ++propertyIndex;
                if (propertyIndex == indices[selectedIndex])
                {
                    ++selectedIndex;
                    if (IsAvifBoxType(data, offset + 4, 'i', 's', 'p', 'e'))
                    {
                        if (sawDimensions || payloadLength != 12 || ReadBe32(data, payloadOffset) != 0) return false;
                        uint unsignedWidth = ReadBe32(data, payloadOffset + 4);
                        uint unsignedHeight = ReadBe32(data, payloadOffset + 8);
                        if (unsignedWidth == 0 || unsignedHeight == 0 || unsignedWidth > int.MaxValue || unsignedHeight > int.MaxValue) return false;
                        width = (int)unsignedWidth;
                        height = (int)unsignedHeight;
                        sawDimensions = true;
                    }
                    else if (IsAvifBoxType(data, offset + 4, 'p', 'i', 'x', 'i'))
                    {
                        if (sawPixels || payloadLength < 5 || ReadBe32(data, payloadOffset) != 0) return false;
                        int channels = data[payloadOffset + 4];
                        if (channels == 0 || payloadLength != 5 + channels) return false;
                        int bitDepth = data[payloadOffset + 5];
                        bool uniform = true;
                        for (int channel = 1; channel < channels; ++channel)
                        {
                            if (data[payloadOffset + 5 + channel] != bitDepth) uniform = false;
                        }
                        if (uniform && (bitDepth == 8 || bitDepth == 10 || bitDepth == 12) &&
                            (channels == 1 || channels == 3 || channels == 4))
                        {
                            pixelFacts.BitDepth = bitDepth;
                            pixelFacts.BitDepthKnown = true;
                            pixelFacts.Channels = channels;
                            pixelFacts.ChannelsKnown = true;
                        }
                        sawPixels = true;
                    }
                }
                offset = next;
            }
            return sawDimensions;
        }

        private static bool TryReadAvifBox(byte[] data, int offset, int end, out int next, out int payloadOffset, out int payloadLength)
        {
            next = offset;
            payloadOffset = 0;
            payloadLength = 0;
            if (offset < 0 || offset > end || end - offset < 8) return false;
            uint size = ReadBe32(data, offset);
            int headerLength = 8;
            ulong boxLength;
            if (size == 1)
            {
                if (end - offset < 16) return false;
                boxLength = ((ulong)ReadBe32(data, offset + 8) << 32) | ReadBe32(data, offset + 12);
                headerLength = 16;
            }
            else boxLength = size == 0 ? (ulong)(end - offset) : size;
            if (boxLength < (ulong)headerLength || boxLength > (ulong)(end - offset)) return false;
            next = offset + (int)boxLength;
            payloadOffset = offset + headerLength;
            payloadLength = (int)boxLength - headerLength;
            return true;
        }

        private static bool IsAvifBrand(byte[] data, int offset)
        {
            return offset >= 0 && data.Length - offset >= 4 &&
                data[offset] == (byte)'a' && data[offset + 1] == (byte)'v' &&
                data[offset + 2] == (byte)'i' && (data[offset + 3] == (byte)'f' || data[offset + 3] == (byte)'s');
        }

        private static bool IsAvifBoxType(byte[] data, int offset, char first, char second, char third, char fourth)
        {
            return offset >= 0 && data.Length - offset >= 4 && data[offset] == (byte)first && data[offset + 1] == (byte)second &&
                data[offset + 2] == (byte)third && data[offset + 3] == (byte)fourth;
        }
    }
}
