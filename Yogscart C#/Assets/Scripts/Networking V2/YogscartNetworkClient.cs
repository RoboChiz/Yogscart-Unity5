using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine.Networking.NetworkSystem;

namespace YogscartNetwork
{
    public class Client : NetworkManager
    {
        protected CurrentGameData gd;
        protected NetworkSelection networkSelection;

        //If we're one of the people currentely racing
        public bool isRacing { get; protected set; }
        protected Coroutine characterSelectCoroutine;

        //Used by Timer
        public float timeLeft = -1f;
        private float rotation = 0f, timerSize = 0f;
        Texture2D timerIcon;

        //------------------------------------------------------------------------------
        //Timer Code
        public virtual void Update()
        {
            if (timeLeft > 0)
            {
                timeLeft -= Time.deltaTime;
                rotation += Time.deltaTime * 50f;
            }
        }

        public void OnGUI()
        {
            GUI.skin = Resources.Load<GUISkin>("GUISkins/Timer");

            if (timerIcon ==  null)
                timerIcon = Resources.Load<Texture2D>("UI/Main Menu/Timer");

            if (timeLeft > 0)
                timerSize = Mathf.Lerp(timerSize, 1f, Time.deltaTime * 3f);
            else
                timerSize = Mathf.Lerp(timerSize, 0f, Time.deltaTime * 3f);

            if (timerSize > 0)
            {
                int fontSize = (int)Mathf.Lerp(0f, 35f, timerSize);

                GUI.skin.label.fontSize = fontSize;
                Rect timerRect = GUIHelper.CentreRect(new Rect(20, -5, 80, 80), timerSize);

                GUIUtility.RotateAroundPivot(rotation, new Vector2(60, 35));
                GUI.DrawTexture(timerRect, timerIcon);
                GUIUtility.RotateAroundPivot(-rotation, new Vector2(60, 35));

                GUIHelper.OutLineLabel(timerRect, ((int)Mathf.Max(0, timeLeft)).ToString(), 1, Color.black);
            }

        }

        //------------------------------------------------------------------------------
        //Register the handlers required for the client
        public virtual void RegisterHandlers()
        {
            gd = FindObjectOfType<CurrentGameData>();
            networkSelection = FindObjectOfType<NetworkSelection>();

            //Stop Host from registering Client messages it should never use
            if (!NetworkServer.active)
            {
                client.RegisterHandler(UnetMessages.acceptedMsg, OnClientAccepted);
                client.RegisterHandler(UnetMessages.clientErrorMsg, OnCustomError);
                client.RegisterHandler(UnetMessages.addPlayerInfo, OnAddPlayerInfo);
                client.RegisterHandler(UnetMessages.clearPlayerInfo, OnClearPlayerInfo);
                client.RegisterHandler(UnetMessages.changeGamemode, OnChangeGamemode);
                client.RegisterHandler(UnetMessages.timerMsg, OnTimer);
                client.RegisterHandler(UnetMessages.clearMsg, OnClear);
                client.RegisterHandler(UnetMessages.returnLobbyMsg, OnReturnLobby);
                client.RegisterHandler(UnetMessages.raceGamemodeMsg, OnGamemodeRace);
            }

            //Messages for all Clients and Host

            //Register all Power Ups as Spawn Items
            foreach (PowerUp powerUp in gd.powerUps)
            {
                if (powerUp.onlineModel != null)
                    ClientScene.RegisterPrefab(powerUp.onlineModel.gameObject);
            }
        }

        //------------------------------------------------------------------------------
        // Called when connected to a server
        public override void OnClientConnect(NetworkConnection conn)
        {
            //Stop Host from sending information it dosen't need to...
            if (!NetworkServer.active)
            {
                //Tell Host what version we are
                VersionMessage myMsg = new VersionMessage();
                myMsg.version = gd.version;
                client.Send(UnetMessages.versionMsg, myMsg);
            }
        }

        //------------------------------------------------------------------------------
        // called when disconnected from a server
        public override void OnClientDisconnect(NetworkConnection conn)
        {
            EndClient("Disconnected from Server");
        }

        //------------------------------------------------------------------------------
        // Called when a network error occurs
        public override void OnClientError(NetworkConnection conn, int errorCode)
        {
            EndClient("ERROR\n" + ((NetworkConnectionError)errorCode).ToString());
        }

        //------------------------------------------------------------------------------
        // Called when a custom network error occurs
        public virtual void OnCustomError(NetworkMessage netMsg)
        {
            ClientErrorMessage msg = netMsg.ReadMessage<ClientErrorMessage>();
            EndClient(msg.message);
        }

        //------------------------------------------------------------------------------
        //Called when a Server wants a client to load a level by ID
        private void OnLoadLevelID(NetworkMessage netMsg)
        {
            StringMessage msg = netMsg.ReadMessage<StringMessage>();
            SceneManager.LoadSceneAsync(msg.value);
        }

        //------------------------------------------------------------------------------
        public virtual void EndClient(string message)
        {
            StopClient();

            //Stop Character Select if it's open
            if(characterSelectCoroutine != null)
                StopCoroutine(characterSelectCoroutine);

            CharacterSelect cs = FindObjectOfType<CharacterSelect>();
            if(cs.enabled)
            {
                cs.HideCharacterSelect(CharacterSelect.csState.Off);
            }

            VotingScreen vs = FindObjectOfType<VotingScreen>();
            if(vs != null)
            {
                vs.HideScreen();
            }

            //Lock the Pause Menu
            PauseMenu.canPause = false;
            FindObjectOfType<PauseMenu>().HidePause();

            if (message != null && message != "")
            {
                networkSelection.Popup(message);
            }
            else
            {
                networkSelection.ChangeState(NetworkSelection.MenuState.ServerList);
            }

            //Kill itself
            DestroyImmediate(GetComponent<YogscartNetwork.Client>());
        }

        //------------------------------------------------------------------------------
        // Called when a host accepts the client (Correct Version .etc)
        public void OnClientAccepted(NetworkMessage netMsg)
        {
            ClientScene.Ready(netMsg.conn);

            AcceptedMessage msg = netMsg.ReadMessage<AcceptedMessage>();

            if (msg.playerUp)
            {
                //Select a character if we've been asked to
                StartCoroutine(ActualDoCharacterSelect());
            }
            else if (msg.currentState == GameState.Lobby)
            {
                networkSelection.ChangeState(NetworkSelection.MenuState.Lobby);
            }
        }

        //------------------------------------------------------------------------------
        public void DoCharacterSelect()
        {
            characterSelectCoroutine = StartCoroutine(ActualDoCharacterSelect());
        }

        private IEnumerator ActualDoCharacterSelect()
        {
            CharacterSelect cs = FindObjectOfType<CharacterSelect>();
            cs.enabled = true;

            networkSelection.ChangeState(NetworkSelection.MenuState.CharacterSelect);
            yield return cs.StartCoroutine("ShowCharacterSelect", CharacterSelect.csState.Character);

            //Wait until all characters have been selected
            while (cs.State != CharacterSelect.csState.Finished && cs.State != CharacterSelect.csState.Off)
            {
                yield return null;
            }

            characterSelectCoroutine = null;

            if (cs.State == CharacterSelect.csState.Off)
            {
                Debug.Log("Didn't make it through the Character Select!");
                if (!isRacing) //Don't send the rejection message if we've correctely set our character select before
                {                    
                    client.Send(UnetMessages.rejectPlayerUpMsg, new EmptyMessage());
                    yield break;
                }
            }
            else
            {
                PlayerInfoMessage myMSG = new PlayerInfoMessage();
                myMSG.character = CurrentGameData.currentChoices[0].character;
                myMSG.hat = CurrentGameData.currentChoices[0].hat;
                myMSG.kart = CurrentGameData.currentChoices[0].kart;
                myMSG.wheel = CurrentGameData.currentChoices[0].wheel;
                myMSG.displayName = gd.playerName;

                if (!isRacing) //If first time sending
                {
                    client.Send(UnetMessages.playerInfoMsg, myMSG);
                    isRacing = true;
                }
                else
                {
                    //Check for Host
                    if (!NetworkServer.active)
                    {
                        client.Send(UnetMessages.playerInfoUpdateMsg, myMSG);
                    }
                    else
                    {
                        Host hostComp = this as Host;
                        hostComp.UpdatePlayerInfo(hostComp.finalPlayers[0], new LoadOut(myMSG.character, myMSG.hat, myMSG.kart, myMSG.wheel));
                        hostComp.UpdatePlayerInfoList();
                    }
                    isRacing = true;
                }
            }

            networkSelection.ChangeState(NetworkSelection.MenuState.Lobby);
        }

        //------------------------------------------------------------------------------
        public void OnAddPlayerInfo(NetworkMessage netMsg) { PlayerInfoMessage msg = netMsg.ReadMessage<PlayerInfoMessage>(); OnAddPlayerInfo(new PlayerInfo(msg)); }
        public void OnAddPlayerInfo(PlayerInfo _playerInfo)
        {
            networkSelection.playerList.Add(_playerInfo);
        }

        //------------------------------------------------------------------------------
        public void OnClearPlayerInfo(NetworkMessage netMsg) { OnClearPlayerInfo(); }
        public void OnClearPlayerInfo()
        {
            networkSelection.playerList = new List<PlayerInfo>();
        }

        //------------------------------------------------------------------------------
        public void OnChangeGamemode(NetworkMessage netMsg) { IntMessage msg = netMsg.ReadMessage<IntMessage>(); OnChangeGamemode(msg.value); }
        public void OnChangeGamemode(int _newGamemode)
        {
            networkSelection.currentGamemode = _newGamemode;
        }

        //------------------------------------------------------------------------------
        public void OnTimer(NetworkMessage netMsg) { IntMessage msg = netMsg.ReadMessage<IntMessage>(); OnTimer(msg.value); }
        public void OnTimer(int _newTimweVal)
        {           
            timeLeft = _newTimweVal;
        }

        //------------------------------------------------------------------------------
        public void OnClear(NetworkMessage netMsg) { OnClear(); }
        public void OnClear()
        {
            //Stop Character Select if it's open
            if (characterSelectCoroutine != null)
                StopCoroutine(characterSelectCoroutine);

            CharacterSelect cs = FindObjectOfType<CharacterSelect>();
            if (cs != null && cs.enabled)
            {
                cs.HideCharacterSelect(CharacterSelect.csState.Off);
            }

            //Stop Level Select if it's open
            LevelSelect ls = FindObjectOfType<LevelSelect>();
            if(ls != null && ls.enabled)
            {
                ls.CancelLevelSelect();
                ls.enabled = false;
            }

            networkSelection.ChangeState(NetworkSelection.MenuState.Gamemode);
        }

        //------------------------------------------------------------------------------
        public void OnReturnLobby(NetworkMessage netMsg) { OnReturnLobby(); }
        public void OnReturnLobby()
        {
            //Clear up Current Gamemode

            //Load the lobby level

            //Go to the lobby
            networkSelection.ChangeState(NetworkSelection.MenuState.Lobby);

        }

        //------------------------------------------------------------------------------
        // Gamemode Setup
        //------------------------------------------------------------------------------
        public void OnGamemodeRace(NetworkMessage netMsg) { OnGamemodeRace(); }
        public GameMode OnGamemodeRace()
        {
            NetworkRace comp = gameObject.AddComponent<NetworkRace>();
            comp.StartGameMode();

            return comp;
        }

        public void OnGamemodeBattle(NetworkMessage netMsg) { OnGamemodeBattle(); }
        public GameMode OnGamemodeBattle()
        {
            return null;
        }

        public void OnGamemodeYogball(NetworkMessage netMsg) { OnGamemodeYogball(); }
        public GameMode OnGamemodeYogball()
        {
            return null;
        }
    }
}
