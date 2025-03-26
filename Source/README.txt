Since 4.10.2 the depencencies libvmdk, libvhdi, libewf no longer needs to be build. They are fetched from nuget.

SLEUTHKIT:

1. When upgrading sleuthkit, just replace the contents of their folder with the new downloaded zip of the source code .
2. Open the solution ("Source\sleuthkit\win32\tsk-win.sln") in the latest Visual Studio version that is supported by the builders and update the toolset if required.
   Make sure you adjust 4 xcopy rows in libtsk.vcxproj if toolset is changed.
   xcopy /E /Y "$(VCInstallDir)\redist\MSVC\$(VCToolsRedistVersion)\$(PlatformTarget)\Microsoft.VC142.CRT" "$(OutDir)
   to
   xcopy /E /Y "$(VCInstallDir)\redist\MSVC\$(VCToolsRedistVersion)\$(PlatformTarget)\Microsoft.VC143.CRT" "$(OutDir)

   Apply patches with our changes
      git am Source/0001-make-a-few-changes-from-original-sleuthkit-source.patch
      git am Source/0002-add-includes-needed-by-new-platformtoolset.patch
3. Disable build of the libtsk_jni project. Remove it from solution file.
4. Run SetEnvironmentVaraiblesForSleuthkit.ps1 to set environment variables for build, step might not be needed anymore!?


SLEUTHKIT SHARP:

When building libtsk4 files are copied from Release folder of above dependencies into Lib\x64 folder.
When building unit-tests project files from Lib\x64 are copied into this project as a PreBuildEvent


Building a nuget locally:

nuget.exe pack "Source\Sleuthkit Wrapper\src\NuSpec\SleuthkitSharp.nuspec" -OutputDirectory .
