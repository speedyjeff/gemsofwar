## Gems of War Machine Learning Models

There are a number of phases to achieve being able to translate a picture of the Gems of War board into the correct gem types.

### Finding the board of gems

Given an image the first step is to identify the board of gems.

![gemsofwar.console](https://github.com/speedyjeff/gemsofwar/blob/master/images/treasurehunt.jpg)

As a first attempt, I leverged a number of simple techniques to modify the image in the attempt to identify the edges and highlight the gems.  Each of these were a heuristic based approach and each suffered from the fact that specific colors were used as the identifiers, which would naturally vary from camera to camera.  As such, time was spent with this approach but it was not successful.

 - Pixelate
Average the pixel color per chunk and replace

![gemsofwar.console](https://github.com/speedyjeff/gemsofwar/blob/master/images/grayscale.jpg)

 - Flatten colors
Only keep predetermined colors (common colors for each gem)

![gemsofwar.console](https://github.com/speedyjeff/gemsofwar/blob/master/images/flatten.jpg)

 - Remove colors
Remove colors that make up the background and seperators

![gemsofwar.console](https://github.com/speedyjeff/gemsofwar/blob/master/images/remove.jpg)

 - Grayscale
Convert to grayscale

![gemsofwar.console](https://github.com/speedyjeff/gemsofwar/blob/master/images/grayscale.jpg)

 - Resize
Reduce the resolution to remove the depth of color gradient

After experimenting with a heuristic based approach, I moved to train a classic image classification model.  The images were first transformed into grayscale to normalized and trained into a model.  

The trained model was successful in finding the character on the left of the Treasure Hunt map (see below), but was not able to identify the other edge nor the gems.

![gemsofwar.console](https://github.com/speedyjeff/gemsofwar/blob/master/images/treasurehunt-identify.jpg)

The end result, was I was not able to train a model to correctly find the edges (without more time) and the simple heuristic based approach did not yield promising results that were repeatable across different cameras.

The proposed solution to this problem was for the user to draw a bounding box within the image where the gem board was.  This approach was simple and has yielded good results.

### Identifying gems

Now that we have an approach to extract the gem board (via a user defined bounding rectangle) the next problem was to identify what type of gem based on the image.  Given the bounding box is a rectangle and the gems are proportionally layed out, it was easy to break the board into 64 chunks and consider each gem individually.

Battle gems consist of Red, Blue, Green, Purple, Yellow, Brown, and Skulls, and Treasure Hunts consist of Bronze, Silver, Gold, Bags, Chest, Green Chest, Red Chest, and Safes.  Each gems have identifying markings and colors.

![red gem](https://github.com/speedyjeff/gemsofwar/blob/master/images/red.png) | ![blue gem](https://github.com/speedyjeff/gemsofwar/blob/master/images/blue.png) | ![green gem](https://github.com/speedyjeff/gemsofwar/blob/master/images/green.png) | ![yellow gem](https://github.com/speedyjeff/gemsofwar/blob/master/images/yellow.png) | ![brown gem](https://github.com/speedyjeff/gemsofwar/blob/master/images/brown.png) | ![purple gem](https://github.com/speedyjeff/gemsofwar/blob/master/images/purple.png) | ![skull gem](https://github.com/speedyjeff/gemsofwar/blob/master/images/skull.png) |
![bronze](https://github.com/speedyjeff/gemsofwar/blob/master/images/bronze.png) | ![silver](https://github.com/speedyjeff/gemsofwar/blob/master/images/silver.png) | ![gold](https://github.com/speedyjeff/gemsofwar/blob/master/images/gold.png) | ![bag](https://github.com/speedyjeff/gemsofwar/blob/master/images/bag.png) | ![chest](https://github.com/speedyjeff/gemsofwar/blob/master/images/chest.png) | ![green chest](https://github.com/speedyjeff/gemsofwar/blob/master/images/greenchest.png) | ![red chest](https://github.com/speedyjeff/gemsofwar/blob/master/images/redchest.png) | ![safe](https://github.com/speedyjeff/gemsofwar/blob/master/images/safe.png)

The first thing was to determine the right normalization of the gem to aid with identification.  The following approaches were used in order.

 - Average color
The process was to take the average red,green,blue colors and use that to determine the overall color.  It turned out with the 3d shading used on the gems that the average color is about the same for all the gems (a lovely mauve).

 - Most common color
The most common color was more promissing, but for the darker gems they were about the same (largely black).

 - Variations on filtering (out black, out white)
Through most of these normalizations I tried to filter out a set of colors - high intensity, low intensity, black, etc.  These were very helpful, but did not really change either the most common color or average.

 - Rounding the colors to the closest 10's
I sampled images from various cameras and found that saturation and brightness varied a lot, so I tried attempts to smooth out the color intensity by rounding.  This did not yield a promising result.

 - Taking an x shape of 10 pixels
The next approach was to sample 10 pixels in an x pattern.  This was very promising and boosted the reliability of the trained model.  The challenge was that with round gems the edges of the x did not hit the gem.

 - Taking a + shape of 10 pixels
The final approach, which has yielded strong results, was to sample 10 pixels in a + shape.  This has a strong correlation of the gem color and could be used to identify the gem.

![blue normalized](https://github.com/speedyjeff/gemsofwar/blob/master/images/blue-normalized.png)

By normalizing to a 0-1 value based on the 255 value for each r,g,b using the + shape, and a decision tree regession is able to identify the shapes at ~97% accuracy.

### Training the model

Three machine models were considered - [image classifier](https://en.wikipedia.org/wiki/Contextual_image_classification), [random tree forest](https://en.wikipedia.org/wiki/Random_tree), and [decision trees](https://en.wikipedia.org/wiki/Decision_tree).  Based on accuracy and performance, the decision tree algorithm was choosen.

The model is trained using [OpenCV](https://opencv.org/) and the [OpenCVSharpv4](https://github.com/shimat/opencvsharp) adaptor for .NET.  The input data is a 30 element float array of r,g,b elements normalized to 0.0-1.0 that represent the 10 pixels sampled from the gem image.

This model is able to achieve a 99.02268210725726% prediction accuracy (as measured by R^2).

Each individual label is also predicted >99%.

```
Purple 99.7519%
Blue   99.7595%
Brown  99.5726%
Red    99.8666%
Skull  99.1851%
Green  99.8961%
Yellow 99.6779%
```

The model exposes a set of configuration nobs that can be adjusted to further optimize the model.

```C#
        // model config
        public int MaxDepth { get; set; }
        public int CVFolds { get; set; }
        public int MaxCategories { get; set; }
        public int MinSampleCount { get; set; }
        public float RegressionAccuracy { get; set; }
        public bool TruncatePrunedTree { get; set; }
        public bool Use1SERule { get; set; }

        // feature config
        public int PrecisionOfRound { get; set; }
        public bool[] SkipFeatures { get; set; }
```

The first set of options are the tranditional knobs provided by the Decision Tree model.  The later adjust the rounding precision of the normalized data and how many of the features to consider when training the model.

### Genetic Algorithm

Genetic algorithms start with a random set of configurations and morph into optimal by process of observation.  

The [genetic algorithm](https://github.com/speedyjeff/gemsofwar/blob/master/gemsofwar.ai/GeneticAlgorithm.cs) follows a very simple process.

1. Generate a population of N random configuration
2. Evaluate each (train a model, and evalute the fitness based on the least performing label)
3. The top 10% of the population is moved to the next generation
4. The remaining 90% of the population is choosen by a combination of the current population
  a. Pairs of configs are choosen from the population to combine
  b. There is a 45% chance of taking a trait from one of the parents, and a 10% chance to have a mutation (a random trait)
5. The process goes back to step 2 for X generations

Here is an example evolution with a population of 100 and 10 generations.

![graph](https://github.com/speedyjeff/gemsofwar/blob/master/images/traininggraph.png)

```
generation 0
fitness : 0.9911110997200012
config  : MaxDepth:2080439750 CVFolds:0 MaxCategories:12 MinSampleCount:2 RegressionAccuracy:0.028f TrucatedPrunedTree:False Use1SRule:True PrecisionOfRound:1 SkipFeatures:000000000000000000000000111000
generation 1
fitness : 0.9918518662452698
config  : MaxDepth:60138830 CVFolds:0 MaxCategories:27 MinSampleCount:0 RegressionAccuracy:0.031f TrucatedPrunedTree:True Use1SRule:True PrecisionOfRound:3 SkipFeatures:101000000000000000000000000000
generation 2
fitness : 0.9925925731658936
config  : MaxDepth:1489792882 CVFolds:0 MaxCategories:5 MinSampleCount:1 RegressionAccuracy:0.008f TrucatedPrunedTree:True Use1SRule:False PrecisionOfRound:1 SkipFeatures:000001000000000000100000000001
generation 3
fitness : 0.9925925731658936
config  : MaxDepth:1489792882 CVFolds:0 MaxCategories:5 MinSampleCount:1 RegressionAccuracy:0.008f TrucatedPrunedTree:True Use1SRule:False PrecisionOfRound:1 SkipFeatures:000001000000000000100000000001
generation 4
fitness : 0.9925925731658936
config  : MaxDepth:1489792882 CVFolds:0 MaxCategories:5 MinSampleCount:1 RegressionAccuracy:0.008f TrucatedPrunedTree:True Use1SRule:False PrecisionOfRound:1 SkipFeatures:000001000000000000100000000001
generation 5
fitness : 0.9957058429718018
config  : MaxDepth:752575344 CVFolds:1 MaxCategories:43 MinSampleCount:1 RegressionAccuracy:0.008f TrucatedPrunedTree:True Use1SRule:True PrecisionOfRound:1 SkipFeatures:000001000100000000100100000001
generation 6
fitness : 0.9957058429718018
config  : MaxDepth:752575344 CVFolds:1 MaxCategories:43 MinSampleCount:1 RegressionAccuracy:0.008f TrucatedPrunedTree:True Use1SRule:True PrecisionOfRound:1 SkipFeatures:000001000100000000100100000001
generation 7
fitness : 0.9957058429718018
config  : MaxDepth:752575344 CVFolds:1 MaxCategories:43 MinSampleCount:1 RegressionAccuracy:0.008f TrucatedPrunedTree:True Use1SRule:True PrecisionOfRound:1 SkipFeatures:000001000100000000100100000001
generation 8
fitness : 0.9957058429718018
config  : MaxDepth:752575344 CVFolds:1 MaxCategories:43 MinSampleCount:1 RegressionAccuracy:0.008f TrucatedPrunedTree:True Use1SRule:True PrecisionOfRound:1 SkipFeatures:000001000100000000100100000001
generation 9
fitness : 0.9957058429718018
config  : MaxDepth:752575344 CVFolds:1 MaxCategories:43 MinSampleCount:1 RegressionAccuracy:0.008f TrucatedPrunedTree:True Use1SRule:True PrecisionOfRound:1 SkipFeatures:000001000100000000100100000001
```

Using this approach an optimal set of configurations can be determined for the set of input.

### Choosing the optimal move

In order to choose an optimal move, each possible move is done and the impact measured.  The biggest caveat to this approach is that new gems are not added to board (as that is random and not attempted to be modeled).  

A tree of moves is generated acculated how many gems are matched and what are created.  An ideal match is then determined by a set of heuristics.

```
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
```

The strategies differ depending on if you are doing a Battle (where an opponent goes next) or Treasure Hunt where you want to setup the next move.
