/* Copyright (C) 2014 Tal Aloni <tal.aloni.il@gmail.com>. All rights reserved.
 * 
 * You can redistribute this program and/or modify it under the terms of
 * the GNU Lesser Public License as published by the Free Software Foundation,
 * either version 3 of the License, or (at your option) any later version.
 */
using System;
using System.Collections.Generic;
using System.Text;
using Utilities;

namespace SMBLibrary.SMB1
{
    /// <summary>
    /// SMB_FIND_FILE_BOTH_DIRECTORY_INFO
    /// </summary>
    public class FindFileBothDirectoryInfo : FindInformationEntry
    {
        public const int FixedLength = 94;

        public uint NextEntryOffset;
        public uint FileIndex; // SHOULD be set to zero when sent in a response and SHOULD be ignored when received by the client
        public DateTime CreationTime;
        public DateTime LastAccessTime;
        public DateTime LastWriteTime;
        public DateTime LastChangeTime;
        public ulong EndOfFile;
        public ulong AllocationSize;
        public ExtendedFileAttributes ExtFileAttributes;
        //uint FileNameLength; // In bytes, MUST exclude the null termination.
        public uint EASize;
        //byte ShortNameLength; // In bytes
        public byte Reserved;
        public string ShortName; // 24 bytes, 8.3 name of the file in Unicode format
        public string FileName; // OEM / Unicode character array. MUST be written as SMB_STRING, and read as fixed length string.
        // Omitting the NULL termination from the FileName field in a single SMB_FIND_FILE_BOTH_DIRECTORY_INFO structure
        // (as a response to TRANS2_QUERY_PATH_INFORMATION on a single directory)
        // Will, in some rare but repeatable cases, cause issues with Windows XP SP3 as a client
        // (the client will display an error message that the folder "refers to a location that is unavailable"...)

        public FindFileBothDirectoryInfo() : base(false)
        {
        }

        public FindFileBothDirectoryInfo(byte[] buffer, ref int offset, bool isUnicode) : base(false)
        {
            NextEntryOffset = LittleEndianReader.ReadUInt32(buffer, ref offset);
            FileIndex = LittleEndianReader.ReadUInt32(buffer, ref offset);
            CreationTime = SMBHelper.ReadFileTime(buffer, ref offset);
            LastAccessTime = SMBHelper.ReadFileTime(buffer, ref offset);
            LastWriteTime = SMBHelper.ReadFileTime(buffer, ref offset);
            LastChangeTime = SMBHelper.ReadFileTime(buffer, ref offset);
            EndOfFile = LittleEndianReader.ReadUInt64(buffer, ref offset);
            AllocationSize = LittleEndianReader.ReadUInt64(buffer, ref offset);
            ExtFileAttributes = (ExtendedFileAttributes)LittleEndianReader.ReadUInt32(buffer, ref offset);
            uint fileNameLength = LittleEndianReader.ReadUInt32(buffer, ref offset);
            EASize = LittleEndianReader.ReadUInt32(buffer, ref offset);
            byte shortNameLength = ByteReader.ReadByte(buffer, ref offset);
            Reserved = ByteReader.ReadByte(buffer, ref offset);
            ShortName = ByteReader.ReadUTF16String(buffer, ref offset, 12);
            ShortName = ShortName.Substring(0, shortNameLength);
            FileName = SMBHelper.ReadFixedLengthString(buffer, ref offset, isUnicode, (int)fileNameLength);
        }

        public override void WriteBytes(byte[] buffer, ref int offset, bool isUnicode)
        {
            uint fileNameLength = (uint)(isUnicode ? FileName.Length * 2 : FileName.Length);
            byte shortNameLength = (byte)(ShortName.Length * 2);

            LittleEndianWriter.WriteUInt32(buffer, ref offset, NextEntryOffset);
            LittleEndianWriter.WriteUInt32(buffer, ref offset, FileIndex);
            SMBHelper.WriteFileTime(buffer, ref offset, CreationTime);
            SMBHelper.WriteFileTime(buffer, ref offset, LastAccessTime);
            SMBHelper.WriteFileTime(buffer, ref offset, LastWriteTime);
            SMBHelper.WriteFileTime(buffer, ref offset, LastChangeTime);
            LittleEndianWriter.WriteUInt64(buffer, ref offset, EndOfFile);
            LittleEndianWriter.WriteUInt64(buffer, ref offset, AllocationSize);
            LittleEndianWriter.WriteUInt32(buffer, ref offset, (uint)ExtFileAttributes);
            LittleEndianWriter.WriteUInt32(buffer, ref offset, fileNameLength);
            LittleEndianWriter.WriteUInt32(buffer, ref offset, EASize);
            ByteWriter.WriteByte(buffer, ref offset, shortNameLength);
            ByteWriter.WriteByte(buffer, ref offset, Reserved);
            ByteWriter.WriteUTF16String(buffer, ref offset, ShortName, 12);
            SMBHelper.WriteSMBString(buffer, ref offset, isUnicode, FileName);
        }

        public override int GetLength(bool isUnicode)
        {
            int length = FixedLength;

            if (isUnicode)
            {
                length += FileName.Length * 2 + 2;
            }
            else
            {
                length += FileName.Length + 1;
            }
            return length;
        }
    }
}
