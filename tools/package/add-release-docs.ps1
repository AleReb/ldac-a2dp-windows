param(
    [Parameter(Mandatory)] [string] $PackagePath,
    [Parameter(Mandatory)] [string] $DotnetRoot
)
$ErrorActionPreference = 'Stop'
$root = (Resolve-Path "$PSScriptRoot\..\..").Path
foreach ($folder in 'docs', 'third_party\libldac', 'third_party\sbc_encoder', 'third_party\dotnet') {
    New-Item -ItemType Directory -Path (Join-Path $PackagePath $folder) -Force | Out-Null
}
foreach ($relative in 'LICENSE', 'NOTICE', 'docs\windows-install.md', 'docs\instalacion-windows.md',
                      'third_party\libldac\LICENSE', 'third_party\libldac\NOTICE',
                      'third_party\sbc_encoder\LICENSE') {
    Copy-Item -LiteralPath (Join-Path $root $relative) -Destination (Join-Path $PackagePath $relative)
}
foreach ($file in 'LICENSE.txt', 'ThirdPartyNotices.txt') {
    Copy-Item -LiteralPath (Join-Path $DotnetRoot $file) -Destination (Join-Path $PackagePath 'third_party\dotnet')
}
