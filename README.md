# What is it?
This is a tool (or a collection of them) that makes Unity Development (or development in general) on the vita much easier. It uses Silica's Unity tools and my remake of NetDbgLogPc by @Princess-of-Sleeping.

This tool is based off of a program made my Jordi at jllopisol@gmail.com. This program wouldn't be possible without his work so if you like this program consider donating to him via paypal [here](https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=RMFDRTBU49E8E)

This program also uses Sillica's UnityTools for building the game and this wouldn't be possible without it either.

This also uses WinSCP to transfer files and commands over FTP so credit goes to those guys as well.

Do note that you NEED vitashell to be installed to use usb mode. You should already have vitashell installed so you can ignore this in most cases.

# How to use
## Installing (Unity): 
1. Download and extract the Tools.zip folder from the releases tab.
1. Copy the Editor folder to your assets folder.
1. Copy the Uploader folder to any folder in your pc (try documents).
1. Enter your vita's IP address and other settings in the menu item VitaFTPI/Options and if you haven't placed the uploader folder in your project directory enable custom uploader path and browse to your uploader folder.
1. Make sure your vita has vitacompanion installed. It's a plugin by devnoname120.
1. Install the vita app for this tool from [here](https://github.com/Ibrahim778/Unity-Loader/releases).
1. Done!! You  can now do a complete install once and then later on use the replace install method.

## Using the Debug feature (Unity): 
This is not a full blown debug implimentation in unity. This will only allow you to read the Debug.Log's called in your app running in the vita inside unity like you do on PC.
For this to work you need to setup PrincessLog on your vita, it's a tool made by @The-Princess-of-Sleeping. You can download the files from [here](https://github.com/CelesteBlue-dev/PSVita-RE-tools/tree/master/PrincessLog/build), Install the skprx under kernel in your taihen config and Install the vpk and enter your PC's IP leave the port at default and press save config then reboot your vita. Do note that when you press start debug a new window will open. This is normal and you can ignore it but *DO NOT CLOSE IT*.

Then to call the logs from within unity you need to copy the Plugins folder from the release into your assets folder. Then you can use the sample log script provided and copy it into your project or you can make your own. If you use the sample script it will automatically override your debug.logs depending on the platform so there is no need to worry.

Do remember this uses the official sdk for printing it to the logs. So do remember to remove the plugins and debug script when compiling the final release.

# Known Issues:
1. Udcd unloading works but once the usb transfer has completed reloading it hangs unity loader. you need to exit to the livearea and re-enter into the app for it to work.

# Credits: 
1. Jordi at jllopisol@gmail.com for making the original program
1. __*HUGE*__ thanks to Silica, Princess of Sleeping, Sarcastic Cat, たまご, TheFlow, DevNoName120, Rinnegatamante, s1ngyy and anyone else who helped for answering all of my questions.
1. WinSCP
1. @The-Princess-of-Sleeping for the original NetDbLogPc and PrincessLog.
1. TheOfficialFlow for the USB suppourt I stole the code for it from there.
1. Sillica for UnityTools
1. Icon for UnityLoader was very kindly provided by @noradninja on discord.
1. TheOfficialFlow for making our vita's hackable on newer firmwares
1. Team Molecule for Henkaku
1. @CTPEJIOK22 for giving me the idea on discord
1. @asdronin on discord for making the server
1. @iDevOnAParkingLot on Discord for the new layout.
1. Sony for the console.
1. Everyone who contributed to VitaSDK. 
1. Anyone else who helped out on the discord servers.

# Updating

Updating is simple *replace* the Uploader, Editor and Plugins inside the Plugins folder.

# Warnings:
1. If uma0: is mounted usb mode will corrupt the data in it. uma0: is usally used for usb's in a pstv and for psvsd. So be careful! You have been warned.

# FAQ:
**1. Can I use USB mode with udcv-uvc plugins?**

Ans: Yes! Install the vpk called UnityLoader_udcd, this will automatically unload and load udcd_uvc before and after the transfer. Do be warned this has bugs and I don't recommend you use it on a daily basis. *IMPORTANT* For this to work the plugin needs to be in ur0:tai or ux0:tai and it needs to be named udcd_uvc.skprx so if you use any of the oled or lcd_off plugins just rename them to udcd_uvc.skprx and make sure to change it in your config.txt as well.

# Notes:
1. Version 1 of the app only worked with vitashell's ftp making it useless and is no longer suppourted. Versions 2 and above of the app require the accompanying app for the vita to be installed. You can download the VPK from [here](https://github.com/Ibrahim778/Unity-Loader/releases).

1. A previous version of this app VitaFTPI worked only for some people so I have migrated it to .NET Core here.
