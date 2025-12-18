using System.Collections.Generic;
using FluentAssertions;
using NssOrderTool.Models;
using NssOrderTool.Services.Domain;
using Xunit;

namespace NssOrderTool.Tests.Services.Domain
{
    public class GraphVizServiceTests
    {
        private readonly GraphVizService _service;

        public GraphVizServiceTests()
        {
            _service = new GraphVizService();
        }

        [Fact]
        public void GenerateMermaid_ShouldIncludeBasicSyntax_AndPairs()
        {
            // Arrange
            var pairs = new List<OrderPair>
            {
                new OrderPair("A", "B"),
                new OrderPair("B", "C")
            };
            var layers = new List<List<string>>(); // 階層情報は一旦空でテスト

            // Act
            var result = _service.GenerateMermaid(pairs, layers);

            // Assert
            result.Should().Contain("graph TD;"); // ヘッダー
            result.Should().Contain("A --> B;");  // ペア1
            result.Should().Contain("B --> C;");  // ペア2
        }

        [Fact]
        public void GenerateMermaid_ShouldGenerateSubgraphs_ForRanks()
        {
            // Arrange
            var pairs = new List<OrderPair>();
            var layers = new List<List<string>>
            {
                new List<string> { "A" },      // Rank 1
                new List<string> { "B", "C" }  // Rank 2 (同率)
            };

            // Act
            var result = _service.GenerateMermaid(pairs, layers);

            // Assert
            result.Should().Contain("subgraph Rank1 [Rank 1]");
            result.Should().Contain("subgraph Rank2 [Rank 2]");
            result.Should().Contain("A;");
            result.Should().Contain("B;");
            result.Should().Contain("C;");
        }
    }
}
