## Playing [Gems of War](https://gemsofwar.com/)

![game play](https://github.com/speedyjeff/gemsofwar/blob/master/images/winapp.png)

The UI is was written for function over beauty.

### UI Explained

Column 1 | Column 2 
---------|----------
Live view | Gems Board
Image Controls | Gems Board
Actions | Gems Board
Gem Preview | Move Details

 - Live View - This is view that is within the cameras view. Click and stretch a rectangle to restrict the viewing region to only the gems.
 - Gems Board - This is what the computer thinks it seems.
   - Each of the cells can be clicked to change the gem color (if chosen incorrect).  Clicking high in the cell cycles the color up, and clicking low cycles the color down.
   - The left most part is the toolbar where you can assign special effects to gems like 'lycanthropy' or 'x5 doom skull'.
   - The list of colors on the far left is the preferred order of what colors to match first.  Click on the ellipses to cycle to the next color.
 - Image Controls - Control the 'brightness', 'contrast', and 'gamma' which can help with the trained models
 - Actions
   - Capture - Take a live image capture to process.
   - Train - Once labels have been assigned to _every_ gem the model can be trained.  This will also drop an image along with the labels in the working directory.
   - Guess - Once an image has been captured, this will refresh the guess (the white arrow within the Gems Board)
   - Demo - You can choose to use static images as opposed to the camera.  Click and choose the provided images in ./data/.  Click again to choose the next image.
   - Demo Path - You can choose a different path of static images.
   - Battle drop down - Choose the rule set between 'Battle' and 'Treasure'
 - Gems Preview - Each individual gem is pulled out of the larger image and shown here along with the label.  The '->' buttons can be used to cycle through all the gems.  The drop downs can be used to provide a label for what each of these gems are (to train the model).
 - Move Details - Provides a list of what impact each move will have if chosen.  This is in a sorted order and the most impactful option is presented first and shown with an arrow in the Gems Board.

### Suggested workflow

First thing is to choose if you plan on playing a 'Battle' or 'Treasure' hunt via the Actions->Battle drop down selector.

#### Demo Capture

This demo is the first recommended way to give the app and model a try.

Before starting the app copy a previously trained model into the applications working directory.

```
xcopy data\battle\3\battle.* gemsofwar.window\bin\Debug\net5.0-windows
```

1. Start the gemsofwar.window app
2. Click on the Demo button
3. Choose the './data/battle/3' directory

This will choose one of the demo static images and offer a suggested move

4. Click the Demo button to rotate to the next image

#### Demo Capture + Train

This demo will walk through how to train a new model for the images.

1. Start the gemsofwar.window app
2. Click on the Demo button
3. Choose the './data/battle/3' directory

This will choose one of the demo static images.  BUT since there is no existing model all the labels are left in 'unknown' and require you to select a label for each.

4. Within the 'Gems Preview' window add a label via the drop down for each gem.
5. Press the 'Train' button
6. Go back to step 2 (do this for 4-5 images)

After training the model for 4-5 images, you should start to see the model being able to accurately predict the gem.

#### Live Capture + Train

If you have Gems of War on a console, you can run with live capture.

1. Start Gems of War Battle or Treasure Hunt
2. Position the camera as square in front of the television as possible
3. Drag a rectangle around only the gems (the top left corner should be right at the first gem, and the bottom right corner should be right be the last gem)
4. Click the 'Capture' button

This will grab a live image from your rear facing camera (if available).  BUT since there is no existing model all the labels are left in 'unknown' and require you to select a label for each.

4. Within the 'Gems Preview' window add a label via the drop down for each gem.
5. Press the 'Train' button
6. Go back to step 2 (do this for 4-5 images)

#### Advanced Capture + Train

Follow all the steps in 'Demo Capture + Train' or 'Live Capture + Train'.

At this point, the working directory for the application has a *.json, *.json.config, *.bmp, and *.dat files which can be used to further refine and train the model.  For this walkthrough we are going to use [gemsofwar.console](https://github.com/speedyjeff/gemsofwar/blob/master/gemsofwar.console/readme.md) to train and evolve the model.

1. Open a command prompt
2. Navigate to './gemsofwar.console/bin/debug/net5.0-windows'
3. Train the model

```
> gemsofwar.console.exe train -path ..\..\..\..\gemsofwar.window\bin\Debug\net5.0-windows -output ..\..\..\..\gemsofwar.window\bin\Debug\net5.0-windows\battle.json
```

4. Evaluate how well this model works

```
> gemsofwar.console.exe evaluate -input ..\..\..\..\gemsofwar.window\bin\Debug\net5.0-windows\battle.json
Split   : 79.78154
R2      : 0.9857523295464622
Total   : 13824
Success : 13759
```

5. Run the genetic algorithm to determine the best fit for the model.

```
> gemsofwar.console.exe evolve -path ..\..\..\..\gemsofwar.window\bin\Debug\net5.0-windows -input ..\..\..\..\gemsofwar.window\bin\Debug\net5.0-windows\battle.json
```

6. Now restart the application and execute the first step of either 'Demo Capture' or 'Live Capture'

#### Playing the Game

Follow all the steps in 'Live Capture + Train', you can use the model to suggest moves and play the game.

To assist the suggestion engine to choose the colors in the order that you want them chosen, use the tool bar in the Gems Board.  Start with the 1st slot and chose the 1st color (by clicking), the 2nd, and so forth.  In addition, choose the 'wolf' and 'x5' to select gems that have these effects.

The model is not 100% accurate all the time, so you may need to correct a color of a gem and then press 'Guess' to reselect the optimal move.

Enjoy!

