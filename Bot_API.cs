/*
 * Copyright (c) Contributors, http://opensimulator.org/
 * See CONTRIBUTORS.TXT for a full list of copyright holders.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 *     * Redistributions of source code must retain the above copyright
 *       notice, this list of conditions and the following disclaimer.
 *     * Redistributions in binary form must reproduce the above copyright
 *       notice, this list of conditions and the following disclaimer in the
 *       documentation and/or other materials provided with the distribution.
 *     * Neither the name of the OpenSimulator Project nor the
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

using LSL_Float = Aurora.ScriptEngine.AuroraDotNetEngine.LSL_Types.LSLFloat;
using LSL_Integer = Aurora.ScriptEngine.AuroraDotNetEngine.LSL_Types.LSLInteger;
using LSL_Key = Aurora.ScriptEngine.AuroraDotNetEngine.LSL_Types.LSLString;
using LSL_List = Aurora.ScriptEngine.AuroraDotNetEngine.LSL_Types.list;
using LSL_Rotation = Aurora.ScriptEngine.AuroraDotNetEngine.LSL_Types.Quaternion;
using LSL_String = Aurora.ScriptEngine.AuroraDotNetEngine.LSL_Types.LSLString;
using LSL_Vector = Aurora.ScriptEngine.AuroraDotNetEngine.LSL_Types.Vector3;
using Aurora.ScriptEngine.AuroraDotNetEngine.APIs.Interfaces;
using Aurora.ScriptEngine.AuroraDotNetEngine.Runtime;
using OpenSim.Services.Interfaces;
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
                    IRexBot bot = World.RequestModuleInterface<IRexBot>();
                    bot = (IRexBot)sp.ControllingClient;
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


        public string botSpawnAttackBot(string FirstName, string LastName, string appearanceToClone, string AvatarToFollowName)
        {
            UUID botUUID;
            IBotManager manager = World.RequestModuleInterface<IBotManager>();
            if (manager != null)
            {
                botUUID = manager.SpawnAttackBot(FirstName, LastName, m_host.ParentEntity.Scene, UUID.Parse(appearanceToClone), AvatarToFollowName);
                return botUUID.ToString();
            }
            return "";
        }

        public string botCreateBot(string FirstName, string LastName, string appearanceToClone)
        {
            //ScriptProtection.CheckThreatLevel (ThreatLevel.Moderate, "botCreateBot", m_host, "bot");
            IBotManager manager = World.RequestModuleInterface<IBotManager>();
            if (manager != null)
                return manager.CreateAvatar(FirstName, LastName, m_host.ParentEntity.Scene, UUID.Parse(appearanceToClone)).ToString();
            return "";
        }

        public void botSetShouldFly (string keyOfBot, int ShouldFly)
        {
            //ScriptProtection.CheckThreatLevel (ThreatLevel.Moderate, "botSetShouldFly", m_host, "bot");
            IBotManager manager = World.RequestModuleInterface<IBotManager> ();
            if (manager != null)
               manager.SetBotShouldFly (UUID.Parse(keyOfBot), ShouldFly == 1);
        }

        public void botSetMap(string keyOfBot, LSL_List positions, LSL_List movementType)
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
                manager.SetBotMap(UUID.Parse(keyOfBot), PositionsMap, TravelMap);
        }

        public void botPause(string bot)
        {
            //ScriptProtection.CheckThreatLevel (ThreatLevel.Moderate, "botPause", m_host, "bot");
            IBotManager manager = World.RequestModuleInterface<IBotManager>();
            if (manager != null)
                manager.PauseAutoMove(UUID.Parse(bot));
        }

        public void botResume(string bot)
        {
            //ScriptProtection.CheckThreatLevel (ThreatLevel.Moderate, "botResume", m_host, "bot");
            IBotManager manager = World.RequestModuleInterface<IBotManager>();
            if (manager != null)
                manager.UnpauseAutoMove(UUID.Parse(bot));
        }

        public void botStop(string bot)
        {
            //ScriptProtection.CheckThreatLevel (ThreatLevel.Moderate, "botStop", m_host, "bot");
            IBotManager manager = World.RequestModuleInterface<IBotManager>();
            if (manager != null)
                manager.StopAutoMove(UUID.Parse(bot));
        }

        public void botStart(string bot)
        {
            //ScriptProtection.CheckThreatLevel (ThreatLevel.Moderate, "botStart", m_host, "bot");
            IBotManager manager = World.RequestModuleInterface<IBotManager>();
            if (manager != null)
                manager.EnableAutoMove(UUID.Parse(bot));
        }

        public void botRemoveBot (string bot)
        {
            //ScriptProtection.CheckThreatLevel (ThreatLevel.Moderate, "botRemoveBot", m_host, "bot");
            IBotManager manager = World.RequestModuleInterface<IBotManager> ();
            if (manager != null)
                manager.RemoveAvatar (UUID.Parse (bot), m_host.ParentEntity.Scene);
        }

        public void botFollowAvatar (string bot, string avatarName, LSL_Float followDistance)
        {
            //ScriptProtection.CheckThreatLevel (ThreatLevel.Moderate, "botFollowAvatar", m_host, "bot");
            IBotManager manager = World.RequestModuleInterface<IBotManager> ();
            if (manager != null)
                manager.FollowAvatar (UUID.Parse (bot), avatarName, (float)followDistance);
        }

        public void botStopFollowAvatar (string bot)
        {
            //ScriptProtection.CheckThreatLevel (ThreatLevel.Moderate, "botStopFollowAvatar", m_host, "bot");
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
            //ScriptProtection.CheckThreatLevel (ThreatLevel.Moderate, "botSendChatMessage", m_host, "bot");
            IBotManager manager = World.RequestModuleInterface<IBotManager> ();
            if (manager != null)
                manager.SendChatMessage (UUID.Parse (bot), message, sayType, channel);
//        Whisper = 0,
        //Say = 1,
        //Shout = 2,
        //ObsoleteSay = 3, // 3 is an obsolete version of Say
        //StartTyping = 4,
        //StopTyping = 5,
        //DebugChannel = 6,
        //Region = 7,
        //Owner = 8,
        //Custom = 10,
        //SayTo = 11,
        //Broadcast = 0xFF
        }
    }
}
