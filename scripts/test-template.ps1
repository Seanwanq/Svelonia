# Svelonia Template End-to-End Test Script

$ErrorActionPreference = "Stop"

Write-Host ">>> Starting Template End-to-End Test <<<" -ForegroundColor Cyan

# 1. Rebuild Generator with unique name to kill caching
$ticks = (Get-Date).Ticks
$genPath = 'src/Svelonia.Gen/Svelonia.Gen.csproj'
Write-Host "--- Patching Generator AssemblyName to avoid caching..."
$genContent = Get-Content $genPath -Raw
$genContent = $genContent -replace '<AssemblyName>.*?</AssemblyName>', ''
$genContent = $genContent -replace '<PropertyGroup>', ("<PropertyGroup>`n    <AssemblyName>Svelonia.Gen.$ticks</AssemblyName>")
$genContent | Set-Content $genPath

try {
    Write-Host "--- Building Generator..."
    dotnet build src/Svelonia.Gen/Svelonia.Gen.csproj -c Debug

    $dll = (Get-ChildItem src/Svelonia.Gen/bin/Debug/netstandard2.0/Svelonia.Gen.*.dll | Select-Object -First 1).FullName
    Write-Host "--- Using Analyzer: $dll"

    # 2. Setup TestApp
    if (Test-Path TestApp) { 
        Write-Host "--- Cleaning old TestApp..."
        Remove-Item TestApp -Recurse -Force 
    }
    New-Item -ItemType Directory -Path TestApp | Out-Null
    Write-Host "--- Copying template files..."
    Copy-Item -Path templates/svelonia.app/* -Destination TestApp -Recurse -Force
    if (Test-Path TestApp/.template.config) {
        Remove-Item TestApp/.template.config -Recurse -Force
    }

    # 3. Patch TestApp.csproj
    Write-Host "--- Patching TestApp.csproj to use local projects..."
    $appProj = 'TestApp/SveloniaApp.csproj'
    $content = Get-Content $appProj -Raw
    $content = $content -replace '<PackageReference Include="Svelonia.Core".*?/>', '<ProjectReference Include="../src/Svelonia.Core/Svelonia.Core.csproj" />'
    $content = $content -replace '<PackageReference Include="Svelonia.Data".*?/>', '<ProjectReference Include="../src/Svelonia.Data/Svelonia.Data.csproj" />'
    $content = $content -replace '<PackageReference Include="Svelonia.Fluent".*?/>', '<ProjectReference Include="../src/Svelonia.Fluent/Svelonia.Fluent.csproj" />'
    $content = $content -replace '<PackageReference Include="Svelonia.Kit".*?/>', '<ProjectReference Include="../src/Svelonia.Kit/Svelonia.Kit.csproj" />'
    $content = $content -replace '(?s)<PackageReference Include="Svelonia.Gen".*?/>', ("<Analyzer Include=`"$($dll.Replace('\', '/'))`" />")
    $content = $content -replace '<PackageReference Include="Svelonia.DevTools".*?/>', '<ProjectReference Include="../src/Svelonia.DevTools/Svelonia.DevTools.csproj" />'
    $content | Set-Content $appProj

    # 4. Build and Run
    Write-Host "--- Building TestApp..."
    Push-Location TestApp
    dotnet build -p:UseSharedCompilation=false
    Pop-Location

    Write-Host "--- Smoke Testing (8s)..."
    $p = Start-Process dotnet -ArgumentList "run --project TestApp/SveloniaApp.csproj" -PassThru -NoNewWindow -RedirectStandardOutput 'smoke_test.log' -RedirectStandardError 'smoke_err.log'
    
    Start-Sleep -Seconds 8
    
    if ($p.HasExited) {
        Write-Host "ERROR: App crashed!" -ForegroundColor Red
        if (Test-Path smoke_err.log) { Get-Content smoke_err.log }
        exit 1
    } else {
        Write-Host "SUCCESS: App is stable." -ForegroundColor Green
        Stop-Process -Id $p.Id -Force
    }
}
finally {
    # 5. Cleanup
    Write-Host "--- Cleaning up..."
    git checkout src/Svelonia.Gen/Svelonia.Gen.csproj
}

Write-Host "Done."
