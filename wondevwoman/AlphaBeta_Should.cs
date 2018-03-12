using FluentAssertions;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace CG.WondevWoman
{
    [TestFixture]
    public class AlphaBeta_Should
    {
        [SetUp]
        public void SetUp()
        {
            evaluations = new HashSet<TreeNode>();
            alphaBeta = new AlphaBeta<TreeNode>(
                n => n.Children,
                n =>
                {
                    evaluations.Add(n);
                    return n.Score;
                },
                n => null);
        }

        private HashSet<TreeNode> evaluations;
        private AlphaBeta<TreeNode> alphaBeta;


        [TestCase(0, -6.0, 1)]
        [TestCase(1, -3.0, 3)]
        [TestCase(2, -5.0, 4)]
        [TestCase(3, -4.0, 6)]
        [TestCase(100500, 6.0, 9)]
        public void BigTest_WithLimitedDepth(int depth, double ans, int evaluationsCount)
        {
            var root = GetTreeFromWikipediaSample();
            alphaBeta.Search(root, depth).Score
                .Should().Be(ans);
            evaluations.Should().HaveCount(evaluationsCount);
        }

        public static TreeNode T(int score, params TreeNode[] children)
        {
            return new TreeNode(score, children);
        }

        public class TreeNode
        {
            public readonly TreeNode[] Children;

            public readonly double Score;

            public TreeNode(double score, params TreeNode[] children)
            {
                Score = score;
                Children = children;
            }
        }

        private static TreeNode GetTreeFromWikipediaSample()
        {
            // https://upload.wikimedia.org/wikipedia/commons/thumb/9/91/AB_pruning.svg/400px-AB_pruning.svg.png
            var root =
                T(
                    -6,
                    T(
                        -3,
                        T(
                            -5,
                            T(-5, T(5), T(6)),
                            T(-4, T(7), T(4), T(5))),
                        T(-3, T(-3, T(3)))),
                    T(
                        -6,
                        T(
                            -6,
                            T(-6, T(6)),
                            T(-6, T(6), T(9))),
                        T(-7, T(-7, T(7)))),
                    T(
                        -5,
                        T(-5, T(-5, T(5))),
                        T(
                            -8,
                            T(-8, T(9), T(8)),
                            T(-6, T(6)))));
            return root;
        }

        [Test]
        public void BigTest()
        {
            var root = GetTreeFromWikipediaSample();
            var move = alphaBeta.Search(root, 4);
            move.Score.Should().Be(6.0);
            evaluations.Should().HaveCount(9);
        }

        [Test]
        public void NoChild()
        {
            var root = new TreeNode(42);
            alphaBeta.Search(root, 1).Score
                .Should().Be(42.0);
            evaluations.Should().BeEquivalentTo(new[] { root });
        }

        [Test]
        public void ReturnMaxChildScore_ForFlatTree()
        {
            var root = T(42, T(1), T(2), T(0));
            alphaBeta.Search(root, 1).Score
                .Should().Be(2.0);
            evaluations.Should().BeEquivalentTo(root.Children);
        }

        [Test]
        public void ReturnMaxMinGrandChildScore_For2LevelTree()
        {
            var root = T(42, T(20, T(30), T(10)));
            alphaBeta.Search(root, 2).Score
                .Should().Be(10.0);
            evaluations.Should().BeEquivalentTo(root.Children[0].Children);
        }
    }
}
