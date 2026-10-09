<#
.SYNOPSIS
    S TOM'S — Local Developer Environment & Profile Setup Script
.DESCRIPTION
    Configures a machine-local, private developer profile in %LOCALAPPDATA%\STOMS\
    with Windows DPAPI verification and restrictive filesystem ACLs.
    Does NOT alter Git branches, remotes, or repository configuration.
.PARAMETER TestOnly
    Runs diagnostic checks (DPAPI, ACLs, .NET SDK, and paths) without writing files.
.PARAMETER Force
    Allows updating an existing local profile after explicit user confirmation.
#>

[CmdletBinding()]
param (
    [switch]$TestOnly,
    [switch]$Force
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

Write-Host '==========================================================' -ForegroundColor Cyan
Write-Host '   S TOM''S — Developer Local Environment Setup Utility    ' -ForegroundColor Cyan
Write-Host '==========================================================' -ForegroundColor Cyan

# 1. Diagnostic Checks
Write-Host ''
Write-Host '[1/4] Checking Prerequisites...' -ForegroundColor Yellow

# Verify .NET SDK
try {
    $dotnetVersion = & dotnet --version 2>$null
    Write-Host "  [PASS] .NET SDK Detected: $dotnetVersion" -ForegroundColor Green
} catch {
    Write-Warning '  [WARN] .NET SDK not detected in PATH. Please install .NET 8.0 SDK.'
}

# Verify Windows DPAPI Availability
try {
    Add-Type -AssemblyName System.Security
    $testBytes = [System.Text.Encoding]::UTF8.GetBytes('STOMS_DPAPI_HEALTH_CHECK')
    $encrypted = [System.Security.Cryptography.ProtectedData]::Protect($testBytes, $null, [System.Security.Cryptography.DataProtectionScope]::CurrentUser)
    $decrypted = [System.Security.Cryptography.ProtectedData]::Unprotect($encrypted, $null, [System.Security.Cryptography.DataProtectionScope]::CurrentUser)
    $resultText = [System.Text.Encoding]::UTF8.GetString($decrypted)
    if ($resultText -eq 'STOMS_DPAPI_HEALTH_CHECK') {
        Write-Host '  [PASS] Windows DPAPI Encryption Verified (CurrentUser scope)' -ForegroundColor Green
    } else {
        throw 'Decryption mismatch'
    }
} catch {
    Write-Warning "  [FAIL] Windows DPAPI not operational on this system: $_"
}

# Target Local Path
$stomsDir = Join-Path $env:LOCALAPPDATA 'STOMS'
$profilePath = Join-Path $stomsDir 'profile.json'

Write-Host "  [INFO] Target Local Profile Path: $profilePath" -ForegroundColor Gray

if ($TestOnly) {
    Write-Host ''
    Write-Host '[DIAGNOSTIC COMPLETE] TestOnly mode completed successfully. Zero files modified.' -ForegroundColor Green
    return
}

# 2. Existing Profile Protection
if ((Test-Path -Path $profilePath) -and (-not $Force)) {
    Write-Host ''
    Write-Host "[NOTICE] An existing profile was found at: $profilePath" -ForegroundColor Yellow
    Write-Host 'Use -Force to overwrite with new configuration. Existing profile preserved.' -ForegroundColor Yellow
    return
}

# 3. Interactive Input Collection
Write-Host ''
Write-Host '[2/4] Collecting Developer Details...' -ForegroundColor Yellow

$preferredName = Read-Host '  Enter your preferred work name'
if ([string]::IsNullOrWhiteSpace($preferredName)) {
    Write-Error 'Preferred name cannot be empty.'
    return
}

Write-Host '  Available Roles: 1. UI/UX Developer, 2. Engine Developer, 3. PM/Tester, 4. Vision' -ForegroundColor Gray
$roleChoice = Read-Host '  Select Role (1-4)'
$role = switch ($roleChoice) {
    '1' { 'UI/UX Developer' }
    '2' { 'Engine Developer' }
    '3' { 'PM/Tester' }
    '4' { 'Vision' }
    default { $roleChoice }
}

$validRoles = @('UI/UX Developer', 'Engine Developer', 'PM/Tester', 'Vision')
if ($role -notin $validRoles) {
    Write-Error "Invalid role selected. Must be one of: $($validRoles -join ', ')"
    return
}

if ($role -eq 'Vision') {
    Write-Host '  [SECURITY CHECK] The Vision role requires project-owner authorization.' -ForegroundColor Yellow
    $confirmVision = Read-Host '  Are you the authorized project owner? (yes/no)'
    if ($confirmVision -ne 'yes') {
        Write-Error 'Unauthorized role selection. Please select UI/UX Developer, Engine Developer, or PM/Tester.'
        return
    }
}

Write-Host '  Preferred Language: 1. English (UK), 2. Hinglish (Hindi), 3. Gujarati' -ForegroundColor Gray
$langChoice = Read-Host '  Select Language (1-3)'
$language = switch ($langChoice) {
    '1' { 'English (UK)' }
    '2' { 'Hinglish (Hindi)' }
    '3' { 'Gujarati' }
    default { 'English (UK)' }
}

$repoUrl = Read-Host '  Confirm GitHub Repository URL (default: https://github.com/Saumya25-hub/TEAM-SOFT-SB1)'
if ([string]::IsNullOrWhiteSpace($repoUrl)) {
    $repoUrl = 'https://github.com/Saumya25-hub/TEAM-SOFT-SB1'
}

# 4. Profile Construction & Restricted Storage
Write-Host ''
Write-Host '[3/4] Securing Local Profile Directory...' -ForegroundColor Yellow

if (-not (Test-Path -Path $stomsDir)) {
    New-Item -ItemType Directory -Path $stomsDir -Force | Out-Null
}

# Restrict ACLs using icacls so only the current user has access
try {
    $currentUser = $env:USERNAME
    $grantArg = $currentUser + ':(OI)(CI)F'
    & icacls $stomsDir /inheritance:r /grant:r $grantArg 2>$null | Out-Null
    Write-Host '  [PASS] Filesystem ACL restricted to current user.' -ForegroundColor Green
} catch {
    Write-Warning "  [WARN] Could not set restrictive ACL: $_"
}

$profileData = [PSCustomObject]@{
    PreferredName     = $preferredName
    AssignedRole      = $role
    PreferredLanguage = $language
    GitHubRepository  = $repoUrl
    ConfiguredAt      = (Get-Date).ToString('yyyy-MM-dd HH:mm:ss zzz')
    MachineName       = $env:COMPUTERNAME
    WindowsUser       = $env:USERNAME
}

$profileJson = $profileData | ConvertTo-Json -Depth 4
Set-Content -Path $profilePath -Value $profileJson -Encoding UTF8

Write-Host ''
Write-Host '[4/4] Setup Complete!' -ForegroundColor Green
Write-Host "  Profile saved to: $profilePath" -ForegroundColor Cyan
Write-Host '  Zero credentials stored. Shared Git files remain clean.' -ForegroundColor Green
