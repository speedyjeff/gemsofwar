using gemsofwar.engine;
using gemsofwar.engine.ai;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace gemsofwar.console
{
    static class Domain
    {
        public static long Generate(int dim, Gem[] gems)
        {
            StreamWriter nstream;
            var srow = 0;
            var scol = 0;

            if (File.Exists(NextList)) File.Delete(NextList);

            // queues to hold current cells
            if (File.Exists(CurrentList))
            {
                // read first row to set starting row and starting col
                var reader = File.OpenText("current");
                var line = reader.ReadLine();
                Console.WriteLine($"found {line}");
                reader.Close();
                var compact = new Compact(line);
                // find the row and col that is the first that is empty
                var found = false;
                for(;srow<dim && !found; srow++)
                {
                    for(scol=0;scol<dim && !found; scol++)
                    {
                        Console.WriteLine($"{srow},{scol} {compact[srow, scol]}");
                        if (compact[srow,scol] == GemId.Empty)
                        {
                            found = true;
                            break;
                        }
                    }
                }

                if (srow == dim && scol == dim)
                {
                    Console.WriteLine("no work to be done");
                }
            }
            else
            {
                File.WriteAllText(CurrentList, new Compact(dim).ToString());
            }

            // create all boards
            var count = 0L;
            for (int row = srow; row < dim; row++)
            {
                for (int col = scol; col < dim; col++)
                {
                    Console.Write($"[{(row * dim) + col} of {dim * dim} count = {count}] ");
                    nstream = File.CreateText(NextList);
                    count = 0;

                    // apply to all active boards
                    var reader = File.OpenText("current");
                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                        var current = new Compact(line);
                        if (current[row, col] == GemId.Empty)
                        {
                            // consider all gems
                            foreach (var gem in gems)
                            {
                                // check if would be valid if this piece is added
                                var match =
                                    (GetValue(current, row - 1, col, dim) == gem.Id && GetValue(current, row - 2, col, dim) == gem.Id)
                                    ||
                                    (GetValue(current, row, col - 1, dim) == gem.Id && GetValue(current, row, col - 2, dim) == gem.Id);

                                if (!match)
                                {
                                    // copy the current
                                    var copy = new Compact(current);
                                    // set this cell with this gem
                                    copy[row, col] = gem.Id;
                                    // add for next round
                                    nstream.WriteLine(copy.ToString());
                                    count++;
                                }
                            }
                        }
                        else
                        {
                            // add to next round
                            nstream.WriteLine(line);
                            count++;
                        }

                        // boards that do not have any valid next piece selections are trimmed

                    } // while

                    // close the stream
                    reader.Close();

                    // set for the next round
                    nstream.Flush();
                    nstream.Close();
                    File.Delete(CurrentList);
                    File.Move(NextList, CurrentList);
                }
            }

            // move the current to a permanent location
            File.Move(CurrentList, $"{dim}.{DateTime.Now:yyyy-MM-dd_hh-mm-ss}");

            // return the count
            return count;
        }

        #region private
        private const string CurrentList = "current";
        private const string NextList = "next";

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static GemId GetValue(Compact compact, int row, int col, int dim)
        {
            if (row < 0 || col < 0 || row >= dim || col >= dim) return (byte)GemId.Empty;
            return compact[row,col];
        }
        #endregion
    }
}
