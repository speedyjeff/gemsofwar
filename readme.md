![Gems of War](https://gemsofwar.com/webtest/wp-content/uploads/2019/06/logo_v2_medium.png)

## Automatically play [Gems of War](https://gemsofwar.com/)

Our family has enjoyed playing the free to play [Gems of War](https://gemsofwar.com/) together.  One day it was asked if a program could be built to play Gems of War.
Well, sure.  This project started the process of building a tool that can play Gems of War.

### Decomposing the problem into phases

The task of 'playing Gems of War' is actually a number of smaller components that can be built independently and combined.

The end-to-end can be seen in the [gemsofwar.window](https://github.com/speedyjeff/gemsofwar/blob/master/gemsofwar.window/readme.md) application.  There is sample data contained within the repo to give this a try.

####Multiple phases

1. (done) Build the Gems of War game engine

The most basic building block is that you need to build a [gemsofwar.engine](https://github.com/speedyjeff/gemsofwar/blob/master/gemsofwar.engine/readme.md) which includes the rules of the original game.
The engine is a fairly simple 8x8 grid with match semantics.  There are two rule sets for matches: Battle the gems disapear and Treasure Maps where the gem is upgraded to the next type (at the point of the match).

The simplest form of the game can be seen in the [gemsofwar.console](https://github.com/speedyjeff/gemsofwar/blob/master/gemsofwar.console/readme.md).

```
./gemsofwar.console.exe play -mode battle
```
![gemsofwar.console](https://github.com/speedyjeff/gemsofwar/blob/master/images/gemsofwar.console.png)

2. (done*) Intrepret the visual board and translate into the game engine

This has been frankly the hardest part of the project as we play on a console and need an external camera to intrepret what is on the board to then be fed into the local game engine.
Gems of War gems offer two defining characteristics - their color and markings.  The [gemsofwar.ai](https://github.com/speedyjeff/gemsofwar/blob/master/gemsofwar.ai/readme.md) contains most of the details on how a machine learning model was built to detect these gems.

![red gem](https://github.com/speedyjeff/gemsofwar/blob/master/images/red.png)
![green chest](https://github.com/speedyjeff/gemsofwar/blob/master/images/greenchest.png)

The distinct color was the primary feature used to automatically determine the color of gems.

* The model(s) can achieve a higher than 97% accuracy, so in a typical 8x8 grid (64 gems) it misses 2-3 which means that it is not viable for a realtime engine.  More work is needed to improve the accuracy.

3. (done) Determine an optimal move

Originally the plan was to train a model to learn to play, perhaps leveraing [q-learning](https://en.wikipedia.org/wiki/Q-learning), however upon 'doing the math' there are well over a billion individual game boards.  As such, a simpilier approach of hand written heuristics where choose.
An explaination of the heuristics and order can be found in [StrategyCompare.cs](https://github.com/speedyjeff/gemsofwar/blob/master/gemsofwar.ai/StrategyCompare.cs).

Experimentally, the hueristics do a good job of predicting the impact (though the randomization of gems fallen is not taken into account).

4. (not planned) Apply it to the game

At this point we have a system that can intrepret the live game on a console, translate it into a local system, and determine the optimal move.  The next logical step is to automatically perform the move on the console (via some sort of automation).  
I have no plan to make this happen.

5. (not planned) Model more than just matching gems into considering characters abilities

Beyond matching gems, Gems of War offers a very rich set of character abilities that executed.  A complete game engine would need to take these into account and model there effects.
I have no plan to make this happen.

### Setup

```
git clone https://github.com/speedyjeff/gemsofwar
git submodule init
git submodule update
```

#### Building

This project was built with [Visual Studio 2022 Preview 1](https://visualstudio.microsoft.com/vs/preview/vs2022/).

```
gemsofwar.window - Primary application
gemsofwar.console - Command line utility useful for training the machine learning model
gemsofwar.engine - Core logic for the Gems of War game play
gemsofwar.engine.ai - Machine Learning model determining the gem color based on camera images
gemsofwar.engine.tests - Unit tests
engine.Common - UI framework for the GemBoard Winforms control
engein.Winforms -   "
```

