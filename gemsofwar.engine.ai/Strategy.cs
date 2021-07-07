using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gemsofwar.engine.ai
{   public class Strategy
    {
        public Move Move { get; private set; }
        // after first move, this is what is matched
        public int LargestGemCount { get; private set; }
        public Dictionary<GemId,int> GemCounts { get; private set; }
        // the next move 'could' yield the following
        public Dictionary<GemId, int> CreatesGemCounts { get; private set; }
        public int CreatesLargestGemCount { get; private set; }

        public static List<Strategy> Analyze(GemsBoard board, int depth, GemId[] gemIdOrder)
        {
            // generate a tree of outcomes based on the current board and moves
            if (board == null || board.AvailableMoves.Count == 0) throw new Exception("Inavlid board");

            // apply moves 'depth' into the future to record potential moves
            var tree = GenerateTree(board, depth);

            // cacluate the potential strategies and return the one with the most value
            return ComputeStrategies(tree, gemIdOrder, board.Rules);
        }

        #region private
        class Tree<T, K>
        {
            public Tree()
            {
                Children = new List<Tree<T, K>>();
            }
            public T Context { get; set; }
            public K Outcome { get; set; }
            public List<Tree<T, K>> Children { get; private set; }
        }

        private static Tree<Move, List<Match>> GenerateTree(GemsBoard board, int depth)
        {
            var queue = new Queue<Tuple<Tree<Move, List<Match>>, GemsBoard>>();
            var root = new Tree<Move, List<Match>>();

            try
            {
                // pause new gem creation during prediction
                board.Factory.NewGemPause = true;

                // prepare for first iteration
                queue.Enqueue(new Tuple<Tree<Move, List<Match>>, GemsBoard>(root, board));

                // generate depth iterations
                while (depth-- > 0)
                {
                    var next = new Queue<Tuple<Tree<Move, List<Match>>, GemsBoard>>();
                    while (queue.Count > 0)
                    {
                        var tup = queue.Dequeue();
                        var croot = tup.Item1;
                        var cboard = tup.Item2;

                        // try all the moves
                        foreach (var cmov in cboard.AvailableMoves)
                        {
                            // make copy of the board
                            var boardcopy = new GemsBoard(cboard.Factory, cboard.Rules, cboard.Gems);
                            // check that it is valid
                            if (!string.Equals(cboard.ToString(), boardcopy.ToString())) throw new Exception("failed to get same board");
                            // make the move
                            if (!boardcopy.TryMakeMove(cmov, out List<Match> matches)) throw new Exception("failed to make move");

                            // add to the children and next queue
                            var nroot = new Tree<Move, List<Match>>() { Context = cmov, Outcome = matches };
                            next.Enqueue(new Tuple<Tree<Move, List<Match>>, GemsBoard>(nroot, boardcopy));

                            // add to the children of the croot
                            croot.Children.Add(nroot);
                        }
                    }

                    // setup for the next iteration
                    queue = next;
                }
            }
            finally
            {
                // allow for new gems again
                board.Factory.NewGemPause = false;
            }

            return root;
        }

        private static List<Strategy> ComputeStrategies(Tree<Move, List<Match>> tree, GemId[] gemIdOrder, Rules rules)
        {
            if (tree == null || tree.Children.Count == 0) throw new Exception("invalid input");

            var results = new List<Strategy>();

            // review the contents of the tree and add labels to the move, so that they can be ranked
            foreach(var child in tree.Children)
            {
                var strategy = new Strategy()
                { 
                    Move = child.Context, 
                    CreatesGemCounts = new Dictionary<GemId, int>(),
                    GemCounts = new Dictionary<GemId, int>()
                };

                // aggregate
                foreach(var match in child.Outcome)
                {
                    // largest match
                    strategy.LargestGemCount = Math.Max(strategy.LargestGemCount, match.Size);

                    // sum gem counts
                    if (!strategy.GemCounts.ContainsKey(match.Gem.Id)) strategy.GemCounts.Add(match.Gem.Id, 0);
                    strategy.GemCounts[match.Gem.Id] += match.Size;

                    // add labels - by searching all its children for cateristics (this is speculative)
                    foreach(var grandchild in child.Children)
                    {
                        foreach(var grandmatch in grandchild.Outcome)
                        {
                            // largest match
                            strategy.CreatesLargestGemCount = Math.Max(strategy.CreatesLargestGemCount, grandmatch.Size);

                            // sum gem counts
                            if (!strategy.CreatesGemCounts.ContainsKey(match.Gem.Id)) strategy.CreatesGemCounts.Add(match.Gem.Id, 0);
                            strategy.CreatesGemCounts[match.Gem.Id] += match.Size;
                        }
                    }
                }

                results.Add(strategy);
            }

            // sort before returning
            results.Sort((x, y) => StrategyCompare.Compare(rules, gemIdOrder, x, y));

            return results;
        }
        #endregion
    }
}
