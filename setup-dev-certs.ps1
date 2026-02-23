# Script to generate and export .NET development certificates for Docker
# Run this from your project root

Write-Host "Setting up SSL certificates for Docker..." -ForegroundColor Cyan

# Create directory for certificates if it doesn't exist
$certPath = ".\CommandBot\My_Keys"
if (-not (Test-Path $certPath)) {
    New-Item -ItemType Directory -Path $certPath -Force | Out-Null
    Write-Host "Created certificate directory" -ForegroundColor Green
}

# Generate a dev certificate
Write-Host "Generating development certificate..." -ForegroundColor Yellow
dotnet dev-certs https --clean
dotnet dev-certs https --trust

# Export the certificate for Docker
$certFile = Join-Path $certPath "aspnetapp.pfx"
$password = "PASSWORD_HERE"

Write-Host "Exporting certificate to $certFile..." -ForegroundColor Yellow
dotnet dev-certs https -ep $certFile -p $password

if (Test-Path $certFile) {
    Write-Host "Certificate exported successfully!" -ForegroundColor Green
    Write-Host ""
    Write-Host "Certificate location: $certFile" -ForegroundColor Cyan
    Write-Host "Certificate password: $password" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "IMPORTANT: The docker-compose files are already configured." -ForegroundColor Yellow
    Write-Host "Now run: docker-compose up -d --build commandbot" -ForegroundColor White
} else {
    Write-Host "Failed to export certificate" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "Setup complete!" -ForegroundColor Green