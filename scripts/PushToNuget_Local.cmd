Echo Creates Debug Packages and pushes to Local Nuget Repo

set packages="..\packages\release"
del %packages%\*.nupkg

dotnet pack -o %packages% ..\src\IS.AppRun

for %%n in (%packages%\*.nupkg) do  dotnet nuget push -s d:\a_dev\LocalNugetPackages "%%n"
