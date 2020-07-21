Push-Location (Split-Path -Path $MyInvocation.MyCommand.Definition -Parent)

$api_token = "4e9bc169a3ef996122f4202665cef35d";
$projectId = "258717";

function DownloadLanguageFile($language, $directory)
{
    if (!$directory)
    {
        $directory = $language
    }

    $contentType = 'application/x-www-form-urlencoded' 
    $body = @{
        api_token = $api_token
        id = $projectId
        language = $language
        type = "resw"
    }
    
    $response = Invoke-RestMethod -Uri "https://api.poeditor.com/v2/projects/export" -Method Post -Body $body -ContentType $contentType

    Invoke-WebRequest -Uri $response.result.url -OutFile "FluentTerminal.App/Strings/$($directory)/Resources.resw"
}

DownloadLanguageFile "en" "en-US"
DownloadLanguageFile "zh-TW" "zh-Hant"
DownloadLanguageFile "zh-Hans"
DownloadLanguageFile "de"
DownloadLanguageFile "es"
DownloadLanguageFile "fr"
DownloadLanguageFile "he"
DownloadLanguageFile "hi"
DownloadLanguageFile "hr"
DownloadLanguageFile "pt-BR"
DownloadLanguageFile "ru"
DownloadLanguageFile "ro"
DownloadLanguageFile "it"
DownloadLanguageFile "nl"
DownloadLanguageFile "pl"
DownloadLanguageFile "ja"
DownloadLanguageFile "ar-iq"
DownloadLanguageFile "uk"
DownloadLanguageFile "ko"
DownloadLanguageFile "az" "az-Latn"
DownloadLanguageFile "ar"
DownloadLanguageFile "id"
DownloadLanguageFile "fa"
DownloadLanguageFile "tr"
DownloadLanguageFile "hu"
DownloadLanguageFile "sl"
DownloadLanguageFile "ug" "ug-Arab"
DownloadLanguageFile "eo"
