/*
* The Sleuth Kit
*
* Brian Carrier [carrier <at> sleuthkit [dot] org]
* Copyright (c) 2012-2014 Brian Carrier.  All rights reserved
*
*
* This software is distributed under the Common Public License 1.0
*/

/**
* \file encase.c
* Contains the Encase hash database specific extraction and printing routines.
*/

#include "tsk_hashdb_i.h"

/**
* Test the file to see if it is an Encase database
*
* @param hFile File handle to hash database
*
* @return 1 if encase and 0 if not
*/
uint8_t
    encase_test(FILE * hFile)
{
    char buf[8];

    fseeko(hFile, 0, SEEK_SET);
    if (8 != fread(buf, sizeof(char), 8, hFile))
        return 0;

    if (memcmp(buf, "HASH\x0d\x0a\xff\x00", 8))
        return 0;

    return 1;
}

/**
* Set db_name using information from this database type
*
* @param hdb_info the hash database object
*/
static void
    encase_name(TSK_HDB_BINSRCH_INFO * hdb_info)
{
    FILE * hFile = hdb_info->hDb;
    wchar_t buf[40];
    UTF16 *utf16;
    UTF8 *utf8;
    size_t ilen;
    memset(hdb_info->base.db_name, '\0', TSK_HDB_NAME_MAXLEN);
    if(!hFile) {
        if (tsk_verbose)
            fprintf(stderr,
            "Error getting name from Encase hash db; using file name instead");
        hdb_base_db_name_from_path(&hdb_info->base);
        return;
    }

    memset(buf, '\0', sizeof(buf));

    fseeko(hFile, 1032, SEEK_SET);
    if (39 != fread(buf, sizeof(wchar_t), 39, hFile)) {
        if (tsk_verbose)
            fprintf(stderr,
            "Error getting name from Encase hash db; using file name instead");
        hdb_base_db_name_from_path(&hdb_info->base);
        return;
    }

    // do some arithmetic on type sizes to be platform independent
    ilen = wcslen(buf) * (sizeof(wchar_t) / sizeof(UTF16));

    utf8 = (UTF8 *) hdb_info->base.db_name;
    utf16 = (UTF16 *) buf;

    tsk_UTF16toUTF8(TSK_LIT_ENDIAN,
        (const UTF16 **) &utf16,
        &utf16[ilen], &utf8, &utf8[78],
        TSKlenientConversion);
}


TSK_HDB_INFO *encase_open(FILE *hDb, const TSK_TCHAR *db_path)
{
    TSK_HDB_BINSRCH_INFO *hdb_binsrch_info = NULL;

    // get the basic binary-search info struct
    hdb_binsrch_info = hdb_binsrch_open(hDb, db_path);
    if (NULL == hdb_binsrch_info) {
        return NULL;
    }

    // overwrite the database-specific ones
    hdb_binsrch_info->base.db_type = TSK_HDB_DBTYPE_ENCASE_ID;
    encase_name(hdb_binsrch_info);
    hdb_binsrch_info->base.make_index = encase_make_index;
    hdb_binsrch_info->get_entry = encase_get_entry;

    return (TSK_HDB_INFO*)hdb_binsrch_info;
}

/**
* Process the database to create a sorted index of it. Consecutive
* entries with the same hash value are not added to the index, but
* will be found during lookup.
*
* @param hdb_info_base Hash database to make index of.
* @param dbtype Type of hash database (should always be TSK_HDB_DBTYPE_ENCASE_STR)
*
* @return 1 on error and 0 on success.
*/
uint8_t
    encase_make_index(TSK_HDB_INFO * hdb_info_base, TSK_TCHAR * dbtype)
{
    TSK_HDB_BINSRCH_INFO *hdb_binsrch_info = (TSK_HDB_BINSRCH_INFO*)hdb_info_base;
    unsigned char buf[19];
    char phash[19];
    TSK_OFF_T offset = 0;
    int db_cnt = 0, idx_cnt = 0;

    /* Initialize the TSK index file */
    if (hdb_binsrch_idx_initialize(hdb_binsrch_info, dbtype)) {
        tsk_error_set_errstr2( "encase_makeindex");
        return 1;
    }

    /* Status */
    if (tsk_verbose)
        TFPRINTF(stderr, _TSK_T("Extracting Data from Database (%" PRIttocTSK ")\n"),
        hdb_binsrch_info->base.db_fname);

    memset(phash, '0', sizeof(phash));
    memset(buf, '0', sizeof(buf));

    /* read the file and add to the index */
    fseek(hdb_binsrch_info->hDb, 1152, SEEK_SET);
    while (18 == fread(buf,sizeof(char),18,hdb_binsrch_info->hDb)) {
        db_cnt++;

        /* We only want to add one of each hash to the index */
        if (memcmp(buf, phash, 18) == 0) {
            continue;
        }

        /* Add the entry to the index */
        if (hdb_binsrch_idx_add_entry_bin(hdb_binsrch_info, buf, 16, offset)) {
            tsk_error_set_errstr2( "encase_make_index");
            return 1;
        }

        idx_cnt++;

        /* Set the previous has value */
        memcpy(phash, buf, 18);
        offset += 18;
    }

    if (idx_cnt > 0) {
        if (tsk_verbose) {
            fprintf(stderr, "  Valid Database Entries: %d\n", db_cnt);
            fprintf(stderr, "  Index File Entries %s: %d\n",
                (idx_cnt == db_cnt) ? "" : "(optimized)", idx_cnt);
        }

        /* Close and sort the index */
        if (hdb_binsrch_idx_finalize(hdb_binsrch_info)) {
            tsk_error_set_errstr2( "encase_makeindex");
            return 1;
        }
    }
    else {
        tsk_error_reset();
        tsk_error_set_errno(TSK_ERR_HDB_CORRUPT);
        tsk_error_set_errstr(
            "encase_makeindex: No valid entries found in database");
        return 1;
    }

    return 0;
}

/**
* Find the entry at a
* given offset.  The offset was likely determined from the index.
* The callback is called for each entry. EnCase does not store names,
* so the callback is called with just the hash value.
*
* @param hdb_info Hash database to get data from
* @param hash MD5 hash value that was searched for
* @param offset Byte offset where hash value should be located in db_file
* @param flags (not used)
* @param action Callback used for each entry found in lookup
* @param cb_ptr Pointer to data passed to callback
*
* @return 1 on error and 0 on succuss
*/
uint8_t
encase_get_entry(
  TSK_HDB_INFO * hdb_info,
  const char *hash,
  TSK_OFF_T offset,
  [[maybe_unused]] TSK_HDB_FLAG_ENUM flags,
  TSK_HDB_LOOKUP_FN action,
  void *cb_ptr)
{
    TSK_HDB_BINSRCH_INFO *hdb_binsrch_info = (TSK_HDB_BINSRCH_INFO*)hdb_info;
    int found = 0;
    char buf[19];

    if (tsk_verbose)
        fprintf(stderr,
        "encase_getentry: Lookup up hash %s at offset %" PRIdOFF
        "\n", hash, offset);

    if (strlen(hash) != TSK_HDB_HTYPE_MD5_LEN) {
        tsk_error_reset();
        tsk_error_set_errno(TSK_ERR_HDB_ARG);
        tsk_error_set_errstr(
            "encase_getentry: Invalid hash value: %s", hash);
        return 1;
    }

    memset(buf, 0, sizeof(buf));

    /* Loop so that we can find multiple occurrences of the same hash */
    fseeko(hdb_binsrch_info->hDb, offset, SEEK_SET);
    while (1) {
        int retval;
        char hash_str[TSK_HDB_HTYPE_MD5_LEN+1];

        if (18 != fread(buf,sizeof(char),18,hdb_binsrch_info->hDb)) {
            if (feof(hdb_binsrch_info->hDb)) {
                break;
            }
            tsk_error_reset();
            tsk_error_set_errno(TSK_ERR_HDB_READDB);
            tsk_error_set_errstr(
                "encase_getentry: Error reading database");
            return 1;
        }

        snprintf(hash_str, TSK_HDB_HTYPE_MD5_LEN+1, "%02X%02X%02X%02X%02X%02X%02X%02X%02X%02X%02X%02X%02X%02X%02X%02X",
            buf[0], buf[1], buf[2], buf[3], buf[4], buf[5], buf[6], buf[7],
            buf[8], buf[9], buf[10], buf[11], buf[12], buf[13], buf[14], buf[15]);

        /* Is this the one that we want? */
        if (0 != strcasecmp(hash_str, hash)) {
            break;
        }

        retval = action(hdb_info, hash, "", cb_ptr);
        if (retval == TSK_WALK_ERROR) {
            return 1;
        }
        else if (retval == TSK_WALK_STOP) {
            return 0;
        }
        found = 1;

        /* Advance to the next row */
        offset += 18;
    }

    if (found == 0) {
        tsk_error_reset();
        tsk_error_set_errno(TSK_ERR_HDB_ARG);
        tsk_error_set_errstr(
            "encase_getentry: Hash not found in file at offset: %lu",
            (unsigned long) offset);
        return 1;
    }

    return 0;
}
