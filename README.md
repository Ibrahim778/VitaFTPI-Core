# VitaFTPI-Core
This is a tool to be used with Sillica's Unity Tools and unity for quickly installing unity games for the vita over FTP or USB Based on .NET Core.

# What is it?
This is a tool to be used with Sillica's Unity Tools and unity for quickly installing unity games for the vita over FTP.

This tool is based off of a program made my Jordi at jllopisol@gmail.com. This program wouldn't be possible without his work so if you like this program consider donating to him via paypal [here](https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=RMFDRTBU49E8E)

This program also uses Sillica's UnityTools for building the game and this wouldn't be possible without it either.

This also uses WinSCP to transfer files and commands over FTP so credit goes to those guys as well.

Do note that you NEED vitashell to be installed to use usb mode. You should already have vitashell installed so you can ignore this in most cases.

# How to use: 
1. Download and extract the Tools.zip folder from the releases tab.
1. Copy the Editor folder to your assets folder.
1. Copy the Uploader folder to any folder in your pc (try documents).
1. Enter your vita IP and other settings in the menu item VitaFTPI/Options and if you haven't placed the uploader folder in your project directory enable custom uploader path and browse to your uploader folder.
1. Make sure your vita has vitacompanion installed. It's a plugin by devnoname120.
1. Install the vita app for this tool from [here](https://github.com/Ibrahim778/Unity-Loader/releases).
1. Try to build like normal and sit back and relax.

# Known Issues:
***Nothing to see here***

# Credits: 
1. Jordi at jllopisol@gmail.com for making the original program
1. __*HUGE*__ thanks to Silica, Princess of Sleeping, Sarcastic Cat, たまご, TheFlow, DevNoName120, Rinnegatamante, s1ngyy and anyone else who helped for answering all of my questions.
1. WinSCP
1. TheOfficialFlow for the USB suppourt I stole the code for it from there.
1. Sillica for UnityTools
1. Icon for UnityLoader was very kindly provided by @noradninja on discord.
1. TheOfficialFlow for making our vita's hackable on newer firmwares
1. Team Molecule for Henkaku
1. @CTPEJIOK22 for giving me the idea on discord
1. @asdronin on discord for making the server
1. @iDevOnAParkingLot on Discord for inviting me to the server
1. Sony for the console.
1. Everyone who contributed to VitaSDK. 
1. Anyone else who helped out on the discord servers.

# Updating

Updating is simple REPLACE not merge both the uploader and editor folder.

# Warnings:
1. If uma0: is mounted usb mode will corrupt the data in it. uma0: is usally used for usb's in a pstv and for psvsd. So be careful! You have been warned.

# FAQ:
**1. What do I put as the drive letter?**

Ans: You should only care about this if you are using USB mode you can enable / disable it by changing UseUSB to true. If you are using USB then you can go into vitashell connect your PSVITA to your pc via usb and note the drive letter you can set that as the letter, including the colon, and disconnect the vita now. Do remember that you vita needs to be connected to your pc via usb while building and only disconnect it after the install process has completed or else your vita WILL freeze.

**2. Can I use USB mode with udcv-uvc plugins?**

Ans: Yes! UnityLoader will automatically unload and load udcd_uvc before and after the transfer. *IMPORTANT* For this to work the plugin needs to be in ur0:tai or ux0:tai and it needs to be named udcd_uvc.skprx so if you use any of the oled or lcd_off plugins just rename them to udcd_uvc.skprx and make sure to change it in your config.txt as well. For this to work you need UnityLoader V1.6 or later.

# Notes:
1. Version 1 of the app only worked with vitashell's ftp making it useless and is no longer suppourted. Versions 2 and above of the app require the accompanying app for the vita to be installed. You can download the VPK from [here](https://github.com/Ibrahim778/Unity-Loader/releases).

1. A previous version of this app VitaFTPI worked only for some people so I have migrated it to .NET Core here.
