```
      _            _   _     _    _ _              _                      
     | |          | | | |   | |  (_) |            | |                     
  ___| | ___ _   _| |_| |__ | | ___| |_ ______ ___| |__   __ _ _ __ _ __  
 / __| |/ _ \ | | | __| '_ \| |/ / | __|______/ __| '_ \ / _` | '__| '_ \ 
 \__ \ |  __/ |_| | |_| | | |   <| | |_       \__ \ | | | (_| | |  | |_) |
 |___/_|\___|\__,_|\__|_| |_|_|\_\_|\__|      |___/_| |_|\__,_|_|  | .__/ 
                                                                   | |    
                                                                   |_|
```
																   
#### Was ist das? What is it? Que es?
A .NET wrapper for The Sleuth Kit [www.sleuthkit.org].  It provides callers easy access to disks, volumes, filesystems, and files.  This assumes that you have some understanding of what The Sleuth Kit (TSK) is, and maybe some concept of digital forensics.  You don't necessarily need to know much about these things, after all that is the point of this library.  To lower the bar and make it easier for you to deal directly with disk images (dd, encase, etc), filesystems (ntfs, fat, etc), and all that other jazz.

#### License
Eclipse Public License v1.0 <http://www.eclipse.org/legal/epl-v10.html>

#### Not Building
just get one of the binary releases and be happy with it.  it was compiled it on what we believe is a clean virtual machine, should not cause too many dependency issues.  maybe visual c redistributable.  figuring this out as we go.

#### Building
if you are on windows, do not use mono - we need msbuild to be able to create libtsk3.dll
in other words you need the microsoft c/c++ compiler - available in the Windows SDK or Visual Studio (C++ Express Edition OK)

on mac and linux:
instructions are still in progress.  if you figure it out or otherwise hack it together let me know and your instructions can be pasted here.

On Mac and Linux when you build (aka "configure; make; sudo make install") TSK there is a shared library that gets created (libtsk3) that contains all of the methods that we use for P/Invoke.  The Visual Studio version of TSK does not make a DLL, but rather a few static libraries - so we have libtsk3-win32, our friendly DLL wrapper of all of those libraries.  Presumably it gives us parity with the Linux and Mac builds of TSK.
Windows version of libtsk3 (libtsk3.dll).  That way the same .NET wrapper can be used across all OSes that can build TSK and run .NET/Mono.