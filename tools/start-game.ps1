param(
    [string]$serverAddress = "https://snow-rover.azurewebsites.net",
    [string]$gameId = "a",
    [int]$rechargePerSecond = 150,
    [Parameter(Mandatory=$true)][string]$secret
)

$body = @{
    RechargePointsPerSecond=250; 
    GameID=$gameId; 
    Password=$secret
} | convertto-json

Invoke-RestMethod -Method POST -Uri "$serverAddress/admin/startgame" -Body $body -ContentType 'application/json'