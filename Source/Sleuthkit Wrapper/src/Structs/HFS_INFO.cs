using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace SleuthKit.Structs
{
    [StructLayout(LayoutKind.Explicit,
    Size = 4968 + 16
    )]
    public struct HFS_INFO
    {
        [FieldOffset(0)]
        private TSK_FS_INFO fs_info;

        [FieldOffset(424 + 16)]
        private IntPtr fs_ptr;

        [FieldOffset(432 + 16)]
        private char is_case_sensitive;

        [FieldOffset(440 + 16)]
        private tsk_lock_t protection_lock;

        [FieldOffset(480 + 16)]
        private IntPtr blockmap_file_ptr;

        [FieldOffset(488 + 16)]
        private IntPtr blockmap_attr_ptr;

        [FieldOffset(496 + 16)]
        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 4096 + 16)]
        private byte[] blockmap_cache;

        [FieldOffset(4592 + 16)]
        private long blockmap_cache_start;

        [FieldOffset(4600 + 16)]
        private UIntPtr blockmap_cache_len;

        [FieldOffset(4608 + 16)]
        private IntPtr catalog_file_ptr;

        [FieldOffset(4616 + 16)]
        private IntPtr catalog_attr_ptr;

        [FieldOffset(4624 + 16)]
        private tsk_lock_t catalog_header; //TODO: type

        [FieldOffset(4736 + 16)]
        private IntPtr extents_file_ptr;

        [FieldOffset(4744 + 16)]
        private IntPtr extents_attr_ptr;

        [FieldOffset(4752 + 16)]
        private tsk_lock_t extents_header; //TODO: type

        [FieldOffset(4864 + 16)]
        private long hfs_wrapper_offset;

        [FieldOffset(4872 + 16)]
        private long root_crtime;

        [FieldOffset(4880 + 16)]
        private long meta_crtime;

        [FieldOffset(4888 + 16)]
        private long metadir_crtime;

        [FieldOffset(4896 + 16)]
        private bool has_root_crtime;

        [FieldOffset(4897 + 16)]
        private bool has_meta_crtime;

        [FieldOffset(4898 + 16)]
        private bool has_meta_dir_crtime;

        [FieldOffset(4904 + 16)]
        private ulong meta_inum;

        [FieldOffset(4912 + 16)]
        private ulong meta_dir_inum;

        [FieldOffset(4920 + 16)]
        private IntPtr meta_dir_ptr;

        [FieldOffset(4928 + 16)]
        private IntPtr dir_meta_dir_ptr;

        [FieldOffset(4936 + 16)]
        private tsk_lock_t metadata_dir_cache_lock;

        [FieldOffset(4966 + 16)]
        private bool has_extents_file;

        [FieldOffset(4977 + 16)]
        private bool has_startup_file;

        [FieldOffset(4978 + 16)]
        private bool has_attributes_file;

        public IEnumerable<TSK_FS_ATTR> CatalogAttributes
        {
            get
            {
                if (catalog_attr_ptr != IntPtr.Zero)
                {
                    TSK_FS_ATTR current = ((TSK_FS_ATTR)Marshal.PtrToStructure(catalog_attr_ptr, typeof(TSK_FS_ATTR)));

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