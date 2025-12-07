using System.Collections.Generic;
using FluentAssertions;
using NssOrderTool.Models;
using NssOrderTool.Services.Domain;
using Xunit;

namespace NssOrderTool.Tests.Services.Domain
{
    public class OrderSorterTests
    {
        private readonly OrderSorter _sorter;

        public OrderSorterTests()
        {
            _sorter = new OrderSorter();
        }

        [Fact]
        public void Sort_ShouldReturnCorrectOrder_ForLinearGraph()
        {
            // Arrange
            // A -> B -> C の順序関係
            var pairs = new List<OrderPair>
            {
                new OrderPair("A", "B"),
                new OrderPair("B", "C")
            };

            // Act
            var result = _sorter.Sort(pairs);

            // Assert
            // 期待値: 
            // 1位: A
            // 2位: B
            // 3位: C
            result.Should().HaveCount(3);
            result[0].Should().ContainSingle("A");
            result[1].Should().ContainSingle("B");
            result[2].Should().ContainSingle("C");
        }

        [Fact]
        public void Sort_ShouldGroupNodes_ForBranchingGraph()
        {
            // Arrange
            // A -> B
            // A -> C
            // BとCには優劣がない -> 同率2位になるはず
            var pairs = new List<OrderPair>
            {
                new OrderPair("A", "B"),
                new OrderPair("A", "C")
            };

            // Act
            var result = _sorter.Sort(pairs);

            // Assert
            // 期待値:
            // 1位: A
            // 2位: B, C (同率)
            result.Should().HaveCount(2);
            result[0].Should().ContainSingle("A");
            result[1].Should().HaveCount(2);
            result[1].Should().Contain(new[] { "B", "C" });
        }

        [Fact]
        public void Sort_ShouldReturnEmpty_WhenCycleDetected()
        {
            // Arrange
            // A -> B -> A (矛盾/サイクル)
            var pairs = new List<OrderPair>
            {
                new OrderPair("A", "B"),
                new OrderPair("B", "A")
            };

            // Act
            var result = _sorter.Sort(pairs);

            // Assert
            // サイクルがある場合は空リストが返る仕様
            result.Should().BeEmpty();
        }

        [Fact]
        public void Sort_ShouldHandleDisconnectedGraphs()
        {
            // Arrange
            // A -> B
            // C -> D
            // (A,B)グループと(C,D)グループは無関係だが、どちらも「入次数0」から始まる
            var pairs = new List<OrderPair>
            {
                new OrderPair("A", "B"),
                new OrderPair("C", "D")
            };

            // Act
            var result = _sorter.Sort(pairs);

            // Assert
            // 期待値:
            // 1位グループ: A, C (どちらも誰にも負けてない)
            // 2位グループ: B, D (それぞれA, Cに負けている)
            result.Should().HaveCount(2);
            result[0].Should().Contain(new[] { "A", "C" });
            result[1].Should().Contain(new[] { "B", "D" });
        }
    }
}