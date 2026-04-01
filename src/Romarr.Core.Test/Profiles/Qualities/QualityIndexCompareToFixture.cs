using FluentAssertions;
using NUnit.Framework;
using Romarr.Core.Profiles.Qualities;
using Romarr.Core.Test.Framework;

namespace Romarr.Core.Test.Qualities
{
    [TestFixture]
    public class QualityIndexCompareToFixture : CoreTest
    {
        [TestCase(1, 0, 1, 0, 0)]
        [TestCase(1, 1, 1, 0, 1)]
        [TestCase(2, 0, 1, 0, 1)]
        [TestCase(1, 0, 1, 1, -1)]
        [TestCase(1, 0, 2, 0, -1)]
        public void should_match_expected_when_respect_group_order_is_true(int leftIndex, int leftGroupIndex, int rightIndex, int rightGroupIndex, int expected)
        {
            var left = new QualityIndex(leftIndex, leftGroupIndex);
            var right = new QualityIndex(rightIndex, rightGroupIndex);
            left.CompareTo(right, true).Should().Be(expected);
        }

        [TestCase(1, 0, 1, 0, 0)]
        [TestCase(1, 1, 1, 0, 0)]
        [TestCase(2, 0, 1, 0, 1)]
        [TestCase(1, 0, 1, 1, 0)]
        [TestCase(1, 0, 2, 0, -1)]
        public void should_match_expected_when_respect_group_order_is_false(int leftIndex, int leftGroupIndex, int rightIndex, int rightGroupIndex, int expected)
        {
            var left = new QualityIndex(leftIndex, leftGroupIndex);
            var right = new QualityIndex(rightIndex, rightGroupIndex);
            left.CompareTo(right, false).Should().Be(expected);
        }
    }
}
