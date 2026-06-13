# スクリプト自身のディレクトリ（docs/database）を基準にする
$scriptDir = $PSScriptRoot
$outputDir = Join-Path $scriptDir "dist"
$outputFile = Join-Path $outputDir "schema.mmd"

# distフォルダが存在しない場合は自動作成する
if (-Not (Test-Path $outputDir)) {
  New-Item -ItemType Directory -Path $outputDir | Out-Null
}

# 先頭の宣言を書き込む
"erDiagram" | Out-File $outputFile -Encoding utf8

# Tablesの結合
"`n    %% --- Tables ---" | Out-File $outputFile -Append -Encoding utf8
$tablesPath = Join-Path $scriptDir "tables\*.mmd"
if (Test-Path $tablesPath) {
  Get-ChildItem -Path $tablesPath | ForEach-Object {
    Get-Content $_.FullName | Out-File $outputFile -Append -Encoding utf8
    "`n" | Out-File $outputFile -Append -Encoding utf8
  }
}

# Relationsの結合
"`n    %% --- Relationships ---" | Out-File $outputFile -Append -Encoding utf8
$relationsPath = Join-Path $scriptDir "relations\*.mmd"
if (Test-Path $relationsPath) {
  Get-ChildItem -Path $relationsPath | ForEach-Object {
    Get-Content $_.FullName | Out-File $outputFile -Append -Encoding utf8
    "`n" | Out-File $outputFile -Append -Encoding utf8
  }
}

Write-Host "✅ 生成成功: $outputFile"
