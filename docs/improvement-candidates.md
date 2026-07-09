# 実装改善候補

現状のコードベース調査（2026-07-09時点）で見つかった、次にやると価値が高そうな改善候補と、それぞれの対応方針をまとめる。目立った未実装(TODO/FIXME)は少なく、大半は「動いてはいるが整えるべき」箇所。

## 優先度: 高

### 1. README刷新

**現状の問題**
- `README.md:46` の「入力欄へのプレイヤー名の入力（カンマ区切り）」という説明が、実際のUI（`NssOrderTool/Views/SimulationView.axaml` + `SimulationViewModel.cs`、8人固定スロット入力方式）と一致していない。
- Arena/Season機能（対戦記録、レーティング計算、シーズン管理タブ）が実装済みにもかかわらず、READMEに一切記載がない。

**対応方針**
- 順序推定タブの操作説明を、現行の固定スロット入力方式に合わせて書き直す。
- Arena機能（アリーナ集計・アリーナデータ・シーズン管理の各タブ）の概要と使い方セクションを追加する。
- 対象ファイル: `README.md`

## 優先度: 中

### 2. `reset-db prod` の誤操作防止

**現状の問題**
- `NssOrderTool/App.axaml.cs` の `RunDbResetMode`（200〜237行目付近）が、`reset-db prod` 実行時に確認なしで即座に `local_db_prod.db` を削除する。CLIの打ち間違いでPRODデータを消失しうる。

**対応方針**
- `prod` を対象に含む場合のみ、追加の確認入力（例: `y/N` の標準入力プロンプト、または `--force` フラグの明示要求）を挟む。
- 対象ファイル: `NssOrderTool/App.axaml.cs`（`RunDbResetMode` メソッド）

### 3. 例外処理の統一

**現状の問題**
- `ArenaViewModel.cs`（341〜345行目付近）、`SimulationViewModel.cs`（87〜91行目付近）、`ArenaDataViewModel.cs`（126〜131行目、158〜161行目付近）が、読み込み系の例外を `Debug.WriteLine` のみで処理しており、Serilogへの永続ログ記録にもユーザーへの通知にも繋がっていない。
- `PlayerHubViewModel.cs`（160〜168行目付近）のエイリアス登録処理は、例外種別を区別せず `catch { /* 重複エラー等はスキップ */ }` で握りつぶしており、DB接続断などの想定外エラーも静かに失敗する。
- `SettingsViewModel` のみ `ILogger`（Serilog）を使っており、ViewModel間で例外処理の作法が統一されていない。

**対応方針**
- 例外を握りつぶしている箇所は、想定内エラー（例: 重複）と想定外エラーを区別し、後者は最低限Serilogに記録する。
- 各ViewModelでのログ出力を `Debug.WriteLine` から `ILogger`（DIで注入済みのSerilogロガー）に統一する。
- 必要に応じて、ユーザーへのエラー通知（既存の `ConfirmationDialog` 等の仕組みを流用できないか確認）を追加する。
- 対象ファイル: `NssOrderTool/ViewModels/ArenaViewModel.cs`、`SimulationViewModel.cs`、`ArenaDataViewModel.cs`、`PlayerHubViewModel.cs`

### 4. テストプロジェクトの復活

**現状の問題**
- `NssOrderTool.Tests`（xUnit）が、大規模リファクタリング（コミット `b69f5e8`）に伴い `Order-Estimator.sln` から除外され、`.github/workflows/dotnet.yml` の `dotnet test` ステップもコメントアウトされたままになっている（`CLAUDE.md` にも記載済み）。

**対応方針**
- リファクタリング後のアーキテクチャ（Repository/ViewModelのコンストラクタ変更など）に合わせて既存テストを修正し、ビルドが通る状態に戻す。
- `.sln` へのプロジェクト再登録、CIの `dotnet test` ステップのコメント解除。
- 対象ファイル: `NssOrderTool.Tests/*`、`Order-Estimator.sln`、`.github/workflows/dotnet.yml`
- 備考: 対象範囲が広く、他の候補より着手コストが大きい見込み。

## 優先度: 低（参考）

### 5. 入力バリデーションの強化

**現状の問題**
- `PlayerRepository.GetOrCreatePlayersAsync`（`NssOrderTool/Repositories/PlayerRepository.cs:26` 付近）は、トリムと空白チェックのみで、文字数上限や禁止文字のチェックがない。
- `PlayerHubViewModel.cs:153` 付近のエイリアス `Split(',')` も同様に簡易的で、空要素や本人名との重複以外は無検証。

**対応方針**
- プレイヤー名・エイリアス入力に対し、文字数上限・禁止文字（カンマ自体を含む名前など）の検証を追加する。
- 対象ファイル: `NssOrderTool/Repositories/PlayerRepository.cs`、`NssOrderTool/ViewModels/PlayerHubViewModel.cs`

### 6. DIライフタイムの見直し

**現状の問題**
- `App.axaml.cs`（60〜91行目付近）で `AppDbContext` を含むほぼ全サービスが `Transient` 登録されている。`AppDbContext` をTransientにするのはEF Coreの一般的な推奨（`Scoped`）から外れており、接続コストやトラッキング汚染のリスクがある。

**対応方針**
- デスクトップアプリでの利用パターン（1操作＝1スコープ相当）を踏まえ、`AppDbContext` とリポジトリ群を `Scoped` に変更した場合の影響範囲を調査した上で移行する。
- 対象ファイル: `NssOrderTool/App.axaml.cs`
- 備考: 影響範囲の調査コストが高いため、他の改善より優先度は低い。
