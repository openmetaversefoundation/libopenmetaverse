using System;
using System.Threading;

namespace libsecondlife.InventorySystem
{
    public class DownloadRequest_Folder
    {
        public LLUUID FolderID;

        public int Expected = int.MaxValue;
        public int Received = 0;
        public int LastReceivedAtTick = 0;

        public bool FetchFolders = true;
        public bool FetchItems = true;

        /// <summary>
        /// Do we want to recursively download this folder?
        /// </summary>
        public bool Recurse = true;

        public ManualResetEvent RequestComplete = new ManualResetEvent(false);

        public DownloadRequest_Folder(LLUUID folderID)
        {
            FolderID = folderID;
            LastReceivedAtTick = Environment.TickCount;
        }

        public DownloadRequest_Folder(LLUUID folderID, bool recurse)
        {
            FolderID = folderID;
            LastReceivedAtTick = Environment.TickCount;
            Recurse = recurse;
        }

        public DownloadRequest_Folder(LLUUID folderID, bool fetchFolders, bool fetchItems)
        {
            FolderID = folderID;
            FetchFolders = fetchFolders;
            FetchItems = fetchItems;
            LastReceivedAtTick = Environment.TickCount;
        }
    }

    public class DownloadRequest_EventArgs : EventArgs
    {
        public DownloadRequest_Folder DownloadRequest;
    }
}
