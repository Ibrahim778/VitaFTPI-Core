public class UploadData
{
    //NO NEED TO CHANGE ANY OF THIS ANYMORE USE THE EDITOR WINDOW IN VitaFTPI/Options


    public string IP = "192.168.1.1";

    //No real need to change this.
    public string File_Name = "Build";

    //Only use this when UseUSB in set to true. This will transfer the VPK over usb but still install it via ftp so the ftpanywhere plugin is required.
    public string DriveLetter = "D:";

    // Transfer the VPK via USB instead of ftp.
    public bool UseUSB = false;

    //If UseUSB = true you need to set this to your storage (Memory card) type (sd2vita or OFFICIAL) if useUSB is false then you can ignore this.
    public string storageType = "OFFICIAL";

    public bool startOnBuildEnd = false;

    public bool CustomUploaderFolder;

    public string UploaderFolder;

    public bool KeepFolderAfterBuild;

    public bool ExtractOnPC = true;
}