set shell := ["powershell", "-Command"]

# justfile for Svelonia (Windows Optimized)

default:
	just --list

clean:
	dotnet clean
	if (Test-Path artifacts) { rm artifacts -Recurse -Force }
	echo "Cleaned up."

# Pack all libraries
pack version:
	echo "1. Updating Directory.Build.props to {{version}}..."
	(Get-Content Directory.Build.props) -replace '<Version>.*?</Version>', '<Version>{{version}}</Version>' | Set-Content Directory.Build.props
	
	echo "2. Updating Template References..."
	(Get-Content templates/svelonia.app/SveloniaApp.csproj) -replace '(Include="Svelonia\..*?" Version)=".*?"', ('$1="{{version}}"') | Set-Content templates/svelonia.app/SveloniaApp.csproj

	echo "3. Packing..."
	if (!(Test-Path artifacts)) { mkdir artifacts }
	dotnet pack src/Svelonia.Core/Svelonia.Core.csproj -c Release -o artifacts
	dotnet pack src/Svelonia.Data/Svelonia.Data.csproj -c Release -o artifacts
	dotnet pack src/Svelonia.Fluent/Svelonia.Fluent.csproj -c Release -o artifacts
	dotnet pack src/Svelonia.Gen/Svelonia.Gen.csproj -c Release -o artifacts
	dotnet pack src/Svelonia.Kit/Svelonia.Kit.csproj -c Release -o artifacts
	dotnet pack templates/Svelonia.Templates.csproj -c Release -o artifacts

# End-to-end test of the template
test-template:
	powershell -File scripts/test-template.ps1
