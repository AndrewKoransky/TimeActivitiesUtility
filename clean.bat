@setlocal
@echo off
if .%1. NEQ .. echo Cleaning %1
if .%1. NEQ .. pushd "%1"
if .%1. EQU .. echo Cleaning "%CD%"
if exist *.plg del *.plg
if exist *.opt del *.opt
if exist *.suo attrib -h *.suo
if exist *.suo del *.suo
if exist *.user del *.user
if exist *.csproj.webinfo del *.csproj.webinfo
if exist *.ncb del *.ncb
if exist *.aps del *.aps
if exist *.tlh del *.tlh
if exist *.tli del *.tli
if exist *.clw del *.clw
if exist *.vbw del *.vbw
if exist *.lnt del *.lnt
if exist *.tmp del *.tmp
if exist *.tgs del *.tgs
if exist *.tgw del *.tgw
if exist *.WW del *.WW
if exist *.stt del *.stt
if exist *.scc del /f *.scc
if exist setup.exe del setup.exe
if exist *.cab del *.cab
if exist del *~ del *~
if exist *.exe del *.exe
if exist *.oca del /s *.oca
if exist *.class del /s *.class
if exist *.msi del *.msi
if exist MAKE*.TXT del MAKE*.TXT
if exist *.pat del /s *.pat
if exist *.chm del /s *.chm
if exist *.log del /s *.log
if exist *.dso del /s *.dso
if exist ftsource*.* del /f ftsource*.*
if exist html rmdir /q /s html
if exist ".vs" rmdir /q /s ".vs"
if exist hlp\html rmdir /q /s hlp\html
if exist WMIPSDbg rmdir /q /s WMIPSDbg
if exist WMIPSRel rmdir /q /s WMIPSRel
if exist WCESH3Dbg rmdir /q /s WCESH3Dbg
if exist WCESH3Rel rmdir /q /s WCESH3Rel
if exist x86emDbg rmdir /q /s x86emDbg
if exist x86emRel rmdir /q /s x86emRel
if exist Support rmdir /q /s Support
if exist LibrarySupport rmdir /q /s LibrarySupport
if exist obj rmdir /q /s obj
if exist bin rmdir /q /s bin
for /d %%1 in (Debug*) do rmdir /s /q %%1
for /d %%1 in (Release*) do rmdir /s /q %%1
for /d %%1 in ("Win32 Release*") do rmdir /s /q "%%1"
for /d %%1 in ("Win32 Debug*") do rmdir /s /q "%%1"
if "%2" == "" goto end
if exist dlldata.c del dlldata.c
if exist %2.h del %2.h
if exist %2_i.c del %2_i.c
if exist %2_p.c del %2_p.c
if exist %2.tlb del %2.tlb
:end
if .%1. NEQ .. popd
endlocal
