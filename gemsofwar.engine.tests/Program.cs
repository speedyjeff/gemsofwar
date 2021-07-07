using System;
using System.Collections.Generic;

using gemsofwar.engine;
using gemsofwar.engine.ai;

namespace gemsofwar.engine.tests
{
    class Program
    {
        static void Main(string[] args)
        {
            engine.MatchTest.Run();
            engine.MoveTest.Run();
            engine.GameBoardTest.Run(GameMode.Treasure);
            engine.GameBoardTest.Run(GameMode.Battle);
            engine.ai.StrategyTest.Run(GameMode.Battle);
            engine.ai.CompactTest.Run(GameMode.Battle);
            engine.ai.CompactTest.Run(GameMode.Treasure);
            engine.ai.ImageClassifierTest.Run();
            engine.ai.GeneticAlgorithmTest.Run();
        }
     }
}
