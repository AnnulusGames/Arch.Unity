cd /d %~dp0

dotnet build Arch.Unity.SourceGenerator.sln -c Release

copy /Y ".\Arch.Unity.SourceGenerator\bin\Release\netstandard2.0\Arch.Unity.SourceGenerator.dll" "..\Arch.Unity\Assets\Arch.Unity\Runtime\SourceGenerators\Arch.Unity.SourceGenerator.dll"
copy /Y ".\Arch.Unity.SourceGenerator\bin\Release\netstandard2.0\Arch.Unity.SourceGenerator.pdb" "..\Arch.Unity\Assets\Arch.Unity\Runtime\SourceGenerators\Arch.Unity.SourceGenerator.pdb"