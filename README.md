# NSS Player Order Estimation Tool (Desktop App)
Nintendo Switch Sports（サッカー）などのゲームにおけるプレイヤーの並び順（観測データ）を基に、内部的なソートキー（Hidden ID）によるプレイヤー間の順序関係を推定・管理するためのデスクトップアプリケーションです。

バックエンドに AWS RDS (MySQL) を使用し、フロントエンドは C# (Avalonia UI) で構築されているため、Windows と macOS の両方で動作します。

---

## 1. 開発環境のセットアップ
開発および実行には以下の環境が必要です。

### 1.1. ランタイム & SDK
* **.NET SDK 9.0 (または 8.0)**
  * アプリケーションのビルドと実行に必要です。
  * [Microsoft公式サイト](https://dotnet.microsoft.com/ja-jp/download/dotnet) からインストールしてください。
* **Python 3.10+** (オプション)
  * AWSのセキュリティグループ自動更新スクリプト (`scripts/update_security_group.py`) を使用する場合のみ必要です。

### 1.2. 開発ツール（IDE）
* **Windows**: Visual Studio 2026 (推奨) または 2022
    * ワークロード: 「.NET デスクトップ開発」
* **macOS**: Visual Studio Code
    * 拡張機能: [C# Dev Kit](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csdevkit)

### 1.3. インフラ
* **AWS RDS (MySQL)**
    * 稼働中の MySQL データベースが必要です。

---

## 2. セットアップ手順

### 2.1. リポジトリのクローン

```bash
git clone <repository-url>
cd NSS-Order_Estimator
```

### 2.2. 環境変数の設定(.env)

データベース接続情報を含む`.env`ファイルを作成し、`NssOrderTool`プロジェクトフォルダの直下に配置します。

`NssOrderTool.env`

```TOML
# AWS RDS 接続情報
DB_HOST=your-db-endpoint.rds.amazonaws.com
DB_PORT=3306
DB_NAME=order_ranking_db
DB_USER=app_user
DB_PASSWORD=your_password

# AWS IP自動更新用 (Pythonスクリプト用)
AWS_REGION=ap-southeast-2
AWS_SECURITY_GROUP_ID=sg-xxxxxxxxxxxxxxxxx
```

### 2.3. AWS接続許可（IP自動更新）
AWS RDS はセキュリティグループによりアクセスが制限されています。開発を始める前に、現在の場所（IPアドレス）からの接続を許可する必要があります。

付属の Python スクリプトを使用すると、現在のグローバルIPを自動的に AWS の許可リストに追加できます。

```bash
# 初回のみライブラリインストール
pip install boto3 requests python-dotenv

# スクリプト実行 (プロジェクトルートから)
python scripts/update_security_group.py
```

---

# 3. アプリケーションの実行

**Windows (Visual Studio)**
1. `NssOrderTool/NssOrderTool.csproj`（またはソリューションファイル）をVisual Studioで開きます。
2. 上部の「開始」ボタン(▶) をクリックしてデバッグ実行します。

**macOS (VS Code/Terminal)**
ターミナルでプロジェクトフォルダに移動し、`dotnet run` コマンドを実行します。

```bash
cd NssOrderTool
dotnet run
```

# 4. プロジェクト構成

```Plaintext
.
├── NssOrderTool/           # C# アプリケーション本体 (Avalonia UI)
│   ├── Services/           # DB接続や計算ロジック
│   ├── ViewModels/         # 画面とデータの橋渡し
│   ├── Views/              # 画面レイアウト (XAML)
│   ├── Program.cs          # エントリーポイント
│   └── .env                # 設定ファイル (手動作成/Git対象外)
│
├── scripts/                # ユーティリティスクリプト
│   └── update_security_group.py  # AWS IP許可ツール (Python)
│
└── .gitignore              # Git除外設定 (C# / Python 両対応)
```

# 5. 開発フロー
* **main ブランチ**: C# (Avalonia) 版の最新コード
* **Python_CLI ブランチ**: 旧Python CLI 版のアーカイブ
