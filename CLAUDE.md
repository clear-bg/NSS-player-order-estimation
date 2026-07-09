# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## プロジェクト概要

NSS Order Estimation Tool — マッチング時の観測データ（ペアワイズ）から、プレイヤーの内部ソート順（Hidden ID）を推定するAvalonia（.NET 9）デスクトップMVVMアプリ。中核アルゴリズムは、「AがBより前に出現した」というペアの有向グラフを構築し、強連結成分分解（Tarjanのアルゴリズム）でサイクル（矛盾）を切り出したうえで、成分グラフをトポロジカルソートして階層化された順序を得る、というもの。加えて、対戦結果を記録しレーティング（マリオカート方式のスコア計算）を算出する「Arena」モジュールも持ち、これはSeason単位で管理される。

## Commands

All commands run from the repo root or `NssOrderTool/`.

```bash
# Build
dotnet build -c Release

# Run the app
dotnet run --project NssOrderTool

# Reset local SQLite DB (deletes the file; recreated via migrations on next launch)
dotnet run --project NssOrderTool -- reset-db test   # local_db_test.db
dotnet run --project NssOrderTool -- reset-db prod   # local_db_prod.db
dotnet run --project NssOrderTool -- reset-db all

# Regenerate ER diagram .puml sources from the current EF Core model
dotnet run --project NssOrderTool -- generate-er
# then render .puml -> .svg (requires a PlantUML jar dropped into NssOrderTool/docs/database/)
pwsh NssOrderTool/docs/database/build_er.ps1

# EF Core migrations (run inside NssOrderTool/)
dotnet ef migrations add <Name>
dotnet ef database update
```

**テストは現状つながっていない。** `NssOrderTool.Tests`（xUnit）は大規模なアーキテクチャ刷新に伴い意図的に `Order-Estimator.sln` から除外されており、`.github/workflows/dotnet.yml` の `dotnet test` ステップもコメントアウトされている（コミット `b69f5e8` 参照）。CIで実行されるのは `dotnet build` のみ。テストを追加・復活させる場合は、`.sln` へのプロジェクト再登録とCIのテストステップのコメント解除が必要。

## アーキテクチャ

**レイヤー構成:** `Views`（Avalonia `.axaml`）→ `ViewModels`（CommunityToolkit.Mvvm、`[ObservableProperty]`/`[RelayCommand]`）→ `Repositories`（EF Coreによるデータアクセス）→ `Database.AppDbContext`（SQLite）。ドメインアルゴリズムは `Services/Domain`、レーティング計算は `Services/Rating` に置く。単純なデータ形状の型は `Models/Domain`・`Models/DTOs`・`Models/UI`、永続化対象の型は `Models/Entities` に配置する。

**DIコンポジションルート:** `App.axaml.cs`。DbContext・リポジトリ・ドメインサービス・ViewModelは基本すべてここで `Transient` として登録される（例外は `IRatingCalculator` の `Singleton`）。`Startup`/`Program` 側に別建てのDI設定は無く、`Program.cs` はAvaloniaの起動のみを行う。

**ViewModel間通信:** `CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger`（`Messages/*Message.cs`）を使用。例として、あるViewModelから `MainWindowViewModel` へのタブ切り替え要求（`IRecipient<T>`、`RegisterAll(this)`）、書き込み後に他のViewModelへ再読み込みを促す `DatabaseUpdatedMessage` などがある。

**DBと環境切り替え:** SQLiteのみを使用。使用するDBファイルは、`appsettings.json` の `AppSettings.Environment`（`TEST` → `local_db_test.db`、`PROD`/`PRODUCTION` → `local_db_prod.db`）に基づき `App.axaml.cs` の起動時に決定される。環境・接続設定はアプリ内の **設定 (Settings)** タブから変更できるが、反映にはアプリの再起動が必要。`AppDbContext.Database.Migrate()` は起動のたびに自動実行される。

**論理削除・監査カラムの規約:** `ISoftDelete`（`IsDeleted`）を実装するEntityは `AppDbContext.OnModelCreating` でグローバルなクエリフィルタが設定されており、削除は物理削除ではなく論理削除（`IsDeleted = true`）が基本。ただし明示的にコメントで例外とされている箇所（例: `SequencePairs` の0件化、`OrderRepository.UndoObservationAsync` 内のコメント参照）は物理削除となる。`ITimestamp`（`CreatedAt`/`UpdatedAt`）を実装するEntityはこれらのフィールドが `AppDbContext.SaveChanges(Async)` 内で自動設定されるため、手動で設定しないこと。プレイヤーの主キーはサロゲートintではなくUUID文字列（`PlayerEntity.Id`）。プレイヤーの同一性統合（エイリアス統合）は `OrderRepository.MergePlayerIdsAsync` で行い、`SequencePairs`・`Players`・`ObservationDetails` を1トランザクション内でまとめて更新する必要がある。

**順序推定のコア:** `Services/Domain/OrderSorter.cs` — `Sort()`（SCC分解＋階層化トポロジカルソート）、`FindCyclePath()`（DFSで矛盾となるサイクルを1つ検出し、ユーザーへの提示に使う）、`FindPath()`（2プレイヤー間の最短経路をBFSで探索し、「なぜAがBより前なのか」の説明UIに使う）。

**プレイヤー同一性を共有する2つのデータ領域:**
- *順序推定*: `Observations`/`ObservationDetails`（生の入力履歴、Undo対応）→ 集約されて `SequencePairs`（predecessor→successor、頻度付き）→ `OrderSorter` に渡される。
- *Arena*: `ArenaSessions`/`ArenaRounds`/`ArenaParticipants`（対戦結果）→ `ArenaLogicService` + `IRatingCalculator`（`ScoreBasedRatingCalculator`）→ `RateHistories`/`PlayerSeasonRatings`。すべて `SeasonEntity` 単位でスコープされ（`IsActive = true` のシーズンは常に1つのみ）、`SeasonRepository` が管理する。

**マイグレーション:** `Migrations/` はEF Core（SQLiteプロバイダ）による自動生成物であり、手で編集しないこと。変更は `dotnet ef migrations add` で行う。

**コーディングスタイル:** `.cs` を含む全ファイルでインデントはスペース2つ（`NssOrderTool/.editorconfig` 参照）、改行コードはLF、文字コードはUTF-8。このコードベース内のコメントやログ/コンソール出力の大半は日本語で書かれているため、既存コードの近くを編集する際は英語に切り替えず既存のスタイルに合わせること。
