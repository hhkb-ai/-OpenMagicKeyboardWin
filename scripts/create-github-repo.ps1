# Requires GitHub CLI: https://cli.github.com/
# Run from the repository root.

$RepoName = "OpenMagicKeyboardWin"
$Owner = "hhkb-ai"

if (-not (Get-Command gh -ErrorAction SilentlyContinue)) {
  Write-Error "GitHub CLI 'gh' is not installed. Install it first or create the repo manually on GitHub."
  exit 1
}

if (-not (Test-Path .git)) {
  git init
  git add .
  git commit -m "Initial clean-room project scaffold"
  git branch -M main
}

gh repo create "$Owner/$RepoName" --public --source . --remote origin --push
