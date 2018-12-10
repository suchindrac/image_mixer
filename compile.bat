csc /target:winexe /win32icon:imageMixer.ico /out:imageMixer.exe imageMixer.cs /reference:Microsoft.VisualBasic.dll
del im.exe
copy imageMixer.exe im.exe