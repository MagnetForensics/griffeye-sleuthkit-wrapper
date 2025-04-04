/*
 *
 * This is a sample file that shows how to use some of the basic
 * POSIX-style library functions in The Sleuth Kit (www.sleuthkit.org).
 * There are also callback-style functions that can be used to read
 * the data and partitions.
 *
 * Copyright (c) 2008-2011, Brian Carrier <carrier <at> sleuthkit <dot> org>
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions
 * are met:
 *
 * - Redistributions of source code must retain the above copyright notice,
 *   this list of conditions and the following disclaimer.
 * - Redistributions in binary form must reproduce the above copyright
 *   notice, this list of conditions and the following disclaimer in the
 *   documentation and/or other materials provided with the distribution.
 * - Neither the Sleuth Kit name nor the names of its contributors may be
 *   used to endorse or promote products derived from this software without
 *   specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
 * "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
 * LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
 * A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT
 * OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED
 * TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
 * PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
 * LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

#include <stdio.h>
#include <string.h>
#include <stdlib.h>

#include <tsk/libtsk.h>


/**
 * Open a directory and cycle through its contents.  Read each file and recurse
 * into each directory.
 *
 * @param fs_info File system to process
 * @param stack Stack to prevent infinite recursion loops
 * @param dir_inum Metadata address of directory to open
 * @param path Path of directory being opened
 * @returns 1 on error
 */
static uint8_t
proc_dir(TSK_FS_INFO * fs_info, TSK_STACK * stack,
    TSK_INUM_T dir_inum, const char *path)
{
    TSK_FS_DIR *fs_dir;
    size_t i;
    char *path2 = NULL;
    char *buf = NULL;

    // open the directory
    if ((fs_dir = tsk_fs_dir_open_meta(fs_info, dir_inum)) == NULL) {
        fprintf(stderr, "Error opening directory: %" PRIuINUM "\n",
            dir_inum);
        tsk_error_print(stderr);
        return 1;
    }

    /* These should be dynamic lengths, but this is just a sample program.
     * Allocate heap space instead of stack to prevent overflow for deep
     * directories. */
    if ((path2 = (char *) malloc(4096)) == NULL) {
        return 1;
    }

    if ((buf = (char *) malloc(2048)) == NULL) {
        free(path2);
        return 1;
    }

    // cycle through each entry
    for (i = 0; i < tsk_fs_dir_getsize(fs_dir); i++) {
        TSK_FS_FILE *fs_file;
        TSK_OFF_T off = 0;
        size_t len = 0;

        // get the entry
        if ((fs_file = tsk_fs_dir_get(fs_dir, i)) == NULL) {
            fprintf(stderr,
                "Error getting directory entry %zu"
                " in directory %" PRIuINUM "\n", i, dir_inum);
            tsk_error_print(stderr);

            free(path2);
            free(buf);
            return 1;
        }

        /* Ignore NTFS System files */
        if ((TSK_FS_TYPE_ISNTFS(fs_file->fs_info->ftype)) &&
            (fs_file->name->name[0] == '$')) {
            tsk_fs_file_close(fs_file);
            continue;
        }

        //printf("Processing %s/%s\n", path, fs_file->name->name);

        // make sure it's got metadata and not only a name
        if (fs_file->meta) {
            ssize_t cnt;

            /* Note that we could also cycle through all of the attributes in the
             * file by using one of the tsk_fs_attr_get() functions and reading it
             * with tsk_fs_attr_read().  See the File Systems section of the Library
             * User's Guide for more details:
             * http://www.sleuthkit.org/sleuthkit/docs/api-docs/ */

            // read file contents
            if (fs_file->meta->type == TSK_FS_META_TYPE_REG) {
                int myflags = 0;

                for (off = 0; off < fs_file->meta->size; off += len) {
                    if (fs_file->meta->size - off < 2048)
                        len = (size_t) (fs_file->meta->size - off);
                    else
                        len = 2048;

                    cnt = tsk_fs_file_read(fs_file, off, buf, len,
                        (TSK_FS_FILE_READ_FLAG_ENUM) myflags);
                    if (cnt == -1) {
                        // could check tsk_errno here for a recovery error (TSK_ERR_FS_RECOVER)
                        fprintf(stderr, "Error reading %s file: %s\n",
                            ((fs_file->name->
                                    flags & TSK_FS_NAME_FLAG_UNALLOC)
                                || (fs_file->meta->
                                    flags & TSK_FS_META_FLAG_UNALLOC)) ?
                            "unallocated" : "allocated",
                            fs_file->name->name);
                        tsk_error_print(stderr);
                        break;
                    }
                    else if (cnt != (ssize_t) len) {
                        fprintf(stderr,
                            "Warning: %zd of %zu bytes read from %s file %s\n",
                            cnt, len,
                            ((fs_file->name->
                                    flags & TSK_FS_NAME_FLAG_UNALLOC)
                                || (fs_file->meta->
                                    flags & TSK_FS_META_FLAG_UNALLOC)) ?
                            "unallocated" : "allocated",
                            fs_file->name->name);
                    }

                    // do something with the data...
                }
            }

            // recurse into another directory (unless it is a '.' or '..')
            else if (TSK_FS_IS_DIR_META(fs_file->meta->type)){
                if (TSK_FS_ISDOT(fs_file->name->name) == 0) {

                    // only go in if it is not on our stack
                    if (tsk_stack_find(stack, fs_file->meta->addr) == 0) {
                        // add the address to the top of the stack
                        tsk_stack_push(stack, fs_file->meta->addr);

                        snprintf(path2, 4096, "%s/%s", path,
                            fs_file->name->name);
                        if (proc_dir(fs_info, stack, fs_file->meta->addr,
                                path2)) {
                            tsk_fs_file_close(fs_file);
                            tsk_fs_dir_close(fs_dir);
                            free(path2);
                            free(buf);
                            return 1;
                        }

                        // pop the address
                        tsk_stack_pop(stack);
                    }
                }
            }
        }
        tsk_fs_file_close(fs_file);
    }
    tsk_fs_dir_close(fs_dir);

    free(path2);
    free(buf);
    return 0;
}





/**
* Analyze the volume starting at byte offset 'start' and look
* for a file system.  When found, the files will be analyzed.
*
* @param img Disk image to be analyzed.
* @param start Byte offset of volume starting location.
*
* @return 1 on error and 0 on success
*/
static uint8_t
proc_fs(TSK_IMG_INFO * img_info, TSK_OFF_T start)
{
    TSK_FS_INFO *fs_info;
    TSK_STACK *stack;

    /* Try it as a file system */
    if ((fs_info =
            tsk_fs_open_img(img_info, start, TSK_FS_TYPE_DETECT)) == NULL)
    {
        fprintf(stderr,
            "Error opening file system in partition at offset %" PRIdOFF
            "\n", start);
        tsk_error_print(stderr);

        /* We could do some carving on the volume data at this point */

        return 1;
    }

    // create a stack to prevent infinite loops
    stack = tsk_stack_create();

    // Process the directories
    if (proc_dir(fs_info, stack, fs_info->root_inum, "")) {
        fprintf(stderr,
            "Error processing file system in partition at offset %" PRIdOFF
            "\n", start);
        tsk_fs_close(fs_info);
        return 1;
    }

    tsk_stack_free(stack);

    /* We could do some analysis of unallocated blocks at this point...  */


    tsk_fs_close(fs_info);
    return 0;
}


/**
* Process the data as a volume system to find the partitions
 * and volumes.
 * File system analysis will be performed on each partition.
 *
 * @param img Image file information structure for data to analyze
 * @param start Byte offset to start analyzing from.
 *
 * @return 1 on error and 0 on success
 */
static uint8_t
proc_vs(TSK_IMG_INFO * img_info, TSK_OFF_T start)
{
    TSK_VS_INFO *vs_info;

    // Open the volume system
    if ((vs_info =
            tsk_vs_open(img_info, start, TSK_VS_TYPE_DETECT)) == NULL) {
        if (tsk_verbose)
            fprintf(stderr,
                "Error determining volume system -- trying file systems\n");

        /* There was no volume system, but there could be a file system */
        tsk_error_reset();
        if (proc_fs(img_info, start)) {
            return 1;
        }
    }
    else {
        fprintf(stderr, "Volume system open, examining each\n");

        // cycle through the partitions
        for (TSK_PNUM_T i = 0; i < vs_info->part_count; i++) {
            const TSK_VS_PART_INFO *vs_part;

            if ((vs_part = tsk_vs_part_get(vs_info, i)) == NULL) {
                fprintf(stderr, "Error getting volume %" PRIuPNUM "\n", i);
                continue;
            }

            // ignore the metadata partitions
            if (vs_part->flags & TSK_VS_PART_FLAG_META)
                continue;

            // could do something with unallocated volumes
            else if (vs_part->flags & TSK_VS_PART_FLAG_UNALLOC) {

            }
            else {
                if (proc_fs(img_info,
                        vs_part->start * vs_info->block_size)) {
                    // We could do more fancy error checking here to see the cause
                    // of the error or consider the allocation status of the volume...
                    tsk_error_reset();
                }
            }
        }
        tsk_vs_close(vs_info);
    }
    return 0;
}


int
main(int argc, char **argv1)
{
    TSK_IMG_INFO *img_info;
    TSK_TCHAR **argv;

#ifdef TSK_WIN32
    // On Windows, get the wide arguments (mingw doesn't support wmain)
    argv = CommandLineToArgvW(GetCommandLineW(), &argc);
    if (argv == NULL) {
        fprintf(stderr, "Error getting wide arguments\n");
        exit(1);
    }
#else
    argv = (TSK_TCHAR **) argv1;
#endif

    if (argc != 2) {
        fprintf(stderr, "Missing image name\n");
        exit(1);
    }

    // open the disk image
    img_info =
        tsk_img_open_sing((const TSK_TCHAR *) argv[1],
        TSK_IMG_TYPE_DETECT, 0);
    if (img_info == NULL) {
        fprintf(stderr, "Error opening file\n");
        tsk_error_print(stderr);
        exit(1);
    }

    // process the volume starting at sector 0
    if (proc_vs(img_info, 0)) {
        tsk_error_print(stderr);
        tsk_img_close(img_info);
        exit(1);
    }

    // close the image
    tsk_img_close(img_info);
    return 0;
}
