param(
    [Parameter(Mandatory = $true)]
    [string]$Version
)

# --- CONFIG ---------------------------------------------------------
$User = "cornelisdejonge"
$Image = "ghcr.io/$User/household-planner"
$Dockerfile = ".\src\HouseholdPlanner\Dockerfile"
$Context = "."
# --------------------------------------------------------------------
# --- GHCR LOGIN ---------------------------------------------------------
Write-Host "Authenticating to GitHub Container Registry..."

# Prefer current-session env var, else load from the User profile
if (-not $env:GITHUB_PAT) {
    $fromUser = [Environment]::GetEnvironmentVariable('GITHUB_PAT', 'User')
    if ($fromUser) { $env:GITHUB_PAT = $fromUser }
}

if (-not $env:GITHUB_PAT) {
    Write-Host "❌ GITHUB_PAT is not set in this session or User env." -ForegroundColor Red
    Write-Host 'Set it with:  setx GITHUB_PAT your_PAT_here   (then open a new PowerShell)'
    exit 1
}

# Sanitize common issues (quotes, spaces, CR/LF)
$env:GITHUB_PAT = $env:GITHUB_PAT.Trim("`"", " ", "`r", "`n")

# Robust STDIN pass-through (no echo)
try {
    [System.Text.Encoding]::UTF8.GetBytes($env:GITHUB_PAT) | docker login ghcr.io -u cornelisdejonge --password-stdin
    if ($LASTEXITCODE -ne 0) { throw "docker login exit code $LASTEXITCODE" }
    Write-Host "✅ GHCR authentication successful."
}
catch {
    Write-Host "❌ Failed to authenticate to GHCR via stdin pipeline." -ForegroundColor Red
    Write-Host "   Tip: Try running the script from a normal PowerShell window (outside Visual Studio)."
    exit 1
}Write-Host "✅ Successfully authenticated to GHCR."
# ------------------------------------------------------------------------

Write-Host "Building Household Planner..."
Write-Host "Using version tag: $Version"
Write-Host "Pushing to: $Image"
Write-Host ""
Write-Host "Dockerfile: $Dockerfile"
Write-Host "Context:    $Context"
Write-Host ""

# Build the Docker image using the nested Dockerfile but root as context
docker build -f $Dockerfile -t ${Image}:${Version} -t ${Image}:latest $Context

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Docker build failed." -ForegroundColor Red
    exit 1
}

# Push versioned image
docker push ${Image}:${Version}
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Failed to push version tag ${Version}." -ForegroundColor Red
    exit 1
}

# Push latest tag
docker push ${Image}:latest
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Failed to push latest tag." -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "✅ Successfully pushed:"
Write-Host "   ${Image}:${Version}"
Write-Host "   ${Image}:latest"
Write-Host ""
Write-Host "Next steps on the NAS:"
Write-Host "   docker pull ${Image}:${Version}"
Write-Host "   docker rm -f household-planner"
Write-Host "   docker run -d --name household-planner -p 8080:8080 ${Image}:${Version}"