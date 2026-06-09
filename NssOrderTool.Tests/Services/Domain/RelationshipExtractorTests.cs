using System.Collections.Generic;
using NssOrderTool.Models;
using NssOrderTool.Services.Domain;
using Xunit;

namespace NssOrderTool.Tests.Services.Domain
{
  public class RelationshipExtractorTests
  {
    private readonly RelationshipExtractor _extractor;

    public RelationshipExtractorTests()
    {
      _extractor = new RelationshipExtractor();
    }

    [Fact]
    public void ExtractPairs_ShouldReturnAdjacentPairs_WhenListIsProvided()
    {
      // Arrange
      var input = new List<string> { "A", "B", "C", "D" };

      // Act
      var result = _extractor.ExtractPairs(input);

      // Assert
      Assert.Equal(3, result.Count);
      Assert.Contains(new OrderPair("A", "B"), result);
      Assert.Contains(new OrderPair("B", "C"), result);
      Assert.Contains(new OrderPair("C", "D"), result);
    }

    [Fact]
    public void ExtractPairs_ShouldReturnEmptyList_WhenListHasLessThanTwoItems()
    {
      // Arrange
      var input = new List<string> { "A" };

      // Act
      var result = _extractor.ExtractPairs(input);

      // Assert
      Assert.Empty(result);
    }

    [Fact]
    public void ExtractPairs_ShouldReturnEmptyList_WhenListIsNull()
    {
      // Arrange
      List<string>? input = null;

      // Act
      var result = _extractor.ExtractPairs(input!);

      // Assert
      Assert.Empty(result);
    }
  }
}
