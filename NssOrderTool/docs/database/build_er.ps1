# エラー発生時に処理を停止する
$ErrorActionPreference = "Stop"

# 現在のスクリプトの場所をカレントディレクトリとする
$scriptDir = Split-Path $MyInvocation.MyCommand.Path
Set-Location $scriptDir

$plantUmlJar = "plantuml-1.2021.16.jar"

# jarファイルが存在するかチェック
if (-Not (Test-Path $plantUmlJar)) {
  Write-Error "$plantUmlJar が見つかりません。同じディレクトリに配置してください。"
  exit 1
}

Write-Host "=== ER図 (PlantUML) 画像生成を開始します ==="

# 1. 各テーブルの単体図を生成
Write-Host "テーブル定義の画像を生成中..."
# java -jar plantuml.jar -svg [入力ディレクトリ] -o [出力先ディレクトリ（入力元からの相対パス）]
# ※PlantUMLの -o オプションは、対象ファイルがあるディレクトリからの相対パスになるため、../../../images/tables のように指定します
# ※-charset を指定しないと、.pumlファイル(UTF-8)をJVMのデフォルトチャーセット(日本語環境ではShift_JIS系)で
#   読み込んでしまい、画像内の日本語が文字化けするため明示的にUTF-8を指定する
java -jar $plantUmlJar -charset UTF-8 -svg "plantuml/src/tables" -o "../../../images/tables"

# 2. ドメイン別・全体のリレーション図を生成
Write-Host "リレーション図の画像を生成中..."
java -jar $plantUmlJar -charset UTF-8 -svg "plantuml/relations" -o "../../images/relations"

Write-Host "✅ すべての画像生成が完了しました。 'images' フォルダを確認してください。"
