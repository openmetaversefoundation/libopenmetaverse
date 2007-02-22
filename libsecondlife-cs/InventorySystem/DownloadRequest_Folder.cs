using System;
using System.Collections.Generic;
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

        public Queue<DownloadRequest_Folder> ChildFolders = new Queue<DownloadRequest_Folder>();
        public DownloadRequest_Folder ParentRequest = null;
        public bool WaitOnChildren = true;

        /// <summary>
        /// Do we want to recursively download this folder?
        /// </summary>
        public bool Recurse = true;

        public ManualResetEvent RequestComplete = new ManualResetEvent(false);

        public DownloadRequest_Folder(LLUUID folderID, bool recurse, bool fetchFolders, bool fetchItems)
        {
            FolderID = folderID;
            Recurse = recurse;
            FetchFolders = fetchFolders;
            FetchItems = fetchItems;
            LastReceivedAtTick = Environment.TickCount;
        }

        /// <summary>
        /// Used to track the download of a folder
        /// </summary>
        /// <param name="folderID"></param>
        /// <param name="recurse"></param>
        /// <param name="fetchFolders"></param>
        /// <param name="fetchItems"></param>
        /// <param name="parent">The parent folder of this request</param>
        /// <param name="waitOnChildren">True to wait to signal completed, until the child queue is empty</param>
        public DownloadRequest_Folder(LLUUID folderID, bool recurse, bool fetchFolders, bool fetchItems, DownloadRequest_Folder parent, bool waitOnChildren)
        {
            FolderID = folderID;
            Recurse = recurse;
            FetchFolders = fetchFolders;
            FetchItems = fetchItems;
            LastReceivedAtTick = Environment.TickCount;
            ParentRequest = parent;
            WaitOnChildren = waitOnChildren;
        }

    }

    public class DownloadRequest_EventArgs : EventArgs
    {
        public DownloadRequest_Folder DownloadRequest;
    }
}
