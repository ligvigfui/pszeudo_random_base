::version number increase
dotnet build --configuration Release
cd bin\Release
dotnet nuget push Pszeudo_random_base.1.1.0.nupkg --api-key oy2gylpvqnsg5v64dk5aggx53gudli3e7odkplvljzpa5i --source https://api.nuget.org/v3/index.json