using System;
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
        static bool CompareReplace = false;
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
        static string configDir = "ux0:/data/UnityLoader";
        static string TempFileName = "tempFile";
        static string TitleID = "NULL";
        static MD5 Md5 = MD5.Create();
        static string[] configFiles = { "/EXTRACTED", "/CONFIG_READY", "/USB", "/sd2vita", "/OFFICIAL", "/RUNCOMPLETE", "/COPYING", "/INSTALL" };


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
                if (args[x] == "--vpk")
                {
                    VPKPath = args[x + 1];
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
                    if(Directory.Exists(args[x + 1]))
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
                    string givenID = args[x + 1];
                    MatchCollection mc =  Regex.Matches(givenID, "([A-Z][A-Z][A-Z][A-Z][0-9][0-9][0-9][0-9][0-9])");
                    if(mc.Count == 0)
                    {
                        Console.WriteLine("Error invalid TitleID passed!");
                        Thread.Sleep(5000);
                        return;
                    }
                    TitleID = mc[0].ToString();
                }
                if (args[x] == "--replace-install")
                {
                    Console.WriteLine("Setting install mode to replace install!");
                    installMode = InstallMode.EXTRACT_REPLACE;
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
                if(args[x] == "--compare")
                {
                    CompareReplace = true;
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
            ConfigureOptions();
            if(transferOptions.useUSB && (transferOptions.driveLetter == "" || transferOptions.driveLetter == null))
            {
                Console.WriteLine("Getting list of all drives...");
                transferOptions.InitialDrives = GetDriveLetters();
            }
            Console.WriteLine("Connecting to vita...");
            ftpSession.FileTransferProgress += new FileTransferProgressEventHandler(ProgressChanged);
            ftpSession.Open(sessionOptions);
            Console.WriteLine("Connected!");
            Console.WriteLine("Making temporary file...");
            File.WriteAllText(TempFileName, "Just a temporary file!");
            Console.WriteLine("Deleting old config files...");
            foreach (string file in configFiles)
                if (ftpSession.FileExists(configDir + file)) ftpSession.RemoveFile(configDir + file);
            Console.WriteLine("Done!");
            Console.WriteLine("Creating config...");
            ftpCreateFile(configDir + "/CONFIG_READY");

            //We tell unityLoader to start usb when it launches
            if (transferOptions.useUSB) LoadUSB();

            if (installMode == InstallMode.EXTRACT_PC_PROMOTE_VITA || installMode == InstallMode.EXTRACT_REPLACE)
                if(!preExtracted) ExtractVPK();

            if (TitleID == "NULL") TitleID = GetTitleID();

            if (installMode == InstallMode.EXTRACT_PC_PROMOTE_VITA || installMode == InstallMode.PROMOTE_EXTRACT_VITA)
                ftpCreateFile(configDir + "/INSTALL");

            if (installMode == InstallMode.EXTRACT_PC_PROMOTE_VITA)
                ftpCreateFile(configDir + "/EXTRACTED");

            if(transferOptions.useUSB)
            {
                launchApp("UNITYLOAD");
                if(transferOptions.driveLetter == "" || transferOptions.driveLetter == null)
                {
                    while (Enumerable.SequenceEqual(transferOptions.InitialDrives, GetDriveLetters()))
                        Thread.Sleep(1);
                    transferOptions.driveLetter = GetNewDriveLetter();
                    if(transferOptions.driveLetter == "" || transferOptions.driveLetter == null)
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
                    goto EXT_REP;

                if (installMode == InstallMode.EXTRACT_PC_PROMOTE_VITA)
                    CopyAll(new DirectoryInfo(ExtractPath), new DirectoryInfo(transferOptions.driveLetter + pkgTempFolder));
                
                
                if(installMode == InstallMode.PROMOTE_EXTRACT_VITA)
                    File.Copy(VPKPath, transferOptions.driveLetter + "/data/sent.vpk", true);
                
                Thread.Sleep(100);
                ftpSession.RemoveFile(configDir + "/COPYING");
                goto EXIT;
            }
            else
            {
                if (installMode == InstallMode.EXTRACT_REPLACE)
                    goto EXT_REP;

                if (installMode == InstallMode.EXTRACT_PC_PROMOTE_VITA)
                    ftpUploadDirectory(ExtractPath, "ux0:" + pkgTempFolder);
                
                if(installMode == InstallMode.PROMOTE_EXTRACT_VITA)
                    ftpUploadFile(VPKPath, SendPath);

                launchApp("UNITYLOAD");
                goto EXIT;
            }

        EXT_REP:
            if (transferOptions.useUSB)
            {
                if (CompareReplace)
                {
                    CopyDifferentFiles(new DirectoryInfo(ExtractPath), new DirectoryInfo(transferOptions.driveLetter + "/app/" + TitleID));
                }
                else
                {
                    Directory.Delete(transferOptions.driveLetter + "/app/" + TitleID, true);
                    CopyAll(new DirectoryInfo(ExtractPath), new DirectoryInfo(transferOptions.driveLetter + "/app/" + TitleID));
                }
                ftpSession.RemoveFile(configDir + "/COPYING");
                Thread.Sleep(100);
            }
            else
            {
                ftpSession.RemoveFiles("ux0:app/" + TitleID);
                ftpUploadDirectory(ExtractPath, "ux0:app/" + TitleID);
            }
            Thread.Sleep(200);
            for(int i = 0; i < 5; i++)
                launchApp(TitleID);
            goto EXIT;


        EXIT:
            Console.WriteLine("Closing connection...");
            ftpSession.Close();
            File.Delete(TempFileName);
            Console.WriteLine($"Exiting in {0} seconds", exitTime);
            Thread.Sleep(exitTime * 1000);
            Environment.Exit(0);
        }

        static void CopyDifferentFiles(DirectoryInfo directory1, DirectoryInfo directory2)
        {
            foreach(DirectoryInfo directoryInfo in directory1.GetDirectories())
            {
                if (!Directory.Exists(Path.Combine(directory2.FullName, directoryInfo.Name)))
                    CopyAll(directoryInfo, new DirectoryInfo(Path.Combine(directory2.FullName, directoryInfo.Name)));

                else
                {
                    CopyDifferentFiles(directoryInfo, new DirectoryInfo(Path.Combine(directory2.FullName, directoryInfo.Name)));
                }
            }
            foreach(FileInfo file in directory1.GetFiles())
            {
                if(!File.Exists(Path.Combine(directory2.FullName, file.Name)))
                {
                    Console.WriteLine("Copying File " + file.Name);
                    file.CopyTo(Path.Combine(directory2.FullName, file.Name));
                }
                else
                {
                    if (file.Length.Equals(new FileInfo(Path.Combine(directory2.FullName, file.Name)).Length))
                    {
                        if (!UnsafeCompare(GetHashSha256(file.FullName), GetHashSha256(Path.Combine(directory2.FullName, file.Name))))
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

        // Copyright (c) 2008-2013 Hafthor Stefansson
        // Distributed under the MIT/X11 software license
        // Ref: http://www.opensource.org/licenses/mit-license.php.
        static unsafe bool UnsafeCompare(byte[] a1, byte[] a2)
        {
            if (a1 == a2) return true;
            if (a1 == null || a2 == null || a1.Length != a2.Length)
                return false;
            fixed (byte* p1 = a1, p2 = a2)
            {
                byte* x1 = p1, x2 = p2;
                int l = a1.Length;
                for (int i = 0; i < l / 8; i++, x1 += 8, x2 += 8)
                    if (*((long*)x1) != *((long*)x2)) return false;
                if ((l & 4) != 0) { if (*((int*)x1) != *((int*)x2)) return false; x1 += 4; x2 += 4; }
                if ((l & 2) != 0) { if (*((short*)x1) != *((short*)x2)) return false; x1 += 2; x2 += 2; }
                if ((l & 1) != 0) if (*((byte*)x1) != *((byte*)x2)) return false;
                return true;
            }
        }

        static byte[] GetHashSha256(string filename)
        {
            using (var bstream = new BufferedStream(File.OpenRead(filename), 100))
            {
                using (FileStream stream = File.OpenRead(filename))
                {
                    return Md5.ComputeHash(stream);
                }
            }
        }

        static string GetTitleID()
        {
            string Hex = File.ReadAllText(ExtractPath + "/sce_sys/param.sfo");
            Match titleid = Regex.Match(Hex, "([A-Z][A-Z][A-Z][A-Z][0-9][0-9][0-9][0-9][0-9])");
            return titleid.Value;
        }

        static string[] GetDriveLetters()
        {
            List<string> driveNames = new List<string>();
            foreach (DriveInfo drive in DriveInfo.GetDrives()) driveNames.Add(drive.Name);
            return driveNames.ToArray();
        }

        static string GetNewDriveLetter()
        {
            List<string> CurrentDrives = new List<string>();

            foreach (DriveInfo drive in DriveInfo.GetDrives()) CurrentDrives.Add(drive.Name);

            foreach (string drive in transferOptions.InitialDrives) CurrentDrives.Remove(drive);

            return CurrentDrives[0].Remove(CurrentDrives[0].Length - 1);
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
            if(!ftpSession.Opened)
            {
                Console.WriteLine("No FTP session opened, please open one first!");
                return;
            }
            if (storageType == StorageType.sd2vita)
                ftpCreateFile(configDir + "/sd2vita");
            if (storageType == StorageType.OFFICIAL)
                ftpCreateFile(configDir + "/OFFICIAL");
            ftpCreateFile(configDir + "/USB");
            ftpCreateFile(configDir + "/COPYING");
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
