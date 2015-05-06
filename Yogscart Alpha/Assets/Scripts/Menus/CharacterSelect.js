#pragma strict

private var gd : CurrentGameData;
private var im : InputManager;
private var sm : Sound_Manager;
private var km : KartMaker;

var hidden : boolean;

enum csState {Character,Hat,Kart,Off};
var state : csState = csState.Off;

private var cursorPosition : Vector2[];
var cursorSpeed : float = 2.5;
var rotateSpeed : float = 2.5;

var Platforms : Transform[];

private var ready : boolean[];
private var kartSelected : boolean[];

private var loadedChoice : LoadOut[];
private var loadedModels : Transform[];

//Content
private var nameList : Texture2D;

private var mouseSelecting : boolean;

function Start () {
	gd = GameObject.Find("GameData").GetComponent(CurrentGameData);
	im = GameObject.Find("GameData").GetComponent(InputManager);
	sm = GameObject.Find("Sound System").GetComponent(Sound_Manager); 
	km = GameObject.Find("GameData").GetComponent(KartMaker);
	
	//state = csState.Off;
	
	cursorPosition = new Vector2[4];
	
	ResetEverything();
	
	//Load Content
	nameList = Resources.Load("UI/Lobby/NamesList",Texture2D);
	
}

function OnGUI()
{
	
	GUI.skin = Resources.Load("Font/Menu",GUISkin);
	
	GUI.depth = -5;

	var chunkSize = Screen.width/10f;
	
	var choice = gd.currentChoices;
	
	var nameListRect : Rect;
	
	var mouseClick = im.GetClick();

	switch(state)
	{
		case(csState.Character):
		
		nameListRect = Rect(10,chunkSize/2f,chunkSize*5f,Screen.height - chunkSize*1.5f);
		GUI.DrawTexture(nameListRect,nameList);
		
		//Draw Character Heads		
		mouseSelecting = false;
		var choicesPerColumn : float = ((gd.Characters.Length / 5f)/5f) * 5f;
		for(var i : int = 0; i < choicesPerColumn;i++){
		
			for(var j : int = 0; j < 5;j++){
			
				var characterInt : int = (i*5) + j;
			
				if(characterInt < gd.Characters.Length){
				
					var iconRect : Rect = Rect(10 + (j*chunkSize),nameListRect.y + (i*chunkSize),chunkSize,chunkSize);
					var icon : Texture2D;
					
					if(gd.Characters[characterInt].Unlocked != UnlockedState.Locked)
						icon = gd.Characters[characterInt].Icon;
					else
						icon = Resources.Load("UI Textures/Character Icons/question_mark",Texture2D);

					GUI.DrawTexture(iconRect,icon);
					
					if(im.MouseIntersects(iconRect))
					{
						choice[im.keyboardPlayer].character = (i*5) + j;
						mouseSelecting = true;
					}

				}else{
					break;
				}
			}
		}	
		break;
		
		case(csState.Hat):
		
			nameListRect = Rect(10,chunkSize/2f,chunkSize*5f,Screen.height - chunkSize*1.5f);
			GUI.DrawTexture(nameListRect,nameList);
			
			//Draw Character Heads		
			mouseSelecting = false;
			choicesPerColumn = ((gd.Hats.Length / 5f)/5f) * 5f;
			for(i = 0; i < choicesPerColumn;i++){
			
				for(j = 0; j < 5;j++){
				
					var hatInt : int = (i*5) + j;
				
					if(hatInt < gd.Hats.Length){
					
						iconRect = Rect(10 + (j*chunkSize),nameListRect.y + (i*chunkSize),chunkSize,chunkSize);
						
						if(gd.Hats[hatInt].Unlocked)
							icon = gd.Hats[hatInt].Icon;
						else
							icon = Resources.Load("UI Textures/Character Icons/question_mark",Texture2D);
						
						GUI.DrawTexture(iconRect,icon);
						
						if(im.MouseIntersects(iconRect))
						{
							choice[im.keyboardPlayer].hat = (i*5) + j;
							mouseSelecting = true;
						}


					}else{
						break;
					}
				}
			}
				
		break;
		
	}
	
	
	var readyCheck : boolean = true;
	
	for(var s : int = 0; s < im.c.Length; s++)
	{
	
		//Load Character
		
		if(state == csState.Character || state == csState.Hat)
		{
			if(loadedChoice[s].character != choice[s].character)
			{
				var oldRot : Quaternion;
				if(loadedModels[s] != null){
					oldRot = loadedModels[s].rotation;
					Destroy(loadedModels[s].gameObject);
				}else
					oldRot = Quaternion.identity;
					
				if(gd.Characters[choice[s].character].Unlocked != UnlockedState.Locked){
					loadedModels[s] = Instantiate(gd.Characters[choice[s].character].CharacterModel_Standing,Platforms[s].FindChild("Spawn").position,oldRot);
					loadedModels[s].GetComponent.<Rigidbody>().isKinematic = true;
				}
				
				loadedChoice[s].character = choice[s].character;
				
			}
		
			loadedChoice[s].kart = -1;
			loadedChoice[s].wheel = -1;
			
			if(loadedChoice[s].hat != choice[s].hat)
			{
				if(loadedModels[s].GetComponent(StandingCharacter)!= null)
				{
					var allChildren = loadedModels[s].GetComponent(StandingCharacter).hatHolder.GetComponentsInChildren(Transform);
					for(var ch : int = 0; ch < allChildren.Length; ch++)
					{	
						if(allChildren[ch] != loadedModels[s].GetComponent(StandingCharacter).hatHolder)
							Destroy(allChildren[ch].gameObject);
					}
				}
				
				if(gd.Hats[choice[s].hat].Model != null && gd.Hats[choice[s].hat].Unlocked)
				{
					var HatObject = Instantiate(gd.Hats[choice[s].hat].Model,loadedModels[s].GetComponent(StandingCharacter).hatHolder.position,loadedModels[s].GetComponent(StandingCharacter).hatHolder.rotation);
					HatObject.parent = loadedModels[s].GetComponent(StandingCharacter).hatHolder;
				}
				
				loadedChoice[s].character = choice[s].character;
				loadedChoice[s].hat = choice[s].hat;
				
			}
		}
		
		if(state == csState.Kart)
		{
		
			loadedChoice[s].character = -1;
			loadedChoice[s].hat = -1;
		
			if(loadedChoice[s].kart != choice[s].kart || loadedChoice[s].wheel != choice[s].wheel)
			{
				if(loadedModels[s] != null){
					oldRot = loadedModels[s].rotation;
					Destroy(loadedModels[s].gameObject);
				}else
					oldRot = Quaternion.identity;
					
 				loadedModels[s] = km.SpawnKart(KartType.Display,Platforms[s].FindChild("Spawn").position + Vector3.up/2f,oldRot,choice[s].kart,choice[s].wheel,choice[s].character,choice[s].hat);
					
				loadedChoice[s].kart = choice[s].kart;
				loadedChoice[s].wheel = choice[s].wheel;
			
			}
		}
		
		var oldRect : Vector4;
		var newRect : Vector4;
		var nRect : Vector4;
		var cam : Camera;

		//Default off screen
		if(im.c.Length == 0 || hidden){
			cam = Platforms[0].FindChild("Camera").GetComponent.<Camera>();
			oldRect = Vector4(cam.rect.x,cam.rect.y,cam.rect.width,cam.rect.height);
			newRect = Vector4(1.5,0,oldRect.z,oldRect.w);
			nRect = Vector4.Lerp(oldRect,newRect,Time.deltaTime*5f);
			cam.rect = Rect(nRect.x,nRect.y,nRect.z,nRect.w);
		}

		if(im.c.Length <= 1 || hidden){
			cam = Platforms[1].FindChild("Camera").GetComponent.<Camera>();
			oldRect = Vector4(cam.rect.x,cam.rect.y,cam.rect.width,cam.rect.height);
			newRect = Vector4(1.5,0,oldRect.z,oldRect.w);
			nRect = Vector4.Lerp(oldRect,newRect,Time.deltaTime*5f);
			cam.rect = Rect(nRect.x,nRect.y,nRect.z,nRect.w);
		}

		if(im.c.Length <= 2 || hidden){
			cam = Platforms[2].FindChild("Camera").GetComponent.<Camera>();
			oldRect = Vector4(cam.rect.x,cam.rect.y,cam.rect.width,cam.rect.height);
			newRect = Vector4(1.5,0,oldRect.z,oldRect.w);
			nRect = Vector4.Lerp(oldRect,newRect,Time.deltaTime*5f);
			cam.rect = Rect(nRect.x,nRect.y,nRect.z,nRect.w);
		}

		if(im.c.Length <= 3 || hidden){
			cam = Platforms[3].FindChild("Camera").GetComponent.<Camera>();
			oldRect = Vector4(cam.rect.x,cam.rect.y,cam.rect.width,cam.rect.height);
			newRect = Vector4(1.5,0,oldRect.z,oldRect.w);
			nRect = Vector4.Lerp(oldRect,newRect,Time.deltaTime*5f);
			cam.rect = Rect(nRect.x,nRect.y,nRect.z,nRect.w);
		}

		if(!hidden){

			if(im.c.Length == 1){
				cam = Platforms[0].FindChild("Camera").GetComponent.<Camera>();
				oldRect = Vector4(cam.rect.x,cam.rect.y,cam.rect.width,cam.rect.height);
				newRect = Vector4(0.5,0,0.5,1);
				nRect = Vector4.Lerp(oldRect,newRect,Time.deltaTime*5f);
				cam.rect = Rect(nRect.x,nRect.y,nRect.z,nRect.w);
			}

			if(im.c.Length == 2){
				cam = Platforms[0].FindChild("Camera").GetComponent.<Camera>();
				oldRect = Vector4(cam.rect.x,cam.rect.y,cam.rect.width,cam.rect.height);
				newRect = Vector4(0.5,0.5,0.5,0.5);
				nRect = Vector4.Lerp(oldRect,newRect,Time.deltaTime*5f);
				cam.rect = Rect(nRect.x,nRect.y,nRect.z,nRect.w);

				cam = Platforms[1].FindChild("Camera").GetComponent.<Camera>();
				oldRect = Vector4(cam.rect.x,cam.rect.y,cam.rect.width,cam.rect.height);
				newRect = Vector4(0.5,0,0.5,0.5);
				nRect = Vector4.Lerp(oldRect,newRect,Time.deltaTime*5f);
				cam.rect = Rect(nRect.x,nRect.y,nRect.z,nRect.w);
			}



			if(im.c.Length == 3){
				cam = Platforms[0].FindChild("Camera").GetComponent.<Camera>();
				oldRect = Vector4(cam.rect.x,cam.rect.y,cam.rect.width,cam.rect.height);
				newRect = Vector4(0.5,0.5,0.25,0.5);
				nRect = Vector4.Lerp(oldRect,newRect,Time.deltaTime*5f);
				cam.rect = Rect(nRect.x,nRect.y,nRect.z,nRect.w);

				cam = Platforms[1].FindChild("Camera").GetComponent.<Camera>();
				oldRect = Vector4(cam.rect.x,cam.rect.y,cam.rect.width,cam.rect.height);
				newRect = Vector4(0.75,0.5,0.25,0.5);
				nRect = Vector4.Lerp(oldRect,newRect,Time.deltaTime*5f);
				cam.rect = Rect(nRect.x,nRect.y,nRect.z,nRect.w);

				cam = Platforms[2].FindChild("Camera").GetComponent.<Camera>();
				oldRect = Vector4(cam.rect.x,cam.rect.y,cam.rect.width,cam.rect.height);
				newRect = Vector4(0.5,0,0.25,0.5);
				nRect = Vector4.Lerp(oldRect,newRect,Time.deltaTime*5f);
				cam.rect = Rect(nRect.x,nRect.y,nRect.z,nRect.w);
			}

			if(im.c.Length == 4){
				cam = Platforms[0].FindChild("Camera").GetComponent.<Camera>();
				oldRect = Vector4(cam.rect.x,cam.rect.y,cam.rect.width,cam.rect.height);
				newRect = Vector4(0.5,0.5,0.25,0.5);
				nRect = Vector4.Lerp(oldRect,newRect,Time.deltaTime*5f);
				cam.rect = Rect(nRect.x,nRect.y,nRect.z,nRect.w);

				cam = Platforms[1].FindChild("Camera").GetComponent.<Camera>();
				oldRect = Vector4(cam.rect.x,cam.rect.y,cam.rect.width,cam.rect.height);
				newRect = Vector4(0.75,0.5,0.25,0.5);
				nRect = Vector4.Lerp(oldRect,newRect,Time.deltaTime*5f);
				cam.rect = Rect(nRect.x,nRect.y,nRect.z,nRect.w);

				cam = Platforms[2].FindChild("Camera").GetComponent.<Camera>();
				oldRect = Vector4(cam.rect.x,cam.rect.y,cam.rect.width,cam.rect.height);
				newRect = Vector4(0.5,0,0.25,0.5);
				nRect = Vector4.Lerp(oldRect,newRect,Time.deltaTime*5f);
				cam.rect = Rect(nRect.x,nRect.y,nRect.z,nRect.w);

				cam = Platforms[3].FindChild("Camera").GetComponent.<Camera>();
				oldRect = Vector4(cam.rect.x,cam.rect.y,cam.rect.width,cam.rect.height);
				newRect = Vector4(0.75,0,0.25,0.5);
				nRect = Vector4.Lerp(oldRect,newRect,Time.deltaTime*5f);
				cam.rect = Rect(nRect.x,nRect.y,nRect.z,nRect.w);
			}
		}
		
		if(loadedModels[s] != null)
			loadedModels[s].Rotate(Vector3.up,-im.c[s].GetInput("Rotate") * Time.deltaTime * rotateSpeed);
		
		
		//Get Inputs

		var hori : float = im.c[s].GetMenuInput("Horizontal");
		var vert : float = -im.c[s].GetMenuInput("Vertical");
		var submit : boolean = (im.c[s].GetMenuInput("Submit") != 0);
		var cancel : boolean = (im.c[s].GetMenuInput("Cancel") != 0);
		
		if(hori != 0)
		{
		
			var amount : int = Mathf.Sign(hori);
			
			if(state == csState.Character)
			{
			
				var itemOnRow : int = 5;
				var itemsLeft : int = gd.Characters.Length%5;
				
				if(itemsLeft != 0 && choice[s].character >= gd.Characters.Length - itemsLeft)
					itemOnRow = itemsLeft;
					
				if((choice[s].character%5) + amount >= itemOnRow)
					choice[s].character -= itemOnRow-1;
				else if((choice[s].character%5) + amount < 0)
					choice[s].character += itemOnRow-1;
				else
					choice[s].character += Mathf.Sign(hori);
			}
			
			if(state == csState.Hat)
			{
			
				itemOnRow = 5;
				itemsLeft = gd.Hats.Length%5;
				
				if(itemsLeft != 0 && choice[s].hat >= gd.Hats.Length - itemsLeft)
					itemOnRow = itemsLeft;
					
				if((choice[s].hat%5) + amount >= itemOnRow)
					choice[s].hat -= itemOnRow-1;
				else if((choice[s].hat%5) + amount < 0)
					choice[s].hat += itemOnRow-1;
				else
					choice[s].hat += Mathf.Sign(hori);
			}
				
		}
		
		if(vert != 0)
		{
			if(state != csState.Kart)
				amount = Mathf.Sign(vert) * 5;
			else
				amount = Mathf.Sign(vert);
		
			if(state == csState.Character)
			{
					
				itemsLeft = gd.Characters.Length%5;
				var rowNumber : int = gd.Characters.Length/5; 
				if(itemsLeft != 0)
					rowNumber ++;
					
				if(choice[s].character + amount >= gd.Characters.Length)
					choice[s].character = (choice[s].character + amount)%5;
				else if(choice[s].character + amount < 0)
				{
					var toAdd : int = choice[s].character + ((rowNumber-1)*5);
					if(toAdd >= gd.Characters.Length)
					{
						choice[s].character = toAdd - 5;
					}
					else 
					{
						choice[s].character = toAdd;
					}	
						
				}
				else 
					choice[s].character += amount;
			}
			
			if(state == csState.Hat)
			{
					
				itemsLeft = gd.Hats.Length%5;
				rowNumber = gd.Hats.Length/5; 
				
				if(itemsLeft != 0)
					rowNumber ++;
					
				if(choice[s].hat + amount >= gd.Hats.Length)
					choice[s].hat = (choice[s].hat + amount)%5;
				else if(choice[s].hat + amount < 0)
				{
					toAdd = choice[s].hat + ((rowNumber-1)*5);
					if(toAdd >= gd.Hats.Length)
					{
						choice[s].hat = toAdd - 5;
					}
					else 
					{
						choice[s].hat = toAdd;
					}	
						
				}
				else 
					choice[s].hat += amount;
			}
			
			if(state == csState.Kart)
			{
				if(!kartSelected[s])
				{
					choice[s].kart += amount;
					
					if(choice[s].kart >= gd.Karts.Length)
						choice[s].kart = 0;
					if(choice[s].kart < 0)
						choice[s].kart = gd.Karts.Length -1;
				}
				else
				{
					choice[s].wheel += amount;
					
					if(choice[s].wheel >= gd.Wheels.Length)
						choice[s].wheel = 0;
					if(choice[s].wheel < 0)
						choice[s].wheel = gd.Wheels.Length -1;
				}
			}
		}
		
		if(submit || (mouseClick && mouseSelecting)){
		
		
			if(state == csState.Character)
			{
				if(gd.Characters[choice[s].character].Unlocked != UnlockedState.Locked)
				{
					if(gd.Characters[choice[s].character].selectedSound != null)
						sm.PlaySFX(gd.Characters[choice[s].character].selectedSound);
						
					loadedModels[s].GetComponent(Animator).CrossFade("Selected",0.01);
						
					ready[s] = true;
					mouseSelecting = false;
				}	
			}	
			
			if(state == csState.Hat)
			{
				if(gd.Hats[choice[s].hat].Unlocked)
				{
					ready[s] = true;
					mouseSelecting = false;
				}	
			}			
			
			if(state == csState.Kart)
			{
				if(kartSelected[s])
				{
					ready[s] = true;
					mouseSelecting = false;
				}
				else
				{
					kartSelected[s] = true;
					mouseSelecting = false;
				}
			}				
				
			
		}
		
		if(cancel)
		{
			//if(state == csState.Character)
				//Cancel();
				
			if(state == csState.Hat)
				state = csState.Character;
				
			if(state == csState.Kart)
				if(kartSelected[s])
					kartSelected[s] = false;
				else
					state = csState.Hat;
		}
			
		
		if(state != csState.Off && state != csState.Kart)
		{
			var selectedIcon : int;
			
			if(state == csState.Character)
				selectedIcon = gd.currentChoices[s].character;
			
			if(state == csState.Hat)
				selectedIcon = gd.currentChoices[s].hat;

			var iconSelection : Vector2 = Vector2(selectedIcon%5,selectedIcon/5);
			cursorPosition[s] = Vector2.Lerp(cursorPosition[s],iconSelection,Time.deltaTime*cursorSpeed);

			var CursorRect : Rect = Rect(10 + cursorPosition[s].x * chunkSize,nameListRect.y + cursorPosition[s].y * chunkSize,chunkSize,chunkSize);
			var CursorTexture : Texture2D = Resources.Load("UI Textures/Cursors/Cursor_"+s,Texture2D);
			GUI.DrawTexture(CursorRect,CursorTexture);
		}
		
		//Render Kart And Wheel
		if(state == csState.Kart)
		{
		
			var kartgroup : Rect;
			var scaleAmount : float = 1f;
			
			kartgroup = Rect(10,10,Screen.width - 20, Screen.height - 20);
			
			GUI.BeginGroup(kartgroup);
			
			var scale : float = chunkSize / scaleAmount;
		
			nameListRect = Rect(0,kartgroup.height/2f - scale*1.25f,scale*5f,scale*2.5f);
			GUI.DrawTexture(nameListRect,nameList);
			
			var kartListRect = Rect(10,kartgroup.height/2f - scale*1.25f + 10,scale*5f - 20,scale*2.5f - 20);
			
			GUI.BeginGroup(kartListRect);
			
			var kartIcon : Texture2D = gd.Karts[choice[s].kart].Icon;
			var wheelIcon : Texture2D = gd.Wheels[choice[s].wheel].Icon;
			var arrowIcon : Texture2D = Resources.Load("UI Textures/New Character Select/Arrow",Texture2D);
			var downArrowIcon : Texture2D = Resources.Load("UI Textures/New Character Select/Arrow_Down",Texture2D);
			
			var kartRect : Rect = Rect(scale/4f, kartListRect.height/2f - scale*1.125, scale*2.5f,((scale*2.5f)/kartIcon.width) * kartIcon.height); 
			var wheelRect : Rect = Rect(kartListRect.x + ((kartListRect.width/5f)*3) - (kartListRect.height *0.35f), kartListRect.height/2f - scale*1.125, kartRect.width,kartRect.height);
			
			GUI.DrawTexture(kartRect,kartIcon,ScaleMode.ScaleToFit);
			GUI.DrawTexture(wheelRect,wheelIcon,ScaleMode.ScaleToFit);
			
			if(s == im.keyboardPlayer)
			{
				
				if(im.MouseIntersects(Rect(kartgroup.x + kartListRect.x + kartRect.x,kartgroup.y + kartListRect.y + kartRect.y,kartRect.width,kartRect.height)) && !kartSelected[s] && mouseClick)
				{
					kartSelected[s] = true;
					mouseSelecting = false;
				}
				
				if(im.MouseIntersects(Rect(kartgroup.x + kartListRect.x + wheelRect.x,kartgroup.y + kartListRect.y + wheelRect.y,wheelRect.width,wheelRect.height)) && kartSelected[s] && mouseClick)
				{
					ready[s] = true;
				}
			}
			
			GUI.EndGroup();
			
			if(!kartSelected[s])
			{
			
				var kartUp = Rect(kartListRect.x + ((kartListRect.width/5f)) ,kartListRect.y - scale*0.8f, scale*0.75f, scale*0.75f);
				GUI.DrawTexture(kartUp,arrowIcon,ScaleMode.ScaleToFit);
				
				var kartDown = Rect(kartListRect.x + ((kartListRect.width/5f)) ,kartListRect.y + kartListRect.height, scale*0.75f, scale*0.75f);
				GUI.DrawTexture(kartDown,downArrowIcon,ScaleMode.ScaleToFit);
				
				if(im.MouseIntersects(Rect(kartgroup.x + kartUp.x,kartgroup.y + kartUp.y,kartUp.width,kartUp.height)) && mouseClick)
				{
						choice[s].kart ++;						
				}
				
				if(im.MouseIntersects(Rect(kartgroup.x + kartDown.x,kartgroup.y + kartDown.y,kartDown.width,kartDown.height)) && mouseClick)
				{
						choice[s].kart --;
				}
				
				if(choice[s].kart >= gd.Karts.Length)
					choice[s].kart = 0;
						
				if(choice[s].kart < 0)
					choice[s].kart = gd.Karts.Length -1;
					
			}
			else
			{
				var wheelUp = Rect(kartListRect.x + ((kartListRect.width/5f)*3) ,kartListRect.y - scale*0.8f, scale*0.75f, scale*0.75f);
				GUI.DrawTexture(wheelUp,arrowIcon,ScaleMode.ScaleToFit);
				
				var wheelDown = Rect(kartListRect.x + ((kartListRect.width/5f)*3) ,kartListRect.y + kartListRect.height, scale*0.75f, scale*0.75f);
				GUI.DrawTexture(wheelDown,downArrowIcon,ScaleMode.ScaleToFit);
			
				if(im.MouseIntersects(Rect(kartgroup.x + wheelUp.x,kartgroup.y + wheelUp.y,wheelUp.width,wheelUp.height)) && mouseClick)
				{
						choice[s].wheel ++;						
				}
				
				if(im.MouseIntersects(Rect(kartgroup.x + wheelDown.x,kartgroup.y + wheelDown.y,wheelDown.width,wheelDown.height)) && mouseClick)
				{
						choice[s].wheel --;
				}
				
				if(choice[s].wheel >= gd.Wheels.Length)
					choice[s].wheel = 0;
						
				if(choice[s].wheel < 0)
					choice[s].wheel = gd.Wheels.Length -1;
					
			}
			
			
			GUI.EndGroup();
			
		}
		
		if(!ready[s])
			readyCheck = false;
		
	}
	
	if(readyCheck)
	{
		if(state == csState.Character)
			state = csState.Hat;		
		else if(state == csState.Hat)
			state = csState.Kart;		
		else if(state == csState.Kart)
			Finished();
			
		ResetReady();
	}
	
}

function ResetReady()
{
	ready = new boolean[4];
}

function ResetEverything()
{

	ResetReady();
	
	if(loadedModels != null)
		for(var i : int = 0; i < 4; i++)
		{
			if(loadedModels[i] != null)
				Destroy(loadedModels[i].gameObject);
		}

	loadedModels = new Transform[4];
	
	loadedChoice = new LoadOut[4];
	
	kartSelected =  new boolean[4];
	
	state = csState.Character;
	
	for(i = 0; i < 4; i++)
	{
		loadedChoice[i] = new LoadOut();
		loadedChoice[i].character = -1;
		loadedChoice[i].hat = -1;
		loadedChoice[i].kart = -1;
		loadedChoice[i].wheel = -1;
		
	}
}

function Cancel()
{

	if(loadedModels != null)
		for(var i : int = 0; i < 4; i++)
		{
			if(loadedModels[i] != null)
				Destroy(loadedModels[i].gameObject);
		}
		
}

function Finished()
{
	//Finished Character Select
	Debug.Log("Finished");
	
	if(loadedModels != null)
		for(var i : int = 0; i < 4; i++)
		{
			if(loadedModels[i] != null)
				Destroy(loadedModels[i].gameObject);
		}
		
	this.enabled = false;
}

class LoadOut{

	var character : int = 0;
	var hat : int = 0;
	var wheel : int= 0;
	var kart : int = 0;

}