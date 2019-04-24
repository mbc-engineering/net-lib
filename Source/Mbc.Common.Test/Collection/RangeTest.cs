using FluentAssertions;
using Mbc.Common.Collection;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Mbc.Common.Test.Collection
{
    public class RangeTest
    {
        [Fact]
        public void AllTest()
        {
            // Arrange
            var range = Range.All<int>();

            // Assert
            range.Contains(42).Should().BeTrue();
            range.ContainsAll(Enumerable.Range(1, 10)).Should().BeTrue();
            range.Encloses(Range.All<int>()).Should().BeTrue();
            range.IsConnect(Range.All<int>()).Should().BeTrue();
            range.Should().Be(Range.All<int>());
            range.HasLowerBound.Should().BeFalse();
            range.HasUpperBound.Should().BeFalse();
            range.IsEmtpy.Should().BeFalse();
            range.LowerBoundTyp.Should().Be(BoundType.Closed);
            range.UpperBoundTyp.Should().Be(BoundType.Closed);
            range.Invoking(x => { var _ = x.LowerEndpoint; }).Should().Throw<InvalidOperationException>();
            range.Invoking(x => { var _ = x.UpperEndpoint; }).Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void CloseRangeTest()
        {
            // Arrange
            var range = Range.Closed(-10, 10);

            // Assert
            range.Contains(0).Should().BeTrue();
            range.Contains(42).Should().BeFalse();
            range.Contains(-10).Should().BeTrue();
            range.Contains(10).Should().BeTrue();
            range.ContainsAll(Enumerable.Range(1, 10)).Should().BeTrue();
            range.Encloses(Range.Closed(1, 3)).Should().BeTrue();
            range.Encloses(Range.Closed(5, 15)).Should().BeFalse();
            range.IsConnect(Range.Closed(5, 15)).Should().BeTrue();
            range.IsConnect(Range.Closed(11, 15)).Should().BeFalse();
            range.Should().Be(Range.Closed(-10, 10));
            range.HasLowerBound.Should().BeTrue();
            range.HasUpperBound.Should().BeTrue();
            range.IsEmtpy.Should().BeFalse();
            range.LowerBoundTyp.Should().Be(BoundType.Closed);
            range.UpperBoundTyp.Should().Be(BoundType.Closed);
            range.LowerEndpoint.Should().Be(-10);
            range.UpperEndpoint.Should().Be(10);
        }

        [Fact]
        public void OpenRangeTest()
        {
            // Arrange
            var range = Range.Open(-10, 10);

            // Assert
            range.Contains(0).Should().BeTrue();
            range.Contains(42).Should().BeFalse();
            range.Contains(-10).Should().BeFalse();
            range.Contains(10).Should().BeFalse();
            range.ContainsAll(Enumerable.Range(1, 9)).Should().BeTrue();
            range.Encloses(Range.Closed(1, 3)).Should().BeTrue();
            range.Encloses(Range.Closed(5, 15)).Should().BeFalse();
            range.IsConnect(Range.Closed(5, 15)).Should().BeTrue();
            range.IsConnect(Range.Closed(11, 15)).Should().BeFalse();
            range.Should().Be(Range.Open(-10, 10));
            range.HasLowerBound.Should().BeTrue();
            range.HasUpperBound.Should().BeTrue();
            range.IsEmtpy.Should().BeFalse();
            range.LowerBoundTyp.Should().Be(BoundType.Open);
            range.UpperBoundTyp.Should().Be(BoundType.Open);
            range.LowerEndpoint.Should().Be(-10);
            range.UpperEndpoint.Should().Be(10);
        }

        [Fact]
        public void EmptyRangeTest()
        {
            // Arrange
            var range = Range.OpenClosed(0, 0);

            // Assert
            range.Contains(0).Should().BeFalse();
            range.Should().Be(Range.OpenClosed(0, 0));
            range.HasLowerBound.Should().BeTrue();
            range.HasUpperBound.Should().BeTrue();
            range.IsEmtpy.Should().BeTrue();
            range.LowerBoundTyp.Should().Be(BoundType.Open);
            range.UpperBoundTyp.Should().Be(BoundType.Closed);
            range.LowerEndpoint.Should().Be(0);
            range.UpperEndpoint.Should().Be(0);
        }

        [Fact]
        public void AtLeastRangeTest()
        {
            // Arrange
            var range = Range.AtLeast(10);

            // Assert
            range.Contains(10).Should().BeTrue();
            range.Contains(9).Should().BeFalse();
            range.ContainsAll(Enumerable.Range(10, 15)).Should().BeTrue();
            range.HasLowerBound.Should().BeTrue();
            range.HasUpperBound.Should().BeFalse();
            range.IsEmtpy.Should().BeFalse();
            range.LowerBoundTyp.Should().Be(BoundType.Closed);
            range.UpperBoundTyp.Should().Be(BoundType.Closed);
            range.LowerEndpoint.Should().Be(10);
        }

        [Fact]
        public void GreaterThanRangeTest()
        {
            // Arrange
            var range = Range.GreaterThan(10);

            // Assert
            range.Contains(11).Should().BeTrue();
            range.Contains(10).Should().BeFalse();
            range.HasLowerBound.Should().BeTrue();
            range.HasUpperBound.Should().BeFalse();
            range.IsEmtpy.Should().BeFalse();
            range.LowerBoundTyp.Should().Be(BoundType.Open);
            range.UpperBoundTyp.Should().Be(BoundType.Closed);
            range.LowerEndpoint.Should().Be(10);
        }

        [Fact]
        public void AtMostRangeTest()
        {
            // Arrange
            var range = Range.AtMost(10);

            // Assert
            range.Contains(10).Should().BeTrue();
            range.Contains(11).Should().BeFalse();
            range.ContainsAll(Enumerable.Range(0, 10)).Should().BeTrue();
            range.HasLowerBound.Should().BeFalse();
            range.HasUpperBound.Should().BeTrue();
            range.IsEmtpy.Should().BeFalse();
            range.LowerBoundTyp.Should().Be(BoundType.Closed);
            range.UpperBoundTyp.Should().Be(BoundType.Closed);
            range.UpperEndpoint.Should().Be(10);
        }

        [Fact]
        public void LessThanRangeTest()
        {
            // Arrange
            var range = Range.LessThan(10);

            // Assert
            range.Contains(9).Should().BeTrue();
            range.Contains(10).Should().BeFalse();
            range.HasLowerBound.Should().BeFalse();
            range.HasUpperBound.Should().BeTrue();
            range.IsEmtpy.Should().BeFalse();
            range.LowerBoundTyp.Should().Be(BoundType.Closed);
            range.UpperBoundTyp.Should().Be(BoundType.Open);
            range.UpperEndpoint.Should().Be(10);
        }

        [Fact]
        public void ToStringTest()
        {
            Range.All<int>().ToString().Should().Be("(-∞..∞)");
            Range.Closed(-10, 10).ToString().Should().Be("[-10..10]");
            Range.Open(-10, 10).ToString().Should().Be("(-10..10)");
            Range.OpenClosed(-10, 10).ToString().Should().Be("(-10..10]");
            Range.ClosedOpen(-10, 10).ToString().Should().Be("[-10..10)");
            Range.AtLeast(0).ToString().Should().Be("[0..∞)");
            Range.AtMost(0).ToString().Should().Be("(-∞..0]");
        }

        [Fact]
        public void EncloseAllRangeTest()
        {
            // Arrange
            var range = Range.EncloseAll(new List<int> { 3, 7, -5 });

            // Assert
            range.Should().Be(Range.Closed(-5, 7));
        }

        [Fact]
        public void EnclosesTest()
        {
            // Assert
            Range.Closed(3, 6).Encloses(Range.Closed(4, 5)).Should().BeTrue();
            Range.Open(3, 6).Encloses(Range.Open(4, 5)).Should().BeTrue();
            Range.Closed(3, 6).Encloses(Range.ClosedOpen(4, 4)).Should().BeTrue();
            Range.OpenClosed(3, 6).Encloses(Range.Closed(3, 6)).Should().BeFalse();
            Range.Closed(4, 5).Encloses(Range.Open(3, 6)).Should().BeFalse();
            Range.Closed(3, 6).Encloses(Range.OpenClosed(1, 1)).Should().BeFalse();
        }

        [Fact]
        public void IntersectionRange()
        {
            Range.Closed(1, 5).Intersection(Range.Open(3, 7)).Should().Be(Range.OpenClosed(3, 5));
            Range.Closed(1, 5).Invoking(x => x.Intersection(Range.Closed(6, 7))).Should().Throw<ArgumentException>();
        }

        [Fact]
        public void IsConnected()
        {
            Range.ClosedOpen(2, 4).IsConnect(Range.ClosedOpen(5, 7)).Should().BeFalse();
            Range.ClosedOpen(2, 4).IsConnect(Range.ClosedOpen(3, 5)).Should().BeTrue();
            Range.ClosedOpen(2, 4).IsConnect(Range.ClosedOpen(4, 6)).Should().BeTrue();
        }

        [Fact]
        public void Singleton()
        {
            Range.Singleton(42).Should().Be(Range.Closed(42, 42));
        }

        [Fact]
        public void Span()
        {
            Range.Closed(1, 3).Span(Range.Open(5, 7)).Should().Be(Range.ClosedOpen(1, 7));
        }

        [Fact]
        public void EqualityOperatorTest()
        {
            (Range.Closed(1, 3) == Range.Closed(1, 3)).Should().BeTrue();
            (Range.Closed(1, 3) == Range.Closed(1, 2)).Should().BeFalse();
            (Range.Closed(1, 3) != Range.Closed(1, 3)).Should().BeFalse();
            (Range.Closed(1, 3) != Range.Closed(1, 2)).Should().BeTrue();
        }

        [Fact]
        public void LogicalAndOperatorTest()
        {
            (Range.Closed(1, 3) & Range.Closed(2, 5)).Should().Be(Range.Closed(2, 3));
        }


        [Fact]
        public void LogicalOrOperatorTest()
        {
            (Range.Closed(1, 3) | Range.Closed(2, 5)).Should().Be(Range.Closed(1, 5));
        }
    }
}
