# DeathTracker (A Celeste Mod)

Keep track of your current and your personal best death counts while playing a chapter!
Comes with an optional feature to restart the chapter when you can no longer beat your PB.

![](DeathTrackerDisplay.png)

## Display format settings

You can customize the text on the display via the in-game settings / the config file.  
(Everest doesn't show text settings when you have a file selected, you need to go back to the title screen!)

Enter any format you like there. Certain values act as placeholders:

`$C` - Deaths of the current attempt  
`$B` - Best (lowest death count) clear  
`$A` - Total deaths of the current level  
`$T` - Total deaths of the save file  
`$L` - Deaths since the last level load  
`$S` - Deaths since the last screen transition

`$L` and `$S` are not saved when you leave the level / screen. Restarting the game will also reset them to 0.

## Custom skull icons

This mod can load skull icons that have been customized with CollabUtils2 or AltSidesHelper.
