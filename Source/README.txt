The folders libewf, zlib, bzip2 and and sleuthkit contains the release contents of their respective projects.

When upgrading libewf and sleuthkit, just replace the contents of their folder with the new release contents.

After that some changes are necessary to get the builds working.

LIBEWF (Library to access the Expert Witness Compression Format (EWF)):
Use the version here: https://github.com/sleuthkit/libewf_64bit

1. Open the solution (libewf\msvscpp\libewf.sln) in the latest Visual Studio version that is supported by the builders
2. Disable build of pyewf project (and dokan and ewfmount if they exist in the solution)
3. Change build output of bzip2 project to static library if it exists
4. Build in Release

LIBVMDK (Library to access the VMware Virtual Disk (VMDK) format):
Use the version here: https://github.com/sleuthkit/libvmdk_64bit
Open and build libvmdk\libvmdk\msvscpp\libvmdk.sln in Release

LIBVHDI (Library to access the Virtual Hard Disk (VHD) image format):
Use the version here: https://github.com/sleuthkit/libvhdi_64bit
Open and build libvhdi\msvscpp\libvhdi.sln in Release


SLEUTHKIT:

1. Open the solution in the latest Visual Studio version that is supported by the builders and update the toolset if required.
   Make sure you adjust 4 xcopy rows in libtsk.vcxproj if toolset is changed.
   Possibly discard changes to Source\sleuthkit\tools\logicalimager\LogicalImagerRuleBase.cpp
   Apply patches with our changes
      git am Source/0001-make-a-few-changes-from-original-sleuthkit-source.patch
2. Disable build of the libtsk_jni project. Remove it from solution file.
3. Run SetEnvironmentVaraiblesForSleuthkit.ps1 to set environment variables for build


SLEUTHKIT SHARP:

When building libtsk4 files are copied from Release folder of above dependencies into Lib\x64 folder.
When building unit-tests project files from Lib\x64 are copied into this project as a PreBuildEvent


Building a nuget locally:

nuget.exe pack "Source\Sleuthkit Wrapper\src\NuSpec\SleuthkitSharp.nuspec" -OutputDirectory .
