using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.ComponentModel;
using System.Xml;
using System.Xml.Serialization;
using OpenMetaverse;
using OpenMetaverse.Packets;
using OpenMetaverse.TestClient;

namespace OpenMetaverse.TestClient
{
    public class QueuedDownloadInfo
    {
        public UUID TransferID;
        public UUID AssetID;
        public UUID ItemID;
        public UUID TaskID;
        public UUID OwnerID;
        public AssetType Type;
        public string FileName;
        public DateTime WhenRequested;
        public bool IsRequested;

        public QueuedDownloadInfo(string file, UUID asset, UUID item, UUID task, UUID owner, AssetType type)
        {
            FileName = file;
            AssetID = asset;
            ItemID = item;
            TaskID = task;
            OwnerID = owner;
            Type = type;
            TransferID = UUID.Zero;
            WhenRequested = DateTime.Now;
            IsRequested = false;
        }
    }

    public class BackupCommand : Command
    {
        /// <summary>Maximum number of transfer requests to send to the server</summary>
        private const int MAX_TRANSFERS = 10;

        // all items here, fed by the inventory walking thread
        private Queue<QueuedDownloadInfo> PendingDownloads = new Queue<QueuedDownloadInfo>();

        // items sent to the server here
        private List<QueuedDownloadInfo> CurrentDownloads = new List<QueuedDownloadInfo>(MAX_TRANSFERS);

        // background workers
        private BackgroundWorker BackupWorker;
        private BackgroundWorker QueueWorker;

        // some stats
        private int TextItemsFound;
        private int TextItemsTransferred;
        private int TextItemErrors;

        #region Properties

        /// <summary>
        /// true if either of the background threads is running
        /// </summary>
        private bool BackgroundBackupRunning
        {
            get { return InventoryWalkerRunning || QueueRunnerRunning; }
        }

        /// <summary>
        /// true if the thread walking inventory is running
        /// </summary>
        private bool InventoryWalkerRunning
        {
            get { return BackupWorker != null; }
        }

        /// <summary>
        /// true if the thread feeding the queue to the server is running
        /// </summary>
        private bool QueueRunnerRunning
        {
            get { return QueueWorker != null; }
        }

        /// <summary>
        /// returns a string summarizing activity
        /// </summary>
        /// <returns></returns>
        private string BackgroundBackupStatus
        {
            get
            {
                StringBuilder sbResult = new StringBuilder();
                sbResult.AppendFormat("{0} is {1} running.", Name, BoolToNot(BackgroundBackupRunning));
                if (TextItemErrors != 0 || TextItemsFound != 0 || TextItemsTransferred != 0)
                {
                    sbResult.AppendFormat("\r\n{0} : Inventory walker ( {1} running ) has found {2} items.",
                                            Name, BoolToNot(InventoryWalkerRunning), TextItemsFound);
                    sbResult.AppendFormat("\r\n{0} : Server Transfers ( {1} running ) has transferred {2} items with {3} errors.",
                                            Name, BoolToNot(QueueRunnerRunning), TextItemsTransferred, TextItemErrors);
                    sbResult.AppendFormat("\r\n{0} : {1} items in Queue, {2} items requested from server.",
                                            Name, PendingDownloads.Count, CurrentDownloads.Count);
                }
                return sbResult.ToString();
            }
        }

        #endregion Properties

        public BackupCommand(TestClient testClient)
        {
            Name = "backuptext";
            Description = "Backup inventory to a folder on your hard drive. Usage: " + Name + " [to <directory>] | [abort] | [status]";
            testClient.Assets.OnAssetReceived += new AssetManager.AssetReceivedCallback(Assets_OnAssetReceived);
        }

        public override string Execute(string[] args, UUID fromAgentID)
        {

            if (args.Length == 1 && args[0] == "status")
            {
                return BackgroundBackupStatus;
            }
            else if (args.Length == 1 && args[0] == "abort")
            {
                if (!BackgroundBackupRunning)
                    return BackgroundBackupStatus;

                BackupWorker.CancelAsync();
                QueueWorker.CancelAsync();

                Thread.Sleep(500);

                // check status
                return BackgroundBackupStatus;
            }
            else if (args.Length != 2)
            {
                return "Usage: " + Name + " [to <directory>] | [abort] | [status]";
            }
            else if (BackgroundBackupRunning)
            {
                return BackgroundBackupStatus;
            }

            QueueWorker = new BackgroundWorker();
            QueueWorker.WorkerSupportsCancellation = true;
            QueueWorker.DoWork += new DoWorkEventHandler(bwQueueRunner_DoWork);
            QueueWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bwQueueRunner_RunWorkerCompleted);

            QueueWorker.RunWorkerAsync();

            BackupWorker = new BackgroundWorker();
            BackupWorker.WorkerSupportsCancellation = true;
            BackupWorker.DoWork += new DoWorkEventHandler(bwBackup_DoWork);
            BackupWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bwBackup_RunWorkerCompleted);

            BackupWorker.RunWorkerAsync(args);
            return "Started background operations.";
        }

        void bwQueueRunner_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            QueueWorker = null;
            Console.WriteLine(BackgroundBackupStatus);
        }

        void bwQueueRunner_DoWork(object sender, DoWorkEventArgs e)
        {
            TextItemErrors = TextItemsTransferred = 0;

            while (QueueWorker.CancellationPending == false)
            {
                // have any timed out?
                if (CurrentDownloads.Count > 0)
                {
                    foreach (QueuedDownloadInfo qdi in CurrentDownloads)
                    {
                        if ((qdi.WhenRequested + TimeSpan.FromSeconds(60)) < DateTime.Now)
                        {
                            Logger.DebugLog(Name + ": timeout on asset " + qdi.AssetID.ToString(), Client);
                            // submit request again
                            qdi.TransferID = Client.Assets.RequestInventoryAsset(
                                qdi.AssetID, qdi.ItemID, qdi.TaskID, qdi.OwnerID, qdi.Type, true);
                            qdi.WhenRequested = DateTime.Now;
                            qdi.IsRequested = true;
                        }
                    }
                }

                if (PendingDownloads.Count != 0)
                {
                    // room in the server queue?
                    if (CurrentDownloads.Count < MAX_TRANSFERS)
                    {
                        // yes
                        QueuedDownloadInfo qdi = PendingDownloads.Dequeue();
                        qdi.WhenRequested = DateTime.Now;
                        qdi.IsRequested = true;
                        qdi.TransferID = Client.Assets.RequestInventoryAsset(
                            qdi.AssetID, qdi.ItemID, qdi.TaskID, qdi.OwnerID, qdi.Type, true);

                        lock (CurrentDownloads) CurrentDownloads.Add(qdi);
                    }
                }

                if (CurrentDownloads.Count == 0 && PendingDownloads.Count == 0 && BackupWorker == null)
                {
                    Logger.DebugLog(Name + ": both transfer queues empty AND inventory walking thread is done", Client);
                    return;
                }

                Thread.Sleep(100);
            }
        }

        void bwBackup_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Console.WriteLine(Name + ": Inventory walking thread done.");
            BackupWorker = null;
        }

        private void bwBackup_DoWork(object sender, DoWorkEventArgs e)
        {
            string[] args;

            TextItemsFound = 0;

            args = (string[])e.Argument;

            lock (CurrentDownloads) CurrentDownloads.Clear();

            // FIXME:
            //Client.Inventory.RequestFolderContents(Client.Inventory.Store.RootFolder.UUID, Client.Self.AgentID, 
            //    true, true, false, InventorySortOrder.ByName);

            DirectoryInfo di = new DirectoryInfo(args[1]);

            // recurse on the root folder into the entire inventory
            BackupFolder(Client.Inventory.Store.RootNode, di.FullName);
        }

        /// <summary>
        /// BackupFolder - recurse through the inventory nodes sending scripts and notecards to the transfer queue
        /// </summary>
        /// <param name="folder">The current leaf in the inventory tree</param>
        /// <param name="sPathSoFar">path so far, in the form @"c:\here" -- this needs to be "clean" for the current filesystem</param>
        private void BackupFolder(InventoryNode folder, string sPathSoFar)
        {

            // FIXME:
            //Client.Inventory.RequestFolderContents(folder.Data.UUID, Client.Self.AgentID, true, true, false, 
            //    InventorySortOrder.ByName);

            // first scan this folder for text
            foreach (InventoryNode iNode in folder.Nodes.Values)
            {
                if (BackupWorker.CancellationPending)
                    return;
                if (iNode.Data is OpenMetaverse.InventoryItem)
                {
                    InventoryItem ii = iNode.Data as InventoryItem;
                    if (ii.AssetType == AssetType.LSLText || ii.AssetType == AssetType.Notecard)
                    {
                        // check permissions on scripts
                        if (ii.AssetType == AssetType.LSLText)
                        {
                            if ((ii.Permissions.OwnerMask & PermissionMask.Modify) == PermissionMask.None)
                            {
                                // skip this one
                                continue;
                            }
                        }

                        string sExtension = (ii.AssetType == AssetType.LSLText) ? ".lsl" : ".txt";
                        // make the output file
                        string sPath = sPathSoFar + @"\" + MakeValid(ii.Name.Trim()) + sExtension;

                        // create the new qdi
                        QueuedDownloadInfo qdi = new QueuedDownloadInfo(sPath, ii.AssetUUID, iNode.Data.UUID, UUID.Zero,
                            Client.Self.AgentID, ii.AssetType);

                        // add it to the queue
                        lock (PendingDownloads)
                        {
                            TextItemsFound++;
                            PendingDownloads.Enqueue(qdi);
                        }
                    }
                }
            }

            // now run any subfolders
            foreach (InventoryNode i in folder.Nodes.Values)
            {
                if (BackupWorker.CancellationPending)
                    return;
                else if (i.Data is OpenMetaverse.InventoryFolder)
                    BackupFolder(i, sPathSoFar + @"\" + MakeValid(i.Data.Name.Trim()));
            }
        }

        private string MakeValid(string path)
        {
            // FIXME: We need to strip illegal characters out
            return path.Trim().Replace('"', '\'');
        }

        private void Assets_OnAssetReceived(AssetDownload asset, Asset blah)
        {
            lock (CurrentDownloads)
            {
                // see if we have this in our transfer list
                QueuedDownloadInfo r = CurrentDownloads.Find(delegate(QueuedDownloadInfo q)
                {
                    return q.TransferID == asset.ID;
                });

                if (r != null && r.TransferID == asset.ID)
                {
                    if (asset.Success)
                    {
                        // create the directory to put this in
                        Directory.CreateDirectory(Path.GetDirectoryName(r.FileName));

                        // write out the file
                        File.WriteAllBytes(r.FileName, asset.AssetData);
                        Logger.DebugLog(Name + " Wrote: " + r.FileName, Client);
                        TextItemsTransferred++;
                    }
                    else
                    {
                        TextItemErrors++;
                        Console.WriteLine("{0}: Download of asset {1} ({2}) failed with status {3}", Name, r.FileName,
                            r.AssetID.ToString(), asset.Status.ToString());
                    }

                    // remove the entry
                    CurrentDownloads.Remove(r);
                }
            }
        }

        /// <summary>
        /// returns blank or "not" if false
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        private static string BoolToNot(bool b)
        {
            return b ? String.Empty : "not";
        }
    }
}
