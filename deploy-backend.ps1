#!/usr/bin/env pwsh
# Azure App Service Deployment Script
# Usage: .\deploy-backend.ps1

param(
    [string]$ResourceGroup = "naar-noor-rg",
    [string]$AppServiceName = "naar-noor",
    [string]$PublishPath = "$PSScriptRoot/api-server/src/NaarNoor.API/publish",
    [switch]$Login
)

Write-Host "🚀 Naar & Noor Backend Deployment Script" -ForegroundColor Cyan

# Check if logged in to Azure
if ($Login) {
    Write-Host "📝 Logging into Azure..." -ForegroundColor Yellow
    az login
}

# Verify Azure CLI is available
if (-not (Get-Command az -ErrorAction SilentlyContinue)) {
    Write-Host "❌ Azure CLI not found. Please install it first." -ForegroundColor Red
    Write-Host "   Download from: https://docs.microsoft.com/en-us/cli/azure/install-azure-cli" -ForegroundColor Gray
    exit 1
}

# Check if publish folder exists
if (-not (Test-Path $PublishPath)) {
    Write-Host "⚠️  Publish folder not found at: $PublishPath" -ForegroundColor Yellow
    Write-Host "📦 Building backend..." -ForegroundColor Yellow
    
    $apiPath = "$PSScriptRoot/api-server/src/NaarNoor.API"
    Push-Location $apiPath
    dotnet publish -c Release -o publish
    Pop-Location
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "❌ Build failed" -ForegroundColor Red
        exit 1
    }
}

# Create ZIP file
Write-Host "📦 Creating deployment package..." -ForegroundColor Yellow
$zipPath = "$PSScriptRoot/deploy.zip"
Remove-Item $zipPath -ErrorAction SilentlyContinue
Compress-Archive -Path "$PublishPath/*" -DestinationPath $zipPath -Force

Write-Host "✅ Created: $zipPath" -ForegroundColor Green
Write-Host "📊 Size: $((Get-Item $zipPath).Length / 1MB)MB" -ForegroundColor Cyan

# Deploy
Write-Host "🔄 Deploying to Azure App Service..." -ForegroundColor Yellow
az webapp deployment source config-zip `
  --resource-group $ResourceGroup `
  --name $AppServiceName `
  --src $zipPath

if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Deployment successful!" -ForegroundColor Green
    Write-Host "🌐 Check your app at: https://$AppServiceName.azurewebsites.net" -ForegroundColor Cyan
    
    # Wait for app to restart
    Write-Host "⏳ Waiting for app to restart (30 seconds)..." -ForegroundColor Yellow
    Start-Sleep -Seconds 30
    
    # Test endpoint
    Write-Host "🧪 Testing API..." -ForegroundColor Yellow
    $testUrl = "https://$AppServiceName.azurewebsites.net/api/reviews"
    try {
        $response = Invoke-WebRequest -Uri $testUrl -UseBasicParsing -TimeoutSec 10
        Write-Host "✅ API is responding: $($response.StatusCode)" -ForegroundColor Green
        Write-Host "📍 Access Swagger at: https://$AppServiceName.azurewebsites.net/api/docs" -ForegroundColor Cyan
    } catch {
        Write-Host "⚠️  Could not reach API yet. It may still be starting up." -ForegroundColor Yellow
        Write-Host "🔗 Try again in a few moments or check Azure Portal logs." -ForegroundColor Gray
    }
} else {
    Write-Host "❌ Deployment failed" -ForegroundColor Red
    exit 1
}

# Cleanup
Remove-Item $zipPath -ErrorAction SilentlyContinue
Write-Host "🧹 Cleaned up temporary files" -ForegroundColor Gray
Write-Host "`n✨ Done!" -ForegroundColor Green
