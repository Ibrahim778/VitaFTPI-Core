# VitaFTPI-Core
This is a tool to be used with Sillica's Unity Tools and unity for quickly installing unity games for the vita over FTP Based on .NET Core.

# What is it?
This is a tool to be used with Sillica's Unity Tools and unity for quickly installing unity games for the vita over FTP.

This tool is based off of a program made my Jordi at jllopisol@gmail.com. This program wouldn't be possible without his work so if you like this program consider donating to him via paypal [here](https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=RMFDRTBU49E8E)

This program also uses Sillica's UnityTools for building the game and this wouldn't be possible without it either.

This also uses WinSCP to transfer files and commands over FTP so credit goes to those guys as well.

Do note that you NEED vitashell to be installed to use usb mode. You should already have vitashell installed so you can ignore this in most cases.

# How to use: 
1. Download and extract the Tools.zip folder from the releases tab.
1. Copy the Editor folder to your assets folder.
1. Change the values in the script in the Editor folder to match your vita's.
1. Copy the Uploader folder to the root of your project directory (So thats the parent directory of the Assets folder).
1. Make sure your vita has vitacompanion installed. It's a plugin by devnoname120.
1. Install the vita app for this tool from [here](https://github.com/Ibrahim778/Unity-Loader/releases).
1. Try to build like normal and sit back and relax.

# Known issues:
1. USB mode currently doesn't work properly. The VPK doesn't get installed and the rest of the files are copied. (SOLVED)
1. Works on some systems and not on others. A patch should be released soon for this. (SOLVED)

# Credits: 
1. Jordi at jllopisol@gmail.com for making the original program
1. __*HUGE*__ thanks to Sillica, TheFlow, DevNoName120, Rinnegatamante, sarcastic cat, s1ngyy and anyone else who helped for answering all of my questions.
1. WinSCP
1. TheOfficialFlow for the USB suppourt I stole the code for it from there.
1. Sillica for UnityTools
1. TheOfficialFlow for making our vita's hackable on newer firmwares
1. Team Molecule for Henkaku
1. @CTPEJIOK22 for giving me the idea on discord
1. @asdronin on discord for making the server
1. @iDevOnAParkingLot on Discord for inviting me to the server
1. Sony for the console.
1. Everyone who contributed to VitaSDK. 
1. Anyone else who helped out on the discord servers.

# FAQ:
**1. What do I put as the drive letter?**

Ans: You should only care about this if you are using USB mode you can enable / disable it by changing UseUSB to true. If you are using USB then you can go into vitashell connect your PSVITA to your pc via usb and note the drive letter you can set that as the letter, including the colon, and disconnect the vita now. Do remember that you vita needs to be connected to your pc via usb while building and only disconnect it after the install process has completed or else your vita WILL freeze.

**2. Can I use USB mode with udcv-uvc plugins?**

Ans: Yes! As mentioned before it uses vitashells code / plugin for usb mode and it works fine with udcd_uvc.

# Notes:
1. Version 1 of the app only worked with vitashell's ftp making it useless and is no longer suppourted. Version 2 of the app requires the accompanying app for the vita to be installed. You can download the VPK from [here](https://github.com/Ibrahim778/Unity-Loader/releases).

1. A previous version of this app VitaFTPI worked only for some people so I have migrated it to .NET Core here.
