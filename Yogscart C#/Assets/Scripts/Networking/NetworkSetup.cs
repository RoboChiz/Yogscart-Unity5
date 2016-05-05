using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class NetworkSetup : MonoBehaviour
{

    public string ipAddress = "localhost", portString = "43";
    public int port = 43;

	// Update is called once per frame
	void OnGUI ()
    {

        GUI.Label(new Rect(10, 10, 200, 20), "IP Address:");
        ipAddress = GUI.TextField(new Rect(10, 30, 200, 20), ipAddress);

        int testPort;
        if (int.TryParse(portString, out testPort))
            port = testPort;
        else
            portString = port.ToString();

        GUI.Label(new Rect(10, 60, 200, 20), "Port:");
        portString = GUI.TextField(new Rect(10, 80, 200, 20), portString);

        if(GUI.Button(new Rect(10,110,200,20),"Join Game"))
        {
            DeleteExistingNetworkManager();
            UnetClient client = FindObjectOfType<CurrentGameData>().gameObject.AddComponent<UnetClient>();

            client.networkAddress = ipAddress;
            client.networkPort = port;

            client.StartClient();
        }

        if (GUI.Button(new Rect(10, 140, 200, 20), "Host Game"))
        {
            DeleteExistingNetworkManager();
            UnetHost host = FindObjectOfType<CurrentGameData>().gameObject.AddComponent<UnetHost>();

            host.networkAddress = ipAddress;
            host.networkPort = port;

            host.StartHost();

        }

        if (GUI.Button(new Rect(10, 170, 200, 20), "Host Server Only"))
        {
            DeleteExistingNetworkManager();
            UnetHost host = FindObjectOfType<CurrentGameData>().gameObject.AddComponent<UnetHost>();

            host.networkAddress = ipAddress;
            host.networkPort = port;

            host.StartServer();
        }


    }

    private void DeleteExistingNetworkManager()
    {
        NetworkManager[] networkManagers = FindObjectsOfType<NetworkManager>();
        foreach(NetworkManager nm in networkManagers)
        {
            Destroy(nm);
        }
    }
}
