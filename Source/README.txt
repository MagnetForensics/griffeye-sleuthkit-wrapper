The folders libewf, zlib, bzip2 and and sleuthkit contains the release contents of their respective projects.

When upgrading libewf and sleuthkit, just replace the contents of their folder with the new release contents.

After that some changes are necessary to get the builds working.

LIBEWF:
Use the version here: https://github.com/sleuthkit/libewf_64bit

1. Open the solution in the latest Visual Studio version that is supported by the builders
2. Disable build of pyewf project (and dokan and ewfmount if they exist in the solution)
3. Add signing of ewf.net under Properties->Linker->Advanced
	Set LinkerKeyFile to ..\..\..\ncpub.snk
	Set LinkDellaySign to Yes
4. Change build output of bzip2 project to static library 

LIBVMDK:
Use the version here: https://github.com/sleuthkit/libvmdk_64bit

LIBVHDI:
Use the version here: https://github.com/sleuthkit/libvhdi_64bit

SLEUTHKIT:

1. Open the solution in the latest Visual Studio version that is supported by the builders
2. Disable build of libtsk_jni project
3. Replace $(LIBEWF_HOME) with $(ProjectDir)\..\..\..\libewf in all .vcxproj files 
	(For example by opening all files in  notepad++ and doing a find and replace in all opened documents)
3. Replace $(LIBVMDK_HOME) with $(ProjectDir)\..\..\..\libvmdk\libvmdk in all .vcxproj files 
3. Replace $(LIBVHDI_HOME) with $(ProjectDir)\..\..\..\libvhdi in all .vcxproj files 
4. Replace "Windows7.1SDK" with "v140" in all .vcxproj files
	
	
SLEUTHKIT SHARP:
Following copies are necessary
To build x86:
copy /y "Source\libewf\msvscpp\Release\ewf.net.dll" "Lib\x86\"
copy /y "Source\libewf\msvscpp\Release\libewf.dll" "Lib\x86\"
copy /y "Source\libewf\msvscpp\Release\libewf.lib" "Lib\x86\"
copy /y "Source\libewf\msvscpp\Release\zlib.dll" "Lib\x86\"
copy /y "Source\libewf\msvscpp\Release\zlib.lib" "Lib\x86\"
copy /y "Source\sleuthkit\win32\Release\libtsk.lib" "Lib\x86\"

To run unit tests in x86:
copy /y "Lib\x86\libewf.dll" "Source\Sleuthkit Wrapper\unit-tests\bin\x86\Release\"
copy /y "Lib\x86\libtsk4.dll" "Source\Sleuthkit Wrapper\unit-tests\bin\x86\Release\"
copy /y "Lib\x86\zlib.dll" "Source\Sleuthkit Wrapper\unit-tests\bin\x86\Release\"
copy /y "Lib\x86\ewf.net.dll" "Source\Sleuthkit Wrapper\unit-tests\bin\x86\Release\"
copy /y "Source\Sleuthkit Wrapper\src\bin\x86\Release\sleuthkit-sharp.dll" "Source\Sleuthkit Wrapper\unit-tests\bin\x86\Release\"

To build x64:
copy /y "Source\libewf\msvscpp\x64\Release\ewf.net.dll" "Lib\x64\"
copy /y "Source\libewf\msvscpp\x64\Release\libewf.dll" "Lib\x64\"
copy /y "Source\libewf\msvscpp\x64\Release\libewf.lib" "Lib\x64\"
copy /y "Source\libewf\msvscpp\x64\Release\zlib.dll" "Lib\x64\"
copy /y "Source\libewf\msvscpp\x64\Release\zlib.lib" "Lib\x64\"
copy /y "Source\sleuthkit\win32\x64\Release\libtsk.lib" "Lib\x64\"

To run unit tests in x64:
copy /y "Lib\x64\libewf.dll" "Source\Sleuthkit Wrapper\unit-tests\bin\x64\Release\"
copy /y "Lib\x64\libtsk4.dll" "Source\Sleuthkit Wrapper\unit-tests\bin\x64\Release\"
copy /y "Lib\x64\zlib.dll" "Source\Sleuthkit Wrapper\unit-tests\bin\x64\Release\"
copy /y "Lib\x64\ewf.net.dll" "Source\Sleuthkit Wrapper\unit-tests\bin\x64\Release\"
copy /y "Source\Sleuthkit Wrapper\src\bin\x64\Release\sleuthkit-sharp.dll" "Source\Sleuthkit Wrapper\unit-tests\bin\x64\Release\"
	