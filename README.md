# Yogscart Beta

## Changelog : Started 04/08/2016

# 27/11/2017
* Moved Saving to new Save Data manager
* Added unlocking Insane mode if you get a gold or higher on any difficulty on every cup
* Added Achievement System
* Added "Whole New Game" Achievement
* Added "Lapis" Achievement for 50, 500 and 1000 pieces of Lapis collected
* Added "Eggs Hit" Achievement for 50, 100 and 200 racers hit by eggs
* Added "Perfect Rank" Achievement
* Added "Dead Last" Achievement
* Added "64 VS Races" Achievement
* Made Credits scroll properly and faster
* Added "Stats" option to Main Menu

# 26/11/2017
* Added Snow Flakes to Main Menu
* Added Christmas Music to Main Menu
* Added Placeholder Santa Hat
* Added Christmas Sjin's Farm (Haven't actually started decorating it though)
* Levels with multiple versions(Sjin's Farm) now have a random chance of swapping version
* Wearing the Witches Hat will increase chances of Spooky Sjin's Farm
* Wearing the Santa Hat will increase chances of Christmas Sjin's Farm
* Fixed bug where the maximum number of VS Races was 63

# 24/11/2017
* Fixed Eggs not firing properly

# 05/11/2017
* Added Chilli PowerUp
* Fixed inferior boost overriding longer / more powerful boost
* Fixed making Kart a god, not effecting kart collisions
* Fixed Eggs breaking on owner when just fired

# 23/10/2017
* Added Ghost Shader to Time Trial Ghosts
* Fixed Online Karts not unlocking for Spectators
* Fixed Map Viewer not being hidden with Spectator UI
* Fixed Spinning out and tricks not being sent over server
* Fixed Canvas Names appearing to glitch
* Moved Kart Item input to Kart Item
* Added (Broken) Power Ups to Online
* Made Port a textfield in Server Settings
* Added Bloom
* Added Bloom to Options
* Adjusted Effects Manager to use simple version saving
* Adjusted Time Trials to use simple version saving
* Fixed Dropdowns overlapping other GUI
* Fixed Cancel not closing anti-aliasing dropdown

# 22/10/2017
* Added button to Popup
* Swapped input on Freecam height changing
* Made Host Lobby continue to show useful info on gamemode start
* Added Back button to client conenction and host server settings
* Fixed submit key not working for server selection
* Fixed using magic numbers for gamemode selecting .etc

# 19/10/2017
* Fixed deleting servers from list not saving
* Fixed Karts not spawning in Online
* Added disconnecting early on connectin wheel
* Fixed brief broken screen for spectators between intro and countdown
* Started Spooky Sjin's Farm

# 16/10/2017
* Cleaned up Network Selection (Now uses a nice sting array)
* Fixed Spectator UI not showing
* Fixed Racer Names not facing Cutscene and Spectator Cam
* Fixed Pig Syncing over Network (Sorta)
* Fixed Leaderboard not being deleted on Spectator

# 15/10/2017
* Fixed voting timer not hiding when all votes recieved
* Fixed selected track from vote not being loaded
* Fixed Change Character not working in new Lobby
* Fixed Gamemodes persisting when not needed in main menu
* Fixed MapViewer persisting when not needed
* Updated to Unity 2017.2 (Which fixed wierd Controller Bug)
* Made sure Inputs are locked inside of online (Except for character changing)
* Fixed Pasue Menu not locking at end of game
* Online Stuff now takes place in a seperate "Lobby" Scene

# 14/10/2017
* Fixed Host not recognising Client Ready Message
* Made Music go faster on last lap of Race in Online
* Added Racing Behaviour to Host in Online
* Added 60 Second timer when first player crosses Finish Line
* Fixed Local Kart Lag Issues on Client
* Fixed Kart Syncing Issues across Network (Now use new Kart Body Lerp Stuff)
* Fixed Kart Input Syncing Issues across Network
* Fixed moving mouse allowing player to change character after selection
* Made whichever player is using the keyboard have their loadout effected by the mouse
* Fixed Hats despawning when you go back on character select
* Added Leaderboard at end of Race
* Added new Lobby Scene
* Added Gamemode Ending

# 11/10/2017
* Fixed Pause Menu pausing time in Online
* Added Readying up Method to Online Race
* Added Countdown to Online Race
* Added Kart Unlocking to Online Race
* Added RaceInfo to Karts in Online Race
* Made the wreck it ralph hacker message it's own function

# 10/10/2017
* Added an audio listener to Intro Camera
* Added Kart Spawning
* Centred Online PopUp
* Input Manager hides on start of Gamemode
* Added up to Countdown
* Turned off all Audio Listeners during intro

# 09/10/2017
* Finished Voting Screen and Level Select
* Started implementing Race functionality

# 08/10/2017
* Added Character Select for Client
* Allowed Client and Host to change Loadout in Lobby
* Added list of current players to lobby
* Added gamemode selection to lobby
* Added gamemode selection to server selection
* Added timer functionality to online
* Started added Level Selecting and Voting

# 06/10/2017
* Fixed issues with Network Selection
* Added basic Lobby
* Added basic Host behaviour
* Added basic Client behaviour

# 27/09/2017
* Fixed Kart/God Collisions
* Added Pig Idle Noises
* Added Pig Hit Noises
* Added Hay Stacks that can be driven through

# 26/09/2017
* Added Ghost Replaying from Time Trial Menu
* Fixed camera effects being affected by multiple karts in replay
* Fixed camera effects showing up on Freecam

# 25/09/2017
* Fixed position of VS Race Counter and Ghost Counter
* Added turning off all cameras during a cutscene to improve performance
* Fixed Cancel and Accept not being selectable in Graphics menu
* Fixed selecting Cancel and Accept glitch
* Fixed AA selecting not working
* Made Replay recording more accurate, added float to recorder to make it even more accurate
* Added resume button to Replay pause menu
* Fixed replay cameras not pausing on pause
* Fixed pause menu unpausing, instead of closing options menu
* Renamed kartItem to KartItem
* Fixed bug where unpausing would use item
* Fixed stream button in options not highlighting during a race
* Fixed kart camera can be changed when paused
* Added Chromatic Abberation on Replay when using Player Cam

# 24/09/2017
* Added Unity Post Processing Stack
* Added FXAA and TAA to game
* Added Chromatic Abbreation on Boost
* Added Ambient Occulusion
* Added new graphics options to options menu
* Added Effects Manager which ensures all camera use the correct effects

# 20/09/2017
* Updated to the latest Unity
* Added more SFX for Characters
* Blocked Track Cam option from replay (Temporarily)

# 19/09/2017
* Added Custom Sound Packs to characters depending on Hat

# 30/07/2017
* Inverted D-Pad Vertical on Menus

# 28/07/2017
* New Input Manager
* Fixed selecting other input in Binding Menu
* Implemented Add/Change function in Control Layout
* Added Layout Selection
* Item Balancing
* Added a basic win screen cutscene
* Added using default controller inputs for menus
* Added not being able to delete layouts during a race
* Fixed changing binding during race
* Added clear function to binding
* Various bug fixes and improvements

# 24/07/2017
* Increased kart collision push distance
* Added Ghost Data updating
* Added New Options Tab
* Move Input tab to Binding
* Added Mouse Sensitivity Options
* Added Controller Sensitivity Options
* Added options to replay pause menu

# 23/07/2017
* Added Kart Body smoothing on Kart Replayer
* Fixed Leaderboard text being cut off
* Fixed position of "You Lose" text
* Made Free Cam faster
* Fixed Free Cam / Orbit Cam rotating
* Made Level Select say "N/A" when no time trial data for a track
* Fixed Confirm Changes showing when no changes made
* Added Stream Mode to Options
* Made Race avoid using characters already selected by Players (If possible)
* Adjusted naming of target in Replay

# 18/07/2017
* Added Kart Collisions (Again)
* Added twist animation to Kart Collisions
* Added Particle Effect to Kart Collisions
* Fixed Pigs
* Added particle effect to ghost death
* Fixed issue where ghosts wouldn't fade away on repeats
* Adjusted Kart Camera to focus on Kart Body
* Added Kart Collisions to Head IK
* Fixed Ghost Karts colliding with real ones
* Made a basic taint shader for Sjin's Farm

# 17/07/2017
* Added Replay Camera Modes
* Added Replay UI

# 16/07/2017
* Stopped Ghost from collecting Lapis
* Fixed Music Pitch not resetting on Race Start
* Made Music Pitch slowly change rather than snapping to the new pitch

# 15/07/2017
* Fixed Kart Item auto using items when button held down
* Added info about ghost to Time Trial Leaderboard
* Fixed Replay Item Usage
* Fixed Replay Start Boosting
* Added 'Inputs Locked' UI
* Added Outro Music to Win Screen
* Disabled Online
* Fixed using items as shields
* Adjusted Kart IK to look at karts spinning out only
* Add Growing Animation for Items
* Add UI and buttons for number of races in VS Race
* Fixed broken Dev Ghost selection and use
* Fixed Ghosts not having names
* Fixed double submit noises on Time Trial Menu
* Adjusted times to get drift boost, depending on wheter you steer into a drift

# 14/07/2017
* Added Ghost Counter to Level Select
* Fixed mouse hovering over options when there is a popup
* Made 'A' add controller to game as well as 'Start'
* Fixed popup rendering
* Added explanation text to nickname window
* Added keyboard to nickname window
* Made Ghost faded in game (And turned it off cause it adds 15 minutes to build time)
* Adjusted how nickname window uses input manager
* Made default drift looser
* Added wider range when drifting
* Added mouse support to drifting
* Made sure items can't be used when game is paused
* Fixed Pause Menu issues
* Fixed Reset Data button
* Added version compatibility to Ghost Data
* Added unlocking delay to Input Manager

# 11/07/2017
* Added ability to change number of races in VS Race
* Added Ghost Selection for Time Trial
* Added Ghost Racing in Time Trial
* Added Dev Ghosts
* Changed Company Name in Unity Player
* Created Replay Component
* Added basic Replay Functionality
* Added delay to end of kart recording
* Added Player Names
* Added Change Player Name Dialogue
* Added forcing new players to give a name
* Created a new function for scaling rects relative to other rects
* Updated Executable Icon
* Added Ghosts to Map Viewer during Time Trial
* Fixed issues with FauxCollider and other karts (ghosts)
* Fixed issues with FauxCollider and some walls
* Made Ghost disappear when it finishes
* Fixed Kart Recorder not instantly taking reading
* Added Drift Steer to Kart Recorder

# 10/07/2017
* Created a new Race Script
* Created a new Tournament Script from Race Script
* Created a new VS RaceScript from Race Script
* Created a new Time Trial Script from Race Script
* Added Ghost Saving
* Adjusted timer to stop more accurately on Time Trial
* Redesigned Race Info screen
* Redesigned Next Menu screen
* Fixed Character Select using mouse when not visible
* Fixed corner Back Button on Character Select
* Fixed corner Back Button on Level Select
* Added Time Trial Menu
* Fixed broken save data bug
* A lot of bug fixes and improvements
* Created Win Screen
* Added (Debug) Clapping Animation
* Started animating Win Screen Animation

# 09/07/2017
* Made drift tighter
* Fixed JR not always targetting closest player
* Added Next Race option to VS / Tournament Replays

# 08/07/2017
* Reduced Debug Track to 1 lap
* Fixed DPad only working in one direction
* Fixed End of Race menu being inverted
* Added Kart Recorder
* Added Kart Replayer
* Added Replay option to races

## 05/07/2017
* Removed any and all traces of Wheel Colliders
* Created new FauxCollider (Replacing Wheel Colliders)
* Created new KartMovement (Replacing KartScript)
* Created new KartInput (Replacing kartInput)
* Fixed Sound Distorion
* Fixed Character Taunts being distorted
* Fixed Egg/JR snapping to ground
* Fixed Egg/JR colliding with ground
* Fixed JR jumping to target
* Fixed AI continuing to drive at end of non-looped track
* Added New Tags
* Made Track Layout Editor force it's children to be tagged as roads
* Fixed Boost Noise playing multiple times

## 03/07/2017
* Fixed JR not looping on looped Tracks
* Added firing JR backwards
* Fixed music volume bug
* Fixed reset save button not changing colour
* Made Sjin's Farm final lap slower

## 02/07/2017
* Fixed bug where option to reset data was below options
* Added popup when you don't confirm graphics changes
* Added Basic AI item usage
* Added D-Pad support to Menus
* Redid JR PowerUp

## 01/07/2017
* Added Option to delete save data
* Added Pop Up to confirm save data deletion
* Redid Race Countdown to be more efficent
* Tried to fix wheel issue
* Made basic AI for new track system
* Made AI Stop at end of looped track
* Made Key Kart less shiny
* Fixed Death Catch
* Made Options render ontop of race gui
* Updated to Unity 5.6.2

## 29/06/2017
* Finished Track Editor
* Rewrote Position Finding
* Fixed wheel colliders falling through the ground
* Added hiding mouse when not in use
* Adjusted time required for drifting
* Added a Restart button to Time Trials
* Added a secret boost to Downtown Debug
* Adjusted how levels are loading to avoid stuttering on loading screens
* Converted all tracks to new track format

## 28/06/2017
* Gave each Track new banner image
* Redesigned Track Banner
* Rewrote large chunk of Track Editor

## 27/06/2017
* Added new Debug Testing Track
* Added Carnival Del Banjo Layout Track
* Added Downtown Debug Track
* Fixed Input Manager allowing controllers during intro (Now only does it in editor)
* Added SFX to Pause Menu and Level Select
* Fixed bug where level select wouldn't show after VS Race
* Fixed Export Obj button on Track Layout Editor
* Removed engines noises from track intro
* Fixed issue with sorting script if too many people cross finish line
* Gave Lapis Item SFX
* Adjusted Kart pushing Force
* Fixed engine noises when game is paused
* Fixed being able to open pause menu on Countdown
* Redesigned Map overlay and map tracking
* Fixed Leaderboard sometimes requiring two presses
* Fixed issue with Postion Finding
* Fixed Eggs not staying close to the ground
* Various Bug Fixes and Improvements

## 25/06/2017
* Improved Track Layout Editor

## 17/06/2017
* Added Node Deletion to Track Layout Editor

## 16/06/2016
* Sorta added reversing to AI
* Broke the rest of the AI
* Added Window for Track Layout Editing
* Added exporting track layout to .OBJ

## 14/06/2017
* Started Implementing Track Layout Creator

## 09/06/2017
* Improved AI
* Improved Drift Detection for AI
* Improved AI Drifting Ability

## 27/05/2017
* Adjusted Kart Collisions so both karts move during collision
* Made Characters look at other characters heads (Rather than the ground beneath them)
* Adjusted kartScript to use a modifer rather than a direct speed change
* Created Debug Kart Spawner to create a kart for testing
* Adjusted wheel friction to stop sliding
* Reduced Physics Timestep and increased gravity to imrprove driving
* Added Kart Ground hugging to avoid kart flying upwards on gradients
* Stopped Kart from sliding up walls (Slide effect of last fix)

## 26/05/2017
* Add pitch and volume change on engine during start boost
* Made engine go quieter after 5 seconds of max throttle
* Adjusted kartScript to store all it's particle systems as a dictionary
* Made Kart Maker setup particle systems from a predefined particle pack
* Adjusted KartSkeleton to include particle position
* Made Kart Maker create wheel colliders from a predefined wheel collider

## 23/05/2017
* Hid starting boost clouds for AI Racers
* Added an Incoming Item Indicator
* Fixed bug where JR would not fly down towards target
* Added Hit Sounds to spinout Method if wanted
* Made Egg and JR cause hit sound
* Made Dirtblock cause hit sound
* Added storing parent kartScript of item in DamagingItem Class
* Added owner Taunt Sounds when Dirtblock, Egg or JR hits
* Fixed bug where characters would taunt if hit by own item

## 22/05/2017
* Fixed pitched of lapis, crates and egg power ups on drive by
* Made pitch change for background music on final lap
* Added a Top Down Map
* Implemented Top Down Map on Sjin's Farm

## 18/05/2017
* Added Sji'n Farm Intro Cutscene
* Updated name textures for scoreboards to new style
* Added Pigs to Sjin's Farm
* Fixed Collision Handler Bug

## 17/05/2017
* Fixed centering on Rotate GUI Indicator
* Added kinematic rigidbodies to Sjin's Farm Walls
* Fixed character select cameras
* Fixed spamming submit on multiplayer selection
* Fixed some gui fading for no reason on character select
* Adding cursor removal when character/hat is selected
* Fixed side image changing on difficulty selection
* Redid Simon Icon to match style

## 12/05/2017
* Added IK Steering for sparkles kart
* Added IK Steering for owl kart
* Removed IK cubes from models
* Added Egg fire sound
* Added Egg hold sound
* Added Egg bounce sound
* Added create smash sound
* Added random sound on Lapis pickup
* Fixed character names on character select not facing camera
* Added rotate indicator UI to character select

## 10/05/2017
* Multiplayer item box position fixed
* Made eggs faster
* Fixed falling into ground on Sjin's Farm
* Added IK Rotations for pedal pressing
* Added IK Steering for default kart

## 08/05/2017
* Added leveling out on big jump
* Small puffs of cloud when drifting
* Wheel Spin on start up
* Smoke on Wheel Spin
* Wheel Trails when drifting
* Lens flare on Sjin's Farm

## 20/04/2017
* Upgraded to 5.6

## 17/04/2017
* Fixed Egg Firing Direction Bug
* Added Owl Kart

## 11/04/2017
* Fixed Kart Collisions (Again)
* Added Options Menu to Pause screen
* Fixed bug where tracks got grayed out in Level Select
* Fixed various options menu bugs
* Fixed Eggs not travelling in a direction

## 23/01/2016 - 10/04/2016
* Some stuff probably happened... I forgot to write it down

## 22/01/2017
* Fixed bug when firing JR backwards
* Fixed bug where egge and JR would clip road after spawning

## 21/01/2017
* Added JR Eggs back into the game
* Added Dirtblocks back into the game
* Updated intro message

## 20/01/2017
* Added Eggs back into the game

## 17/01/2017
* Fixed Controller not being able to adjust volume
* Added Space Icon to Input Selection
* Fixed bug where ai item boxes were visible
* Added A button to leader board

## 17/12/2016
* FINALLY Fixed the Kart Suspension

## 26/11/2016
* Fixed Leaderboard fast forwarding
* Improve Suspension so start boost dosen't break everything
* Stop collision from stopping Kart

## 25/11/2016
* Fixed(???) Suspension of Karts
* Implemented Basic Kart Collision

## 26/09/2016
* FIX Soundmanger now controls all Audio Source Volumes
* Fixed bug where leaderboard would break randomly
* Fixed bug where input was required twice to go to page afetr leaderboard if point adding had not been skipped

## 20/09/2016
* Added X icon to Layout Select
* Fixed Kart Item rendering GUI for AI
* Fixed Title fading back in after Character Select
* Added skip function to leaderboard.

## 2/09/2016
* Added Start Boosting to AI
* Added (basic) AI changing playstyle depending on Intelligence
* Replace old AI code with new AI

## 1/09/2016
* Fixed bug where drifting didn't steer a lot
* Added Boosting to AI
* Improved quality of "Perfect" AI

## 31/08/2016
* Started working on the new AI system

## 18/08/2016
* Added Right Stick icon to Layout Scrollbar
* Fixed bug where menu wouldn't activate mouse mode correctly
* Fixed bug where side image would slide in after main menu had faded in
* Fixed bug where quiting race would break main menu
* Fixed bug where input menu required double tap to exit mouse mode.
* Deleted some not needed textures
* Added Controller Support to Input Menu

## 17/08/2016
* Added Controller Support to Game and Graphics Menu
* Fixed bug where dropdown menu wouldn't count selection

## 16/08/2016
* Implemented Group and Scrollview controls into GUIHelper to help improve efficiency of code

## 10/08/2016

* Fixed icon scaling when using keyboard in Character Select
* Main Menu now remembers which options you selected and automatically picks them if you go back
* Recoded side picture so it fades in and out correctly

## 09/08/2016

* Fixed bug where Level Selected would need the cancel button to be pressed twice
* Fixed bug causing backing out from level select from breaking main menu
* Fixed bug where mouse player could scale other players icons
* Fixed bug where changing between menus didn't reset the current menu selection
* Fixed bug where 2nd player inputlayout would not scroll if mouse had been moved
* Fixed bug where mouse could click arrows belonging to other players
* Add Mouse Support to Player 1's layout
* Added ability to choose control layout
* Input Manager now checks default layouts if a value hasn't been assigned for menu inputs

## 08/08/2016

* Added Arrow Keys, ctrl, alt and shift inputs to bindings
* Added Q/E and LB/RB icons to the options menu

## 05/08/2016

* Added Input Binding for Xbox Controller

## 04/08/2016

* Fixed dropdown toggle not animating
* Fixed dropdown box not closing on click
* Did a bunch of stuff that I forgot to write down