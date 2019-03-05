# Exiled Presence

![Image2](Resources/image.gif)

## About

Despite many popular online games adding rich presence support to Discrd, Path of
Exile has not. This is a third party tool that scans the game's log file and displays
game events on your Discord profile.

## Usage

Download the application from [releases](https://github.com/siegrest/Exiled-Presence/releases). 
It should work out-of-the-box.

Upon running the executable it will start searching for the game process. 
Once found, it will attempt to parse the game's log file. Note that if the log file
is over 10MB in size this can be extremely slow and resource consuming (you will get 
a warning message if this is the case).

To have it display information about your character one of two things are required.
First, right click the tray icon and open up the "menu". Navigate to the `config`
tab and input your account name so the program can get the character you're playing.
If your profile is set to private, the POESESSID is requird as well. The instructions
on how to find your POESESSID can be found in the correct menu or simply by googling.

All config option will be saved and automatically loaded the next time you run the 
application.

Navigate back to the main menu and stop and restart the service. After this you can
input `X` to hide the console, the service will remain running in the tray.

Load up the game and log in, you should see your Discord presence update momentarily.

## Addendum

This project was based on [PathOfExileRPC](https://github.com/xKynn/PathOfExileRPC) 
RPC client written in Python. 
Also, shoutouts to [poe-log-monitor](https://github.com/viktorgullmark/poe-log-monitor)
for providing examples of log messages and possible ways to parse them.
