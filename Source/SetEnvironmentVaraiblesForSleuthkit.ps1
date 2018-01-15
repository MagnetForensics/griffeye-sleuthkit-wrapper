$path = (Get-Item -Path ".\" -Verbose).FullName

$path_libvmdk = Join-Path -path $path -childpath "libvmdk\libvmdk"
[Environment]::SetEnvironmentVariable("LIBVMDK_HOME", $path_libvmdk ,"User")

$path_libvhdi = Join-Path -path $path -childpath "libvhdi"
[Environment]::SetEnvironmentVariable("LIBVHDI_HOME", $path_libvhdi ,"User")

$path_libewf = Join-Path -path $path -childpath "libewf"
[Environment]::SetEnvironmentVariable("LIBEWF_HOME", $path_libewf ,"User")
