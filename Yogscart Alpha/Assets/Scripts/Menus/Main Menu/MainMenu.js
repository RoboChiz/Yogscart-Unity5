#pragma strict

private var gd : CurrentGameData;

function Start () {
	
	//Load Libaries
	gd = GameObject.Find("GameData").GetComponent(CurrentGameData);
	
	//Select Random Level
	/*
	if(gd.Tournaments != null && gd.Tournaments.Length > 0)
	{
		var levelName : String;
		
		var tournamentID : int;
		var trackID : int;
		
		tournamentID = Random.Range(0,gd.Tournaments.Length);
		
		if(gd.Tournaments[tournamentID].Tracks != null)
		{
			trackID = Random.Range(0,gd.Tournaments[tournamentID].Tracks.Length);
			
			levelName = gd.Tournaments[tournamentID].Tracks[trackID].SceneID;
			yield Application.LoadLevelAdditiveAsync(levelName);
		}
	}*/
	
	yield WaitForSeconds(0.5f);		
	gd.BlackOut = false;

}

function OnGUI () {
	
	//Render the Background

}