# Exiled Presence

![Image1](Assets/image.gif)

## About

Despite many popular online games adding rich presence support to Discrd, Path of
Exile has not. This is a third party tool that scans the game's log file and displays
game events on your Discord profile.

## Usage

Download the application from [releases](https://github.com/siegrest/Exiled-Presence/releases). 
It should work out-of-the-box.

Upon running the executable it will start searching for the game process. Once found, 
it will parse the game's log file.

To have it display information about your character you need to input your account name.
First, right click the tray icon and select `Edit config`. Input your account name so 
the program can get the character you're playing. If your profile is set to private, 
the POESESSID is requird as well. After saving click on `Reload config`.

Load up the game and log in, you should see your Discord presence update momentarily.

## Addendum

This project was based on [PathOfExileRPC](https://github.com/xKynn/PathOfExileRPC) 
client written in Python. 
Also, shoutouts to the guys at [poe-log-monitor](https://github.com/viktorgullmark/poe-log-monitor)
for providing a list of up to date areas and map tiers.
