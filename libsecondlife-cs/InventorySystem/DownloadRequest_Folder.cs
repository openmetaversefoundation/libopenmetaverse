using System;
using System.Collections.Generic;
using System.Threading;

namespace libsecondlife.InventorySystem
{
    public class DownloadRequest_Folder
    {
        public string Name;
        public LLUUID FolderID;

        public int Expected = int.MaxValue;
        public int Received = 0;
        public int LastReceivedAtTick = 0;

        public bool FetchFolders = true;
        public bool FetchItems = true;

        public bool IsCompleted
        {
            get { return (Received >= Expected);}
        }

        /// <summary>
        /// Do we want to recursively download this folder?
        /// </summary>
        public bool Recurse = true;

        public ManualResetEvent RequestComplete = new ManualResetEvent(false);

        internal DownloadRequest_Folder(LLUUID folderID, bool recurse, bool fetchFolders, bool fetchItems, string requestName)
        {
            FolderID = folderID;
            Recurse = recurse;
            FetchFolders = fetchFolders;
            FetchItems = fetchItems;
            LastReceivedAtTick = Environment.TickCount;
            Name = requestName;
        }

        public override string ToString()
        {
            // return FolderID.ToStringHyphenated() + " [Pg:" + Received + "/" + Expected + "](R:" + Recurse + ",F:" + FetchFolders + ",I:" + FetchItems + ")" + Name;
            return " [Pg:" + Received + "/" + Expected + "](R:" + Recurse + ",F:" + FetchFolders + ",I:" + FetchItems + ")\t" + Name;
        }

        public override bool Equals(object obj)
        {
            if (obj is DownloadRequest_Folder)
            {
                DownloadRequest_Folder df = (DownloadRequest_Folder)obj;

                if ((this.FolderID == df.FolderID)
                    && (this.Recurse == df.Recurse)
                    && (this.FetchFolders == df.FetchFolders)
                    && (this.FetchItems == df.FetchItems)
                    && (this.Received == df.Received)
                    && (this.Expected == df.Expected)
                    )
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return base.Equals(obj);
            }
        }

        public override int GetHashCode()
        {
            string Compound = FolderID.ToString() + Recurse + Received + FetchFolders + Expected + FetchItems;
            return Compound.GetHashCode();
        }
    }

    public class DownloadRequest_EventArgs : EventArgs
    {
        public DownloadRequest_Folder DownloadRequest;
    }
}
