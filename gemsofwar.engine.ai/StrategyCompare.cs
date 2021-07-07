using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gemsofwar.engine.ai
{
    class StrategyCompare
    {
        // Strategy
        //                                           RemoveGemOnMatch  UpgradeGemOnMatch
        //  1) Match a 4+                            1                 1
        //    a) Creates a 4+                        1.a               1.a
        //    b) Match preference                    1.b               1.b
        //    c) Creates a match peference           1.c               1.c
        //    d) Most matched                        1.d               1.d
        //    e) Creates the most to match           1.e               1.e
        //    f) equal                               1.f               1.f
        //  2) Match a 3                             2                 2
        //    a) Does not create a 4+                2.a               opposite (does create a 4+)
        //    b) Most matched                        2.b               2.b
        //    c) Match preference                    2.c               2.c
        //    d) Creates the least count             2.d               opposite (creates the most count)
        //    e) Does not create a match preference  2.e               opposite (creates a preferred match)
        //    f) equal                               2.f               2.f

        public static int Compare(Rules rules, GemId[] gemIdOrder, Strategy x, Strategy y)
        {
            // -1 x sorts before y
            // 0 they are equal
            // 1 x sorts after y

            if (rules != Rules.UpgradeGemOnMatch && rules != Rules.RemoveGemOnMatch) throw new Exception($"unknown rule set : {rules}");

            // 1) Match a 4+
            if (x.LargestGemCount >= 4 && y.LargestGemCount < 4) return -1;
            if (x.LargestGemCount < 4 && y.LargestGemCount >= 4) return 1;

            if (x.LargestGemCount >= 4 && y.LargestGemCount >= 4)
            {
                // both of these may be good, see what they create

                // 1.a) Creates a 4+
                if (x.CreatesLargestGemCount >= 4 && y.CreatesLargestGemCount < 4) return -1;
                if (x.CreatesLargestGemCount < 4 && y.CreatesLargestGemCount >= 4) return 1;

                // 1.b) Match preference
                foreach (var id in gemIdOrder)
                {
                    var xmatch = x.GemCounts.ContainsKey(id);
                    var ymatch = y.GemCounts.ContainsKey(id);

                    // it both match then use the next criteria
                    if (xmatch && ymatch) break;
                    else if (xmatch) return -1;
                    else if (ymatch) return 1;
                }

                // 1.c) Creates a match peference
                foreach (var id in gemIdOrder)
                {
                    var xmatch = x.CreatesGemCounts.ContainsKey(id);
                    var ymatch = y.CreatesGemCounts.ContainsKey(id);

                    // it both match then use the next criteria
                    if (xmatch && ymatch) break;
                    else if (xmatch) return -1;
                    else if (ymatch) return 1;
                }

                // 2.d) Most matched
                var xsum = x.GemCounts.Sum(g => g.Value);
                var ysum = y.GemCounts.Sum(g => g.Value);
                if (xsum > ysum) return -1;
                else if (xsum < ysum) return 1;

                // 2.e) Creates the most to match
                xsum = x.CreatesGemCounts.Sum(g => g.Value);
                ysum = y.CreatesGemCounts.Sum(g => g.Value);
                if (xsum > ysum) return -1;
                else if (xsum < ysum) return 1;
            }
            else
            {
                // 2) Match a 3

                // 2.a) 
                if (rules == Rules.RemoveGemOnMatch)
                {
                    // Does not create a 4+
                    if (x.CreatesLargestGemCount >= 4 && y.CreatesLargestGemCount < 4) return 1;
                    if (x.CreatesLargestGemCount < 4 && y.CreatesLargestGemCount >= 4) return -1;
                }
                else if (rules == Rules.UpgradeGemOnMatch)
                {
                    // Does create a 4+
                    if (x.CreatesLargestGemCount >= 4 && y.CreatesLargestGemCount < 4) return -1;
                    if (x.CreatesLargestGemCount < 4 && y.CreatesLargestGemCount >= 4) return 1;
                }

                // 2.b) Most matched
                var xsum = x.GemCounts.Sum(g => g.Value);
                var ysum = y.GemCounts.Sum(g => g.Value);
                if (xsum > ysum) return -1;
                else if (xsum < ysum) return 1;

                // 2.c) Match preference
                foreach (var id in gemIdOrder)
                {
                    var xmatch = x.GemCounts.ContainsKey(id);
                    var ymatch = y.GemCounts.ContainsKey(id);

                    // it both match then use the next criteria
                    if (xmatch && ymatch) break;
                    else if (xmatch) return -1;
                    else if (ymatch) return 1;
                }

                // 2.d) 
                xsum = x.CreatesGemCounts.Sum(g => g.Value);
                ysum = y.CreatesGemCounts.Sum(g => g.Value);

                if (rules == Rules.RemoveGemOnMatch)
                {
                    // Creates the least count
                    if (xsum > ysum) return 1;
                    else if (xsum < ysum) return -1;
                }
                else if (rules == Rules.UpgradeGemOnMatch)
                {
                    // Creates the most count
                    if (xsum > ysum) return -1;
                    else if (xsum < ysum) return 1;
                }

                // 2.e) 
                foreach (var id in gemIdOrder)
                {
                    var xmatch = x.CreatesGemCounts.ContainsKey(id);
                    var ymatch = y.CreatesGemCounts.ContainsKey(id);

                    // it both match then use the next criteria
                    if (xmatch && ymatch) break;

                    if (rules == Rules.RemoveGemOnMatch)
                    {
                        // Does not create a match preference
                        if (xmatch) return 1;
                        else if (ymatch) return -1;
                    }
                    else if (rules == Rules.UpgradeGemOnMatch)
                    {
                        // Does create a match preference
                        if (xmatch) return -1;
                        else if (ymatch) return 1;
                    }
                }
            }

            // equal            
            return 0;
        }
    }
}
