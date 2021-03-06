﻿using System;
using System.Net.Sockets;
using System.IO;
using WinSCP;
using System.Net;
using System.Linq;
using System.Collections.Generic;
using System.IO.Compression;
using System.Text.RegularExpressions;
using System.Threading;
using System.Security.Cryptography;


enum StorageType
{
    Unconfigured,
    sd2vita,
    OFFICIAL
}

enum ReplaceInstallMethod
{
    Partial,
    Compare,
    Full
}

enum InstallMode
{
    PROMOTE_EXTRACT_VITA,
    EXTRACT_PC_PROMOTE_VITA,
    EXTRACT_REPLACE
}

struct GameTransferOptions
{
    public string driveLetter;
    public bool useUSB;
    public string[] InitialDrives;
}

struct FTPOptions
{
    public int CMD_PORT;
    public IPAddress IP;
    public int PORT;
}

namespace Vita_FTPI_Core
{
    class Program
    {
        static int exitTime = 1;
        static ProgressBar currentBar;
        static bool preExtracted = false;
        static int tries = 1;
        static bool udcd = false;
        static string udcdPath = "";
        static ReplaceInstallMethod replaceInstallMethod = ReplaceInstallMethod.Full;
        static InstallMode installMode = InstallMode.PROMOTE_EXTRACT_VITA;
        static StorageType storageType;
        static FTPOptions ftpOptions;
        static GameTransferOptions transferOptions;
        static string VPKPath = "";
        static string UploadFolder = "";
        static SessionOptions sessionOptions;
        static Session ftpSession = new Session();
        static string ExtractPath = "Extracted";
        static private string pkgTempFolder = "/temp/pkg";
        static string SendPath = "ux0:/data/sent.vpk";
        static string TempFileName = "tempFile";
        static string TitleID = "NULL";
        static SHA1 Sha1 = SHA1.Create();


        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("No input specified Aboring!");
                return;
            }

            //Setting all the arguments
            for (int x = 0; x < args.Length; x += 2)
            {
                if (args[x] == "--udcd")
                {
                    udcd = true;
                    udcdPath = args[x + 1];
                }
                if (args[x] == "--vpk")
                {
                    VPKPath = args[x + 1];
                }
                if (args[x] == "--tries")
                {
                    tries = int.Parse(args[x + 1]);
                }
                if (args[x] == "--ip")
                {
                    ftpOptions.IP = IPAddress.Parse(args[x + 1]);
                }
                if (args[x] == "--port")
                {
                    ftpOptions.PORT = int.Parse(args[x + 1]);
                }
                if (args[x] == "--command-port")
                {
                    ftpOptions.CMD_PORT = int.Parse(args[x + 1]);
                }
                if (args[x] == "--usb")
                {
                    transferOptions.useUSB = (args[x + 1] == "true");
                }
                if (args[x] == "--drive-letter")
                {
                    if (Regex.Matches(args[x + 1], "[A-Z]|[a-z]:").Count != 0)
                    {
                        transferOptions.driveLetter = args[x + 1];
                    }
                    else
                    {
                        Console.WriteLine("Incorrect drive letter given. Make sure it is just one letter and ends with a colon. For example: D:");
                        return;
                    }

                }
                if (args[x] == "--storage-type")
                {
                    storageType = parseStorageType(args[x + 1]);
                    if (storageType == StorageType.Unconfigured)
                    {
                        Console.WriteLine("Incorrect storage type given it should either be sd2vita or OFFICIAL remember it is case sensitive");
                        return;
                    }
                }
                if (args[x] == "--upload-dir")
                {
                    if (Directory.Exists(args[x + 1]))
                    {
                        UploadFolder = args[x + 1];
                    }
                    else
                    {
                        Console.WriteLine("Error uploader folder not found exiting....");
                        Console.WriteLine(UploadFolder);
                        Thread.Sleep(5000);
                        return;
                    }
                }
                if (args[x] == "--titleid")
                {
                    TitleID = args[x + 1];
                }
                if (args[x] == "--replace-install")
                {
                    Console.WriteLine("Setting install mode to replace install!");
                    installMode = InstallMode.EXTRACT_REPLACE;
                    replaceInstallMethod = ReplaceInstallMethod.Full;
                    x--;
                }
                if (args[x] == "--extract")
                {
                    Console.WriteLine("Setting mode to extract!");
                    installMode = InstallMode.EXTRACT_PC_PROMOTE_VITA;
                    x--;
                }
                if (args[x] == "--complete-vita-install")
                {
                    installMode = InstallMode.PROMOTE_EXTRACT_VITA;
                    x--;
                }
                if (args[x] == "--pre-extract")
                {
                    Console.WriteLine("Set extract path to: " + args[x + 1]);
                    preExtracted = true;
                    ExtractPath = args[x + 1];
                }
                if (args[x] == "--compare")
                {
                    replaceInstallMethod = ReplaceInstallMethod.Compare;
                    x--;
                }
                if (args[x] == "--partial")
                {
                    replaceInstallMethod = ReplaceInstallMethod.Partial;
                    x--;
                }
            }

            if (Directory.Exists("Uploader") && UploadFolder == "")
                Directory.SetCurrentDirectory("Uploader");
            else if (!(UploadFolder == "")) Directory.SetCurrentDirectory(UploadFolder);

            if (ftpOptions.IP == null || VPKPath == "")
            {
                Console.WriteLine("Invalid Arguments Aborting!");
                Thread.Sleep(5000);
                return;
            }

            if (!File.Exists(VPKPath) && !preExtracted)
            {
                //Checking if the input file specified exists
                Console.WriteLine("No file found. Check your input path and make sure to include the file extension.");
                Console.WriteLine(VPKPath);
                Thread.Sleep(5000);
                return;
            }

            if (ftpOptions.PORT == 0)
                ftpOptions.PORT = 1337;
            if (ftpOptions.CMD_PORT == 0)
                ftpOptions.CMD_PORT = 1338;

            closeAllApps();
            if (transferOptions.useUSB && (transferOptions.driveLetter == "" || transferOptions.driveLetter == null))
            {
                sendCommand("usb disable -");
                Thread.Sleep(100);
                Console.WriteLine("Getting list of all drives...");
                transferOptions.InitialDrives = Directory.GetLogicalDrives();
            }
            if (!transferOptions.useUSB)
            {
                ConfigureOptions();
                Console.WriteLine("Connecting to vita...");
                ftpSession.FileTransferProgress += new FileTransferProgressEventHandler(ProgressChanged);
                ftpSession.Open(sessionOptions);
                Console.WriteLine("Connected!");
            }
            if (installMode == InstallMode.EXTRACT_PC_PROMOTE_VITA || installMode == InstallMode.EXTRACT_REPLACE)
                if (!preExtracted) ExtractVPK();

            if (transferOptions.useUSB)
            {
                LoadUSB();
                if (transferOptions.driveLetter == "" || transferOptions.driveLetter == null)
                {
                    while (Enumerable.SequenceEqual(transferOptions.InitialDrives, Directory.GetLogicalDrives()) && Directory.GetLogicalDrives().Length <= transferOptions.InitialDrives.Length)
                        Thread.Sleep(1);
                    transferOptions.driveLetter = GetNewDriveLetter();
                    if (transferOptions.driveLetter == "" || transferOptions.driveLetter == null)
                    {
                        Console.WriteLine("Error new drive not found! Enter one manually! For example: E:");
                        transferOptions.driveLetter = Console.ReadLine();
                    }
                }
                else
                {
                    while (!Directory.Exists(transferOptions.driveLetter + "\\data"))
                        Thread.Sleep(1);
                }

                if (installMode == InstallMode.EXTRACT_REPLACE)
                {
                    if (Directory.Exists(transferOptions.driveLetter + "/app/" + TitleID))
                        goto EXT_REP;
                    else
                    {
                        Console.WriteLine("Error app not previously installed changing to install mode!\n");
                        installMode = InstallMode.EXTRACT_PC_PROMOTE_VITA;
                    }
                }

                if (installMode == InstallMode.EXTRACT_PC_PROMOTE_VITA)
                {
                    CopyAll(new DirectoryInfo(ExtractPath), new DirectoryInfo(transferOptions.driveLetter + pkgTempFolder));
                    disableUSB();
                    sendCommand("ext_vpk ux0:" + pkgTempFolder);
                    if (TitleID != "NULL")
                        launchApp(TitleID);
                }

                if (installMode == InstallMode.PROMOTE_EXTRACT_VITA)
                {
                    File.Copy(VPKPath, transferOptions.driveLetter + "/data/sent.vpk", true);
                    disableUSB();
                    sendCommand("vpk " + SendPath);
                    if (TitleID != "NULL")
                        launchApp(TitleID);
                }
                Thread.Sleep(100);
                goto EXIT;
            }
            else
            {
                if (installMode == InstallMode.EXTRACT_REPLACE)
                {
                    if (ftpSession.FileExists("ux0:/app/" + TitleID))
                        goto EXT_REP;
                    else
                    {
                        Console.WriteLine("Error app not previously installed changing to install mode!\n");
                        installMode = InstallMode.EXTRACT_PC_PROMOTE_VITA;
                    }
                }

                if (installMode == InstallMode.EXTRACT_PC_PROMOTE_VITA)
                {
                    ftpUploadDirectory(ExtractPath, "ux0:" + pkgTempFolder);
                    sendCommand("ext_vpk ux0:" + pkgTempFolder);
                    if (TitleID != "NULL")
                        launchApp(TitleID);
                }
                if (installMode == InstallMode.PROMOTE_EXTRACT_VITA)
                {
                    ftpUploadFile(VPKPath, SendPath);
                    sendCommand("vpk " + SendPath);
                }
            }

        EXT_REP:
            if (transferOptions.useUSB)
            {
                if (replaceInstallMethod.Equals(ReplaceInstallMethod.Partial))
                {
                    if (Directory.Exists(ExtractPath + "/Media"))
                    {
                        if(Directory.Exists(transferOptions.driveLetter + "/app/" + TitleID + "/Media"))
                            Directory.Delete(transferOptions.driveLetter + "/app/" + TitleID + "/Media", true);
                        CopyAll(new DirectoryInfo(ExtractPath + "/Media"), new DirectoryInfo(transferOptions.driveLetter + "/app/" + TitleID + "/Media"));
                    }
                    else
                    {
                        Console.WriteLine("This is not a unity app the partial install method is only suppourted with unity apps switching to compare");
                        replaceInstallMethod = ReplaceInstallMethod.Compare;
                    }
                }
                if (replaceInstallMethod.Equals(ReplaceInstallMethod.Compare))
                {
                    CopyDifferentFiles(new DirectoryInfo(ExtractPath), new DirectoryInfo(transferOptions.driveLetter + "/app/" + TitleID));
                }
                if (replaceInstallMethod.Equals(ReplaceInstallMethod.Full))
                {
                    Directory.Delete(transferOptions.driveLetter + "/app/" + TitleID, true);
                    CopyAll(new DirectoryInfo(ExtractPath), new DirectoryInfo(transferOptions.driveLetter + "/app/" + TitleID));
                }
                disableUSB();
                Thread.Sleep(100);
            }
            else
            {
                ftpSession.RemoveFiles("ux0:app/" + TitleID);
                ftpUploadDirectory(ExtractPath, "ux0:app/" + TitleID);
            }
            Thread.Sleep(200);
            for (int i = 0; i < tries; i++)
                if(TitleID != "NULL")
                    launchApp(TitleID);
            goto EXIT;


        EXIT:
            if (!transferOptions.useUSB)
            {
                Console.WriteLine("Closing connection...");
                ftpSession.Close();
            }
            Console.WriteLine($"Exiting in {0} seconds", exitTime);
            Thread.Sleep(exitTime * 1000);
            Environment.Exit(0);
        }

        static void disableUSB()
        {
            sendCommand("usb disable -");
            if (udcd) sendCommand("skprx " + udcdPath);
        }

        static void CopyDifferentFiles(DirectoryInfo directory1, DirectoryInfo directory2)
        {
            foreach (DirectoryInfo directoryInfo in directory1.GetDirectories())
            {
                if (!Directory.Exists(Path.Combine(directory2.FullName, directoryInfo.Name)))
                    CopyAll(directoryInfo, new DirectoryInfo(Path.Combine(directory2.FullName, directoryInfo.Name)));

                else
                {
                    CopyDifferentFiles(directoryInfo, new DirectoryInfo(Path.Combine(directory2.FullName, directoryInfo.Name)));
                }
            }
            foreach (FileInfo file in directory1.GetFiles())
            {
                if (!File.Exists(Path.Combine(directory2.FullName, file.Name)))
                {
                    Console.WriteLine("Copying File " + file.Name);
                    file.CopyTo(Path.Combine(directory2.FullName, file.Name));
                }
                else
                {
                    if (file.Length.Equals(new FileInfo(Path.Combine(directory2.FullName, file.Name)).Length))
                    {
                        if (!Enumerable.SequenceEqual(GetHashMD5(file.FullName), GetHashMD5(Path.Combine(directory2.FullName, file.Name))))
                        {
                            Console.WriteLine("Copying file " + file.Name);
                            file.CopyTo(Path.Combine(directory2.FullName, file.Name), true);
                        }
                        else
                        {
                            Console.WriteLine(file.Name + " is the same");
                        }
                    }
                    else
                    {
                        file.CopyTo(Path.Combine(directory2.FullName, file.Name), true);
                    }
                }
            }
        }

        static byte[] GetHashMD5(string filename)
        {
            using (var bstream = new BufferedStream(File.OpenRead(filename), 100))
            {
                using (FileStream stream = File.OpenRead(filename))
                {
                    return Sha1.ComputeHash(stream);
                }
            }
        }

        static string GetNewDriveLetter()
        {
            string driveLetter = "";

            foreach (string drive in Directory.GetLogicalDrives())
                if (Directory.Exists(drive + "app") && Directory.Exists(drive + "appmeta") && Directory.Exists(drive + "data") && Directory.Exists(drive + "cache") && Directory.Exists(drive + "calendar"))
                {
                    driveLetter = drive;
                    break;
                }

            return driveLetter;
        }

        static void ExtractVPK()
        {
            Console.WriteLine("Extracting VPK...");
            if (Directory.Exists(ExtractPath))
                Directory.Delete(ExtractPath, true);

            Directory.CreateDirectory(ExtractPath);

            ZipFile.ExtractToDirectory(VPKPath, ExtractPath);
        }

        static void LoadUSB()
        {
            Console.WriteLine("Loading USB");
            sendCommand("usb enable " + storageType.ToString());
        }

        static void ftpCreateFile(string remotePath)
        {
            if(!ftpSession.Opened)
            {
                Console.WriteLine("No FTP session opened, please open one first!");
                return;
            }
            if (!File.Exists(TempFileName))
                File.WriteAllText(TempFileName, "");
            currentBar = new ProgressBar();
            currentBar.Report(0);
            TransferOperationResult transferResult = ftpSession.PutFiles(TempFileName, remotePath);
            transferResult.Check();
            foreach (FileOperationEventArgs transfer in transferResult.Transfers)
                Console.WriteLine("Creation of file at location:  " + remotePath + " was successful!");
            currentBar.Dispose();
        }

        static void ftpUploadFile(string localPath, string remotePath)
        {
            if (!ftpSession.Opened)
            {
                Console.WriteLine("No FTP session opened, please open one first!");
                return;
            }
            if (!File.Exists(TempFileName))
                File.WriteAllText(TempFileName, "Just a temporary file!");

            currentBar = new ProgressBar();
            currentBar.Report(0);

            TransferOperationResult transferResult = ftpSession.PutFiles(localPath, remotePath);
            transferResult.Check();
            foreach(FileOperationEventArgs transfer in transferResult.Transfers)
                Console.WriteLine("Transfer of " + transfer.FileName + " was successful");
        }

        static void ftpUploadDirectory(string localPath, string remotePath)
        {
            if (!ftpSession.Opened)
            {
                Console.WriteLine("No FTP session opened, please open one first!");
                return;
            }
            if (!File.Exists(TempFileName))
                File.WriteAllText(TempFileName, "Just a temporary file!");

            currentBar = new ProgressBar();
            currentBar.Report(0);

            TransferOperationResult transferResult = ftpSession.PutFilesToDirectory(localPath, remotePath);
            transferResult.Check();
            foreach (FileOperationEventArgs transfer in transferResult.Transfers)
                Console.WriteLine("Transfer of " + transfer.FileName + " Successful!");
        }

        static StorageType parseStorageType(string st)
        {
            if (st == "OFFICIAL")
                return StorageType.OFFICIAL;

            if (st == "sd2vita")
                return StorageType.sd2vita;

            return StorageType.Unconfigured;
        }

        static void closeAllApps()
        {
            Console.WriteLine("Closing all apps...");
            using (TcpClient client = new TcpClient(ftpOptions.IP.ToString(), ftpOptions.CMD_PORT))
            {
                using (NetworkStream ns = client.GetStream())
                {
                    using (StreamWriter sw = new StreamWriter(ns))
                    {
                        sw.Write("destroy\n");
                        sw.Flush();
                        using (StreamReader sr = new StreamReader(ns))
                        {
                            Console.Write(sr.ReadToEnd());
                            sr.Close();
                        }
                        sw.Close();
                    }
                    ns.Close();
                }
                client.Close();
            }
        }

        static void launchApp(string titleID)
        {
            Console.WriteLine("Launching " + titleID + " on vita...");
            using (TcpClient client = new TcpClient(ftpOptions.IP.ToString(), ftpOptions.CMD_PORT))
            {
                using (NetworkStream ns = client.GetStream())
                {
                    using (StreamWriter sw = new StreamWriter(ns))
                    {
                        sw.Write("launch " + titleID + "\n");
                        sw.Flush();
                        using (StreamReader sr = new StreamReader(ns))
                        {
                            Console.Write(sr.ReadToEnd());
                            sr.Close();
                        }
                        sw.Close();
                    }
                    ns.Close();
                }
                client.Close();
            }
        }

        static void sendCommand(string cmd)
        {
            Console.WriteLine("Sending " + cmd + " to vita...");
            using (TcpClient client = new TcpClient(ftpOptions.IP.ToString(), ftpOptions.CMD_PORT))
            {
                using (NetworkStream ns = client.GetStream())
                {
                    using (StreamWriter sw = new StreamWriter(ns))
                    {
                        sw.Write(cmd + "\n");
                        sw.Flush();
                        using (StreamReader sr = new StreamReader(ns))
                        {
                            Console.Write(sr.ReadToEnd());
                            sr.Close();
                        }
                        sw.Close();
                    }
                    ns.Close();
                }
                client.Close();
            }
        }

        public static void CopyAll(DirectoryInfo source, DirectoryInfo target)
        {
            Directory.CreateDirectory(target.FullName);
            foreach (FileInfo fi in source.GetFiles())
            {
                Console.WriteLine(@"Copying {0}\{1}", target.FullName, fi.Name);
                fi.CopyTo(Path.Combine(target.FullName, fi.Name), true);
            }
            foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
            {
                DirectoryInfo nextTargetSubDir = target.CreateSubdirectory(diSourceSubDir.Name);
                CopyAll(diSourceSubDir, nextTargetSubDir);
            }
        }

        static void ConfigureOptions()
        {
            Console.WriteLine("Configuring options.");
            //Configure the options for the FTP transfer
            sessionOptions = new SessionOptions
            {
                Protocol = Protocol.Ftp,
                HostName = ftpOptions.IP.ToString(),
                PortNumber = ftpOptions.PORT,
                UserName = "Anonymous",
                Password = ""
            };
        }

        static void ProgressChanged(object sender, FileTransferProgressEventArgs e)
        {
            currentBar.Report(e.FileProgress);
        }
    }
}
