/*
 * Copyright (c) 2007-2008, openmetaverse.org
 * All rights reserved.
 *
 * - Redistribution and use in source and binary forms, with or without
 *   modification, are permitted provided that the following conditions are met:
 *
 * - Redistributions of source code must retain the above copyright notice, this
 *   list of conditions and the following disclaimer.
 * - Neither the name of the openmetaverse.org nor the names
 *   of its contributors may be used to endorse or promote products derived from
 *   this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE
 * LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
 * CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
 * SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
 * INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
 * CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
 * POSSIBILITY OF SUCH DAMAGE.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using OpenMetaverse;
using OpenMetaverse.Packets;

namespace OpenMetaverse.Utilities
{
    /// <summary>
    /// 
    /// </summary>
    public enum WaterType
    {
        /// <summary></summary>
        Unknown,
        /// <summary></summary>
        Dry,
        /// <summary></summary>
        Waterfront,
        /// <summary></summary>
        Underwater
    }

    public static class Realism
    {
        /// <summary>
        /// Aims at the specified position, enters mouselook, presses and
        /// releases the left mouse button, and leaves mouselook
        /// </summary>
        /// <param name="client"></param>
        /// <param name="target">Target to shoot at</param>
        /// <returns></returns>
        public static bool Shoot(GridClient client, Vector3 target)
        {
            if (client.Self.Movement.TurnToward(target))
                return Shoot(client);
            else
                return false;
        }

        /// <summary>
        /// Enters mouselook, presses and releases the left mouse button, and leaves mouselook
        /// </summary>
        /// <returns></returns>
        public static bool Shoot(GridClient client)
        {
            if (client.Settings.SEND_AGENT_UPDATES)
            {
                client.Self.Movement.Mouselook = true;
                client.Self.Movement.MLButtonDown = true;
                client.Self.Movement.SendUpdate();

                client.Self.Movement.MLButtonUp = true;
                client.Self.Movement.MLButtonDown = false;
                client.Self.Movement.FinishAnim = true;
                client.Self.Movement.SendUpdate();

                client.Self.Movement.Mouselook = false;
                client.Self.Movement.MLButtonUp = false;
                client.Self.Movement.FinishAnim = false;
                client.Self.Movement.SendUpdate();

                return true;
            }
            else
            {
                Logger.Log("Attempted Shoot but agent updates are disabled", Helpers.LogLevel.Warning, client);
                return false;
            }
        }

        /// <summary>
        ///  A psuedo-realistic chat function that uses the typing sound and
        /// animation, types at three characters per second, and randomly 
        /// pauses. This function will block until the message has been sent
        /// </summary>
        /// <param name="client">A reference to the client that will chat</param>
        /// <param name="message">The chat message to send</param>
        public static void Chat(GridClient client, string message)
        {
            Chat(client, message, ChatType.Normal, 3);
        }

        /// <summary>
        /// A psuedo-realistic chat function that uses the typing sound and
        /// animation, types at a given rate, and randomly pauses. This 
        /// function will block until the message has been sent
        /// </summary>
        /// <param name="client">A reference to the client that will chat</param>
        /// <param name="message">The chat message to send</param>
        /// <param name="type">The chat type (usually Normal, Whisper or Shout)</param>
        /// <param name="cps">Characters per second rate for chatting</param>
        public static void Chat(GridClient client, string message, ChatType type, int cps)
        {
            Random rand = new Random();
            int characters = 0;
            bool typing = true;

            // Start typing
            client.Self.Chat(String.Empty, 0, ChatType.StartTyping);
            client.Self.AnimationStart(Animations.TYPE, false);

            while (characters < message.Length)
            {
                if (!typing)
                {
                    // Start typing again
                    client.Self.Chat(String.Empty, 0, ChatType.StartTyping);
                    client.Self.AnimationStart(Animations.TYPE, false);
                    typing = true;
                }
                else
                {
                    // Randomly pause typing
                    if (rand.Next(10) >= 9)
                    {
                        client.Self.Chat(String.Empty, 0, ChatType.StopTyping);
                        client.Self.AnimationStop(Animations.TYPE, false);
                        typing = false;
                    }
                }

                // Sleep for a second and increase the amount of characters we've typed
                System.Threading.Thread.Sleep(1000);
                characters += cps;
            }

            // Send the message
            client.Self.Chat(message, 0, type);

            // Stop typing
            client.Self.Chat(String.Empty, 0, ChatType.StopTyping);
            client.Self.AnimationStop(Animations.TYPE, false);
        }
    }

    public class ConnectionManager
    {
        private GridClient Client;
        private ulong SimHandle;
        private Vector3 Position = Vector3.Zero;
        private System.Timers.Timer CheckTimer;

        public ConnectionManager(GridClient client, int timerFrequency)
        {
            Client = client;

            CheckTimer = new System.Timers.Timer(timerFrequency);
            CheckTimer.Elapsed += new System.Timers.ElapsedEventHandler(CheckTimer_Elapsed);
        }

        public static bool PersistentLogin(GridClient client, string firstName, string lastName, string password,
            string userAgent, string start, string author)
        {
            int unknownLogins = 0;

        Start:

            if (client.Network.Login(firstName, lastName, password, userAgent, start, author))
            {
                Logger.Log("Logged in to " + client.Network.CurrentSim, Helpers.LogLevel.Info, client);
                return true;
            }
            else
            {
                if (client.Network.LoginErrorKey == "god")
                {
                    Logger.Log("Grid is down, waiting 10 minutes", Helpers.LogLevel.Warning, client);
                    LoginWait(10);
                    goto Start;
                }
                else if (client.Network.LoginErrorKey == "key")
                {
                    Logger.Log("Bad username or password, giving up on login", Helpers.LogLevel.Error, client);
                    return false;
                }
                else if (client.Network.LoginErrorKey == "presence")
                {
                    Logger.Log("Server is still logging us out, waiting 1 minute", Helpers.LogLevel.Warning, client);
                    LoginWait(1);
                    goto Start;
                }
                else if (client.Network.LoginErrorKey == "disabled")
                {
                    Logger.Log("This account has been banned! Giving up on login", Helpers.LogLevel.Error, client);
                    return false;
                }
                else if (client.Network.LoginErrorKey == "timed out" ||client.Network.LoginErrorKey == "no connection" )
                {
                    Logger.Log("Login request timed out, waiting 1 minute", Helpers.LogLevel.Warning, client);
                    LoginWait(1);
                    goto Start;
                } else if (client.Network.LoginErrorKey == "bad response") {
                    Logger.Log("Login server returned unparsable result", Helpers.LogLevel.Warning, client);
                    LoginWait(1);
                    goto Start;
                } else
                {
                    ++unknownLogins;

                    if (unknownLogins < 5)
                    {
                        Logger.Log("Unknown login error, waiting 2 minutes: " + client.Network.LoginErrorKey,
                            Helpers.LogLevel.Warning, client);
                        LoginWait(2);
                        goto Start;
                    }
                    else
                    {
                        Logger.Log("Too many unknown login error codes, giving up", Helpers.LogLevel.Error, client);
                        return false;
                    }
                }
            }
        }

        public void StayInSim(ulong handle, Vector3 desiredPosition)
        {
            SimHandle = handle;
            Position = desiredPosition;
            CheckTimer.Start();
        }

        private static void LoginWait(int minutes)
        {
            Thread.Sleep(1000 * 60 * minutes);
        }

        private void CheckTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (SimHandle != 0)
            {
                if (Client.Network.CurrentSim.Handle != 0 &&
                    Client.Network.CurrentSim.Handle != SimHandle)
                {
                    // Attempt to move to our target sim
                    Client.Self.Teleport(SimHandle, Position);
                }
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [Obsolete("ParcelDownloader has been replaced by Parcels.RequestAllSimParcels()")]
    public class ParcelDownloader
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="simulator">Simulator where the parcels are located</param>
        /// <param name="Parcels">Mapping of parcel LocalIDs to Parcel objects</param>
        /// <param name="map"></param>
        public delegate void ParcelsDownloadedCallback(Simulator simulator, Dictionary<int, Parcel> Parcels, int[,] map);


        /// <summary>
        /// 
        /// </summary>
        public event ParcelsDownloadedCallback OnParcelsDownloaded;

        private GridClient Client;
        /// <summary>Dictionary of 64x64 arrays of parcels which have been successfully downloaded 
        /// for each simulator (and their LocalID's, 0 = Null)</summary>
        private Dictionary<Simulator, int[,]> ParcelMarked = new Dictionary<Simulator, int[,]>();
        private Dictionary<Simulator, Dictionary<int, Parcel>> Parcels = new Dictionary<Simulator, Dictionary<int, Parcel>>();
        private List<Simulator> active_sims = new List<Simulator>();

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="client">A reference to the GridClient object</param>
        public ParcelDownloader(GridClient client)
        {
            Client = client;
            Client.Parcels.OnParcelProperties += new ParcelManager.ParcelPropertiesCallback(Parcels_OnParcelProperties);
            Client.Parcels.OnAccessListReply += new ParcelManager.ParcelAccessListReplyCallback(Parcels_OnParcelAccessList);
        }

        public void DownloadSimParcels(Simulator simulator)
        {
            if (simulator == null)
            {
                Logger.Log("DownloadSimParcels() will not work with a null simulator", Helpers.LogLevel.Error, Client);
                return;
            }

            lock (active_sims)
            {
                if (active_sims.Contains(simulator))
                {
                    Logger.Log("DownloadSimParcels(" + simulator + ") called more than once?", Helpers.LogLevel.Error, Client);
                    return;
                }

                active_sims.Add(simulator);
            }

            lock (ParcelMarked)
            {
                if (!ParcelMarked.ContainsKey(simulator))
                {
                    ParcelMarked[simulator] = new int[64, 64];
                    Parcels[simulator] = new Dictionary<int, Parcel>();
                }
            }

            Client.Parcels.PropertiesRequest(simulator, 0.0f, 0.0f, 0.0f, 0.0f, 0, false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="map"></param>
        /// <param name="localid"></param>
        /// <returns></returns>
        public float GetHeightRange(int[,] map, int localid)
        {
            float min = Single.MaxValue;
            float max = 0.0f;

            for (int y = 0; y < 64; y++)
            {
                for (int x = 0; x < 64; x++)
                {
                    if (map[y, x] == localid)
                    {
                        for (int y1 = 0; y1 < 4; y1++)
                        {
                            for (int x1 = 0; x1 < 4; x1++)
                            {
                                float height;
                                int tries = 0;

                            CheckHeight:

                                if (Client.Terrain.TerrainHeightAtPoint(Client.Network.CurrentSim.Handle,
                                    x * 4 + x1, y * 4 + y1, out height))
                                {
                                    if (height < min)
                                        min = height;
                                    if (height > max)
                                        max = height;
                                }
                                else if (tries > 4)
                                {
                                    Logger.Log("Too many tries on this terrain block, skipping",
                                        Helpers.LogLevel.Warning, Client);
                                    continue;
                                }
                                else
                                {
                                    Logger.Log(String.Format("Terrain height is null at {0},{1} retrying",
                                        x * 4 + x1, y * 4 + y1), Helpers.LogLevel.Info, Client);

                                    // Terrain at this point hasn't been downloaded, move the camera to this spot
                                    // and try again
                                    Vector3 position = new Vector3((float)(x * 4 + x1), (float)(y * 4 + y1),
                                        Client.Self.SimPosition.Z);
                                    Client.Self.Movement.Camera.Position = position;

                                    Client.Self.Movement.SendUpdate(true);

                                    Thread.Sleep(1000);
                                    goto CheckHeight;
                                }
                            }
                        }
                    }
                }
            }

            if (min != Single.MaxValue)
            {
                return max - min;
            }
            else
            {
                Logger.Log("Error decoding terrain for parcel " + localid, Helpers.LogLevel.Error, Client);
                return Single.NaN;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="map"></param>
        /// <param name="localid"></param>
        /// <returns></returns>
        public WaterType GetWaterType(int[,] map, int localid)
        {
            if (!Client.Settings.STORE_LAND_PATCHES)
            {
                Logger.Log("GetWaterType() will not work without Settings.STORE_LAND_PATCHES set to true",
                    Helpers.LogLevel.Error, Client);
                return WaterType.Unknown;
            }
            else if (!Client.Network.Connected && Client.Network.CurrentSim != null)
            {
                Logger.Log("GetWaterType() can only be used with an online client", Helpers.LogLevel.Error, Client);
                return WaterType.Unknown;
            }

            bool underwater = false;
            bool abovewater = false;

            for (int y = 0; y < 64; y++)
            {
                for (int x = 0; x < 64; x++)
                {
                    if (map[y, x] == localid)
                    {
                        for (int y1 = 0; y1 < 4; y1++)
                        {
                            for (int x1 = 0; x1 < 4; x1++)
                            {
                                float height;
                                int tries = 0;

                            CheckHeight:
                                tries++;

                                if (Client.Terrain.TerrainHeightAtPoint(Client.Network.CurrentSim.Handle,
                                    x * 4 + x1, y * 4 + y1, out height))
                                {
                                    if (height < Client.Network.CurrentSim.WaterHeight)
                                    {
                                        underwater = true;
                                    }
                                    else
                                    {
                                        abovewater = true;
                                    }
                                }
                                else if (tries > 4)
                                {
                                    Logger.Log("Too many tries on this terrain block, skipping", 
                                        Helpers.LogLevel.Warning, Client);
                                    continue;
                                }
                                else
                                {
                                    Logger.Log(String.Format("Terrain height is null at {0},{1} retrying",
                                        x * 4 + x1, y * 4 + y1), Helpers.LogLevel.Info, Client);

                                    // Terrain at this point hasn't been downloaded, move the camera to this spot
                                    // and try again
                                    Vector3 position = new Vector3((float)(x * 4 + x1), (float)(y * 4 + y1),
                                        Client.Self.SimPosition.Z);
                                    Client.Self.Movement.Camera.Position = position;

                                    Client.Self.Movement.SendUpdate(true);

                                    Thread.Sleep(1000);
                                    goto CheckHeight;
                                }
                            }
                        }
                    }
                }
            }

            if (underwater && abovewater)
            {
                return WaterType.Waterfront;
            }
            else if (abovewater)
            {
                return WaterType.Dry;
            }
            else if (underwater)
            {
                return WaterType.Underwater;
            }
            else
            {
                Logger.Log("Error decoding terrain for parcel " + localid, Helpers.LogLevel.Error, Client);
                return WaterType.Unknown;
            }
        }

        public int GetRectangularDeviation(Vector3 aabbmin, Vector3 aabbmax, int area)
        {
            int xlength = (int)(aabbmax.X - aabbmin.X);
            int ylength = (int)(aabbmax.Y - aabbmin.Y);
            int aabbarea = xlength * ylength;
            return (aabbarea - area) / 16;
        }

        private void Parcels_OnParcelAccessList(Simulator simulator, int sequenceID, int localID, uint flags,
                                                List<ParcelManager.ParcelAccessEntry> accessEntries)
        {
            if (simulator != null && Parcels.ContainsKey(simulator) && Parcels[simulator].ContainsKey(localID))
            {
                Parcel parcel = Parcels[simulator][localID];
                parcel.AccessList = accessEntries;
                Parcels[simulator][localID] = parcel;
            }
        }

        private void Parcels_OnParcelProperties(Simulator simulator, Parcel parcel, ParcelResult result,
            int selectedPrims, int sequenceID, bool snapSelection)
        {
            // Check if this is for a simulator we're concerned with
            if (!active_sims.Contains(simulator)) return;

            // Warn about parcel property request errors and bail out
            if (result == ParcelResult.NoData)
            {
                Logger.Log("ParcelDownloader received a NoData response, sequenceID " + sequenceID,
                    Helpers.LogLevel.Warning, Client);
                return;
            }

            // Warn about unexpected data and bail out
            if (!ParcelMarked.ContainsKey(simulator))
            {
                Logger.Log("ParcelDownloader received unexpected parcel data for " + simulator,
                    Helpers.LogLevel.Warning, Client);
                return;
            }

            int x, y, index, bit;
            int[,] markers = ParcelMarked[simulator];

            // Add this parcel to the dictionary of LocalID -> Parcel mappings
            lock (Parcels[simulator])
                if (!Parcels[simulator].ContainsKey(parcel.LocalID))
                    Parcels[simulator][parcel.LocalID] = parcel;

            // Request the access list for this parcel
            Client.Parcels.AccessListRequest(simulator, parcel.LocalID, 
                AccessList.Both, 0);

            // Mark this area as downloaded
            for (y = 0; y < 64; y++)
            {
                for (x = 0; x < 64; x++)
                {
                    if (markers[y, x] == 0)
                    {
                        index = (y * 64) + x;
                        bit = index % 8;
                        index >>= 3;

                        if ((parcel.Bitmap[index] & (1 << bit)) != 0)
                            markers[y, x] = parcel.LocalID;
                    }
                }
            }

            // Request parcel information for the next missing area
            for (y = 0; y < 64; y++)
            {
                for (x = 0; x < 64; x++)
                {
                    if (markers[y, x] == 0)
                    {
                        Client.Parcels.PropertiesRequest(simulator,
                                                         (y + 1) * 4.0f, (x + 1) * 4.0f,
                                                         y * 4.0f, x * 4.0f, 0, false);
                        return;
                    }
                }
            }

            // If we get here, there are no more zeroes in the markers map
            lock (active_sims)
            {
                active_sims.Remove(simulator);

                if (OnParcelsDownloaded != null)
                {
                    // This map is complete, fire callback
                    try { OnParcelsDownloaded(simulator, Parcels[simulator], markers); }
                    catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, Client, e); }
                }
            }
        }
    }
}
