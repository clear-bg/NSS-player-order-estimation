using System.Collections.Generic;
using FluentAssertions;
using NssOrderTool.Services.Domain;
using Xunit;

namespace NssOrderTool.Tests.Services.Domain
{
  public class RelationshipExtractorTests
  {
    private readonly RelationshipExtractor _extractor;

    public RelationshipExtractorTests()
    {
      // テスト対象のクラスを初期化
      _extractor = new RelationshipExtractor();
    }

    // --- NormalizeInput (正規化) のテスト ---

    [Fact] // [Fact] は「これはテストメソッドです」というマーク
    public void NormalizeInput_ShouldReplaceAliases_WhenDictionaryHasMatches()
    {
      // Arrange (準備)
      var input = " Taka ,  Kazu "; // 空白混じりの入力
      var aliases = new Dictionary<string, string>
            {
                { "Taka", "Takahiro" }, // エイリアス辞書
                { "Kazu", "Kazuyoshi" }
            };

      // Act (実行)
      var result = _extractor.NormalizeInput(input, aliases);

      // Assert (検証)
      // "Takahiro, Kazuyoshi" に変換されているべき
      result.Should().Be("Takahiro, Kazuyoshi");
    }

    [Fact]
    public void NormalizeInput_ShouldKeepOriginal_WhenNoMatch()
    {
      // Arrange
      var input = "A, B";
      var aliases = new Dictionary<string, string>(); // 空の辞書

      // Act
      var result = _extractor.NormalizeInput(input, aliases);

      // Assert
      result.Should().Be("A, B");
    }

    [Fact]
    public void NormalizeInput_ShouldReturnEmptyString_WhenInputIsEmpty()
    {
      var result = _extractor.NormalizeInput("", new Dictionary<string, string>());
      result.Should().BeEmpty();
    }

    // --- ExtractFromInput (ペア分解) のテスト ---

    [Fact]
    public void ExtractFromInput_ShouldGenerateAllPairs_ForThreePlayers()
    {
      // Arrange
      var input = "A, B, C";

      // Act
      var pairs = _extractor.ExtractFromInput(input);

      // Assert
      // 3人なら (A,B), (A,C), (B,C) の3ペアになるはず
      pairs.Should().HaveCount(3);

      // 具体的な中身のチェック
      pairs.Should().Contain(p => p.Predecessor == "A" && p.Successor == "B");
      pairs.Should().Contain(p => p.Predecessor == "A" && p.Successor == "C");
      pairs.Should().Contain(p => p.Predecessor == "B" && p.Successor == "C");
    }

    [Fact]
    public void ExtractFromInput_ShouldReturnEmpty_ForSinglePlayer()
    {
      // Arrange
      var input = "LonelyPlayer";

      // Act
      var pairs = _extractor.ExtractFromInput(input);

      // Assert
      pairs.Should().BeEmpty(); // 1人だけならペアは生まれない
    }

    [Fact]
    public void ExtractFromInput_ShouldIgnoreEmptyEntries()
    {
      // Arrange
      // "A, , B" のように、間に空要素があるケース
      var input = "A, , B";

      // Act
      var pairs = _extractor.ExtractFromInput(input);

      // Assert
      // 空要素は無視され、AとBのペアのみが生成されるべき
      pairs.Should().HaveCount(1);
      pairs[0].Predecessor.Should().Be("A");
      pairs[0].Successor.Should().Be("B");
    }

    [Fact]
    public void ExtractFromInput_ShouldReturnEmpty_WhenInputIsOnlyCommas()
    {
      // Arrange
      // カンマだけの入力
      var input = ", , ";

      // Act
      var pairs = _extractor.ExtractFromInput(input);

      // Assert
      // ペアは生成されないべき
      pairs.Should().BeEmpty();
    }

    [Fact]
    public void ExtractFromInput_ShouldHandleDuplicateNames_AsIs()
    {
      // Arrange
      // "A, A" のような重複入力
      // ※現状の仕様では、そのまま (A, A) というペアが生成される挙動を確認する
      // (将来的に重複排除を入れる場合の基準となるテスト)
      var input = "A, A";

      // Act
      var pairs = _extractor.ExtractFromInput(input);

      // Assert
      pairs.Should().HaveCount(1);
      pairs[0].Predecessor.Should().Be("A");
      pairs[0].Successor.Should().Be("A");
    }
  }
}