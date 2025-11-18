# NSS Player Order Estimation Tool
このプロジェクトは、ゲームの観測データ（プレイヤーの並び順）を基に、内部的なソートキーによるプレイヤー間の優劣関係を推定・管理するためのツールです。
---

## 1. 開発環境のセットアップ
本プロジェクトは Docker Compose を使用してMySQLサーバーを構築するため、開発環境によらず一貫した環境で動作します。

### 1.1. 必須ツールのインストール
以下のツールがシステムにインストールされていることを確認してください。
- Git: ソースコード管理
- Python 3.10+: アプリケーション実行環境
- Docker Desktop: MYSQLサーバー（コンテナ）の実行環境

### 1.2. 接続情報の準備
プロジェクトルートに .env ファイルを作成し、MySQLへの接続情報を記述します。

```toml
# .env

# MySQL 接続設定
DB_HOST=localhost         # ホストOSから接続するため
DB_PORT=3306
DB_NAME=order_ranking_db
DB_USER=app_user
DB_PASSWORD=your_app_password # ⚠️ docker-compose.ymlと一致させる
```

## 2. データベースの管理（Docker Compose）
Docker Compose コマンドは、MySQLサーバーの起動、停止、および管理に使用します。
コマンド,説明
docker compose up -d,MySQLサーバーをバックグラウンドで起動します。（初回はイメージのダウンロードが行われます）
docker compose stop,サーバーを停止します。
docker compose down,サーバーを停止し、コンテナを削除します。データボリュームは残るため、次回起動時にデータは引き継がれます。
docker compose ps,現在起動中のコンテナの状態を確認します。STATUSが Up であれば正常です。
docker compose logs -f,サーバーのログをリアルタイムで確認します。（デバッグ時）

## 3. Python 仮想環境の管理
開発に必要なライブラリをプロジェクトごとに隔離し、管理します。

### 3.1. 仮想環境の作成とセットアップ
プロジェクトルートで一度だけ実行します。

```bash
# 仮想環境の作成
python -m venv venv

# 必要なライブラリのインストール
./venv/bin/pip install -r requirements.txt # (requirements.txt作成後はこれを使用)
# もしくは:
# pip install mysql-connector-python networkx python-dotenv
```

### 3.2. 仮想環境への入り方（有効化）
ターミナルセッションを開始するたびに、以下のコマンドで仮想環境を有効化してください。

OS,コマンド
macOS / Linux,source venv/bin/activate
Windows (PowerShell),.\venv\Scripts\Activate.ps1
Windows (Command Prompt),.\venv\Scripts\activate

## 4. 初期セットアップとテスト

### 4.1. データベース構造の作成
仮想環境に入った状態で実行します。

```bash
# サーバーが起動していることを確認してから実行
python core/db_manager.py
```

### 4.2. コア機能のテスト
```bash
./venv/bin/python3 -m src.main
```

## 5. Gitコマンド
基本的なコミットの流れです。

```bash
git add .
git commit -m "feat: [コミット内容の要約]"
git push origin main
```
