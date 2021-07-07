using gemsofwar.engine;
using gemsofwar.engine.ai;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace gemsofwar.console
{
    class Program
    {
        static int Main(string[] args)
        {
            var options = Options.Parse(args);

            if (options.ShowHelp)
            {
                Options.DisplayHelp();
                return -1;
            }

            if (options.Action == Verb.Train)
            {
                ModelTraining.Train(inputdirectory: options.Path, outputfilename: options.Output);
            }
            else if (options.Action == Verb.Evaluate)
            {
                ModelFitness.Evaluate(filename: options.Input);
            }
            else if (options.Action == Verb.Evolve)
            {
                ModelFitness.Evolve(filename: options.Input, options.Generations, options.Population);
            }
            else if (options.Action == Verb.Check)
            {
                ModelTraining.Check(inputdirectory: options.Path, filename: options.Input);
            }
            else if (options.Action == Verb.Create)
            {
                CreateBoards.CreateAllBoards(options.Dimension, options.GemCount);
            }
            else if (options.Action == Verb.Dump)
            {
                CreateBoards.Dump(options.Input, options.Skip, options.Take);
            }
            else if (options.Action == Verb.Play)
            {
                GamePlay.Play(options.GameMode);
            }
            else
            {
                Console.WriteLine($"ERR: unknown action : {options.Action}");
            }

            return 0;
        }
    }
}
