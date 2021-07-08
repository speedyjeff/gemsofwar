### Gems of War Engine

![battle](https://github.com/speedyjeff/gemsofwar/blob/master/data/battle/1/image.0.bmp)

Gems of War is a 3+ gem matching game.  Matching 4+ provides the player with an extra turn.  The colors matched give the players mana which power up their special abilities.

For the scope of this version of the game, the only thing that matters is matching gems in an optimal way.  [StrategyCompare.cs](https://github.com/speedyjeff/gemsofwar/blob/master/gemsofwar.engine.ai/StrategyCompare.cs) has the full list of heuristics used to ensure that the best move is chosen.

There are two game modes: Battle and Treasure Hunts.  These modes are similar but have two different semantics.  In Battle opponents take turns and once a match occurs, the gem disappears allowing the remaining gems to fall into the vacated spaces.  In Treasure Hunts you are playing against a turn counter so you can set up your next move, in addition when matches occur the gems that caused the match is upgraded and then the remaining gems fall into position.

![battle](https://github.com/speedyjeff/gemsofwar/blob/master/data/treasure/1/image.0.bmp)
