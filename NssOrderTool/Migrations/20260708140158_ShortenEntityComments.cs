using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NssOrderTool.Migrations
{
    /// <inheritdoc />
    public partial class ShortenEntityComments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterTable(
                name: "SequencePairs",
                comment: "出現順序ペアの頻度統計テーブル",
                oldComment: "プレイヤーの出現順序ペア（前後関係）の出現頻度統計");

            migrationBuilder.AlterTable(
                name: "Seasons",
                comment: "レート集計期間（シーズン）を管理するテーブル",
                oldComment: "レート計算や集計の期間区切りとなる「シーズン」の情報を管理するテーブル");

            migrationBuilder.AlterTable(
                name: "RateHistories",
                comment: "プレイヤーのレート変動履歴テーブル",
                oldComment: "プレイヤーのレート変動履歴を時系列で記録するテーブル");

            migrationBuilder.AlterTable(
                name: "PlayerSeasonRatings",
                comment: "シーズン別レート・成績のスナップショットを管理するテーブル",
                oldComment: "プレイヤーのシーズンごとのレート情報および成績（スナップショット）を管理するテーブル");

            migrationBuilder.AlterTable(
                name: "Players",
                comment: "プレイヤーの基本情報と成績を管理するテーブル",
                oldComment: "正規のプレイヤー基本情報および通算成績・最新レートを管理するテーブル");

            migrationBuilder.AlterTable(
                name: "Observations",
                comment: "プレイヤー出現順序の観測データを管理するテーブル",
                oldComment: "プレイヤーの出現順序などの観測データ（親レコード）を管理するテーブル");

            migrationBuilder.AlterTable(
                name: "ObservationDetails",
                comment: "観測データの明細（プレイヤー順序）を管理するテーブル",
                oldComment: "1回の観測データに含まれる個々のプレイヤーとその順序（子レコード）を管理するテーブル");

            migrationBuilder.AlterTable(
                name: "ArenaSessions",
                comment: "アリーナ対戦セッションを管理するテーブル",
                oldComment: "アリーナ（対戦環境）の1セッション（試合単位）を管理するテーブル");

            migrationBuilder.AlterTable(
                name: "ArenaRounds",
                comment: "アリーナラウンドの結果を管理するテーブル",
                oldComment: "アリーナセッション内の各ラウンド（局所的な対戦）の結果を管理するテーブル");

            migrationBuilder.AlterTable(
                name: "ArenaParticipants",
                comment: "アリーナ参加者の結果を管理するテーブル",
                oldComment: "アリーナセッションに参加する各プレイヤーの情報と最終結果（座席、勝利数、順位など）を管理するテーブル");

            migrationBuilder.AlterTable(
                name: "Aliases",
                comment: "プレイヤー別名を管理するテーブル",
                oldComment: "プレイヤーの別名（エイリアス）を管理するテーブル");

            migrationBuilder.AlterColumn<DateTime>(
                name: "updated_at",
                table: "SequencePairs",
                type: "TEXT",
                nullable: false,
                comment: "更新日時",
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldComment: "レコード最終更新日時");

            migrationBuilder.AlterColumn<bool>(
                name: "is_deleted",
                table: "SequencePairs",
                type: "INTEGER",
                nullable: false,
                comment: "論理削除フラグ",
                oldClrType: typeof(bool),
                oldType: "INTEGER",
                oldComment: "論理削除フラグ（trueで削除済み）");

            migrationBuilder.AlterColumn<int>(
                name: "frequency",
                table: "SequencePairs",
                type: "INTEGER",
                nullable: false,
                comment: "累計出現頻度",
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldComment: "この順序ペアが観測された累計頻度（回数）");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "SequencePairs",
                type: "TEXT",
                nullable: false,
                comment: "作成日時",
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldComment: "レコード作成日時");

            migrationBuilder.AlterColumn<string>(
                name: "successor_id",
                table: "SequencePairs",
                type: "TEXT",
                nullable: false,
                comment: "後方プレイヤーID",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldComment: "後方に位置するプレイヤーのID");

            migrationBuilder.AlterColumn<string>(
                name: "predecessor_id",
                table: "SequencePairs",
                type: "TEXT",
                nullable: false,
                comment: "前方プレイヤーID",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldComment: "前方に位置するプレイヤーのID");

            migrationBuilder.AlterColumn<DateTime>(
                name: "start_date",
                table: "Seasons",
                type: "TEXT",
                nullable: false,
                comment: "開始日時",
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldComment: "シーズンの開始日時");

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "Seasons",
                type: "TEXT",
                nullable: false,
                comment: "シーズン表示名",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldComment: "シーズンの表示名（例: 'Season 1'、'2026 Spring' など）");

            migrationBuilder.AlterColumn<bool>(
                name: "is_active",
                table: "Seasons",
                type: "INTEGER",
                nullable: false,
                comment: "進行中フラグ",
                oldClrType: typeof(bool),
                oldType: "INTEGER",
                oldComment: "現在アクティブに進行しているシーズンかどうかのフラグ");

            migrationBuilder.AlterColumn<DateTime>(
                name: "end_date",
                table: "Seasons",
                type: "TEXT",
                nullable: true,
                comment: "終了日時（進行中はnull）",
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldNullable: true,
                oldComment: "シーズンの終了日時（現在進行中のシーズンの場合は null）");

            migrationBuilder.AlterColumn<int>(
                name: "id",
                table: "Seasons",
                type: "INTEGER",
                nullable: false,
                comment: "自動採番ID",
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldComment: "シーズンのサロゲートキー（自動インクリメントID）")
                .Annotation("Sqlite:Autoincrement", true)
                .OldAnnotation("Sqlite:Autoincrement", true);

            migrationBuilder.AlterColumn<int>(
                name: "season_id",
                table: "RateHistories",
                type: "INTEGER",
                nullable: false,
                comment: "所属シーズンID",
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldComment: "この履歴が属するシーズンのID（Seasonsテーブルの外部キー）");

            migrationBuilder.AlterColumn<DateTime>(
                name: "RecordedAt",
                table: "RateHistories",
                type: "TEXT",
                nullable: false,
                comment: "記録日時",
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldComment: "レートが変動・記録された日時");

            migrationBuilder.AlterColumn<double>(
                name: "Rate",
                table: "RateHistories",
                type: "REAL",
                nullable: false,
                comment: "記録時点のレート値",
                oldClrType: typeof(double),
                oldType: "REAL",
                oldComment: "記録時点での計算済みレート値");

            migrationBuilder.AlterColumn<string>(
                name: "PlayerId",
                table: "RateHistories",
                type: "TEXT",
                nullable: false,
                comment: "対象プレイヤーID",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldComment: "対象プレイヤーのID（Playersテーブルの外部キー）");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "RateHistories",
                type: "INTEGER",
                nullable: false,
                comment: "自動採番ID",
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldComment: "レート履歴のサロゲートキー（自動インクリメントID）")
                .Annotation("Sqlite:Autoincrement", true)
                .OldAnnotation("Sqlite:Autoincrement", true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "updated_at",
                table: "PlayerSeasonRatings",
                type: "TEXT",
                nullable: false,
                comment: "更新日時",
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldComment: "レコード最終更新日時");

            migrationBuilder.AlterColumn<int>(
                name: "total_wins",
                table: "PlayerSeasonRatings",
                type: "INTEGER",
                nullable: false,
                comment: "シーズン内勝利数",
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldComment: "このシーズンでの累計勝利数");

            migrationBuilder.AlterColumn<int>(
                name: "total_matches",
                table: "PlayerSeasonRatings",
                type: "INTEGER",
                nullable: false,
                comment: "シーズン内試合数",
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldComment: "このシーズンでの累計試合数");

            migrationBuilder.AlterColumn<int>(
                name: "season_id",
                table: "PlayerSeasonRatings",
                type: "INTEGER",
                nullable: false,
                comment: "対象シーズンID",
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldComment: "対象シーズンのID（Seasonsテーブルの外部キー）");

            migrationBuilder.AlterColumn<double>(
                name: "rate_sigma",
                table: "PlayerSeasonRatings",
                type: "REAL",
                nullable: false,
                comment: "シーズン内レート不確実性（σ）",
                oldClrType: typeof(double),
                oldType: "REAL",
                oldComment: "このシーズンにおけるレート不確実性（σ）");

            migrationBuilder.AlterColumn<double>(
                name: "rate_mean",
                table: "PlayerSeasonRatings",
                type: "REAL",
                nullable: false,
                comment: "シーズン内レート平均値（μ）",
                oldClrType: typeof(double),
                oldType: "REAL",
                oldComment: "このシーズンにおけるレート予測平均値（μ）");

            migrationBuilder.AlterColumn<string>(
                name: "player_id",
                table: "PlayerSeasonRatings",
                type: "TEXT",
                nullable: false,
                comment: "対象プレイヤーID",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldComment: "対象プレイヤーのID（Playersテーブルの外部キー）");

            migrationBuilder.AlterColumn<bool>(
                name: "is_deleted",
                table: "PlayerSeasonRatings",
                type: "INTEGER",
                nullable: false,
                comment: "論理削除フラグ",
                oldClrType: typeof(bool),
                oldType: "INTEGER",
                oldComment: "論理削除フラグ（trueで削除済み）");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "PlayerSeasonRatings",
                type: "TEXT",
                nullable: false,
                comment: "作成日時",
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldComment: "レコード作成日時");

            migrationBuilder.AlterColumn<int>(
                name: "id",
                table: "PlayerSeasonRatings",
                type: "INTEGER",
                nullable: false,
                comment: "自動採番ID",
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldComment: "シーズン別レート情報のサロゲートキー（自動インクリメントID）")
                .Annotation("Sqlite:Autoincrement", true)
                .OldAnnotation("Sqlite:Autoincrement", true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "updated_at",
                table: "Players",
                type: "TEXT",
                nullable: false,
                comment: "更新日時",
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldComment: "レコード最終更新日時");

            migrationBuilder.AlterColumn<double>(
                name: "rate_sigma",
                table: "Players",
                type: "REAL",
                nullable: false,
                comment: "現在のレート不確実性（σ）",
                oldClrType: typeof(double),
                oldType: "REAL",
                oldComment: "プレイヤーの現在のレート不確実性（σ）");

            migrationBuilder.AlterColumn<double>(
                name: "rate_mean",
                table: "Players",
                type: "REAL",
                nullable: false,
                comment: "現在のレート平均値（μ）",
                oldClrType: typeof(double),
                oldType: "REAL",
                oldComment: "プレイヤーの現在の内部レート予測平均値（μ）");

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "Players",
                type: "TEXT",
                nullable: true,
                comment: "表示名",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true,
                oldComment: "プレイヤーの正式な表示名");

            migrationBuilder.AlterColumn<string>(
                name: "memo",
                table: "Players",
                type: "TEXT",
                nullable: false,
                comment: "自由記述メモ",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldComment: "プレイヤーに関する自由記述のメモ");

            migrationBuilder.AlterColumn<DateTime>(
                name: "last_played_at",
                table: "Players",
                type: "TEXT",
                nullable: true,
                comment: "最終対戦日時",
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldNullable: true,
                oldComment: "最後に試合を行った日時");

            migrationBuilder.AlterColumn<bool>(
                name: "is_deleted",
                table: "Players",
                type: "INTEGER",
                nullable: false,
                comment: "論理削除フラグ",
                oldClrType: typeof(bool),
                oldType: "INTEGER",
                oldComment: "論理削除フラグ（trueで削除済み）");

            migrationBuilder.AlterColumn<bool>(
                name: "is_active",
                table: "Players",
                type: "INTEGER",
                nullable: false,
                comment: "アクティブフラグ",
                oldClrType: typeof(bool),
                oldType: "INTEGER",
                oldComment: "プレイヤーが現在アクティブかどうかのフラグ");

            migrationBuilder.AlterColumn<DateTime>(
                name: "first_seen",
                table: "Players",
                type: "TEXT",
                nullable: false,
                comment: "初回観測日時",
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldComment: "システム内でプレイヤーが初めて観測された日時");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "Players",
                type: "TEXT",
                nullable: false,
                comment: "作成日時",
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldComment: "レコード作成日時");

            migrationBuilder.AlterColumn<string>(
                name: "player_id",
                table: "Players",
                type: "TEXT",
                nullable: false,
                comment: "プレイヤーID（UUID）",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldComment: "プレイヤーの一意な識別子（UUID文字列）");

            migrationBuilder.AlterColumn<DateTime>(
                name: "updated_at",
                table: "Observations",
                type: "TEXT",
                nullable: false,
                comment: "更新日時",
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldComment: "レコード最終更新日時");

            migrationBuilder.AlterColumn<DateTime>(
                name: "observation_time",
                table: "Observations",
                type: "TEXT",
                nullable: false,
                comment: "観測日時",
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldComment: "観測が実行・記録された日時");

            migrationBuilder.AlterColumn<bool>(
                name: "is_deleted",
                table: "Observations",
                type: "INTEGER",
                nullable: false,
                comment: "論理削除フラグ",
                oldClrType: typeof(bool),
                oldType: "INTEGER",
                oldComment: "論理削除フラグ（trueで削除済み）");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "Observations",
                type: "TEXT",
                nullable: false,
                comment: "作成日時",
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldComment: "レコード作成日時");

            migrationBuilder.AlterColumn<int>(
                name: "observation_id",
                table: "Observations",
                type: "INTEGER",
                nullable: false,
                comment: "自動採番ID",
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldComment: "観測データのサロゲートキー（自動インクリメントID）")
                .Annotation("Sqlite:Autoincrement", true)
                .OldAnnotation("Sqlite:Autoincrement", true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "updated_at",
                table: "ObservationDetails",
                type: "TEXT",
                nullable: false,
                comment: "更新日時",
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldComment: "レコード最終更新日時");

            migrationBuilder.AlterColumn<string>(
                name: "player_id",
                table: "ObservationDetails",
                type: "TEXT",
                nullable: false,
                comment: "観測プレイヤーID",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldComment: "観測されたプレイヤーのID（Playersテーブルの外部キー）");

            migrationBuilder.AlterColumn<int>(
                name: "order_index",
                table: "ObservationDetails",
                type: "INTEGER",
                nullable: false,
                comment: "出現順序（0,1,2...）",
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldComment: "観測されたプレイヤーの順番・配置インデックス（0, 1, 2...）");

            migrationBuilder.AlterColumn<int>(
                name: "observation_id",
                table: "ObservationDetails",
                type: "INTEGER",
                nullable: false,
                comment: "親観測データID",
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldComment: "紐づく親観測データのID（Observationsテーブルの外部キー）");

            migrationBuilder.AlterColumn<bool>(
                name: "is_deleted",
                table: "ObservationDetails",
                type: "INTEGER",
                nullable: false,
                comment: "論理削除フラグ",
                oldClrType: typeof(bool),
                oldType: "INTEGER",
                oldComment: "論理削除フラグ（trueで削除済み）");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "ObservationDetails",
                type: "TEXT",
                nullable: false,
                comment: "作成日時",
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldComment: "レコード作成日時");

            migrationBuilder.AlterColumn<int>(
                name: "detail_id",
                table: "ObservationDetails",
                type: "INTEGER",
                nullable: false,
                comment: "自動採番ID",
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldComment: "観測詳細データのサロゲートキー（自動インクリメントID）")
                .Annotation("Sqlite:Autoincrement", true)
                .OldAnnotation("Sqlite:Autoincrement", true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "updated_at",
                table: "ArenaSessions",
                type: "TEXT",
                nullable: false,
                comment: "更新日時",
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldComment: "レコード最終更新日時");

            migrationBuilder.AlterColumn<DateTime>(
                name: "session_date",
                table: "ArenaSessions",
                type: "TEXT",
                nullable: false,
                comment: "実施日時",
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldComment: "セッションが実際に実施・観測された日時");

            migrationBuilder.AlterColumn<int>(
                name: "season_id",
                table: "ArenaSessions",
                type: "INTEGER",
                nullable: false,
                comment: "所属シーズンID",
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldComment: "このセッションが属するシーズンのID（Seasonsテーブルの外部キー）");

            migrationBuilder.AlterColumn<string>(
                name: "memo",
                table: "ArenaSessions",
                type: "TEXT",
                nullable: false,
                comment: "自由記述メモ",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldComment: "セッションに関する自由記述のメモ");

            migrationBuilder.AlterColumn<bool>(
                name: "is_valid",
                table: "ArenaSessions",
                type: "INTEGER",
                nullable: false,
                comment: "結果有効フラグ",
                oldClrType: typeof(bool),
                oldType: "INTEGER",
                oldComment: "セッションの対戦結果が有効かどうかのフラグ（無効試合の除外用）");

            migrationBuilder.AlterColumn<bool>(
                name: "is_deleted",
                table: "ArenaSessions",
                type: "INTEGER",
                nullable: false,
                comment: "論理削除フラグ",
                oldClrType: typeof(bool),
                oldType: "INTEGER",
                oldComment: "論理削除フラグ（trueで削除済み）");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "ArenaSessions",
                type: "TEXT",
                nullable: false,
                comment: "作成日時",
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldComment: "レコード作成日時");

            migrationBuilder.AlterColumn<int>(
                name: "session_id",
                table: "ArenaSessions",
                type: "INTEGER",
                nullable: false,
                comment: "自動採番ID",
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldComment: "セッションのサロゲートキー（自動インクリメントID）")
                .Annotation("Sqlite:Autoincrement", true)
                .OldAnnotation("Sqlite:Autoincrement", true);

            migrationBuilder.AlterColumn<int>(
                name: "winning_team",
                table: "ArenaRounds",
                type: "INTEGER",
                nullable: false,
                comment: "勝利チーム（0:無効 1:Blue 2:Orange）",
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldComment: "ラウンドの勝利チーム（0: 引き分け/無効, 1: Blueチーム, 2: Orangeチーム）");

            migrationBuilder.AlterColumn<DateTime>(
                name: "updated_at",
                table: "ArenaRounds",
                type: "TEXT",
                nullable: false,
                comment: "更新日時",
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldComment: "レコード最終更新日時");

            migrationBuilder.AlterColumn<int>(
                name: "session_id",
                table: "ArenaRounds",
                type: "INTEGER",
                nullable: false,
                comment: "アリーナセッションID",
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldComment: "紐づくアリーナセッションのID（ArenaSessionsテーブルの外部キー）");

            migrationBuilder.AlterColumn<int>(
                name: "round_number",
                table: "ArenaRounds",
                type: "INTEGER",
                nullable: false,
                comment: "ラウンド番号（1〜14）",
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldComment: "セッション内でのラウンド進行番号（通常 1 〜 14）");

            migrationBuilder.AlterColumn<bool>(
                name: "is_deleted",
                table: "ArenaRounds",
                type: "INTEGER",
                nullable: false,
                comment: "論理削除フラグ",
                oldClrType: typeof(bool),
                oldType: "INTEGER",
                oldComment: "論理削除フラグ（trueで削除済み）");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "ArenaRounds",
                type: "TEXT",
                nullable: false,
                comment: "作成日時",
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldComment: "レコード作成日時");

            migrationBuilder.AlterColumn<int>(
                name: "round_id",
                table: "ArenaRounds",
                type: "INTEGER",
                nullable: false,
                comment: "自動採番ID",
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldComment: "ラウンドのサロゲートキー（自動インクリメントID）")
                .Annotation("Sqlite:Autoincrement", true)
                .OldAnnotation("Sqlite:Autoincrement", true);

            migrationBuilder.AlterColumn<int>(
                name: "win_count",
                table: "ArenaParticipants",
                type: "INTEGER",
                nullable: false,
                comment: "獲得勝利数",
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldComment: "このセッションで獲得した最終的な勝利数");

            migrationBuilder.AlterColumn<DateTime>(
                name: "updated_at",
                table: "ArenaParticipants",
                type: "TEXT",
                nullable: false,
                comment: "更新日時",
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldComment: "レコード最終更新日時");

            migrationBuilder.AlterColumn<int>(
                name: "slot_index",
                table: "ArenaParticipants",
                type: "INTEGER",
                nullable: false,
                comment: "座席番号（0〜7）",
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldComment: "セッション内での座席番号・配置インデックス（通常 0〜7）");

            migrationBuilder.AlterColumn<int>(
                name: "session_id",
                table: "ArenaParticipants",
                type: "INTEGER",
                nullable: false,
                comment: "アリーナセッションID",
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldComment: "紐づくアリーナセッションのID（ArenaSessionsテーブルの外部キー）");

            migrationBuilder.AlterColumn<int>(
                name: "rank",
                table: "ArenaParticipants",
                type: "INTEGER",
                nullable: false,
                comment: "最終順位",
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldComment: "このセッションでの最終順位");

            migrationBuilder.AlterColumn<string>(
                name: "player_id",
                table: "ArenaParticipants",
                type: "TEXT",
                nullable: false,
                comment: "参加プレイヤーID",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldComment: "参加したプレイヤーのID（Playersテーブルの外部キー）");

            migrationBuilder.AlterColumn<bool>(
                name: "is_deleted",
                table: "ArenaParticipants",
                type: "INTEGER",
                nullable: false,
                comment: "論理削除フラグ",
                oldClrType: typeof(bool),
                oldType: "INTEGER",
                oldComment: "論理削除フラグ（trueで削除済み）");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "ArenaParticipants",
                type: "TEXT",
                nullable: false,
                comment: "作成日時",
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldComment: "レコード作成日時");

            migrationBuilder.AlterColumn<int>(
                name: "participant_id",
                table: "ArenaParticipants",
                type: "INTEGER",
                nullable: false,
                comment: "自動採番ID",
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldComment: "参加情報のサロゲートキー（自動インクリメントID）")
                .Annotation("Sqlite:Autoincrement", true)
                .OldAnnotation("Sqlite:Autoincrement", true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "updated_at",
                table: "Aliases",
                type: "TEXT",
                nullable: false,
                comment: "更新日時",
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldComment: "レコード最終更新日時");

            migrationBuilder.AlterColumn<string>(
                name: "target_player_id",
                table: "Aliases",
                type: "TEXT",
                nullable: false,
                comment: "紐付け先プレイヤーID",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldComment: "紐付け先となる正規のプレイヤーID（Playersテーブルの外部キー）");

            migrationBuilder.AlterColumn<bool>(
                name: "is_deleted",
                table: "Aliases",
                type: "INTEGER",
                nullable: false,
                comment: "論理削除フラグ",
                oldClrType: typeof(bool),
                oldType: "INTEGER",
                oldComment: "論理削除フラグ（trueで削除済み）");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "Aliases",
                type: "TEXT",
                nullable: false,
                comment: "作成日時",
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldComment: "レコード作成日時");

            migrationBuilder.AlterColumn<string>(
                name: "alias_name",
                table: "Aliases",
                type: "TEXT",
                nullable: false,
                comment: "観測時に入力された別名",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldComment: "観測時に入力されたプレイヤーの別名");

            migrationBuilder.AlterColumn<int>(
                name: "id",
                table: "Aliases",
                type: "INTEGER",
                nullable: false,
                comment: "自動採番ID",
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldComment: "サロゲートキー（自動インクリメントID）")
                .Annotation("Sqlite:Autoincrement", true)
                .OldAnnotation("Sqlite:Autoincrement", true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterTable(
                name: "SequencePairs",
                comment: "プレイヤーの出現順序ペア（前後関係）の出現頻度統計",
                oldComment: "出現順序ペアの頻度統計テーブル");

            migrationBuilder.AlterTable(
                name: "Seasons",
                comment: "レート計算や集計の期間区切りとなる「シーズン」の情報を管理するテーブル",
                oldComment: "レート集計期間（シーズン）を管理するテーブル");

            migrationBuilder.AlterTable(
                name: "RateHistories",
                comment: "プレイヤーのレート変動履歴を時系列で記録するテーブル",
                oldComment: "プレイヤーのレート変動履歴テーブル");

            migrationBuilder.AlterTable(
                name: "PlayerSeasonRatings",
                comment: "プレイヤーのシーズンごとのレート情報および成績（スナップショット）を管理するテーブル",
                oldComment: "シーズン別レート・成績のスナップショットを管理するテーブル");

            migrationBuilder.AlterTable(
                name: "Players",
                comment: "正規のプレイヤー基本情報および通算成績・最新レートを管理するテーブル",
                oldComment: "プレイヤーの基本情報と成績を管理するテーブル");

            migrationBuilder.AlterTable(
                name: "Observations",
                comment: "プレイヤーの出現順序などの観測データ（親レコード）を管理するテーブル",
                oldComment: "プレイヤー出現順序の観測データを管理するテーブル");

            migrationBuilder.AlterTable(
                name: "ObservationDetails",
                comment: "1回の観測データに含まれる個々のプレイヤーとその順序（子レコード）を管理するテーブル",
                oldComment: "観測データの明細（プレイヤー順序）を管理するテーブル");

            migrationBuilder.AlterTable(
                name: "ArenaSessions",
                comment: "アリーナ（対戦環境）の1セッション（試合単位）を管理するテーブル",
                oldComment: "アリーナ対戦セッションを管理するテーブル");

            migrationBuilder.AlterTable(
                name: "ArenaRounds",
                comment: "アリーナセッション内の各ラウンド（局所的な対戦）の結果を管理するテーブル",
                oldComment: "アリーナラウンドの結果を管理するテーブル");

            migrationBuilder.AlterTable(
                name: "ArenaParticipants",
                comment: "アリーナセッションに参加する各プレイヤーの情報と最終結果（座席、勝利数、順位など）を管理するテーブル",
                oldComment: "アリーナ参加者の結果を管理するテーブル");

            migrationBuilder.AlterTable(
                name: "Aliases",
                comment: "プレイヤーの別名（エイリアス）を管理するテーブル",
                oldComment: "プレイヤー別名を管理するテーブル");

            migrationBuilder.AlterColumn<DateTime>(
                name: "updated_at",
                table: "SequencePairs",
                type: "TEXT",
                nullable: false,
                comment: "レコード最終更新日時",
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldComment: "更新日時");

            migrationBuilder.AlterColumn<bool>(
                name: "is_deleted",
                table: "SequencePairs",
                type: "INTEGER",
                nullable: false,
                comment: "論理削除フラグ（trueで削除済み）",
                oldClrType: typeof(bool),
                oldType: "INTEGER",
                oldComment: "論理削除フラグ");

            migrationBuilder.AlterColumn<int>(
                name: "frequency",
                table: "SequencePairs",
                type: "INTEGER",
                nullable: false,
                comment: "この順序ペアが観測された累計頻度（回数）",
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldComment: "累計出現頻度");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "SequencePairs",
                type: "TEXT",
                nullable: false,
                comment: "レコード作成日時",
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldComment: "作成日時");

            migrationBuilder.AlterColumn<string>(
                name: "successor_id",
                table: "SequencePairs",
                type: "TEXT",
                nullable: false,
                comment: "後方に位置するプレイヤーのID",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldComment: "後方プレイヤーID");

            migrationBuilder.AlterColumn<string>(
                name: "predecessor_id",
                table: "SequencePairs",
                type: "TEXT",
                nullable: false,
                comment: "前方に位置するプレイヤーのID",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldComment: "前方プレイヤーID");

            migrationBuilder.AlterColumn<DateTime>(
                name: "start_date",
                table: "Seasons",
                type: "TEXT",
                nullable: false,
                comment: "シーズンの開始日時",
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldComment: "開始日時");

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "Seasons",
                type: "TEXT",
                nullable: false,
                comment: "シーズンの表示名（例: 'Season 1'、'2026 Spring' など）",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldComment: "シーズン表示名");

            migrationBuilder.AlterColumn<bool>(
                name: "is_active",
                table: "Seasons",
                type: "INTEGER",
                nullable: false,
                comment: "現在アクティブに進行しているシーズンかどうかのフラグ",
                oldClrType: typeof(bool),
                oldType: "INTEGER",
                oldComment: "進行中フラグ");

            migrationBuilder.AlterColumn<DateTime>(
                name: "end_date",
                table: "Seasons",
                type: "TEXT",
                nullable: true,
                comment: "シーズンの終了日時（現在進行中のシーズンの場合は null）",
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldNullable: true,
                oldComment: "終了日時（進行中はnull）");

            migrationBuilder.AlterColumn<int>(
                name: "id",
                table: "Seasons",
                type: "INTEGER",
                nullable: false,
                comment: "シーズンのサロゲートキー（自動インクリメントID）",
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldComment: "自動採番ID")
                .Annotation("Sqlite:Autoincrement", true)
                .OldAnnotation("Sqlite:Autoincrement", true);

            migrationBuilder.AlterColumn<int>(
                name: "season_id",
                table: "RateHistories",
                type: "INTEGER",
                nullable: false,
                comment: "この履歴が属するシーズンのID（Seasonsテーブルの外部キー）",
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldComment: "所属シーズンID");

            migrationBuilder.AlterColumn<DateTime>(
                name: "RecordedAt",
                table: "RateHistories",
                type: "TEXT",
                nullable: false,
                comment: "レートが変動・記録された日時",
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldComment: "記録日時");

            migrationBuilder.AlterColumn<double>(
                name: "Rate",
                table: "RateHistories",
                type: "REAL",
                nullable: false,
                comment: "記録時点での計算済みレート値",
                oldClrType: typeof(double),
                oldType: "REAL",
                oldComment: "記録時点のレート値");

            migrationBuilder.AlterColumn<string>(
                name: "PlayerId",
                table: "RateHistories",
                type: "TEXT",
                nullable: false,
                comment: "対象プレイヤーのID（Playersテーブルの外部キー）",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldComment: "対象プレイヤーID");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "RateHistories",
                type: "INTEGER",
                nullable: false,
                comment: "レート履歴のサロゲートキー（自動インクリメントID）",
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldComment: "自動採番ID")
                .Annotation("Sqlite:Autoincrement", true)
                .OldAnnotation("Sqlite:Autoincrement", true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "updated_at",
                table: "PlayerSeasonRatings",
                type: "TEXT",
                nullable: false,
                comment: "レコード最終更新日時",
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldComment: "更新日時");

            migrationBuilder.AlterColumn<int>(
                name: "total_wins",
                table: "PlayerSeasonRatings",
                type: "INTEGER",
                nullable: false,
                comment: "このシーズンでの累計勝利数",
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldComment: "シーズン内勝利数");

            migrationBuilder.AlterColumn<int>(
                name: "total_matches",
                table: "PlayerSeasonRatings",
                type: "INTEGER",
                nullable: false,
                comment: "このシーズンでの累計試合数",
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldComment: "シーズン内試合数");

            migrationBuilder.AlterColumn<int>(
                name: "season_id",
                table: "PlayerSeasonRatings",
                type: "INTEGER",
                nullable: false,
                comment: "対象シーズンのID（Seasonsテーブルの外部キー）",
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldComment: "対象シーズンID");

            migrationBuilder.AlterColumn<double>(
                name: "rate_sigma",
                table: "PlayerSeasonRatings",
                type: "REAL",
                nullable: false,
                comment: "このシーズンにおけるレート不確実性（σ）",
                oldClrType: typeof(double),
                oldType: "REAL",
                oldComment: "シーズン内レート不確実性（σ）");

            migrationBuilder.AlterColumn<double>(
                name: "rate_mean",
                table: "PlayerSeasonRatings",
                type: "REAL",
                nullable: false,
                comment: "このシーズンにおけるレート予測平均値（μ）",
                oldClrType: typeof(double),
                oldType: "REAL",
                oldComment: "シーズン内レート平均値（μ）");

            migrationBuilder.AlterColumn<string>(
                name: "player_id",
                table: "PlayerSeasonRatings",
                type: "TEXT",
                nullable: false,
                comment: "対象プレイヤーのID（Playersテーブルの外部キー）",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldComment: "対象プレイヤーID");

            migrationBuilder.AlterColumn<bool>(
                name: "is_deleted",
                table: "PlayerSeasonRatings",
                type: "INTEGER",
                nullable: false,
                comment: "論理削除フラグ（trueで削除済み）",
                oldClrType: typeof(bool),
                oldType: "INTEGER",
                oldComment: "論理削除フラグ");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "PlayerSeasonRatings",
                type: "TEXT",
                nullable: false,
                comment: "レコード作成日時",
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldComment: "作成日時");

            migrationBuilder.AlterColumn<int>(
                name: "id",
                table: "PlayerSeasonRatings",
                type: "INTEGER",
                nullable: false,
                comment: "シーズン別レート情報のサロゲートキー（自動インクリメントID）",
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldComment: "自動採番ID")
                .Annotation("Sqlite:Autoincrement", true)
                .OldAnnotation("Sqlite:Autoincrement", true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "updated_at",
                table: "Players",
                type: "TEXT",
                nullable: false,
                comment: "レコード最終更新日時",
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldComment: "更新日時");

            migrationBuilder.AlterColumn<double>(
                name: "rate_sigma",
                table: "Players",
                type: "REAL",
                nullable: false,
                comment: "プレイヤーの現在のレート不確実性（σ）",
                oldClrType: typeof(double),
                oldType: "REAL",
                oldComment: "現在のレート不確実性（σ）");

            migrationBuilder.AlterColumn<double>(
                name: "rate_mean",
                table: "Players",
                type: "REAL",
                nullable: false,
                comment: "プレイヤーの現在の内部レート予測平均値（μ）",
                oldClrType: typeof(double),
                oldType: "REAL",
                oldComment: "現在のレート平均値（μ）");

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "Players",
                type: "TEXT",
                nullable: true,
                comment: "プレイヤーの正式な表示名",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true,
                oldComment: "表示名");

            migrationBuilder.AlterColumn<string>(
                name: "memo",
                table: "Players",
                type: "TEXT",
                nullable: false,
                comment: "プレイヤーに関する自由記述のメモ",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldComment: "自由記述メモ");

            migrationBuilder.AlterColumn<DateTime>(
                name: "last_played_at",
                table: "Players",
                type: "TEXT",
                nullable: true,
                comment: "最後に試合を行った日時",
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldNullable: true,
                oldComment: "最終対戦日時");

            migrationBuilder.AlterColumn<bool>(
                name: "is_deleted",
                table: "Players",
                type: "INTEGER",
                nullable: false,
                comment: "論理削除フラグ（trueで削除済み）",
                oldClrType: typeof(bool),
                oldType: "INTEGER",
                oldComment: "論理削除フラグ");

            migrationBuilder.AlterColumn<bool>(
                name: "is_active",
                table: "Players",
                type: "INTEGER",
                nullable: false,
                comment: "プレイヤーが現在アクティブかどうかのフラグ",
                oldClrType: typeof(bool),
                oldType: "INTEGER",
                oldComment: "アクティブフラグ");

            migrationBuilder.AlterColumn<DateTime>(
                name: "first_seen",
                table: "Players",
                type: "TEXT",
                nullable: false,
                comment: "システム内でプレイヤーが初めて観測された日時",
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldComment: "初回観測日時");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "Players",
                type: "TEXT",
                nullable: false,
                comment: "レコード作成日時",
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldComment: "作成日時");

            migrationBuilder.AlterColumn<string>(
                name: "player_id",
                table: "Players",
                type: "TEXT",
                nullable: false,
                comment: "プレイヤーの一意な識別子（UUID文字列）",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldComment: "プレイヤーID（UUID）");

            migrationBuilder.AlterColumn<DateTime>(
                name: "updated_at",
                table: "Observations",
                type: "TEXT",
                nullable: false,
                comment: "レコード最終更新日時",
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldComment: "更新日時");

            migrationBuilder.AlterColumn<DateTime>(
                name: "observation_time",
                table: "Observations",
                type: "TEXT",
                nullable: false,
                comment: "観測が実行・記録された日時",
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldComment: "観測日時");

            migrationBuilder.AlterColumn<bool>(
                name: "is_deleted",
                table: "Observations",
                type: "INTEGER",
                nullable: false,
                comment: "論理削除フラグ（trueで削除済み）",
                oldClrType: typeof(bool),
                oldType: "INTEGER",
                oldComment: "論理削除フラグ");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "Observations",
                type: "TEXT",
                nullable: false,
                comment: "レコード作成日時",
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldComment: "作成日時");

            migrationBuilder.AlterColumn<int>(
                name: "observation_id",
                table: "Observations",
                type: "INTEGER",
                nullable: false,
                comment: "観測データのサロゲートキー（自動インクリメントID）",
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldComment: "自動採番ID")
                .Annotation("Sqlite:Autoincrement", true)
                .OldAnnotation("Sqlite:Autoincrement", true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "updated_at",
                table: "ObservationDetails",
                type: "TEXT",
                nullable: false,
                comment: "レコード最終更新日時",
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldComment: "更新日時");

            migrationBuilder.AlterColumn<string>(
                name: "player_id",
                table: "ObservationDetails",
                type: "TEXT",
                nullable: false,
                comment: "観測されたプレイヤーのID（Playersテーブルの外部キー）",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldComment: "観測プレイヤーID");

            migrationBuilder.AlterColumn<int>(
                name: "order_index",
                table: "ObservationDetails",
                type: "INTEGER",
                nullable: false,
                comment: "観測されたプレイヤーの順番・配置インデックス（0, 1, 2...）",
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldComment: "出現順序（0,1,2...）");

            migrationBuilder.AlterColumn<int>(
                name: "observation_id",
                table: "ObservationDetails",
                type: "INTEGER",
                nullable: false,
                comment: "紐づく親観測データのID（Observationsテーブルの外部キー）",
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldComment: "親観測データID");

            migrationBuilder.AlterColumn<bool>(
                name: "is_deleted",
                table: "ObservationDetails",
                type: "INTEGER",
                nullable: false,
                comment: "論理削除フラグ（trueで削除済み）",
                oldClrType: typeof(bool),
                oldType: "INTEGER",
                oldComment: "論理削除フラグ");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "ObservationDetails",
                type: "TEXT",
                nullable: false,
                comment: "レコード作成日時",
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldComment: "作成日時");

            migrationBuilder.AlterColumn<int>(
                name: "detail_id",
                table: "ObservationDetails",
                type: "INTEGER",
                nullable: false,
                comment: "観測詳細データのサロゲートキー（自動インクリメントID）",
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldComment: "自動採番ID")
                .Annotation("Sqlite:Autoincrement", true)
                .OldAnnotation("Sqlite:Autoincrement", true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "updated_at",
                table: "ArenaSessions",
                type: "TEXT",
                nullable: false,
                comment: "レコード最終更新日時",
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldComment: "更新日時");

            migrationBuilder.AlterColumn<DateTime>(
                name: "session_date",
                table: "ArenaSessions",
                type: "TEXT",
                nullable: false,
                comment: "セッションが実際に実施・観測された日時",
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldComment: "実施日時");

            migrationBuilder.AlterColumn<int>(
                name: "season_id",
                table: "ArenaSessions",
                type: "INTEGER",
                nullable: false,
                comment: "このセッションが属するシーズンのID（Seasonsテーブルの外部キー）",
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldComment: "所属シーズンID");

            migrationBuilder.AlterColumn<string>(
                name: "memo",
                table: "ArenaSessions",
                type: "TEXT",
                nullable: false,
                comment: "セッションに関する自由記述のメモ",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldComment: "自由記述メモ");

            migrationBuilder.AlterColumn<bool>(
                name: "is_valid",
                table: "ArenaSessions",
                type: "INTEGER",
                nullable: false,
                comment: "セッションの対戦結果が有効かどうかのフラグ（無効試合の除外用）",
                oldClrType: typeof(bool),
                oldType: "INTEGER",
                oldComment: "結果有効フラグ");

            migrationBuilder.AlterColumn<bool>(
                name: "is_deleted",
                table: "ArenaSessions",
                type: "INTEGER",
                nullable: false,
                comment: "論理削除フラグ（trueで削除済み）",
                oldClrType: typeof(bool),
                oldType: "INTEGER",
                oldComment: "論理削除フラグ");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "ArenaSessions",
                type: "TEXT",
                nullable: false,
                comment: "レコード作成日時",
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldComment: "作成日時");

            migrationBuilder.AlterColumn<int>(
                name: "session_id",
                table: "ArenaSessions",
                type: "INTEGER",
                nullable: false,
                comment: "セッションのサロゲートキー（自動インクリメントID）",
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldComment: "自動採番ID")
                .Annotation("Sqlite:Autoincrement", true)
                .OldAnnotation("Sqlite:Autoincrement", true);

            migrationBuilder.AlterColumn<int>(
                name: "winning_team",
                table: "ArenaRounds",
                type: "INTEGER",
                nullable: false,
                comment: "ラウンドの勝利チーム（0: 引き分け/無効, 1: Blueチーム, 2: Orangeチーム）",
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldComment: "勝利チーム（0:無効 1:Blue 2:Orange）");

            migrationBuilder.AlterColumn<DateTime>(
                name: "updated_at",
                table: "ArenaRounds",
                type: "TEXT",
                nullable: false,
                comment: "レコード最終更新日時",
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldComment: "更新日時");

            migrationBuilder.AlterColumn<int>(
                name: "session_id",
                table: "ArenaRounds",
                type: "INTEGER",
                nullable: false,
                comment: "紐づくアリーナセッションのID（ArenaSessionsテーブルの外部キー）",
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldComment: "アリーナセッションID");

            migrationBuilder.AlterColumn<int>(
                name: "round_number",
                table: "ArenaRounds",
                type: "INTEGER",
                nullable: false,
                comment: "セッション内でのラウンド進行番号（通常 1 〜 14）",
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldComment: "ラウンド番号（1〜14）");

            migrationBuilder.AlterColumn<bool>(
                name: "is_deleted",
                table: "ArenaRounds",
                type: "INTEGER",
                nullable: false,
                comment: "論理削除フラグ（trueで削除済み）",
                oldClrType: typeof(bool),
                oldType: "INTEGER",
                oldComment: "論理削除フラグ");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "ArenaRounds",
                type: "TEXT",
                nullable: false,
                comment: "レコード作成日時",
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldComment: "作成日時");

            migrationBuilder.AlterColumn<int>(
                name: "round_id",
                table: "ArenaRounds",
                type: "INTEGER",
                nullable: false,
                comment: "ラウンドのサロゲートキー（自動インクリメントID）",
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldComment: "自動採番ID")
                .Annotation("Sqlite:Autoincrement", true)
                .OldAnnotation("Sqlite:Autoincrement", true);

            migrationBuilder.AlterColumn<int>(
                name: "win_count",
                table: "ArenaParticipants",
                type: "INTEGER",
                nullable: false,
                comment: "このセッションで獲得した最終的な勝利数",
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldComment: "獲得勝利数");

            migrationBuilder.AlterColumn<DateTime>(
                name: "updated_at",
                table: "ArenaParticipants",
                type: "TEXT",
                nullable: false,
                comment: "レコード最終更新日時",
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldComment: "更新日時");

            migrationBuilder.AlterColumn<int>(
                name: "slot_index",
                table: "ArenaParticipants",
                type: "INTEGER",
                nullable: false,
                comment: "セッション内での座席番号・配置インデックス（通常 0〜7）",
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldComment: "座席番号（0〜7）");

            migrationBuilder.AlterColumn<int>(
                name: "session_id",
                table: "ArenaParticipants",
                type: "INTEGER",
                nullable: false,
                comment: "紐づくアリーナセッションのID（ArenaSessionsテーブルの外部キー）",
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldComment: "アリーナセッションID");

            migrationBuilder.AlterColumn<int>(
                name: "rank",
                table: "ArenaParticipants",
                type: "INTEGER",
                nullable: false,
                comment: "このセッションでの最終順位",
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldComment: "最終順位");

            migrationBuilder.AlterColumn<string>(
                name: "player_id",
                table: "ArenaParticipants",
                type: "TEXT",
                nullable: false,
                comment: "参加したプレイヤーのID（Playersテーブルの外部キー）",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldComment: "参加プレイヤーID");

            migrationBuilder.AlterColumn<bool>(
                name: "is_deleted",
                table: "ArenaParticipants",
                type: "INTEGER",
                nullable: false,
                comment: "論理削除フラグ（trueで削除済み）",
                oldClrType: typeof(bool),
                oldType: "INTEGER",
                oldComment: "論理削除フラグ");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "ArenaParticipants",
                type: "TEXT",
                nullable: false,
                comment: "レコード作成日時",
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldComment: "作成日時");

            migrationBuilder.AlterColumn<int>(
                name: "participant_id",
                table: "ArenaParticipants",
                type: "INTEGER",
                nullable: false,
                comment: "参加情報のサロゲートキー（自動インクリメントID）",
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldComment: "自動採番ID")
                .Annotation("Sqlite:Autoincrement", true)
                .OldAnnotation("Sqlite:Autoincrement", true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "updated_at",
                table: "Aliases",
                type: "TEXT",
                nullable: false,
                comment: "レコード最終更新日時",
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldComment: "更新日時");

            migrationBuilder.AlterColumn<string>(
                name: "target_player_id",
                table: "Aliases",
                type: "TEXT",
                nullable: false,
                comment: "紐付け先となる正規のプレイヤーID（Playersテーブルの外部キー）",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldComment: "紐付け先プレイヤーID");

            migrationBuilder.AlterColumn<bool>(
                name: "is_deleted",
                table: "Aliases",
                type: "INTEGER",
                nullable: false,
                comment: "論理削除フラグ（trueで削除済み）",
                oldClrType: typeof(bool),
                oldType: "INTEGER",
                oldComment: "論理削除フラグ");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "Aliases",
                type: "TEXT",
                nullable: false,
                comment: "レコード作成日時",
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldComment: "作成日時");

            migrationBuilder.AlterColumn<string>(
                name: "alias_name",
                table: "Aliases",
                type: "TEXT",
                nullable: false,
                comment: "観測時に入力されたプレイヤーの別名",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldComment: "観測時に入力された別名");

            migrationBuilder.AlterColumn<int>(
                name: "id",
                table: "Aliases",
                type: "INTEGER",
                nullable: false,
                comment: "サロゲートキー（自動インクリメントID）",
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldComment: "自動採番ID")
                .Annotation("Sqlite:Autoincrement", true)
                .OldAnnotation("Sqlite:Autoincrement", true);
        }
    }
}
