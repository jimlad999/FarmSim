# Experimental Farm Simulation Game
Experimenting with procedural terrain generation in form of a farm simulator

## Getting started
* Restore nuget packages
* Run the symlink command `Packages/CreateSymlink.MonoGame.Extended.Content.Pipeline.cmd` to create the symlink to the `MonoGame.Extended.Content.Pipeline` NuGet package cache. This is required for the content pipeline, `Content.mgcb`, to work with `BitmapFont`

## Controls
* WASD to move player
* Ctrl + mouse scroll wheel to zoom
* Mouse wheel to cycle through avilable player actions
* F12 to regenerate with new random seed
* Left mouse click to use selected player action
* Right mouse click to open context menu for entities
* I to open player Inventory

## Possible features?
* Random creature generation
* Trait based creatures - traits pass on to offspring
* Plant based creatures as "farm crops"
* Creatures aid in automation
* Arbitrary building generation - player places rooms and buildings are automatically formed around that
* All farm produce/creature drops can be used for item creation
  * Adventuring equipment
  * Farm tools
  * Building materials
  * Alchemy
* Farm creatures need food - sacrifice smaller farm creatures to feed larger creatures
* Progression/mix of fantasy into sci-fi
* Tile blending to remove squareness of terrain

# External sources
* Terrain generation using OpenSimplexNoise (see comments in code)
