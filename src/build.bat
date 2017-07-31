set WORKING_DIRECTORY=%cd%
::Go in current script folder
SET mypath=%~dp0
cd %mypath%
cd libary
::Clean nuget folder if any
IF NOT EXIST nuget GOTO NONUGET
   rd nuget /S /Q
:NONUGET
mkdir nuget
::restore and build lib
dotnet restore
dotnet build
::go to test dir
cd ..
cd test
::restore,build and run tests
dotnet restore
dotnet build
dotnet test
::back to root
cd ..
::create a nuget to test release building as last step
cd libary
dotnet pack pillowsharp.csproj  -c Release -o nuget
::back to calling folder
cd %WORKING_DIRECTORY%


