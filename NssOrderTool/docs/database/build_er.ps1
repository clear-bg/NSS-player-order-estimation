$outputFile = "dist\schema.mmd"

# 先頭の宣言を書き込む
"erDiagram" | Out-File $outputFile -Encoding utf8

# Tablesの結合
"`n    %% --- Tables ---" | Out-File $outputFile -Append -Encoding utf8
Get-ChildItem -Path "tables\*.mmd" | ForEach-Object {
  Get-Content $_.FullName | Out-File $outputFile -Append -Encoding utf8
  "`n" | Out-File $outputFile -Append -Encoding utf8
}

# Relationsの結合
"`n    %% --- Relationships ---" | Out-File $outputFile -Append -Encoding utf8
Get-ChildItem -Path "relations\*.mmd" | ForEach-Object {
  Get-Content $_.FullName | Out-File $outputFile -Append -Encoding utf8
  "`n" | Out-File $outputFile -Append -Encoding utf8
}

Write-Host "✅ dist\schema.mmd が生成されました！"
