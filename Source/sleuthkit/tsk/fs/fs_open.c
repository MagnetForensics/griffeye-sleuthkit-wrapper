/*
** tsk_fs_open_img
** The Sleuth Kit
**
** Brian Carrier [carrier <at> sleuthkit [dot] org]
** Copyright (c) 2006-2011 Brian Carrier, Basis Technology.  All Rights reserved
** Copyright (c) 2003-2005 Brian Carrier.  All rights reserved
**
** TASK
** Copyright (c) 2002 Brian Carrier, @stake Inc.  All rights reserved
**
** Copyright (c) 1997,1998,1999, International Business Machines
** Corporation and others. All Rights Reserved.
*/

/* TCT */
/*++
 * LICENSE
 *	This software is distributed under the IBM Public License.
 * AUTHOR(S)
 *	Wietse Venema
 *	IBM T.J. Watson Research
 *	P.O. Box 704
 *	Yorktown Heights, NY 10598, USA
 --*/

#include "tsk_fs_i.h"
#include "tsk/util/detect_encryption.h"
#include "encryptionHelper.h"
#include "tsk/img/unsupported_types.h"
#include "tsk/img/logical_img.h"

/**
 * \file fs_open.c
 * Contains the general code to open a file system -- this calls
 * the file system -specific opening routines.
 */


/**
 * \ingroup fslib
 * Tries to process data in a volume as a file system.
 * Returns a structure that can be used for analysis and reporting.
 *
 * @param a_part_info Open volume to read from and analyze
 * @param a_ftype Type of file system (or autodetect)
 *
 * @return NULL on error
 */
TSK_FS_INFO *
tsk_fs_open_vol(const TSK_VS_PART_INFO * a_part_info,
    TSK_FS_TYPE_ENUM a_ftype)
{
    return tsk_fs_open_vol_decrypt(a_part_info, a_ftype, "");
}

/**
 * \ingroup fslib
 * Tries to process data in a volume as a file system.
 * Allows for providing an optional password for decryption.
 * Returns a structure that can be used for analysis and reporting.
 *
 * @param a_part_info Open volume to read from and analyze
 * @param a_ftype Type of file system (or autodetect)
 * @param a_pass Password to decrypt filesystem
 *
 * @return NULL on error
 */
TSK_FS_INFO *
tsk_fs_open_vol_decrypt(const TSK_VS_PART_INFO * a_part_info,
    TSK_FS_TYPE_ENUM a_ftype, const char * a_pass)
{
    TSK_OFF_T offset;
    if (a_part_info == NULL) {
        tsk_error_reset();
        tsk_error_set_errno(TSK_ERR_FS_ARG);
        tsk_error_set_errstr("tsk_fs_open_vol: Null vpart handle");
        return NULL;
    }
    else if ((a_part_info->vs == NULL)
        || (a_part_info->vs->tag != TSK_VS_INFO_TAG)) {
        tsk_error_reset();
        tsk_error_set_errno(TSK_ERR_FS_ARG);
        tsk_error_set_errstr("tsk_fs_open_vol: Null vs handle");
        return NULL;
    }

    offset =
        a_part_info->start * a_part_info->vs->block_size +
        a_part_info->vs->offset;
    return tsk_fs_open_img_decrypt(a_part_info->vs->img_info, offset,
        a_ftype, a_pass);
}

/**
 * \ingroup fslib
 * Tries to process data in a disk image at a given offset as a file system.
 * Returns a structure that can be used for analysis and reporting.
 *
 * @param a_img_info Disk image to analyze
 * @param a_offset Byte offset to start analyzing from
 * @param a_ftype Type of file system (or autodetect)
 *
 * @return NULL on error
 */
TSK_FS_INFO *
tsk_fs_open_img(TSK_IMG_INFO * a_img_info, TSK_OFF_T a_offset,
    TSK_FS_TYPE_ENUM a_ftype)
{
    return tsk_fs_open_img_decrypt(a_img_info, a_offset, a_ftype, "");
}

/**
 * \ingroup fslib
 * Tries to process data in a disk image at a given offset as a file system.
 * Allows for providing an optional password for decryption.
 * Returns a structure that can be used for analysis and reporting.
 *
 * @param a_img_info Disk image to analyze
 * @param a_offset Byte offset to start analyzing from
 * @param a_ftype Type of file system (or autodetect)
 * @param a_pass Password to decrypt filesystem. Currently only used if type is specified.
 *
 * @return NULL on error
 */
TSK_FS_INFO *
tsk_fs_open_img_decrypt(TSK_IMG_INFO * a_img_info, TSK_OFF_T a_offset,
    TSK_FS_TYPE_ENUM a_ftype, const char * a_pass)
{
    TSK_FS_INFO *fs_info;
    const struct {
        char* name;
        TSK_FS_INFO* (*open)(TSK_IMG_INFO*, TSK_OFF_T,
                                 TSK_FS_TYPE_ENUM, const char*, uint8_t);
        // This type should be the _DETECT version because it used
        // during autodetection
        TSK_FS_TYPE_ENUM type;
    } FS_OPENERS[] = {
        { "NTFS",     ntfs_open,    TSK_FS_TYPE_NTFS_DETECT    },
        { "FAT",      fatfs_open,   TSK_FS_TYPE_FAT_DETECT     },
        { "EXT2/3/4", ext2fs_open,  TSK_FS_TYPE_EXT_DETECT     },
        { "UFS",      ffs_open,     TSK_FS_TYPE_FFS_DETECT     },
        { "YAFFS2",   yaffs2_open,  TSK_FS_TYPE_YAFFS2_DETECT  },
        { "XFS",      xfs_open,     TSK_FS_TYPE_XFS_DETECT     },
#if TSK_USE_HFS
        { "HFS",      hfs_open,     TSK_FS_TYPE_HFS_DETECT     },
#endif
        { "ISO9660",  iso9660_open, TSK_FS_TYPE_ISO9660_DETECT },
        { "APFS",     apfs_open_auto_detect,    TSK_FS_TYPE_APFS_DETECT },
        { "BTRFS",    btrfs_open,   TSK_FS_TYPE_BTRFS_DETECT },
    };
    if (a_img_info == NULL) {
        tsk_error_reset();
        tsk_error_set_errno(TSK_ERR_FS_ARG);
        tsk_error_set_errstr("tsk_fs_open_img: Null image handle");
        return NULL;
    }

	/* If the image is type IMG_DIR_INFO, then the file system is
	 * automatically the logical directory file system type. It is an
	 * error to try to use any other file system type in that case.
	 */
	if (a_img_info->itype == TSK_IMG_TYPE_LOGICAL) {
		if (!(a_ftype == TSK_FS_TYPE_DETECT || a_ftype == TSK_FS_TYPE_LOGICAL)) {
			tsk_error_reset();
			tsk_error_set_errno(TSK_ERR_FS_ARG);
			tsk_error_set_errstr("tsk_fs_open_img: Incompatible file system type given for logical file image");
			return NULL;
		}

		return logical_fs_open(a_img_info);
	}

    /* We will try different file systems ...
     * We need to try all of them in case more than one matches
     */
    if (a_ftype == TSK_FS_TYPE_DETECT) {
        unsigned long i;
        const char *name_first = "";
        TSK_FS_INFO *fs_first = NULL;

        if (tsk_verbose)
            tsk_fprintf(stderr,
                "fsopen: Auto detection mode at offset %" PRIdOFF "\n",
                a_offset);

        int haveErrorToPreserve = 0;
        uint32_t errorCodeToPreserve;
        char errorStrToPreserve[TSK_ERROR_STRING_MAX_LENGTH + 1];

        for (i = 0; i < sizeof(FS_OPENERS)/sizeof(FS_OPENERS[0]); ++i) {
            if ((fs_info = FS_OPENERS[i].open(
                    a_img_info, a_offset, FS_OPENERS[i].type, a_pass, 1)) != NULL) {
                // fs opens as type i
                if (fs_first == NULL) {
                    // first success opening fs
                    name_first = FS_OPENERS[i].name;
                    fs_first = fs_info;
                }
                else {
                    // second success opening fs, which means we
                    // cannot autodetect the fs type and must give up
                    fs_first->close(fs_first);
                    fs_info->close(fs_info);
                    tsk_error_reset();
                    tsk_error_set_errno(TSK_ERR_FS_MULTTYPE);
                    tsk_error_set_errstr(
                        "%s or %s", FS_OPENERS[i].name, name_first);
                    return NULL;
                }
            }
            else {
                // fs does not open as type i

                // TSK_ERR_FS_BITLOCKER_ERROR is used when we get pretty far into the BitLocker processing but
                // need something different from the user or find something we can't currently parse. We need
                // to keep trying file systems (we might have a FAT system so we don't want to return after
                // failing to open as NTFS) but we also want to be able to get the error back to the user.
                if (tsk_error_get_errno() == TSK_ERR_FS_BITLOCKER_ERROR) {
                    haveErrorToPreserve = 1;
                    errorCodeToPreserve = tsk_error_get_errno();
                    memset(errorStrToPreserve, 0, TSK_ERROR_STRING_MAX_LENGTH + 1);
                    strncpy(errorStrToPreserve, tsk_error_get_errstr(), TSK_ERROR_STRING_MAX_LENGTH);
                }

                tsk_error_reset();
            }
        }

        // If we have an error to report, set it now and return
        if (haveErrorToPreserve) {
            tsk_error_reset();
            tsk_error_set_errno(errorCodeToPreserve);
            tsk_error_set_errstr("%s", errorStrToPreserve);
            return NULL;
        }

        if (fs_first == NULL) {
            tsk_error_reset();

            // If we're still at the start of the image and haven't identified any volume systems or file
            // systems, check if the image type is a known unsupported type.
            int unsupportedSignatureFound = 0;
            if (a_offset == 0) {
                char * imageType = detectUnsupportedImageType(a_img_info);
                if (imageType != NULL) {
                    unsupportedSignatureFound = 1;
                    tsk_error_reset();
                    tsk_error_set_errno(TSK_ERR_IMG_UNSUPTYPE);
                    tsk_error_set_errstr("%s", imageType);
                    free(imageType);
                }
            }

            if (!unsupportedSignatureFound) {
                // Check if the file system appears to be encrypted
                encryption_detected_result* result = detectVolumeEncryption(a_img_info, a_offset);
                if (result != NULL) {
                    if (result->encryptionType == ENCRYPTION_DETECTED_SIGNATURE) {
                        tsk_error_set_errno(TSK_ERR_FS_ENCRYPTED);
                        tsk_error_set_errstr("%s", result->desc);
                    }
                    else if (result->encryptionType == ENCRYPTION_DETECTED_ENTROPY) {
                        tsk_error_set_errno(TSK_ERR_FS_POSSIBLY_ENCRYPTED);
                        tsk_error_set_errstr("%s", result->desc);
                    }
                    else {
                        tsk_error_set_errno(TSK_ERR_FS_UNKTYPE);
                    }
                    free(result);
                    result = NULL;
                }
                else {
                    tsk_error_set_errno(TSK_ERR_FS_UNKTYPE);
                }
            }
        }
        return fs_first;
    }
    else if (TSK_FS_TYPE_ISNTFS(a_ftype)) {
        return ntfs_open(a_img_info, a_offset, a_ftype, a_pass, 0);
    }
    else if (TSK_FS_TYPE_ISFAT(a_ftype)) {
        return fatfs_open(a_img_info, a_offset, a_ftype, a_pass, 0);
    }
    else if (TSK_FS_TYPE_ISFFS(a_ftype)) {
        return ffs_open(a_img_info, a_offset, a_ftype, a_pass, 0);
    }
    else if (TSK_FS_TYPE_ISEXT(a_ftype)) {
        return ext2fs_open(a_img_info, a_offset, a_ftype, a_pass, 0);
    }
    else if (TSK_FS_TYPE_ISHFS(a_ftype)) {
        return hfs_open(a_img_info, a_offset, a_ftype, a_pass, 0);
    }
    else if (TSK_FS_TYPE_ISISO9660(a_ftype)) {
        return iso9660_open(a_img_info, a_offset, a_ftype, a_pass, 0);
    }
    else if (TSK_FS_TYPE_ISRAW(a_ftype)) {
        return rawfs_open(a_img_info, a_offset);
    }
    else if (TSK_FS_TYPE_ISSWAP(a_ftype)) {
        return swapfs_open(a_img_info, a_offset);
    }
    else if (TSK_FS_TYPE_ISYAFFS2(a_ftype)) {
        return yaffs2_open(a_img_info, a_offset, a_ftype, a_pass, 0);
    }
    else if (TSK_FS_TYPE_ISBTRFS(a_ftype)) {
        return btrfs_open(a_img_info, a_offset, a_ftype, a_pass, 0);
    }
    else if (TSK_FS_TYPE_ISAPFS(a_ftype)) {
        return apfs_open(a_img_info, a_offset, a_ftype, a_pass);
    }
    else if (TSK_FS_TYPE_ISXFS(a_ftype)) {
        return xfs_open(a_img_info, a_offset, a_ftype, a_pass, 0);
    }
    tsk_error_reset();
    tsk_error_set_errno(TSK_ERR_FS_UNSUPTYPE);
    tsk_error_set_errstr("%X", (int) a_ftype);
    return NULL;
}

/**
 * \ingroup fslib
 * Close an open file system.
 * @param a_fs File system to close.
 */
void
tsk_fs_close(TSK_FS_INFO * a_fs)
{
    if ((a_fs == NULL) || (a_fs->tag != TSK_FS_INFO_TAG))
        return;

    // each file system is supposed to call tsk_fs_free()

    a_fs->close(a_fs);
}

/* tsk_fs_malloc - init lock after tsk_malloc
 * This is for fs module and all it's inheritances
 */
TSK_FS_INFO *
tsk_fs_malloc(size_t a_len)
{
    TSK_FS_INFO *fs_info;
    if ((fs_info = (TSK_FS_INFO *) tsk_malloc(a_len)) == NULL)
        return NULL;
    tsk_init_lock(&fs_info->list_inum_named_lock);
    tsk_init_lock(&fs_info->orphan_dir_lock);

    fs_info->list_inum_named = NULL;

    return fs_info;
}

/* tsk_fs_free - deinit lock before free memory
 * This is for fs module and all it's inheritances
 */
void
tsk_fs_free(TSK_FS_INFO * a_fs_info)
{
    if (a_fs_info->list_inum_named) {
        tsk_list_free(a_fs_info->list_inum_named);
        a_fs_info->list_inum_named = NULL;
    }

    /* Free any encryption structures */
    freeEncryptionData(a_fs_info);

    /* we should probably get the lock, but we're
     * about to kill the entire object so there are
     * bigger problems if another thread is still
     * using the fs */
    if (a_fs_info->orphan_dir) {
        tsk_fs_dir_close(a_fs_info->orphan_dir);
        a_fs_info->orphan_dir = NULL;
    }


    tsk_deinit_lock(&a_fs_info->list_inum_named_lock);
    tsk_deinit_lock(&a_fs_info->orphan_dir_lock);

    free(a_fs_info);
}
