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
            //EndClient("Disconnected from Server");
        }

        //------------------------------------------------------------------------------
        // Called when a network error occurs
        public override void OnClientError(NetworkConnection conn, int errorCode)
        {
            //EndClient("ERROR\n" + ((NetworkConnectionError)errorCode).ToString());
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
            stringMessage msg = netMsg.ReadMessage<stringMessage>();
            SceneManager.LoadSceneAsync(msg.value);
        }

        //------------------------------------------------------------------------------
        public virtual void EndClient(string message)
        {
            StopClient();

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
                StartCoroutine(DoCharacterSelect());
            }
            else if (msg.currentState == GameState.Lobby)
            {
                networkSelection.ChangeState(NetworkSelection.MenuState.Lobby);
            }
        }

        //------------------------------------------------------------------------------
        public IEnumerator DoCharacterSelect()
        {
            yield return null;

            client.Send(UnetMessages.playerInfoMsg, new EmptyMessage());
            networkSelection.ChangeState(NetworkSelection.MenuState.Lobby);
        }
    }
}
