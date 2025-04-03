using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace SleuthKit.Structs
{
    [StructLayout(LayoutKind.Sequential)]
    public struct TSK_FS_ATTRLIST
    {
        private IntPtr head_ptr;

        public bool HasHead => head_ptr != IntPtr.Zero;

        public TSK_FS_ATTR Head => ((TSK_FS_ATTR)Marshal.PtrToStructure(head_ptr, typeof(TSK_FS_ATTR)));

        public bool IsEmpty => !HasHead;

        public IEnumerable<TSK_FS_ATTR> List
        {
            get
            {
                if (HasHead)
                {
                    TSK_FS_ATTR current = Head;
                    for (; ; )
                    {
                        yield return current;

                        if (current.HasNext)
                        {
                            current = current.Next;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
        }
    }
}