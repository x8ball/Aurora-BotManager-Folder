/*
 * Copyright (c) Contributors, http://aurora-sim.org/
 * See CONTRIBUTORS.TXT for a full list of copyright holders.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 *     * Redistributions of source code must retain the above copyright
 *       notice, this list of conditions and the following disclaimer.
 *     * Redistributions in binary form must reproduce the above copyright
 *       notice, this list of conditions and the following disclaimer in the
 *       documentation and/or other materials provided with the distribution.
 *     * Neither the name of the Aurora-Sim Project nor the
 *       names of its contributors may be used to endorse or promote products
 *       derived from this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE DEVELOPERS ``AS IS'' AND ANY
 * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL THE CONTRIBUTORS BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

using System;
using System.IO;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting.Lifetime;
using System.Xml;
using Aurora.Framework;
using OpenMetaverse;
using Nini.Config;
using OpenSim;
using OpenSim.Framework;
using OpenSim.Region.Framework.Interfaces;
using OpenSim.Region.Framework.Scenes;
//The following is needed for IInventoryServices
using OpenSim.Services.Interfaces;



using LSL_Float = Aurora.ScriptEngine.AuroraDotNetEngine.LSL_Types.LSLFloat;
using LSL_Integer = Aurora.ScriptEngine.AuroraDotNetEngine.LSL_Types.LSLInteger;
using LSL_Key = Aurora.ScriptEngine.AuroraDotNetEngine.LSL_Types.LSLString;
using LSL_List = Aurora.ScriptEngine.AuroraDotNetEngine.LSL_Types.list;
using LSL_Rotation = Aurora.ScriptEngine.AuroraDotNetEngine.LSL_Types.Quaternion;
using LSL_String = Aurora.ScriptEngine.AuroraDotNetEngine.LSL_Types.LSLString;
using LSL_Vector = Aurora.ScriptEngine.AuroraDotNetEngine.LSL_Types.Vector3;
using Aurora.ScriptEngine.AuroraDotNetEngine.APIs.Interfaces;
using Aurora.ScriptEngine.AuroraDotNetEngine.Runtime;
using Aurora.ScriptEngine.AuroraDotNetEngine;

namespace Aurora.BotManager
{
    [Serializable]
    public class Bot_Api : MarshalByRefObject, IBot_Api, IScriptApi
    {
        internal IScriptModulePlugin m_ScriptEngine;
        internal ISceneChildEntity m_host;
        internal ScriptProtectionModule ScriptProtection;
        internal UUID m_itemID;

        public void Initialize (IScriptModulePlugin ScriptEngine, ISceneChildEntity host, uint localID, UUID itemID, ScriptProtectionModule module)
        {
            m_itemID = itemID;
            m_ScriptEngine = ScriptEngine;
            m_host = host;
            ScriptProtection = module;
        }

        public IScriptApi Copy()
        {
            return new Bot_Api();
        }

        public string Name
        {
            get { return "bot"; }
        }

        public string InterfaceName
        {
            get { return "IBot_Api"; }
        }

        /// <summary>
        /// We have to add a ref here, as this API is NOT inside of the script engine
        /// So we add the referenced assembly to ourselves
        /// </summary>
        public string[] ReferencedAssemblies
        {
            get { return new string[1] {
                AssemblyFileName
            }; }
        }

        /// <summary>
        /// We use "Aurora.BotManager", and that isn't a default namespace, so we need to add it
        /// </summary>
        public string[] NamespaceAdditions
        {
            get { return new string[1] { "Aurora.BotManager" }; }
        }

        /// <summary>
        /// Created by John Sibly @ http://stackoverflow.com/questions/52797/c-how-do-i-get-the-path-of-the-assembly-the-code-is-in
        /// </summary>
        static public string AssemblyFileName
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetFileName(path);
            }
        }

        public void Dispose()
        {
        }

        public override Object InitializeLifetimeService()
        {
            ILease lease = (ILease)base.InitializeLifetimeService();

            if (lease.CurrentState == LeaseState.Initial)
            {
                lease.InitialLeaseTime = TimeSpan.FromMinutes(0);
                //                lease.RenewOnCallTime = TimeSpan.FromSeconds(10.0);
                //                lease.SponsorshipTimeout = TimeSpan.FromMinutes(1.0);
            }
            return lease;

        }

        public IScene World
        {
            get { return m_host.ParentEntity.Scene; }
        }

        public string botCreateBot(string FirstName, string LastName, string appearanceToClone, LSL_Vector startPos)
        {
            //ScriptProtection.CheckThreatLevel (ThreatLevel.Moderate, "botCreateBot", m_host, "bot");
            IBotManager manager = World.RequestModuleInterface<IBotManager>();
            if (manager != null)
                return manager.CreateAvatar (FirstName, LastName, m_host.ParentEntity.Scene, UUID.Parse (appearanceToClone), m_host.OwnerID, new Vector3 ((float)startPos.x, (float)startPos.y, (float)startPos.z)).ToString ();
            return "";
        }

        public LSL_Vector botGetWaitingTime (LSL_Integer waitTime)
        {
            return new LSL_Vector (waitTime, 0, 0);
        }

        public void botPauseMovement (string bot)
        {
            IBotManager manager = World.RequestModuleInterface<IBotManager> ();
            if (manager != null)
                manager.PauseMovement (UUID.Parse (bot));
        }

        public void botResumeMovement (string bot)
        {
            //ScriptProtection.CheckThreatLevel (ThreatLevel.Moderate, "botResumeMovement", m_host, "bot");
            IBotManager manager = World.RequestModuleInterface<IBotManager> ();
            if (manager != null)
                manager.ResumeMovement (UUID.Parse (bot));
        }

        public void botSetShouldFly (string keyOfBot, int ShouldFly)
        {
            //ScriptProtection.CheckThreatLevel (ThreatLevel.Moderate, "botSetShouldFly", m_host, "bot");
            IBotManager manager = World.RequestModuleInterface<IBotManager> ();
            if (manager != null)
               manager.SetBotShouldFly (UUID.Parse(keyOfBot), ShouldFly == 1);
        }

        public void botSetMap(string keyOfBot, LSL_List positions, LSL_List movementType, LSL_Integer flags)
        {
            //ScriptProtection.CheckThreatLevel (ThreatLevel.Moderate, "botSetMap", m_host, "bot");
            List<Vector3> PositionsMap = new List<Vector3>();
            for(int i = 0; i < positions.Length; i++)
            {
                LSL_Vector pos = positions.GetVector3Item(i);
                PositionsMap.Add(new Vector3((float)pos.x, (float)pos.y, (float)pos.z));
            }
            List<TravelMode> TravelMap = new List<TravelMode>();
            for(int i = 0; i < movementType.Length; i++)
            {
                LSL_Integer travel = movementType.GetLSLIntegerItem(i);
                TravelMap.Add((TravelMode)travel.value);
            }

            IBotManager manager = World.RequestModuleInterface<IBotManager>();
            if (manager != null)
                manager.SetBotMap(UUID.Parse(keyOfBot), PositionsMap, TravelMap, flags.value);
        }

        public void botRemoveBot (string bot)
        {
            //ScriptProtection.CheckThreatLevel (ThreatLevel.Moderate, "botRemoveBot", m_host, "bot");
            IBotManager manager = World.RequestModuleInterface<IBotManager> ();
            if (manager != null)
                manager.RemoveAvatar (UUID.Parse (bot), m_host.ParentEntity.Scene);
        }

        public void botFollowAvatar (string bot, string avatarName, LSL_Float startFollowDistance, LSL_Float endFollowDistance)
        {
            //ScriptProtection.CheckThreatLevel (ThreatLevel.Moderate, "botFollowAvatar", m_host, "bot");
            IBotManager manager = World.RequestModuleInterface<IBotManager> ();
            if (manager != null)
                manager.FollowAvatar (UUID.Parse (bot), avatarName, (float)startFollowDistance, (float)endFollowDistance);
        }

        public void botStopFollowAvatar (string bot)
        {
           // ScriptProtection.CheckThreatLevel (ThreatLevel.Moderate, "botStopFollowAvatar", m_host, "bot");
            IBotManager manager = World.RequestModuleInterface<IBotManager> ();
            if (manager != null)
                manager.StopFollowAvatar (UUID.Parse (bot));
        }

        public void botSetPathMap (string bot, string pathMap, int x, int y, int cornerstoneX, int cornerstoneY)
        {
            //ScriptProtection.CheckThreatLevel (ThreatLevel.Moderate, "botSetPathMap", m_host, "bot");
            IBotManager manager = World.RequestModuleInterface<IBotManager> ();
            if (manager != null)
                manager.ReadMap (UUID.Parse (bot), pathMap, x, y, cornerstoneX, cornerstoneY);
        }

        public void botFindPath (string bot, LSL_Vector startPos, LSL_Vector endPos)
        {
            //ScriptProtection.CheckThreatLevel (ThreatLevel.Moderate, "botFindPath", m_host, "bot");
            IBotManager manager = World.RequestModuleInterface<IBotManager> ();
            if (manager != null)
                manager.FindPath (UUID.Parse (bot), new Vector3 ((float)startPos.x, (float)startPos.y, (float)startPos.z),
                    new Vector3 ((float)endPos.x, (float)endPos.y, (float)endPos.z));
        }

        public void botSendChatMessage (string bot, string message, int channel, int sayType)
        {
           // ScriptProtection.CheckThreatLevel (ThreatLevel.Moderate, "botSendChatMessage", m_host, "bot");
            IBotManager manager = World.RequestModuleInterface<IBotManager> ();
            if (manager != null)
                manager.SendChatMessage (UUID.Parse (bot), message, sayType, channel);
        }

        //<Christy Lock Code>
        public void botListenMessage(string fromName, string bot, string message, int sayType, string pos)
        {
            //botListenMessage
            IBotManager manager = World.RequestModuleInterface<IBotManager>();
            if (manager != null)
                // manager.SendChatMessage(UUID.Parse(bot), message, sayType);
                manager.ListenMessage(fromName, UUID.Parse(bot), message, sayType, pos);
        }
        public void botTeleportTo(string bot, string pos)
        {
            IBotManager manager = World.RequestModuleInterface<IBotManager>();
            if (manager != null)

                manager.TeleportTo(UUID.Parse(bot), pos);
        }

        //
        //</Christy Lock Code>

        public void botTouchObject (string bot, string objectID)
        {
            //ScriptProtection.CheckThreatLevel (ThreatLevel.Moderate, "botTouchObject", m_host, "bot");
            SurfaceTouchEventArgs touchArgs = new SurfaceTouchEventArgs();
            
            IScenePresence sp = World.GetScenePresence(UUID.Parse(bot));
            if(sp == null)
                return;
            ISceneChildEntity child = World.GetSceneObjectPart(UUID.Parse(objectID));
            if(child == null)
                throw new Exception("Failed to find entity to touch");

            World.EventManager.TriggerObjectGrab (child, child, Vector3.Zero, sp.ControllingClient, touchArgs);
            World.EventManager.TriggerObjectGrabbing (child, child, Vector3.Zero, sp.ControllingClient, touchArgs);
            World.EventManager.TriggerObjectDeGrab (child, child, sp.ControllingClient, touchArgs);
        }

        public void botSitObject (string bot, string objectID, LSL_Vector offset)
        {
            //ScriptProtection.CheckThreatLevel (ThreatLevel.Moderate, "botTouchObject", m_host, "bot");
            IScenePresence sp = World.GetScenePresence (UUID.Parse (bot));
            if (sp == null)
                return;
            ISceneChildEntity child = World.GetSceneObjectPart (UUID.Parse (objectID));
            if (child == null)
                throw new Exception ("Failed to find entity to touch");

            sp.HandleAgentRequestSit (sp.ControllingClient, child.UUID, new Vector3((float)offset.x, (float)offset.y, (float)offset.z));
        }

        public void botStandUp (string bot)
        {
            //ScriptProtection.CheckThreatLevel (ThreatLevel.Moderate, "botStandUp", m_host, "bot");
            botSitObject (bot, UUID.Zero.ToString (), new LSL_Vector ());
        }

        public void botAddTag (string bot, string tag)
        {
            //ScriptProtection.CheckThreatLevel (ThreatLevel.Moderate, "botAddTag", m_host, "bot");
            IBotManager manager = World.RequestModuleInterface<IBotManager> ();
            if (manager != null)
                manager.AddTagToBot (UUID.Parse (bot), tag);
        }

        public LSL_List botGetBotsWithTag (string tag)
        {
            //ScriptProtection.CheckThreatLevel (ThreatLevel.Moderate, "botGetBotsWithTag", m_host, "bot");
            IBotManager manager = World.RequestModuleInterface<IBotManager> ();
            List<UUID> bots = new List<UUID> ();
            if (manager != null)
                bots = manager.GetBotsWithTag (tag);
            List<object> b = new List<object> ();
            foreach(UUID bot in bots)
                b.Add(bot.ToString());

            return new LSL_List (b);
        }

        public void botRemoveBotsWithTag (string tag)
        {
            //ScriptProtection.CheckThreatLevel (ThreatLevel.Moderate, "botRemoveBotsWithTag", m_host, "bot");
            IBotManager manager = World.RequestModuleInterface<IBotManager> ();
            if (manager != null)
                manager.RemoveBots (tag);
        }
        public string botGetLocation(string bot)
        {
            Vector3 botpos = Vector3.Zero; //set default botpos to zero


            m_host.ParentEntity.Scene.ForEachScenePresence(delegate(IScenePresence sp)
            {
                // this should be the bot id
                if (sp.UUID == UUID.Parse(bot))
                {
                    //here you need to get the bot position and return it
                    botpos = sp.AbsolutePosition;
                }
            });

            return botpos.ToString();
        }
        public void botSetState(string bot, string State)
        {
            //this is a test case of being able to get access to a property
            //of the actual rexbot itself through the botAPI       
            //IBotManager manager = World.RequestModuleInterface<IBotManager>();
            //if (manager != null)
            //{
            //    RexBot rxbot;
            //    rxbot = (RexBot)manager.GetBot(UUID.Parse(bot));
            //    //follow up with this
            //    switch (State.ToLower())
            //    {
            //        case "walking":
            //            rxbot.State = RexBot.RexBotState.Walking;
            //            break;
            //        case "idle":
            //            rxbot.State = RexBot.RexBotState.Idle;
            //            break;
            //        case "flying":
            //            rxbot.State = RexBot.RexBotState.Flying;
            //            break;
            //
            //        default:
            //            rxbot.State = RexBot.RexBotState.Unknown;
            //            break;
            //    }
            //}

        }
        public string botGetState(string bot)
        {
            return "";
            //test case of getting the state back
            //interestingly it usually returns idle
            //even when it's been set to walking by me
            //externally. Which is not a total fail
            //because it means I'm getting something
            //back out of the class rather than the default
            //unknown which would be returned if it was
            //failing. Anyways this is a test case only
            //string strState="unknown";

            //        IBotManager manager = World.RequestModuleInterface<IBotManager>();
            //        if (manager != null)
            //        {

            //            RexBot rxbot;
            //            rxbot = (RexBot)manager.GetBot(UUID.Parse(bot));
            //            RexBot.RexBotState  st = rxbot.State;

            //            switch (st)
            //            {
            //                case RexBot.RexBotState.Walking:
            //                    strState = "walking";
            //                    break;
            //                case RexBot.RexBotState.Idle:
            //                    strState = "idle";
            //                    break;
            //                case RexBot.RexBotState.Flying:
            //                    strState = "flying";
            //                    break;

            //                default:
            //                    strState = "unknown";
            //                    break;
            //            }

            //        }

            //return strState;

        }
        public void botAnimate(string bot, string AnimationUUID)
        {
            m_host.ParentEntity.Scene.ForEachScenePresence(delegate(IScenePresence sp)
            {
                // this should be the bot id
                if (sp.UUID == UUID.Parse(bot))
                {
                    sp.Animator.AddAnimation(UUID.Parse(AnimationUUID), UUID.Zero);
                }
            });
        }
        public void botCauseDamage(string sBotName, float fdamage)
        {

            m_host.ParentEntity.Scene.ForEachScenePresence(delegate(IScenePresence sp)
            {
                // this should be the correct bot by name
                if (sp.Name.ToLower() == sBotName.ToLower())
                {
                    ICombatPresence cp = sp.RequestModuleInterface<ICombatPresence>();
                    Bot bot = World.RequestModuleInterface<Bot>();
                    bot = (Bot)sp.ControllingClient;
                    int ibothealth = bot.Health;
                    ibothealth = ibothealth - (int)fdamage;

                    if (ibothealth > 0)
                    {
                        bot.Health = ibothealth;
                        cp.Health = (float)ibothealth;
                        //cp.IncurDamage(1, fdamage, sp.UUID);
                        if (ibothealth < 0)
                        {
                            IBotManager manager = World.RequestModuleInterface<IBotManager>();
                            if (manager != null)
                            {
                                manager.KillBot(sp.UUID);
                            }
                        }

                    }
                    else
                    {
                        IBotManager manager = World.RequestModuleInterface<IBotManager>();
                        if (manager != null)
                        {
                            manager.KillBot(sp.UUID);
                        }
                    }

                }
            });
        }
        public string botSpawnAttackBot(string FirstName, string LastName, string appearanceToClone, string AvatarToFollowName, string creatorID)
        {
            UUID botUUID;
            IBotManager manager = World.RequestModuleInterface<IBotManager>();
            Vector3 startPos;
            startPos.X = 128;
            startPos.Y = 128;
            startPos.Z = 23;
            if (manager != null)
            {
                
                botUUID = manager.SpawnAttackBot(FirstName, LastName, m_host.ParentEntity.Scene, UUID.Parse(appearanceToClone), AvatarToFollowName, UUID.Parse(creatorID), startPos);
                return botUUID.ToString();
            }
            return "";
        }

        //looks like you need to get an appearance argument and build it up
        //then pass it to this function
        //public void botSetAppearanceAssets(UUID userID, ref AvatarAppearance appearance)
        //{

        //    IInventoryService invService = m_host.ParentEntity.Scene.InventoryService;

        //    for (int i = 0; i < AvatarWearable.MAX_WEARABLES; i++)
        //    {
        //        for (int j = 0; j < appearance.Wearables[j].Count; j++)
        //        {
        //            if (appearance.Wearables[i][j].ItemID == UUID.Zero)
        //                continue;

        //            // Ignore ruth's assets
        //            if (appearance.Wearables[i][j].ItemID == AvatarWearable.DefaultWearables[i][j].ItemID)
        //            {
        //                //m_log.ErrorFormat(
        //                //    "[AvatarFactory]: Found an asset for the default avatar, itemID {0}, wearable {1}, asset {2}" +
        //                //    ", setting to default asset {3}.",
        //                //    appearance.Wearables[i][j].ItemID, (WearableType)i, appearance.Wearables[i][j].AssetID,
        //                //    AvatarWearable.DefaultWearables[i][j].AssetID);
        //                appearance.Wearables[i].Add(appearance.Wearables[i][j].ItemID, appearance.Wearables[i][j].AssetID);
        //                continue;
        //            }

        //            InventoryItemBase baseItem = new InventoryItemBase(appearance.Wearables[i][j].ItemID, userID);
        //            baseItem = invService.GetItem(baseItem);

        //            if (baseItem != null)
        //            {
        //                appearance.Wearables[i].Add(appearance.Wearables[i][j].ItemID, baseItem.AssetID);
        //            }
        //            else
        //            {
        //                //m_log.ErrorFormat(
        //                //    "[AvatarFactory]: Can't find inventory item {0} for {1}, setting to default",
        //                //    appearance.Wearables[i][j].ItemID, (WearableType)i);

        //                appearance.Wearables[i].RemoveItem(appearance.Wearables[i][j].ItemID);
        //                appearance.Wearables[i].Add(AvatarWearable.DefaultWearables[i][j].ItemID, AvatarWearable.DefaultWearables[i][j].AssetID);
        //            }
        //        }
        //    }


        //}

        //public void botForceSendAvatarAppearance(UUID agentid)
        //{
        //    //If the avatar changes appearance, then proptly logs out, this will break!
        //    IScenePresence sp = m_host.ParentEntity.Scene.GetScenePresence(agentid);
        //    if (sp == null || sp.IsChildAgent)
        //    {
        //        //m_log.WarnFormat("[AvatarFactory]: Agent {0} no longer in the scene", agentid);
        //        return;
        //    }

        //    //Force send!
        //    IAvatarAppearanceModule appearance = sp.RequestModuleInterface<IAvatarAppearanceModule>();
        //    sp.ControllingClient.SendWearables(appearance.Appearance.Wearables, appearance.Appearance.Serial);
        //    appearance.SendAvatarDataToAllAgents();

        //    appearance.SendAppearanceToAgent(sp);

        //    appearance.SendAppearanceToAllOtherAgents();
        //}


    }
}
