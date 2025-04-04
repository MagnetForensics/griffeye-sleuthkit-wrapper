/*
 * The Sleuth Kit - Add on for Expert Witness Compression Format (EWF) image support
 *
 * Copyright (c) 2006, 2011 Joachim Metz <jbmetz@users.sourceforge.net>
 *
 * This software is distributed under the Common Public License 1.0
 *
 * Based on raw image support of the Sleuth Kit from
 * Brian Carrier.
 */

/*
 * Header files for EWF-specific data structures and functions.
 */

#ifndef _TSK_IMG_EWF_H
#define _TSK_IMG_EWF_H

#if HAVE_LIBEWF

#include <libewf.h>

#include <optional>
#include <string>
#include <vector>

#include "../base/tsk_os_cpp.h"

#ifdef __cplusplus
extern "C" {
#endif

    extern TSK_IMG_INFO *ewf_open(int, const TSK_TCHAR * const images[],
        unsigned int a_ssize);

    typedef struct {
        TSK_IMG_INFO img_info;
        libewf_handle_t *handle;
        char md5hash[33];
        int md5hash_isset;
        char sha1hash[41];
        int sha1hash_isset;
        tsk_lock_t read_lock;   ///< Lock for reads since libewf is not thread safe -- only works if you have a single instance of EWF_INFO for all threads.
    } IMG_EWF_INFO;


#ifdef __cplusplus
}
#endif
    extern std::string ewf_get_details(IMG_EWF_INFO *);

    std::optional<std::vector<TSK_TSTRING>>
    glob_E01(const TSK_TCHAR* image_native);
#endif
#endif
