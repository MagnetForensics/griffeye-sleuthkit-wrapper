/*
 * Sleuth Kit Data Model
 * 
 * Copyright 2011-2017 Basis Technology Corp.
 * Contact: carrier <at> sleuthkit <dot> org
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *     http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
package org.sleuthkit.datamodel;

import java.util.List;
import java.util.Map;
import org.apache.commons.lang3.ArrayUtils;
import org.apache.commons.lang3.StringUtils;
import com.google.gson.Gson;
import com.google.gson.JsonSyntaxException;
import static org.sleuthkit.datamodel.SleuthkitCase.IMAGE_PASSWORD_KEY;

/**
 * Represents a file system object stored in tsk_fs_info table FileSystem has a
 * parent content object (volume or image) and children content objects (files
 * and directories) and fs-specific attributes. The object also maintains a
 * handle to internal file-system structures and the handle is reused across
 * reads.
 */
public class FileSystem extends AbstractContent {

	private long imgOffset, blockSize, blockCount, rootInum,
			firstInum, lastInum;
	private TskData.TSK_FS_TYPE_ENUM fsType;
	private Content parent;
	private volatile long filesystemHandle = 0;

	/**
	 * Constructor most inputs are from the database
	 *
	 * @param db          the case handle
	 * @param obj_id      the unique object id
	 * @param name        filesystem name
	 * @param img_offset  image offset
	 * @param fs_type     filesystem type
	 * @param block_size  block size in this fs
	 * @param block_count number of blocks in this fs
	 * @param root_inum   the root inum
	 * @param first_inum  the first inum
	 * @param last_inum   the last inum
	 */
	protected FileSystem(SleuthkitCase db, long obj_id, String name, long img_offset,
			TskData.TSK_FS_TYPE_ENUM fs_type, long block_size, long block_count, long root_inum,
			long first_inum, long last_inum) {
		super(db, obj_id, name);
		this.imgOffset = img_offset;
		this.fsType = fs_type;
		this.blockSize = block_size;
		this.blockCount = block_count;
		this.rootInum = root_inum;
		this.firstInum = first_inum;
		this.lastInum = last_inum;
	}

	@Override
	public void close() {
		//does nothing currently, we are caching the fs handles
	}

	@Override
	public int read(byte[] buf, long offset, long len) throws TskCoreException {
		Content dataSource = getDataSource();
		if (dataSource instanceof Image && ArrayUtils.isEmpty(((Image) dataSource).getPaths())) {
			return 0;
		}
		return SleuthkitJNI.readFs(getFileSystemHandle(), buf, offset, len);
	}

	@Override
	public long getSize() {
		return blockSize * blockCount;
	}

	/**
	 * Lazily loads the internal file system structure: won't be loaded until
	 * this is called and maintains the handle to it to reuse it
	 *
	 * @return a filesystem pointer from the sleuthkit
	 *
	 * @throws TskCoreException exception throw if an internal tsk core error
	 *                          occurs
	 */
	long getFileSystemHandle() throws TskCoreException {
		if (filesystemHandle == 0) {
			synchronized (this) {
				if (filesystemHandle == 0) {
					Content dataSource = getDataSource();
					if ((dataSource == null) || ( !(dataSource instanceof Image))) {
						throw new TskCoreException("Data Source of File System is not an image");
					}

					Image image = (Image) dataSource;

					// Check if this file system is in a pool
					if (isPoolContent()) {
						Pool pool = getPool();
						if (pool == null) {
							throw new TskCoreException("Error finding pool for file system");
						}

						Volume poolVolume = getPoolVolume();
						if (poolVolume == null) {
							throw new TskCoreException("File system is in a pool but has no volume");
						}
						filesystemHandle = SleuthkitJNI.openFsPool(image.getImageHandle(), imgOffset, pool.getPoolHandle(), poolVolume.getStart(), getSleuthkitCase());
					} else {
						String password = getImagePasswordFromSettings(image.getAcquisitionToolSettings());
						filesystemHandle = SleuthkitJNI.openFs(image.getImageHandle(), imgOffset, password, getSleuthkitCase());
					}
				}
			}
		}
		return this.filesystemHandle;
	}
	
	/**
	 * Attempt to read the image password from the settings string 
	 * 
	 * @param settingsStr
	 * 
	 * @return the password if found, empty string otherwise
	 */
	private String getImagePasswordFromSettings(String settingsStr) {
		
		if(StringUtils.isBlank(settingsStr)){
			return "";
		}

		try {
			Map<String, Object> settingsMap = (new Gson()).fromJson(settingsStr, Map.class);
			return (String)settingsMap.getOrDefault(IMAGE_PASSWORD_KEY, "");
		} catch (JsonSyntaxException ex) {
			// There's no guarantee that acquisition settings will contain a valid JSON string
			return "";
		}
	}

	public Directory getRootDirectory() throws TskCoreException {

		List<Content> children = getChildren();
		if (children.size() != 1) {
			throw new TskCoreException("FileSystem must have only one child.");
		}

		if (!(children.get(0) instanceof Directory)) {
			throw new TskCoreException("Child of FileSystem must be a Directory.");
		}

		return (Directory) children.get(0);
	}

	/**
	 * Get the byte offset of this file system in the image
	 *
	 * @return offset
	 */
	public long getImageOffset() {
		return imgOffset;
	}

	/**
	 * Get the file system type
	 *
	 * @return enum value of fs type
	 */
	public TskData.TSK_FS_TYPE_ENUM getFsType() {
		return fsType;
	}

	/**
	 * Get the block size
	 *
	 * @return block size
	 */
	public long getBlock_size() {
		return blockSize;
	}

	/**
	 * Get the number of blocks
	 *
	 * @return block count
	 */
	public long getBlock_count() {
		return blockCount;
	}

	/**
	 * Get the inum of the root directory
	 *
	 * @return Root metadata address of the file system
	 */
	public long getRoot_inum() {
		return rootInum;
	}

	/**
	 * Get the first inum in this file system
	 *
	 * @return first inum
	 */
	public long getFirst_inum() {
		return firstInum;
	}

	/**
	 * Get the last inum
	 *
	 * @return last inum
	 */
	public long getLastInum() {
		return lastInum;
	}

	@SuppressWarnings("deprecation")
	@Override
	public void finalize() throws Throwable {
		try {
			if (filesystemHandle != 0) {
				// SleuthkitJNI.closeFs(filesystemHandle); // closeFs is currently a no-op
				filesystemHandle = 0;
			}
		} finally {
			super.finalize();
		}
	}
	
	@Override
	public <T> T accept(SleuthkitItemVisitor<T> v) {
		return v.visit(this);
	}

	@Override
	public <T> T accept(ContentVisitor<T> v) {
		return v.visit(this);
	}

	@Override
	public String toString(boolean preserveState) {
		return super.toString(preserveState) + "FileSystem [\t" + " blockCount " + blockCount + "\t" + "blockSize " + blockSize + "\t" + "firstInum " + firstInum + "\t" + "fsType " + fsType + "\t" + "imgOffset " + imgOffset + "\t" + "lastInum " + lastInum + "\t" + "rootInum " + rootInum + "\t" + "]"; //NON-NLS
	}
}
