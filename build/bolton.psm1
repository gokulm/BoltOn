function LogError([string]$message)
{
	Write-Host "$message" -ForegroundColor Red
}

function LogWarning([string]$message)
{
    Write-Host "$message" -ForegroundColor Yellow
}

function LogInfo([string]$message)
{
    Write-Host "*********** $message ***********" -ForegroundColor DarkGreen
}

function LogDebug([string]$message)
{
    Write-Host "$message" -ForegroundColor DarkCyan
}

export-modulemember -function LogError, LogWarning, LogInfo, LogDebug