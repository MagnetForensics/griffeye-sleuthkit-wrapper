/*
 * Sleuth Kit Data Model
 *
 * Copyright 2017 Basis Technology Corp.
 * Contact: carrier <at> sleuthkit <dot> org
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *  http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
package org.sleuthkit.datamodel;

/**
 * Encapsulates an Account existing on a specific device.
 * 
 * There is a 1:M:N relationship between 
 * Account, AccountDeviceInstance &  AccountFileInstance
 */
public class AccountDeviceInstance {
	private final Account account;				
	private final String deviceID;	
	
	AccountDeviceInstance(Account account, String deviceId) {
		this.account = account;
		this.deviceID = deviceId;
	}

	/**
	 *  Returns the underlying Account
	 * 
	 * @return account
	 */
	public Account getAccount(){
		return this.account;
	}
	
	/**
	 *  Returns the device Id the Account existed on
	 * 
	 * @return device id
	 */
	public String getDeviceId(){
		return this.deviceID;
	}
}
