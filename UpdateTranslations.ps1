Push-Location (Split-Path -Path $MyInvocation.MyCommand.Definition -Parent)

$api_token = "😉";
$projectId = "258717";

function DownloadLanguageFile($language)
{
    $contentType = 'application/x-www-form-urlencoded' 
    $body = @{
        api_token = $api_token
        id = $projectId
        language = $language
        type = "resw"
    }
    
    $response = Invoke-RestMethod -Uri "https://api.poeditor.com/v2/projects/export" -Method Post -Body $body -ContentType $contentType

    Invoke-WebRequest -Uri $response.result.url -OutFile "FluentTerminal.App/Strings/$($language)/Resources.resw"
}

DownloadLanguageFile("en");
DownloadLanguageFile("zh-CN");
DownloadLanguageFile("de");
DownloadLanguageFile("es");
DownloadLanguageFile("fr");
DownloadLanguageFile("ru");
DownloadLanguageFile("ro");
