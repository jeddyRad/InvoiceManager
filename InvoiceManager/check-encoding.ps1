# Script de vérification de l'encodage UTF-8
# Usage: .\check-encoding.ps1

Write-Host "=== Vérification de l'encodage des fichiers ===" -ForegroundColor Cyan

# Fonction pour vérifier l'encodage d'un fichier
function Check-FileEncoding {
    param([string]$FilePath)
    
    $bytes = [System.IO.File]::ReadAllBytes($FilePath)
    
    # Vérifier UTF-8 BOM
    if ($bytes.Length -ge 3 -and $bytes[0] -eq 0xEF -and $bytes[1] -eq 0xBB -and $bytes[2] -eq 0xBF) {
        return "UTF-8-BOM ✓"
    }
    # Vérifier UTF-16 LE BOM
    elseif ($bytes.Length -ge 2 -and $bytes[0] -eq 0xFF -and $bytes[1] -eq 0xFE) {
        return "UTF-16-LE ✗"
    }
    # Probablement UTF-8 sans BOM
    else {
        return "UTF-8 (sans BOM) ⚠"
    }
}

Write-Host "`nVérification des fichiers Razor:" -ForegroundColor Yellow
Get-ChildItem -Path "InvoiceManager\Components" -Filter "*.razor" -Recurse | ForEach-Object {
    $encoding = Check-FileEncoding $_.FullName
    Write-Host "  $($_.Name): $encoding"
}

Write-Host "`nVérification des fichiers C#:" -ForegroundColor Yellow
Get-ChildItem -Path "InvoiceManager" -Filter "*.cs" -Recurse -Exclude "obj","bin" | Select-Object -First 10 | ForEach-Object {
    $encoding = Check-FileEncoding $_.FullName
    Write-Host "  $($_.Name): $encoding"
}

Write-Host "`n=== Vérification des dépendances ===" -ForegroundColor Cyan

# Vérifier si l'application est en cours d'exécution
$process = Get-Process -Name "InvoiceManager" -ErrorAction SilentlyContinue
if ($process) {
    Write-Host "✓ Application en cours d'exécution (PID: $($process.Id))" -ForegroundColor Green
} else {
    Write-Host "✗ Application non lancée" -ForegroundColor Red
}

Write-Host "`n=== Test de connectivité CDN ===" -ForegroundColor Cyan

# Tester Bootstrap Icons CDN
try {
    $response = Invoke-WebRequest -Uri "https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/font/bootstrap-icons.min.css" -Method Head -TimeoutSec 5
    if ($response.StatusCode -eq 200) {
        Write-Host "✓ Bootstrap Icons CDN accessible" -ForegroundColor Green
    }
} catch {
    Write-Host "✗ Bootstrap Icons CDN inaccessible: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "  Conseil: Téléchargez les icônes localement" -ForegroundColor Yellow
}

Write-Host "`n=== Recommandations ===" -ForegroundColor Cyan

# Compter les fichiers avec problèmes d'encodage
$problematicFiles = Get-ChildItem -Path "InvoiceManager" -Filter "*.razor" -Recurse | Where-Object {
    $encoding = Check-FileEncoding $_.FullName
    $encoding -notlike "*UTF-8-BOM*"
}

if ($problematicFiles.Count -gt 0) {
    Write-Host "⚠  $($problematicFiles.Count) fichier(s) sans UTF-8-BOM détecté(s)" -ForegroundColor Yellow
    Write-Host "  Conseil: Utilisez l'EditorConfig pour forcer UTF-8-BOM" -ForegroundColor Yellow
} else {
    Write-Host "✓ Tous les fichiers Razor utilisent UTF-8-BOM" -ForegroundColor Green
}

Write-Host "`nVérification terminée!" -ForegroundColor Cyan
