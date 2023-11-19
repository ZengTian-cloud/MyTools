@echo off
setlocal enabledelayedexpansion
echo "请输入所要添加的标题前缀[不添请回车]"
set /p str1=
echo "请输入所要添加的标题后缀[不添请回车]"
set /p str2=

:chose
echo "是否应用到子文件夹中(Y/N)"
set /p cho=
if "%cho%"=="Y" goto 1
if "%cho%"=="y" goto 1
if "%cho%"=="N" goto 2
if "%cho%"=="n" (goto 2) else (goto chose)

:1
for /f "delims=" %%i in ('dir /a-d/b/s') do (if /i not "%%~fi"=="%~f0" ren "%%i" "%str1%%%~ni%str2%%%~xi")
goto 3

:2
for /f "delims=" %%i in ('dir /a-d /b *.*') do (if /i not "%%~fi"=="%~f0" ren "%%i" "%str1%%%~ni%str2%%%~xi")
goto 3

:3
pause