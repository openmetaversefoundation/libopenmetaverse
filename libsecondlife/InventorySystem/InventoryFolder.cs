using System.Collections.Generic;
using System;
using libsecondlife;

namespace libsecondlife.InventorySystem
{
    /// <summary>
    /// Summary description for InventoryFolder.
    /// </summary>
    public class InventoryFolder : InventoryBase
    {
        public enum FolderUpdateFlag { None, NoRecurse, Recurse };

        public string Name
        {
            get { return _Name; }
        }


        private LLUUID _FolderID;
        public LLUUID FolderID
        {
            get { return _FolderID; }
        }

        internal LLUUID _ParentID;
        public LLUUID ParentID
        {
            get { return _ParentID; }
        }

        internal sbyte _Type;
        public sbyte Type
        {
            get { return _Type; }
        }

        internal List<InventoryBase> _Contents = new List<InventoryBase>();

        #region Constructors
        internal InventoryFolder(InventoryManager manager)
            : base(manager)
        {
            _Name = "";
            _FolderID = LLUUID.Zero;
            _ParentID = LLUUID.Zero;
            _Type = -1;
        }

        internal InventoryFolder(InventoryManager manager, String name, LLUUID folderID, LLUUID parentID)
            : base(manager)
        {
            this._Name = name;
            this._FolderID = folderID;
            this._ParentID = parentID;
            this._Type = 0;
        }

        internal InventoryFolder(InventoryManager manager, String name, LLUUID folderID, LLUUID parentID, sbyte Type)
            : base(manager)
        {
            this._Name = name;
            this._FolderID = folderID;
            this._ParentID = parentID;
            this._Type = Type;
        }
        #endregion

        /// <summary>
        /// Get the contents of this folder
        /// </summary>
        /// <returns>Contents of this folder</returns>
        public List<InventoryBase> GetContents()
        {
            return _Contents;
        }

        /// <summary>
        /// Request a download of this folder's content information.
        /// </summary>
        /// <param name="recurse">Indicate if we should recursively download content information.</param>
        /// <param name="folders">Indicate if folders data should be downloaded</param>
        /// <param name="items">Indicate if item data should be downloaded</param>
        /// <returns>The Request object for this download</returns>
        public DownloadRequest_Folder RequestDownloadContents(bool recurse, bool folders, bool items)
        {
            return RequestDownloadContents(recurse, folders, items, 0);
        }

        [Obsolete("Clearing is no longer an option when requesting a download, you should use another version of this method", false)]
        public DownloadRequest_Folder RequestDownloadContents(bool recurse, bool folders, bool items, bool clear)
        {
            return RequestDownloadContents(recurse, folders, items, 0);
        }


        /// <summary>
        /// Request a download of this folder's content information.  Block until done, or timeout is reached
        /// </summary>
        /// <param name="recurse">Indicate if we should recursively download content information.</param>
        /// <param name="folders">Indicate if sub-folder data should be downloaded (true)</param>
        /// <param name="items">Indicate if item data should be downloaded too (true)</param>
        /// <param name="timeout">Milliseconds to wait before timing out, or -1 to wait indefinately.</param>
        /// <returns>The Request object for this download</returns>
        public DownloadRequest_Folder RequestDownloadContents(bool recurse, bool folders, bool items, int timeout)
        {
            DownloadRequest_Folder dr = iManager.FolderRequestAppend(FolderID, recurse, true, items, Name);
            dr.RequestComplete.WaitOne(timeout, false);
            return dr;
        }

        [Obsolete("Clearing is no longer an option when requesting a download, you should use another version of this method", false)]
        public DownloadRequest_Folder RequestDownloadContents(bool recurse, bool folders, bool items, bool clear, int timeout)
        {
            return RequestDownloadContents(recurse, folders, items, timeout);
        }

        /// <summary>
        /// Request that a sub-folder be created
        /// </summary>
        /// <param name="name">Name of folder</param>
        /// <returns>A reference to the folder, or null if it fails</returns>
        public InventoryFolder CreateFolder(string name)
        {
            // Request folder creation
            LLUUID requestedFolderUUID = iManager.FolderCreate(name, FolderID);

            // Refresh child folders, to find created folder.
            if (RequestDownloadContents(false, true, false).RequestComplete.WaitOne(30000, false) == false)
            {
                // Should probably note the timeout somewhere...
            }

            foreach (InventoryBase ib in GetContents())
            {
                if (ib is InventoryFolder)
                {
                    InventoryFolder iFolder = (InventoryFolder)ib;
                    if (iFolder.FolderID == requestedFolderUUID)
                    {
                        return iFolder;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Request this folder be deleted
        /// </summary>
        /// <remarks>You should re-request the parent folder's contents.</remarks>
        public void Delete()
        {
            iManager.getFolder(this.ParentID)._Contents.Remove(this);
            iManager.FolderRemove(this);
        }

        public void MoveTo(InventoryFolder newParent)
        {
            MoveTo(newParent.FolderID);
        }

        public void MoveTo(LLUUID newParentID)
        {
            iManager.FolderMove(this, newParentID);
        }

        public InventoryNotecard NewNotecard(string name, string description, string body)
        {
            return iManager.NewNotecard(name, description, body, this.FolderID);
        }
		public InventoryLandmark NewLandmark(string name, string description)
        {
            return iManager.NewLandmark(name, description, this.FolderID);
        }
        public InventoryImage NewImage(string name, string description, byte[] j2cdata)
        {
            return iManager.NewImage(name, description, j2cdata, this.FolderID);
        }

        public List<InventoryBase> GetItemByName(string name)
        {
            List<InventoryBase> items = new List<InventoryBase>();
            foreach (InventoryBase ib in _Contents)
            {
                if (ib is InventoryFolder)
                {
                    items.AddRange(((InventoryFolder)ib).GetItemByName(name));
                }
                else if (ib is InventoryItem)
                {
                    if (((InventoryItem)ib).Name.Equals(name))
                    {
                        items.Add(ib);
                    }
                }
            }

            return items;
        }

        public override string GetDisplayType()
        {
            return "Folder";
        }

        /// <summary>
        /// Output this folder as XML
        /// </summary>
        /// <param name="outputAssets">Include an asset data as well, TRUE/FALSE</param>
        override public string toXML(bool outputAssets)
        {
            string output = "<folder ";

            output += "name = '" + xmlSafe(Name) + "' ";
            output += "uuid = '" + FolderID + "' ";
            output += "parent = '" + ParentID + "' ";
            output += "Type = '" + Type + "' ";
            output += ">\n";

            foreach (Object oContent in _Contents)
            {
                output += ((InventoryBase)oContent).toXML(outputAssets);
            }

            output += "</folder>\n";

            return output;
        }
    }
}
