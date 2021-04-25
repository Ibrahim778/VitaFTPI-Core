public class UploadWrapper
{
    public static string path = new System.Diagnostics.StackTrace(true).GetFrame(0).GetFileName();

    public class UploadData
    {
        //NO NEED TO CHANGE ANY OF THIS ANYMORE USE THE EDITOR WINDOW IN VitaFTPI/Options

        public string IP = "192.168.1.1";

        public string File_Name = "Build";

        public bool UseUSB = false;

	    public int storageIndex = 0;
        public string storageType = "OFFICIAL";

        public bool startOnBuildEnd = false;

        public bool CustomUploaderFolder;

        public string UploaderFolder;

        public bool KeepFolderAfterBuild;

        public bool ExtractOnPC = true;

        public bool useUDCD = false;

        public string udcdPath = "ux0:tai/udcd_uvc.skprx";
    }
}
