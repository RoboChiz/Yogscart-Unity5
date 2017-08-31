using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditServer : MonoBehaviour
{
    public string ip = "127.0.0.1", serverName = "Private Server", password = "";
    public int port = 25000;

    private float guiAlpha = 0f;
    private GUISkin skin;
    private CurrentGameData gd;

    private ServerInfo serverInfo;

    //Scale Floats
    int currentSelection;
    float[] floatScales;
    float quitScale;

    private const string validCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ_0123456789 .";

    public void Setup(GUISkin _skin)
    {
        gd = FindObjectOfType<CurrentGameData>();
        skin = _skin;
        currentSelection = 0;
        floatScales = new float[5];
    }

    public void Setup(ServerInfo _serverInfo, GUISkin _skin)
    {
        Setup(_skin);
        ip = _serverInfo.ip;
        port = _serverInfo.port;
        serverName = _serverInfo.serverName;
        password = _serverInfo.password;
        serverInfo = _serverInfo;
    }

    public void Show()
    {
        StartCoroutine(FadeTo(1f));
    }

    public void Hide()
    {
        StartCoroutine(FadeTo(0f));
    }

    void OnGUI()
    {

        GUIHelper.SetGUIAlpha(guiAlpha);
        GUI.matrix = GUIHelper.GetMatrix();
        GUI.skin = skin;
        GUI.depth = -150;

        if (guiAlpha > 0)
        {
            Color rectColour = new Color(0.3f, 0.3f, 0.3f, 0.3f * guiAlpha);

            if (!Cursor.visible && InputManager.controllers[0].inputType != InputType.Keyboard && currentSelection < 0) currentSelection = 0;

            for(int i = 0; i < floatScales.Length; i++)
            {
                if (currentSelection == i)
                    floatScales[i] = Mathf.Clamp(floatScales[i] + (Time.deltaTime * 3f), 1f, 1.2f);
                else
                    floatScales[i] = Mathf.Clamp(floatScales[i] - (Time.deltaTime * 3f), 1f, 1.2f);
            }

            //Background
            Rect backgroundRect = new Rect(260, 100, 1400, 880);
            GUIShape.RoundedRectangle(GUIHelper.CentreRect(backgroundRect, guiAlpha), 20, new Color(52 / 255f, 152 / 255f, 219 / 255f, guiAlpha));

            GUIStyle centre = new GUIStyle(skin.label);
            centre.alignment = TextAnchor.MiddleCenter;
            centre.fontSize = (int)(centre.fontSize * guiAlpha);

            //Draw an X in the corner           
            Rect xButton = GUIHelper.RectScaledbyOtherRect(GUIHelper.CentreRect(new Rect(1600, 110, 50, 50), quitScale), backgroundRect, guiAlpha);

            if (xButton.Contains(GUIHelper.GetMousePosition()))
                quitScale = Mathf.Clamp(quitScale + (Time.deltaTime * 3f), 0.8f, 1f);
            else
                quitScale = Mathf.Clamp(quitScale - (Time.deltaTime * 3f), 0.8f, 1f);

            GUIShape.RoundedRectangle(xButton, 10, rectColour);
            GUI.Label(xButton, "X", centre);

            if (GUI.Button(xButton, ""))
                CloseWindow();

            int currentY = 110;
            Rect serverNameLabelRect = GUIHelper.RectScaledbyOtherRect(new Rect(280, currentY, 1360, 50), backgroundRect, guiAlpha);
            currentY += 60;
            Rect serverNameRect = GUIHelper.RectScaledbyOtherRect(new Rect(280, currentY, 1360, 100), backgroundRect, guiAlpha);
            currentY += 110;

            Rect ipLabelRect = GUIHelper.RectScaledbyOtherRect(new Rect(280, currentY, 1360, 50), backgroundRect, guiAlpha);
            currentY += 60;
            Rect ipRect = GUIHelper.RectScaledbyOtherRect(new Rect(280, currentY, 1360, 100), backgroundRect, guiAlpha);
            currentY += 110;

            Rect portLabelRect = GUIHelper.RectScaledbyOtherRect(new Rect(280, currentY, 1360, 50), backgroundRect, guiAlpha);
            currentY += 60;
            Rect portRect = GUIHelper.RectScaledbyOtherRect(new Rect(280, currentY, 1360, 100), backgroundRect, guiAlpha);
            currentY += 110;

            Rect passwordLabelRect = GUIHelper.RectScaledbyOtherRect(new Rect(280, currentY, 1360, 50), backgroundRect, guiAlpha);
            currentY += 60;
            Rect passwordRect = GUIHelper.RectScaledbyOtherRect(new Rect(280, currentY, 1360, 100), backgroundRect, guiAlpha);
            currentY += 110;

            GUIShape.RoundedRectangle(serverNameRect, 10, rectColour);
            GUIShape.RoundedRectangle(ipRect, 10, rectColour);
            GUIShape.RoundedRectangle(portRect, 10, rectColour);
            GUIShape.RoundedRectangle(passwordRect, 10, rectColour);

            //Keyboard Controls
            if (InputManager.controllers[0].inputType == InputType.Keyboard)
            {
                //Server Name
                GUI.Label(serverNameLabelRect, "Server Name:", centre);

                centre.normal.textColor = (currentSelection == 0) ? Color.yellow : Color.white;
                string tempServerName = GUI.TextField(serverNameRect, serverName, 30, centre);
                centre.normal.textColor = Color.white;

                //Ip Address
                GUI.Label(ipLabelRect, "IP Address:", centre);

                centre.normal.textColor = (currentSelection == 1) ? Color.yellow : Color.white;
                string tempIp = ip;
                if (!gd.streamMode)
                    tempIp = GUI.TextField(ipRect, ip, 45, centre);
                else
                    tempIp = GUI.PasswordField(ipRect, ip, '*', 45, centre);
                centre.normal.textColor = Color.white;

                //Port
                GUI.Label(portLabelRect, "Port:", centre);

                centre.normal.textColor = (currentSelection == 2) ? Color.yellow : Color.white;
                string tempPort = GUI.TextField(portRect, port.ToString(), 7, centre);
                int.TryParse(tempPort, out port);
                centre.normal.textColor = Color.white;

                //Password
                GUI.Label(passwordLabelRect, "Password (Leave Blank for no password):", centre);

                centre.normal.textColor = (currentSelection == 3) ? Color.yellow : Color.white;
                string tempPassword = GUI.TextField(passwordRect, password, 25, centre);
                centre.normal.textColor = Color.white;

                //Check all characters are valid
                if (ValidString(tempServerName))
                    serverName = tempServerName;

                if (ValidString(tempIp))
                    ip = tempIp;

                if (ValidString(tempPassword))
                    password = tempPassword;
            }

            //Save Button
            Rect saveButton = GUIHelper.RectScaledbyOtherRect(GUIHelper.CentreRect(new Rect(460, currentY + 50, 1000, 100), floatScales[4]), backgroundRect, guiAlpha);

            if (Cursor.visible)
            {
                if (saveButton.Contains(GUIHelper.GetMousePosition()))
                    currentSelection = 4;
                else
                    currentSelection = -1;
            }
            else if (InputManager.controllers[0].inputType == InputType.Keyboard)
                currentSelection = -1;

            GUIShape.RoundedRectangle(saveButton, 10, rectColour);
            centre.normal.textColor = (currentSelection == 4) ? Color.yellow : Color.white;
            GUI.Label(saveButton, "Save", centre);
            if(GUI.Button(saveButton,""))
            {
                SaveServer();
            }
        }
    }

    private bool ValidString(string _string)
    {
        bool invalid = false;

        for (int i = 0; i < _string.Length; i++)
        {
            string toUpper = _string[i].ToString().ToUpper();
            if (!validCharacters.Contains(toUpper))
            {
                invalid = true;
                break;
            }
        }

        return !invalid;
    }

    void Update()
    {
        if (guiAlpha == 1f)
        {
            int vertical = InputManager.controllers[0].GetIntInputWithLock("MenuVertical");
            bool submitBool = InputManager.controllers[0].GetButtonWithLock("Submit");
            bool cancelBool = InputManager.controllers[0].GetButtonWithLock("Cancel");

            if(cancelBool)
            {
                CloseWindow();
            }

            if(submitBool)
            {
                if (currentSelection == 4 || InputManager.controllers[0].inputType == InputType.Keyboard)
                    SaveServer();
            }

            if(InputManager.controllers[0].inputType != InputType.Keyboard)
            {
                currentSelection = MathHelper.NumClamp(currentSelection + vertical, 0, 5);
            }
        }
    }

    private void CloseWindow()
    {
        Hide();
        FindObjectOfType<NetworkSelection>().locked = false;
        StartCoroutine(KillThis());
    }

    private void SaveServer()
    {
        if (serverInfo == null)
        {
            serverInfo = new ServerInfo(ip, port, password, serverName, "", 0);
        }
        else
        {
            serverInfo.ip = ip;
            serverInfo.port = port;
            serverInfo.password = password;
            serverInfo.serverName = serverName;
            serverInfo.description = "";
            serverInfo.currentPlayers = 0;
        }

        NetworkSelection ns = FindObjectOfType<NetworkSelection>();

        if (!ns.servers.Contains(serverInfo))
            ns.servers.Add(serverInfo);

        ns.SaveServers();

        CloseWindow();
    }

    private IEnumerator FadeTo(float value)
    {
        float startTime = Time.time, startVal = guiAlpha, travelTime = 0.5f;

        while (Time.time - startTime < travelTime)
        {
            guiAlpha = Mathf.Lerp(startVal, value, (Time.time - startTime) / travelTime);
            yield return null;
        }

        guiAlpha = value;
    }

    private IEnumerator KillThis()
    {
        while (guiAlpha > 0f)
        {
            yield return null;
        }

        Destroy(this);
    }
}
