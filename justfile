# justfile for Svelonia

# Default recipe
default:
    @just --list

# Clean artifacts and build results
clean:
    dotnet clean
    rm -rf artifacts/*
    echo "Cleaned up."

# Pack all libraries and templates with a specific version
# Usage: just pack 0.0.2
pack version:
    @echo "1. Updating Directory.Build.props to {{version}}..."
    perl -pi -e "s/<Version>.*?<\/Version>/<Version>{{version}}<\/Version>/g" Directory.Build.props
    perl -pi -e "s/<AssemblyVersion>.*?<\/AssemblyVersion>/<AssemblyVersion>{{version}}.0<\/AssemblyVersion>/g" Directory.Build.props
    perl -pi -e "s/<FileVersion>.*?<\/FileVersion>/<FileVersion>{{version}}.0<\/FileVersion>/g" Directory.Build.props

    @echo "2. Updating Template References to {{version}}..."
    # Replace only versions in PackageReference items that start with Svelonia
    perl -pi -e "s/(Include=\"Svelonia\..*?" Version)=\".*?"/\$1=\"{{version}}\"/g" templates/svelonia.app/SveloniaApp.csproj

    @echo "3. Packing Libraries..."
    dotnet pack src/Svelonia.Core/Svelonia.Core.csproj -c Release -o artifacts
    dotnet pack src/Svelonia.Data/Svelonia.Data.csproj -c Release -o artifacts
    dotnet pack src/Svelonia.Fluent/Svelonia.Fluent.csproj -c Release -o artifacts
    dotnet pack src/Svelonia.Gen/Svelonia.Gen.csproj -c Release -o artifacts
    dotnet pack src/Svelonia.Kit/Svelonia.Kit.csproj -c Release -o artifacts
    dotnet pack src/Svelonia.DevTools/Svelonia.DevTools.csproj -c Release -o artifacts

    @echo "4. Packing Templates..."
    dotnet pack templates/Svelonia.Templates.csproj -c Release -o artifacts

    @echo "Done! Version {{version}} is ready in artifacts/"
