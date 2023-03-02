using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SleuthKit.Structs
{
    [StructLayout(LayoutKind.Explicit,
    Size = 4968
    )]
    public struct HFS_INFO
    {
        [FieldOffset(0)]
        TSK_FS_INFO fs_info;
        
        [FieldOffset(424)]
        IntPtr fs_ptr;

        [FieldOffset(432)]
        char is_case_sensitive;
        
        [FieldOffset(440)]
        tsk_lock_t protection_lock;
        
        [FieldOffset(480)]
        IntPtr blockmap_file_ptr;
        
        [FieldOffset(488)]
        IntPtr blockmap_attr_ptr;
        
        [FieldOffset(496)]
        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 4096)]
        byte[] blockmap_cache;
        
        [FieldOffset(4592)]
        long blockmap_cache_start;
        
        [FieldOffset(4600)]
        UIntPtr blockmap_cache_len;
        
        [FieldOffset(4608)]
        IntPtr catalog_file_ptr;
        
        [FieldOffset(4616)]
        IntPtr catalog_attr_ptr;
        
        [FieldOffset(4624)]
        tsk_lock_t catalog_header; //TODO: type
        
        [FieldOffset(4736)]
        IntPtr extents_file_ptr;
        
        [FieldOffset(4744)]
        IntPtr extents_attr_ptr;
        
        [FieldOffset(4752)]
        tsk_lock_t extents_header; //TODO: type
        
        [FieldOffset(4864)]
        long hfs_wrapper_offset;
        
        [FieldOffset(4872)]
        long root_crtime;
        
        [FieldOffset(4880)]
        long meta_crtime;
        
        [FieldOffset(4888)]
        long metadir_crtime;
        
        [FieldOffset(4896)]
        bool has_root_crtime;
        
        [FieldOffset(4897)]
        bool has_meta_crtime;
        
        [FieldOffset(4898)]
        bool has_meta_dir_crtime;
        
        [FieldOffset(4904)]
        ulong meta_inum;
        
        [FieldOffset(4912)]
        ulong meta_dir_inum;
        
        [FieldOffset(4920)]
        IntPtr meta_dir_ptr;
        
        [FieldOffset(4928)]
        IntPtr dir_meta_dir_ptr;
        
        [FieldOffset(4936)]
        tsk_lock_t metadata_dir_cache_lock;
        
        [FieldOffset(4966)]
        bool has_extents_file;
        
        [FieldOffset(4977)]
        bool has_startup_file;
        
        [FieldOffset(4978)]
        bool has_attributes_file;

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
