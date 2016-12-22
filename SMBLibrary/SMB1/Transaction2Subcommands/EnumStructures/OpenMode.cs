/* Copyright (C) 2014 Tal Aloni <tal.aloni.il@gmail.com>. All rights reserved.
 * 
 * You can redistribute this program and/or modify it under the terms of
 * the GNU Lesser Public License as published by the Free Software Foundation,
 * either version 3 of the License, or (at your option) any later version.
 */
using System;
using System.Collections.Generic;
using System.Text;

namespace SMBLibrary.SMB1
{
    public enum CreateFile : byte
    {
        ReturnErrorIfNotExist = 0x00,
        CreateIfNotExist = 0x01,
    }

    public enum FileExistsOpts : byte
    {
        ReturnError = 0x00,
        Append = 0x01,
        TruncateToZero = 0x02,
    }

    public struct OpenMode // 2 bytes
    {
        public FileExistsOpts FileExistsOpts;
        public CreateFile CreateFile;

        public OpenMode(byte[] buffer, int offset)
        {
            FileExistsOpts = (FileExistsOpts)(buffer[offset] & 0x3);
            CreateFile = (CreateFile)((buffer[offset] & 0x10) >> 4);
        }

        public void WriteBytes(byte[] buffer, int offset)
        {
            buffer[0] = (byte)FileExistsOpts;
            buffer[0] |= (byte)((byte)CreateFile << 4);
        }

        public static OpenMode Read(byte[] buffer, ref int offset)
        {
            offset += 2;
            return new OpenMode(buffer, offset - 2);
        }
    }
}
