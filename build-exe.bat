@echo off
echo Building single-file executable...
dotnet publish LibraryManagementFE.csproj -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true

if exist "LibraryApp.exe" del "LibraryApp.exe"
copy "bin\Release\net9.0-windows\win-x64\publish\LibraryManagementFE.exe" "LibraryApp.exe"

echo.
echo Done! File exe: LibraryApp.exe
pause
