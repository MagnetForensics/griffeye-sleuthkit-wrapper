using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace SleuthKit.Structs
{
    [StructLayout(LayoutKind.Sequential)]
    public struct TSK_FS_ATTR
    {
        private IntPtr next_ptr;

        private IntPtr fs_file_ptr;

        private AttributeFlags flags;

        private IntPtr name_ptr;

        private ulong name_size;

        private AttributeType type;

        private ushort id;

        private long size;

        private IntPtr nrd_run_ptr;

        private IntPtr nrd_run_end_ptr;

        private uint nrd_skiplen;

        private long nrd_allocsize;

        private long nrg_initsize;

        private uint nrd_compsize;

        private IntPtr rd_buf_ptr;

        private int rd_buf_size;

        private long rd_offset;

        //Some more attributes here, but not wrapped

        public bool HasNext => next_ptr != IntPtr.Zero;

        public TSK_FS_ATTR Next => ((TSK_FS_ATTR)Marshal.PtrToStructure(next_ptr, typeof(TSK_FS_ATTR)));

        public TSK_FS_FILE File => ((TSK_FS_FILE)Marshal.PtrToStructure(fs_file_ptr, typeof(TSK_FS_FILE)));

        public IntPtr FilePointer => fs_file_ptr;

        public AttributeFlags AttributeFlags => flags;

        public String Name
        {
            get
            {
                if (name_size > 0)
                {
                    byte[] buffer = new byte[name_size];
                    Marshal.Copy(name_ptr, buffer, 0, (int)name_size);
                    return Encoding.UTF8.GetString(buffer, 0, (int)name_size).TrimEnd('\0');
                }
                else
                {
                    return String.Empty;
                }
            }
        }

        public AttributeType AttributeType => type;

        public ushort Id => id;

        public long Size => size;

        public IEnumerable<TSK_FS_ATTR_RUN> NonResidentBlocks
        {
            get
            {
                if (nrd_run_ptr != IntPtr.Zero)
                {
                    TSK_FS_ATTR_RUN block = ((TSK_FS_ATTR_RUN)Marshal.PtrToStructure(nrd_run_ptr, typeof(TSK_FS_ATTR_RUN)));

                    for (; ; )
                    {
                        yield return block;

                        if (block.HasNext)
                        {
                            block = block.Next;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
        }

        public String rdBufString => rd_buf_ptr == null
                    ? null
                    : Marshal.PtrToStringUni(rd_buf_ptr, (int)(size / 2));
    }
}