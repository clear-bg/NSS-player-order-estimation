# NSS Player Order Estimation Tool

このプロジェクトは、Nintendo Switch Sports サッカー等のゲームにおけるプレイヤーの並び順（観測データ）を基に、内部的なソートキー（Hidden ID）によるプレイヤー間の順序関係を推定・管理するためのツールです。

AWS RDS (MySQL) をバックエンドに使用し、CLIツールを用いて観測データの入力やランキングの閲覧を行います。

---

## 1. 開発環境のセットアップ

### 1.1. 必須要件
* **Python 3.10+**
* **AWS CLI**: セキュリティグループ更新のために必要です。
* **AWS アカウント & RDS**: 事前にMySQLデータベースが構築されていること。

### 1.2. リポジトリのクローンとライブラリインストール

リポジトリのクローン
```bash
git clone <repository_url>
cd NSS-player-order-estimation-CLI
```


仮想環境の作成
```bash
python -m venv venv
```

仮想環境有効化 (Mac/Linux)
```bash
source venv/bin/activate
```
仮想環境有効化 (Windows)
```bash
.\venv\Scripts\activate
```

ライブラリのインストール
```bash
pip install -r requirements.txt
# (requirements.txtが無い場合は以下を実行)
# pip install mysql-connector-python networkx python-dotenv boto3 requests
```

### 1.3. AWS CLIの初期設定
AWSのセキュリティグループを操作するため、ローカル環境に権限を設定します。

```bash
aws configure
# AWS Access Key ID: [あなたのアクセスキー]
# AWS Secret Access Key: [あなたのシークレットキー]
# Default region name: ap-southeast-2  (RDSのあるリージョンを指定)
# Default output format: json
```

## 2. 接続設定 (.env)
プロジェクトルートに `.env` ファイルを作成し、DB接続情報とAWS設定を記述します。 注意: このファイルは機密情報を含むため、Gitにはコミットしないでください。

## 3. 使用方法 (Mac/Linux)
セキュリティのため、AWS RDSは特定のIPアドレスからの接続のみを許可する設定にします。 ゲームや開発を始める前に、必ず **Step1** を実行して現在の自分のIPを許可リストに追加してください。

**Step 1: 接続元IPの自動更新**
現在のパブリックIPアドレスを取得し、AWSセキュリティグループのインバウンドルールを自動更新します。

```bash
python3 src/aws/update_security_group.py
```

- 成功すると `✅ 新しいルールを追加しました: xxx.xxx.xxx.xxx/32`と表示されます。
- これでDBへの経路が開通します。

**Step 2: アプリケーションの実行**
メインのCLIツールを起動します。

```bash
python3 src/main.py
```

**主な機能**
1. 新規観測データの入力:
   - 例: `PlayerA, PlayerB, PlayerC`
   - 画面上の並び順を入力すると、裏での優劣関係（A>B, A>C, B>C）としてDBに保存されます。
2. 現在の推定ランキングを表示:
   - 蓄積されたデータからグラフ理論（トポロジカルソート）を用いて、最も確からしい並び順を表示します。

## 4. ディレクトリ構造
```Plaintext
.
├── src/
│   ├── aws/               # AWS関連ユーティリティ
│   │   └── update_security_group.py
│   ├── cli/               # CLI操作ハンドラ
│   ├── core/              # DB操作、データ抽出ロジック
│   └── logic/             # ランキング計算アルゴリズム
└── .env                   # 設定ファイル (Git対象外)
```
