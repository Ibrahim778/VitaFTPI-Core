# What is it?
This is a tool (or a collection of them) that makes Unity Development (or development in general) on the vita much easier.  
Other tools used (included) are: Silica's Unity tools and my remake of NetDbgLogPc by @Princess-of-Sleeping.

# How to use
## Installing (Unity): 
1. Download and extract the Unity.zip folder from the releases tab.
1. Run the Installer.bat file.
1. Install the unitypackage.
1. Configure settings via the menu item VitaFTPI>Options.
1. Install the accompanying plugins for this app from [here](https://github.com/Ibrahim778/vitacompanion) OR you can use LoaderCompanionLoader to load the plugins manually if you do not want to install plugins from [here](https://github.com/Ibrahim778/LoaderCompanionLoader)
1. Done!!

## Using the Debug feature (Unity): 
This is not a full blown debug implimentation in unity. This will only allow you to read the Debug.Log's called in your app running in the vita inside unity like you do on PC.
For this to work you need to setup PrincessLog on your vita, it's a tool made by @The-Princess-of-Sleeping. You can download the files from [here](https://github.com/CelesteBlue-dev/PSVita-RE-tools/tree/master/PrincessLog/build), Install the skprx under kernel in your taihen config and Install the vpk and enter your PC's IP leave the port at default and press save config then reboot your vita. Do note that when you press start debug a new window will open. This is normal and you can ignore it but *DO NOT CLOSE IT*.

Then to call the logs from within unity you need to copy the Plugins folder from the release into your assets folder. Then you can use the sample log script provided and copy it into your project or you can make your own. If you use the sample script it will automatically override your debug.logs depending on the platform so there is no need to worry.

Do remember this uses the official sdk for printing it to the logs. So do remember to remove the plugins and debug script when compiling the final release.

# Known Issues:
_**Nothing to see here**_

# Credits: 
1. Jordi at jllopisol@gmail.com for making the inspiration for this program
1. __*HUGE*__ thanks to Silica, Princess of Sleeping, Sarcastic Cat, たまご, TheFlow, DevNoName120, Rinnegatamante, s1ngyy and anyone else who helped for answering all of my questions.
1. WinSCP
1. @The-Princess-of-Sleeping for the original NetDbLogPc and PrincessLog.
1. Teakhanirons for the USB suppourt from EmergencyMount I stole the code for it from there.
1. Sillica for UnityTools
1. Icon for UnityLoader was very kindly provided by @noradninja on discord (sadly no longer needed).
1. TheOfficialFlow for making our vita's hackable on newer firmwares
1. Team Molecule for Henkaku
1. @CTPEJIOK22 for giving me the idea on discord
1. @asdronin on discord for making the server
1. @iDevOnAParkingLot on Discord for the new layout.
1. Sony for the console.
1. Everyone who contributed to VitaSDK. 
1. Anyone else who helped out on the discord servers.

# Updating
## Unity
Updating is simple dowload the new unity.zip file and run the installer again and use the new unitpackage.

# Warnings:
1. If uma0: is mounted usb mode will corrupt the data in it. uma0: is usally used for usb's in a pstv and for psvsd. So be careful! You have been warned.

# FAQ:
**1. Can I use USB mode with udcd_uvc plugins?**

Ans: Yes! Select use udcd_uvc in unity and enter the path.

# Notes:
1. Version 1 of the app only worked with vitashell's ftp making it useless and is no longer suppourted. Versions 2 and above of the app require the accompanying app for the vita to be installed. You can download the VPK from [here](https://github.com/Ibrahim778/Unity-Loader/releases) V4 or above need the accompanying plugin(s) to be installed from [here](https://github.com/Ibrahim778/vitacompanion).

1. A previous version of this app VitaFTPI worked only for some people so I have migrated it to .NET Core here.
