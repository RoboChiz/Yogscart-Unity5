using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class GhostDataManager : MonoBehaviour
{
    private const string saveLocation = "/Ghost Data/";

    public Dictionary<string, GhostData> loadedGhostData { get; private set; }

    private void Start()
    {
        loadedGhostData = new Dictionary<string, GhostData>();
        LoadAllGhosts();
    }

    public void SaveGhost(GhostData _ghostData)
    {
        FileStream file = null;
        try
        {
            //Ensure Save Data has the correct values
            _ghostData.version = SaveData.saveVersion;

            //Create Variables needed to Save
            BinaryFormatter bf = new BinaryFormatter();
            System.DateTime now = System.DateTime.Now;

            //Set the File Location if it dosen't exist
            if(_ghostData.fileLocation == null || _ghostData.fileLocation == "")
            {
                _ghostData.fileLocation = Application.persistentDataPath + saveLocation + now.Day + "_" + now.Month + "_" + now.Year + "_" + now.Hour.ToString("00") + "_" + now.Minute.ToString("00") + ".GhostData";
            }

            file = File.Create(_ghostData.fileLocation);

            bf.Serialize(file, _ghostData);
            file.Close();
        }
        catch (System.Exception error)
        {
            InfoPopUp popUp = gameObject.AddComponent<InfoPopUp>();
            popUp.Setup(error.Message);
        }
        finally
        {
            if (file != null)
                file.Close();
        }
    }

    public void DeleteGhost(GhostData _ghostData)
    {
        File.Delete(_ghostData.fileLocation);

        loadedGhostData.Remove(_ghostData.fileLocation);
        FindObjectOfType<CurrentGameData>().tournaments[_ghostData.cup].tracks[_ghostData.track].ghostDatas.Remove(_ghostData.fileLocation);
    }

    public void LoadAllGhosts()
    {
        if (!Directory.Exists(Application.persistentDataPath + saveLocation))
            Directory.CreateDirectory(Application.persistentDataPath + saveLocation);

        //Check every file in Ghost Data folder
        var info = new DirectoryInfo(Application.persistentDataPath + saveLocation);
        var fileInfo = info.GetFiles();
        foreach (FileInfo file in fileInfo)
        {
            LoadGhost(file);
        }
    }

    private void LoadGhost(FileInfo file)
    {
        try
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream fileStream = file.Open(FileMode.Open);
            GhostData ghostData = (GhostData)bf.Deserialize(fileStream);
            fileStream.Close();

            bool loadedGhost = false;

            //Save File Location
            ghostData.fileLocation = Application.persistentDataPath + saveLocation + file.Name;

            if (ghostData.version != SaveData.saveVersion)
            {
                if(UpdateGhostData(ghostData))
                {
                    loadedGhost = true;
                }
            }
            else
            {
                loadedGhost = true;
            }

            if(loadedGhost)
            {
                //Save Ghost Data here and in the Current Game Database
                loadedGhostData[ghostData.fileLocation] = ghostData;            
                FindObjectOfType<CurrentGameData>().tournaments[ghostData.cup].tracks[ghostData.track].ghostDatas[ghostData.fileLocation] = ghostData;
            }
        }
        catch (System.Exception err)
        {
            Debug.Log("ERROR LOADING SAVE! " + err.Message);
        }
    }

    private bool UpdateGhostData(GhostData _ghostData)
    {
        //Ghost Data was updated successfully

        SaveGhost(_ghostData);

        return true;
    }
	
}
