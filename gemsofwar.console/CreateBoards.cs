using gemsofwar.engine;
using gemsofwar.engine.ai;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gemsofwar.console
{
    class CreateBoards
    {
        public static void CreateAllBoards(int dim, int gemcount)
        {
            var timer = new Stopwatch();
            var gems = new List<Gem>();

            if (dim <= 1 || gemcount < (int)GemId.G1 || gemcount > (int)GemId.G9) throw new Exception($"must provide valid input dim>1 and {(int)GemId.G1}<=gemcount<={(int)GemId.G9}");
            if (dim > 4 || gemcount > 2) Console.WriteLine($"Warning: Performance will be serverly impacted by considering a {dim}x{dim} board with {gemcount} gems");

            for (int id = (int)GemId.G1; id <= gemcount; id++)
            {
                gems.Add(new Gem() { Id = (GemId)id });
            }

            timer.Start();
            var count = Domain.Generate(dim, gems.ToArray());
            timer.Stop();
            Console.WriteLine($"generated {count} boards in {timer.ElapsedMilliseconds} ms");
        }

        public static void Dump(string path, int skip, int take)
        {
            using (var reader = File.OpenText(path))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    // skip until ready to read
                    if (skip-- < 0)
                    {
                        var current = new Compact(line);
                        var board = current.ToBoard();
                        Console.WriteLine($"-------------------------");
                        Console.WriteLine(board.ToString());

                        // only take so many
                        if (--take <= 0) break;
                    }
                }
            }
        }
    }
}
