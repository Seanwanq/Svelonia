# Svelonia Template End-to-End Test Script
param (
    [switch]$Aot
)

$ErrorActionPreference = "Stop"

$modeStr = if ($Aot) { "Native AOT" } else { "JIT" }
$appDir = if ($Aot) { "TestAppAot" } else { "TestApp" }

Write-Host ">>> Starting Template End-to-End Test ($modeStr) -> $appDir <<<" -ForegroundColor Cyan

# 1. Rebuild Generator with unique name to kill caching
$ticks = (Get-Date).Ticks
$uniqueName = "Svelonia.Gen.$ticks"
$genPath = 'src/Svelonia.Gen/Svelonia.Gen.csproj'
Write-Host "--- Patching Generator AssemblyName to avoid caching..."

try {
    $genContent = Get-Content $genPath -Raw
    $genContent = $genContent -replace '<AssemblyName>.*?</AssemblyName>', ''
    $genContent = $genContent -replace '<PropertyGroup>', ("<PropertyGroup>`n    <AssemblyName>$uniqueName</AssemblyName>")
    $genContent | Set-Content $genPath

    Write-Host "--- Building Generator..."
    dotnet build src/Svelonia.Gen/Svelonia.Gen.csproj -c Debug /v:q
    if ($LASTEXITCODE -ne 0) { throw "Generator build failed." }

    $dllPathRaw = "src/Svelonia.Gen/bin/Debug/netstandard2.0/$uniqueName.dll"
    if (-not (Test-Path $dllPathRaw)) { throw "Generator DLL not found at expected path: $dllPathRaw" }
    $dll = (Resolve-Path $dllPathRaw).Path
    Write-Host "--- Using Analyzer: $dll"

    # 2. Setup TestApp
    if (Test-Path $appDir) {
        Remove-Item $appDir -Recurse -Force
    }
    New-Item -ItemType Directory -Path $appDir | Out-Null
    Write-Host "--- Copying template files..."
    Copy-Item -Path templates/svelonia.app/* -Destination $appDir -Recurse -Force
    if (Test-Path "$appDir/.template.config") {
        Remove-Item "$appDir/.template.config" -Recurse -Force
    }

    # 3. Patch TestApp.csproj
    Write-Host "--- Patching $appDir.csproj..."
    $appProj = "$appDir/SveloniaApp.csproj"
    $content = Get-Content $appProj -Raw
    
    # Enable AOT if requested (simulate template replacement)
    if ($Aot) {
        $content = $content -replace '<!--#if \(Aot\)-->\s*<PublishAot>true</PublishAot>\s*<!--#endif-->', '<PublishAot>true</PublishAot>'
    }

    # Use local projects
    $content = $content -replace '<PackageReference Include="Svelonia.Core".*?/>', '<ProjectReference Include="../src/Svelonia.Core/Svelonia.Core.csproj" />'
    $content = $content -replace '<PackageReference Include="Svelonia.Data".*?/>', '<ProjectReference Include="../src/Svelonia.Data/Svelonia.Data.csproj" />'
    $content = $content -replace '<PackageReference Include="Svelonia.Fluent".*?/>', '<ProjectReference Include="../src/Svelonia.Fluent/Svelonia.Fluent.csproj" />'
    $content = $content -replace '<PackageReference Include="Svelonia.Kit".*?/>', '<ProjectReference Include="../src/Svelonia.Kit/Svelonia.Kit.csproj" />'
    # Use regex single-line mode (?s) to handle multi-line PackageReference
    $content = $content -replace '(?s)<PackageReference Include="Svelonia.DevTools".*?/>', '<ProjectReference Include="../src/Svelonia.DevTools/Svelonia.DevTools.csproj" />'
    
    # Patch Generator: Copy to local folder to persist after script ends
    $localAnalyzerDir = "$appDir/analyzers"
    New-Item -ItemType Directory -Path $localAnalyzerDir -Force | Out-Null
    Copy-Item -Path $dll -Destination $localAnalyzerDir
    $dllName = Split-Path $dll -Leaf
    $localDllPath = "analyzers/$dllName" # Relative path for csproj
    
    $analyzerItem = "<Analyzer Include=`"$localDllPath`" />"
    $content = $content -replace '(?s)<PackageReference Include="Svelonia.Gen".*?/>', $analyzerItem
    
    $content | Set-Content $appProj

    # 4. Build and Run
    Write-Host "--- Building $appDir..."
    Push-Location $appDir
    
    if ($Aot) {
        dotnet publish -c Release -r win-x64
    } else {
        dotnet build -p:UseSharedCompilation=false
    }

    if ($LASTEXITCODE -ne 0) { 
        Pop-Location
        throw "$appDir build failed." 
    }
    
    Write-Host "--- Smoke Testing (8s)..."
    $logOut = "smoke_test.log"
    $logErr = "smoke_err.log"
    
    $p = if ($Aot) {
        $exe = (Get-ChildItem -Path "bin/Release/net10.0/win-x64/publish/*.exe" | Select-Object -First 1).FullName
        Write-Host "Running AOT Exe: $exe"
        Start-Process $exe -PassThru -NoNewWindow -RedirectStandardOutput $logOut -RedirectStandardError $logErr
    } else {
        Start-Process dotnet -ArgumentList "run --project SveloniaApp.csproj" -PassThru -NoNewWindow -RedirectStandardOutput $logOut -RedirectStandardError $logErr
    }

    Pop-Location # Back to root so we can wait

    Start-Sleep -Seconds 8

    if ($p.HasExited) {
        Write-Host "ERROR: App crashed!" -ForegroundColor Red
        if (Test-Path "$appDir/$logErr") { 
            Write-Host "--- STDERR ---"
            Get-Content "$appDir/$logErr"
        }
        exit 1
    } else {
        Write-Host "SUCCESS: App is stable." -ForegroundColor Green
        Stop-Process -Id $p.Id -Force
    }
}
catch {
    Write-Host "TEST FAILED: $_" -ForegroundColor Red
    exit 1
}
finally {
    # 5. Cleanup
    Write-Host "--- Cleaning up..."
    git checkout src/Svelonia.Gen/Svelonia.Gen.csproj --quiet 2>&1 | Out-Null
}

Write-Host "Done."
