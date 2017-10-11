using UnityEngine;
using System.Collections;
using Boo.Lang;

public enum ScreenType { Full, TopLeft, TopRight, BottomLeft, BottomRight, Top, Bottom };

public class KartInfo : MonoBehaviour
{

    public ScreenType screenPos = ScreenType.Full;

    public int position = -1;
    private int lastPosition = -1;
    public bool hidden = true;
    private bool flashing, shrunk;
    private float raceGUIAlpha, PosGUISize;

    private int lapisCount = 0, lap = 0;

    [HideInInspector]
    public Camera[] cameras;

    //Finish Stuff
    public bool finishShow;
    public float finishAlpha;
    public Rect finishRect, screenRect;

    //Components
    private TrackData td;
    private SoundManager sm;

    //Textures
    Texture2D incomingBox;

    //Incoming
    public List<IncomingObject> incomingObjects = new List<IncomingObject>();

    void Start()
    {
        td = FindObjectOfType<TrackData>();
        sm = FindObjectOfType<SoundManager>();

        incomingBox = Resources.Load<Texture2D>("UI/Power Ups/Incoming");
    }

    // Update is called once per frame
    void OnGUI()
    {
        GUI.skin = Resources.Load<GUISkin>("GUISkins/Main Menu");

        lapisCount = GetComponent<KartMovement>().lapisAmount;
        lap = GetComponent<PositionFinding>().lap;
        position = GetComponent<PositionFinding>().racePosition;

        //Affect Alpha and Size based on booleans
        if (hidden)
            raceGUIAlpha = Mathf.Lerp(raceGUIAlpha, 0f, Time.deltaTime * 2f);
        else
            raceGUIAlpha = Mathf.Lerp(raceGUIAlpha, 1f, Time.deltaTime * 2f);

        if (shrunk)
            PosGUISize = Mathf.Lerp(PosGUISize, 0f, Time.deltaTime * 3f);
        else
        {
            if (screenPos == ScreenType.Full)
                PosGUISize = Mathf.Lerp(PosGUISize, Screen.height / 6f, Time.deltaTime * 3f);
            else
                PosGUISize = Mathf.Lerp(PosGUISize, Screen.height / 8f, Time.deltaTime * 3f);
        }

        //Play Position Animation
        if (lastPosition != position && !flashing)
        {
            StartCoroutine(FlashPos());
            flashing = true;
        }

        //Adjust Cameras to fit in the Required Space
        if (cameras != null)
            for (int i = 0; i < cameras.Length; i++)
            {
            if (screenPos == ScreenType.Full)
                cameras[i].rect = new Rect(0, 0, 1, 1);

            if (screenPos == ScreenType.TopLeft)
                cameras[i].rect = new Rect(0, 0.5f, 0.5f, 0.5f);

            if (screenPos == ScreenType.TopRight)
                cameras[i].rect = new Rect(0.5f, 0.5f, 0.5f, 0.5f);

            if (screenPos == ScreenType.BottomLeft)
                cameras[i].rect = new Rect(0, 0, 0.5f, 0.5f);

            if (screenPos == ScreenType.BottomRight)
                cameras[i].rect = new Rect(0.5f, 0, 0.5f, 0.5f);

            if (screenPos == ScreenType.Top)
                cameras[i].rect = new Rect(0, 0.5f, 1, 0.5f);

            if (screenPos == ScreenType.Bottom)
                cameras[i].rect = new Rect(0, 0, 1, 0.5f);
        }

        GUIHelper.SetGUIAlpha(raceGUIAlpha);

        Race gamemode = FindObjectOfType<Race>();

        if (gamemode != null)
        {
            if (gamemode is TimeTrial)
            {
                //Draw Timer
                GUI.Label(new Rect(Screen.width - 10 - Screen.width / 5f, Screen.height - 20 - GUI.skin.label.fontSize, Screen.width / 5f, GUI.skin.label.fontSize + 5), TimeManager.ToString(gamemode.Timer));
            }
            else
            { 
                //Render Position GUI
                if (position != -1)
                {
                    var postexture = Resources.Load<Texture2D>("UI/Positions/" + (lastPosition + 1).ToString());
                    Rect posRenderArea = new Rect();

                    if (screenPos == ScreenType.Full || screenPos == ScreenType.BottomRight || screenPos == ScreenType.Bottom)
                        posRenderArea = new Rect(Screen.width - 10 - PosGUISize, Screen.height - 10 - PosGUISize, PosGUISize, PosGUISize);
                    else if (screenPos == ScreenType.TopLeft)
                        posRenderArea = new Rect(Screen.width / 2f - 10 - PosGUISize, Screen.height / 2f - 10 - PosGUISize, PosGUISize, PosGUISize);
                    else if (screenPos == ScreenType.TopRight)
                        posRenderArea = new Rect(Screen.width - 10 - PosGUISize, Screen.height / 2f - 10 - PosGUISize, PosGUISize, PosGUISize);
                    else if (screenPos == ScreenType.BottomLeft)
                        posRenderArea = new Rect(Screen.width / 2f - 10 - PosGUISize, Screen.height - 10 - PosGUISize, PosGUISize, PosGUISize);
                    else if (screenPos == ScreenType.Top)
                        posRenderArea = new Rect(Screen.width - 10 - PosGUISize, Screen.height / 2f - 10 - PosGUISize, PosGUISize, PosGUISize);

                    if (postexture != null)
                        GUI.DrawTexture(posRenderArea, postexture, ScaleMode.ScaleToFit);
                }
            }
        }

        float boxWidth = 0f, boxHeight = 0f;

        if(screenPos == ScreenType.Full)
        {
            boxWidth = Screen.width / 10f;
            boxHeight = Screen.height / 16f;
        }
        else {
            boxWidth = Screen.width / 16f;
            boxHeight = Screen.height / 22f;
        }

        GUIStyle style = new GUIStyle(GUI.skin.GetStyle("Special Box"));

        style.fontSize = (int)((boxHeight + boxWidth) / 8f);

        //Calculate renderarea
        Rect renderArea = new Rect();

        if (screenPos == ScreenType.Full)
            renderArea = new Rect(10, Screen.height - 10 - boxHeight, boxWidth, boxHeight);

        if (screenPos == ScreenType.TopLeft || screenPos == ScreenType.Top)
            renderArea = new Rect(10, Screen.height / 2f - 10 - boxHeight, boxWidth, boxHeight);

        if (screenPos == ScreenType.TopRight)
            renderArea = new Rect(10 + Screen.width / 2f, Screen.height / 2f - 10 - boxHeight, boxWidth, boxHeight);

        if (screenPos == ScreenType.BottomLeft || screenPos == ScreenType.Bottom)
            renderArea = new Rect(10, Screen.height - 10 - boxHeight, boxWidth, boxHeight);

        if (screenPos == ScreenType.BottomRight)
            renderArea = new Rect(10 + Screen.width / 2f, Screen.height - 10 - boxHeight, boxWidth, boxHeight);

        GUI.Box(renderArea, "Lap : " + Mathf.Clamp(lap + 1, 1, td.Laps).ToString() + " / " + td.Laps, style);

        Texture2D LapisTexture = Resources.Load<Texture2D>("UI/Power Ups/Lapis");
        GUI.Box(new Rect(10 + renderArea.x + boxWidth, renderArea.y, boxWidth * 0.75f, boxHeight), "    " + lapisCount, style);
        GUI.DrawTexture(new Rect(10 + renderArea.x + boxWidth, renderArea.y, (boxHeight / LapisTexture.height) * LapisTexture.width, boxHeight), LapisTexture, ScaleMode.ScaleToFit);

        //Finish
        switch (screenPos)
        {
            case ScreenType.Full:
                screenRect = new Rect(0, 0, Screen.width, Screen.height);
                break;
            case ScreenType.TopLeft:
                screenRect = new Rect(0, 0, Screen.width / 2f, Screen.height / 2f);
                break;
            case ScreenType.TopRight:
                screenRect = new Rect(Screen.width / 2f, 0, Screen.width / 2f, Screen.height / 2f);
                break;
            case ScreenType.BottomLeft:
                screenRect = new Rect(0, Screen.height / 2f, Screen.width / 2f, Screen.height / 2f);
                break;
            case ScreenType.BottomRight:
                screenRect = new Rect(Screen.width / 2f, Screen.height / 2f, Screen.width / 2f, Screen.height / 2f);
                break;
            case ScreenType.Top:
                screenRect = new Rect(0, 0, Screen.width, Screen.height / 2f);
                break;
            case ScreenType.Bottom:
                screenRect = new Rect(0, Screen.height / 2f, Screen.width, Screen.height / 2f);
                break;
        }

        GUIHelper.SetGUIAlpha(finishAlpha);
        Texture2D finishTexture = Resources.Load<Texture2D>("UI/CountDown/Finish");

        if (finishTexture != null)
            GUI.DrawTexture(finishRect, finishTexture, ScaleMode.ScaleToFit);

        finishRect.y = Mathf.Lerp(finishRect.y, screenRect.y + (screenRect.height * 0.25f), Time.deltaTime);
        finishRect.height = Mathf.Lerp(finishRect.height, screenRect.height * 0.5f, Time.deltaTime);

        if (finishShow)
            finishAlpha = Mathf.Lerp(finishAlpha, 1f, Time.deltaTime * 7f);
        else
            finishAlpha = Mathf.Lerp(finishAlpha, 0f, Time.deltaTime * 7f);

        //Render Incoming Objects
        float iconSize = screenRect.height / 5f, tenthIconSize = iconSize / 10f,
            widthFactor = 500f / screenRect.width, halfWidth = screenRect.width/2f,
            incomingScale = (Mathf.Sin(Time.realtimeSinceStartup * 5f) * 0.25f) + 1f;
        foreach(IncomingObject ic in incomingObjects.ToArray())
        {
            //Update the Position
            if (ic.powerUpObject != null)
            {
                Vector3 localPos = transform.InverseTransformPoint(ic.powerUpObject.transform.position);
                ic.position = Mathf.Clamp(localPos.x / widthFactor, -halfWidth, halfWidth);
            }

            //Update the Alpha
            if (ic.powerUpObject != null)
                ic.alpha = Mathf.Clamp(ic.alpha + (Time.deltaTime * 4f), 0f, 1f);
            else
            {
                ic.alpha = Mathf.Clamp(ic.alpha - (Time.deltaTime * 4f), 0f, 1f);

                //Destroy Object if it no longer exists
                if (ic.alpha == 0)
                    incomingObjects.Remove(ic);
            }

            GUIHelper.SetGUIAlpha(ic.alpha);

            //Render the Indictor
            float centrePosX = screenRect.x + (screenRect.width / 2f) - (iconSize / 2f),
                posY = screenRect.y + screenRect.height - (iconSize * 1.2f);

            Rect indicatorRect = GUIHelper.CentreRect(new Rect(centrePosX + ic.position, posY, iconSize, iconSize), incomingScale);
            Rect itemRenderRect = GUIHelper.CentreRect(new Rect(centrePosX + ic.position + tenthIconSize, posY + tenthIconSize, iconSize - (2f* tenthIconSize), iconSize - (2f * tenthIconSize)), incomingScale);

            GUI.DrawTexture(indicatorRect, incomingBox);
            GUI.DrawTexture(itemRenderRect, ic.powerUp);
        }

        GUIHelper.ResetColor();

       
    }

    void NewLap()
    {
        sm.PlaySFX(Resources.Load<AudioClip>("Music & Sounds/SFX/newlap"));
    }

    IEnumerator FlashPos()
    {
        shrunk = true;
        yield return new WaitForSeconds(0.5f);

        lastPosition = position;
        shrunk = false;

        yield return new WaitForSeconds(0.5f);

        flashing = false;
    }

    public IEnumerator Finish()
    {
        finishRect = new Rect(screenRect.x, screenRect.y + (screenRect.height / 2f) - ((screenRect.height / 3f) / 2f), screenRect.width, screenRect.height / 3f);

        finishShow = true;
        yield return new WaitForSeconds(1.4f);
        finishShow = false;
    }

    public void NewAttack(Texture2D _powerUp, GameObject _powerUpObject)
    {
        incomingObjects.Add(new IncomingObject(_powerUp, _powerUpObject));
    }

}

public class IncomingObject
{
    public float position, alpha;
    public Texture2D powerUp;
    public GameObject powerUpObject;

    public IncomingObject(Texture2D _powerUp, GameObject _powerUpObject)
    {
        powerUp = _powerUp;
        powerUpObject = _powerUpObject;
    }
}
