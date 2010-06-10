 
echo on
call %1
cd %3..\..\
csc /t:module /out:EasyInstall.netmodule /reference:ICSharpCode.SharpZipLib.dll *.cs .\Properties\*.cs