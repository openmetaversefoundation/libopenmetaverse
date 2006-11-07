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
        public string Name
        {
            get { return _Name; }
            set
            {
                _Name = value;
                base.iManager.FolderRename(this);
            }
        }


        private LLUUID _FolderID;
        public LLUUID FolderID
        {
            get { return _FolderID; }
        }

        private LLUUID _ParentID;
        public LLUUID ParentID
        {
            get { return _ParentID; }
            set
            {
                InventoryFolder ifParent = iManager.getFolder(this.ParentID);
                ifParent.alContents.Remove(this);

                ifParent = iManager.getFolder(value);
                ifParent.alContents.Add(this);

                this._ParentID = value;

                base.iManager.FolderMove(this, value);
            }
        }

        internal sbyte _Type;
        public sbyte Type
        {
            get { return _Type; }
        }

        public List<InventoryBase> alContents = new List<InventoryBase>();

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

        internal InventoryFolder(InventoryManager manager, Dictionary<string, string> htData)
            : base(manager)
        {
            this._Name = htData["name"];
            this._FolderID = new LLUUID(htData["folder_id"]);
            this._ParentID = new LLUUID(htData["parent_id"]);
            this._Type = sbyte.Parse(htData["type_default"].ToString());
        }


        public InventoryFolder CreateFolder(string name)
        {
            return base.iManager.FolderCreate(name, FolderID);
        }

        public void Delete()
        {
            iManager.getFolder(this.ParentID).alContents.Remove(this);
            iManager.FolderRemove(this);
        }

        public void MoveTo(InventoryFolder newParent)
        {
            MoveTo(newParent.FolderID);
        }

        public void MoveTo(LLUUID newParentID)
        {
            this.ParentID = newParentID;
        }

        public InventoryNotecard NewNotecard(string name, string description, string body)
        {
            return base.iManager.NewNotecard(name, description, body, this.FolderID);
        }

        public InventoryImage NewImage(string name, string description, byte[] j2cdata)
        {
            return base.iManager.NewImage(name, description, j2cdata, this.FolderID);
        }

        public List<InventoryBase> GetItemByName(string name)
        {
            List<InventoryBase> items = new List<InventoryBase>();
            foreach (InventoryBase ib in alContents)
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

            foreach (Object oContent in alContents)
            {
                output += ((InventoryBase)oContent).toXML(outputAssets);
            }

            output += "</folder>\n";

            return output;
        }

    }
}
