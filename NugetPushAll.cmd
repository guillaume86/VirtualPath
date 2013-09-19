cd VirtualPath
call NugetPush.cmd %1
cd ..\VirtualPath.AlexFTPS
call NugetPush.cmd %1
cd ..\VirtualPath.SshNet
call NugetPush.cmd %1
cd ..\VirtualPath.DropNet
call NugetPush.cmd %1
cd ..
