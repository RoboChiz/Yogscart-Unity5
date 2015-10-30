#pragma strict

private var gd : CurrentGameData;
private var im : InputManager;
private var sm : Sound_Manager;
private var km : KartMaker;
private var mm : MainMenu;

var hidden : boolean;
var cancelled : boolean;

enum csState {Character,Hat,Kart,Off};
var state : csState = csState.Off;

private var cursorPosition : Vector2[];
var cursorSpeed : float = 2.5;
var rotateSpeed : float = 2.5;

var Platforms : Transform[];

var ready : boolean[];
private var kartSelected : boolean[];

private var loadedChoice : LoadOut[];
private var loadedModels : Transform[];

//Content
private var nameList : Texture2D;

private var mouseSelecting : boolean;

var online : boolean;

private var kartHeight : float;

function Start () {
	gd = GameObject.Find("GameData").GetComponent(CurrentGameData);
	im = GameObject.Find("GameData").GetComponent(InputManager);
	sm = GameObject.Find("Sound System").GetComponent(Sound_Manager); 
	km = GameObject.Find("GameData").GetComponent(KartMaker);
	
	if(transform.GetComponent(MainMenu) != null)
		mm = transform.GetComponent(MainMenu);
	//state = csState.Off;
	
	cursorPosition = new Vector2[4];
	
	ResetEverything();
	
	//Load Content
	nameList = Resources.Load("UI/Lobby/NamesList",Texture2D);
	
}

function OnGUI()
{
	var fontSize = Mathf.Min(Screen.width, Screen.height) / 20f; 
	GUI.skin = Resources.Load("Font/Menu",GUISkin);
	GUI.skin.label.fontSize = fontSize;
	
	GUI.depth = -5;

	var chunkSize = Screen.width/10f;
	
	var choice = gd.currentChoices;
	
	var nameListRect : Rect;
	
	var canInput : boolean = true;
	
	if(mm != null && mm.transitioning)
		canInput = false;
		
	if(canInput)
		var mouseClick = im.GetClick();

	switch(state)
	{
		case(csState.Character):
		
		im.allowedToChange = true;
		
		nameListRect = Rect(10,chunkSize/2f,chunkSize*5f,Screen.height - chunkSize*1.5f);
		GUI.DrawTexture(nameListRect,nameList);
		
		GUI.Label(Rect(10,10,Screen.width,chunkSize/2f),"Select A Character");
		
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
					
					if(canInput && im.MouseIntersects(iconRect))
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
			
			im.allowedToChange = false;
		
			nameListRect = Rect(10,chunkSize/2f,chunkSize*5f,Screen.height - chunkSize*1.5f);
			GUI.DrawTexture(nameListRect,nameList);
			GUI.Label(Rect(10,10,Screen.width,chunkSize/2f),"Select A Hat");
			
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
						
						if(canInput && im.MouseIntersects(iconRect))
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
		case(csState.Kart):
			GUI.Label(Rect(10,10,Screen.width,chunkSize/2f),"Select A Kart");
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



			if(im.c.Length >= 3){
				if(state != csState.Kart)
				{
					cam = Platforms[0].FindChild("Camera").GetComponent.<Camera>();
					oldRect = Vector4(cam.rect.x,cam.rect.y,cam.rect.width,cam.rect.height);
					newRect = Vector4(0.5,0.5,0.25,0.5);
					nRect = Vector4.Lerp(oldRect,newRect,Time.deltaTime*5f);
					cam.rect = Rect(nRect.x,nRect.y,nRect.z,nRect.w);
					
					cam = Platforms[2].FindChild("Camera").GetComponent.<Camera>();
					oldRect = Vector4(cam.rect.x,cam.rect.y,cam.rect.width,cam.rect.height);
					newRect = Vector4(0.5,0,0.25,0.5);
					nRect = Vector4.Lerp(oldRect,newRect,Time.deltaTime*5f);
					cam.rect = Rect(nRect.x,nRect.y,nRect.z,nRect.w);
				}
				else
				{
					cam = Platforms[0].FindChild("Camera").GetComponent.<Camera>();
					oldRect = Vector4(cam.rect.x,cam.rect.y,cam.rect.width,cam.rect.height);
					newRect = Vector4(0.25,0.5,0.25,0.5);
					nRect = Vector4.Lerp(oldRect,newRect,Time.deltaTime*5f);
					cam.rect = Rect(nRect.x,nRect.y,nRect.z,nRect.w);

					cam = Platforms[2].FindChild("Camera").GetComponent.<Camera>();
					oldRect = Vector4(cam.rect.x,cam.rect.y,cam.rect.width,cam.rect.height);
					newRect = Vector4(0.25,0,0.25,0.5);
					nRect = Vector4.Lerp(oldRect,newRect,Time.deltaTime*5f);
					cam.rect = Rect(nRect.x,nRect.y,nRect.z,nRect.w);
				}
				
				cam = Platforms[1].FindChild("Camera").GetComponent.<Camera>();
					oldRect = Vector4(cam.rect.x,cam.rect.y,cam.rect.width,cam.rect.height);
					newRect = Vector4(0.75,0.5,0.25,0.5);
					nRect = Vector4.Lerp(oldRect,newRect,Time.deltaTime*5f);
					cam.rect = Rect(nRect.x,nRect.y,nRect.z,nRect.w);
								
			}

			if(im.c.Length == 4){
				cam = Platforms[3].FindChild("Camera").GetComponent.<Camera>();
				oldRect = Vector4(cam.rect.x,cam.rect.y,cam.rect.width,cam.rect.height);
				newRect = Vector4(0.75,0,0.25,0.5);
				nRect = Vector4.Lerp(oldRect,newRect,Time.deltaTime*5f);
				cam.rect = Rect(nRect.x,nRect.y,nRect.z,nRect.w);
			}
		}
		
		if(loadedModels[s] != null)
			loadedModels[s].Rotate(Vector3.up,-im.c[s].GetInput("Rotate") * Time.deltaTime * rotateSpeed);
		
		var hori : float = 0;
		var vert : float = 0;
		var submit : boolean = false;
		var cancel : boolean = false;
		
		if(!ready[s])
		{
			//Get Inputs
			if(canInput)
			{
				hori = im.c[s].GetMenuInput("Horizontal");
				vert = -im.c[s].GetMenuInput("Vertical");
			}
			
			submit = canInput && (im.c[s].GetMenuInput("Submit") != 0);
		}
		cancel  = canInput && (im.c[s].GetMenuInput("Cancel") != 0);
		
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
			
			if(state == csState.Kart && !choice[s].scrolling)
			{
				if(!kartSelected[s])
				{
					if(amount > 0)
						ScrollKart(choice[s],kartHeight);
					else
						ScrollKart(choice[s],-kartHeight);
										
				}
				else
				{
					if(amount > 0)
						ScrollWheel(choice[s], kartHeight);
					else
						ScrollWheel(choice[s],-kartHeight);
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
		
		var backTexture : Texture2D = Resources.Load("UI Textures/New Main Menu/backnew",Texture2D);	
		var backRatio : float = (Screen.width/6f)/backTexture.width;	
		var backRect : Rect = Rect(MainMenu.xAmount,Screen.height - 10 - (backTexture.height*backRatio),Screen.width/6f,backTexture.height*backRatio);	
		GUI.DrawTexture(backRect,backTexture);
		
		if((!MainMenu.transitioning && im.MouseIntersects(backRect) && mouseClick) || cancel)
		{
			if(!ready[s])
			{
				if(state == csState.Character)
					Cancel();
					
				if(state == csState.Hat)
					state = csState.Character;
					
				if(state == csState.Kart)
					if(kartSelected[s])
						kartSelected[s] = false;
					else
						state = csState.Hat;
			}
			else
			{
				ready[s] = false;
			}
			
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
		
		var topHeight : float =  Screen.height * 0.05f;
		var screenHeight : float = Screen.height - 10 - (backTexture.height*backRatio) - topHeight;
		
		//Render Kart And Wheel
		if(state == csState.Kart)
		{	
			var areaRect : Rect;
			
			if(im.c.Length == 1)
				areaRect = Rect(0,topHeight,Screen.width,screenHeight);
			
			if(im.c.Length == 2)
			{			
				if(s == 0)
					areaRect = Rect(0,topHeight,Screen.width,screenHeight/2f);
				else
					areaRect = Rect(0,topHeight + screenHeight/2f,Screen.width,screenHeight/2f);
			}
			
			if(im.c.Length > 2)
			{			
				if(s == 0)
					areaRect = Rect(0,topHeight,Screen.width/2f,screenHeight/2f);
				if(s == 1)
					areaRect = Rect(Screen.width/2f,topHeight,Screen.width/2f,screenHeight/2f);
				if(s == 2)
					areaRect = Rect(0,topHeight + screenHeight/2f,Screen.width/2f,screenHeight/2f);
				if(s == 3)
					areaRect = Rect(Screen.width/2f,topHeight + screenHeight/2f,Screen.width/2f,screenHeight/2f);
			}
			
			var heightChunk : float = areaRect.height / 6f;
			
			GUI.BeginGroup(areaRect);
			
				var selectionRect : Rect = Rect(10,heightChunk,(areaRect.width/2f) - 10,heightChunk*4f);
				GUI.DrawTexture(selectionRect,nameList);
			
				GUI.BeginGroup(selectionRect);
				
					var kartIcon : Texture2D = gd.Karts[choice[s].kart].Icon;	
					var arrowIcon : Texture2D = Resources.Load("UI Textures/New Character Select/Arrow",Texture2D);
					var downArrowIcon : Texture2D = Resources.Load("UI Textures/New Character Select/Arrow_Down",Texture2D);
					
					var kartWidth : float;
					
					kartWidth = (selectionRect.width/2f) - 10;
					kartHeight = selectionRect.height/3f;
					
					for(var kartI = -2; kartI <= 2; kartI++)
					{
						kartIcon = gd.Karts[ClampRound(choice[s].kart + kartI,0,gd.Karts.Length)].Icon;
						var kartRect : Rect = Rect(20, selectionRect.height/2f - (kartHeight * (kartI+0.5)) - choice[s].kartChangeHeight,kartWidth,kartHeight); 
						
						var nAlpha : float = choice[s].kartAlpha;
						if(kartI == 0)
							nAlpha = 1.4 - nAlpha;
						if(kartI == -2 || kartI == 2)
							nAlpha = 0.4f;
						if(kartI == 1 && choice[s].kartChangeHeight > 0)
							nAlpha = 0.4f;
						if(kartI == -1 && choice[s].kartChangeHeight < 0)
							nAlpha = 0.4f;
							
						GUI.color.a = nAlpha;
						
						GUI.DrawTexture(kartRect,kartIcon,ScaleMode.ScaleToFit);
						
						var wheelIcon : Texture2D = gd.Wheels[ClampRound(choice[s].wheel + kartI,0,gd.Wheels.Length)].Icon;
						var wheelRect : Rect = Rect(30 + kartWidth, selectionRect.height/2f - (kartHeight * (kartI+0.5)) - choice[s].wheelChangeHeight,kartWidth,kartHeight); 
						
						nAlpha = choice[s].wheelAlpha;
						if(kartI == 0)
							nAlpha = 1.4 - nAlpha;
						if(kartI == -2 || kartI == 2)
							nAlpha = 0.4f;
						if(kartI == 1 && choice[s].wheelChangeHeight > 0)
							nAlpha = 0.4f;
						if(kartI == -1 && choice[s].wheelChangeHeight < 0)
							nAlpha = 0.4f;
							
						GUI.color.a = nAlpha;
						
						GUI.DrawTexture(wheelRect,wheelIcon,ScaleMode.ScaleToFit);
						
						GUI.color.a = 1f;
						
						if(kartI == 0)
						{	
							if(!kartSelected[s] && im.MouseIntersects(Rect(areaRect.x + selectionRect.x + kartRect.x,areaRect.y + selectionRect.y + kartRect.y,kartRect.width,kartRect.height)) && mouseClick)
							{
								kartSelected[s] = true;
								mouseSelecting = false;
							}
							if(kartSelected[s] && im.MouseIntersects(Rect(areaRect.x + selectionRect.x + wheelRect.x,areaRect.y + selectionRect.y + wheelRect.y,wheelRect.width,wheelRect.height)) && mouseClick)
							{
								ready[s] = true;
								mouseSelecting = false;
							}
						}
						
					}
					
				GUI.EndGroup();	
				
				var upArrowRect : Rect;
				var downArrowRect : Rect;
				
				if(!kartSelected[s])
				{
					
					upArrowRect =  Rect(20,10,(selectionRect.width/2f) - 10,heightChunk - 10);
					downArrowRect =  Rect(20, heightChunk*5f, (selectionRect.width/2f) - 10,heightChunk - 10);
					
					if(choice[s].kart >= gd.Karts.Length)
						choice[s].kart = 0;
							
					if(choice[s].kart < 0)
						choice[s].kart = gd.Karts.Length -1;
						
					if(im.MouseIntersects(Rect(areaRect.x + upArrowRect.x, areaRect.y + upArrowRect.y, upArrowRect.width, upArrowRect.height)) && mouseClick)
					{
						ScrollKart(choice[s],-kartHeight);
					}
					
					if(im.MouseIntersects(Rect(areaRect.x + downArrowRect.x, areaRect.y + downArrowRect.y, downArrowRect.width, downArrowRect.height)) && mouseClick)
					{
						ScrollKart(choice[s],kartHeight);
					}
						
				}
				else
				{
					
					upArrowRect =  Rect(20 + (selectionRect.width/2f), 10, (selectionRect.width/2f) - 10, heightChunk - 10);
					downArrowRect =  Rect(20 + (selectionRect.width/2f), heightChunk*5f, (selectionRect.width/2f) - 10, heightChunk - 10);
					
					if(choice[s].wheel >= gd.Wheels.Length)
						choice[s].wheel = 0;
							
					if(choice[s].wheel < 0)
						choice[s].wheel = gd.Wheels.Length -1;
						
					if(im.MouseIntersects(Rect(areaRect.x + upArrowRect.x, areaRect.y + upArrowRect.y, upArrowRect.width, upArrowRect.height)) && mouseClick)
					{
						ScrollWheel(choice[s],-kartHeight);
					}
					
					if(im.MouseIntersects(Rect(areaRect.x + downArrowRect.x, areaRect.y + downArrowRect.y, downArrowRect.width, downArrowRect.height)) && mouseClick)
					{
						ScrollWheel(choice[s],kartHeight);
					}
						
					if(ready[s])
					{
						var readyTexture : Texture2D = Resources.Load("UI Textures/New Main Menu/Ready",Texture2D);	
						GUI.DrawTexture(selectionRect,readyTexture,ScaleMode.ScaleToFit);
					}
						
				}
				
				GUI.DrawTexture(upArrowRect,arrowIcon,ScaleMode.ScaleToFit);
				GUI.DrawTexture(downArrowRect,downArrowIcon,ScaleMode.ScaleToFit);
			
			GUI.EndGroup();
			
		}
		
		if(!ready[s])
			readyCheck = false;
		
	}
	
	if(readyCheck)
	{
		if(state == csState.Character)
		{
			state = csState.Hat;
			//Reset the hats
			loadedChoice[0].hat	= -1;	
			loadedChoice[1].hat	= -1;	
			loadedChoice[2].hat	= -1;	
			loadedChoice[3].hat	= -1;	
		}
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
	hidden = false;
	
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
	
	cancelled = false;
	
}

function Cancel()
{

	cancelled = true;

	if(loadedModels != null)
		for(var i : int = 0; i < 4; i++)
		{
			if(loadedModels[i] != null)
				Destroy(loadedModels[i].gameObject);
		}
		
	if(online)
	{ 
		if(!Network.isClient)
			gd.transform.GetComponent(Network_Manager).CancelStartServer();
	}
	else
	{
		transform.GetComponent(MainMenu).CancelCharacterSelect();
	}
	
	hidden = true;
	this.enabled = false;
		
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
	
	var scrolling : boolean;
	var kartChangeHeight : float = 0f;
	var wheelChangeHeight : float = 0f;
	var kartAlpha : float = 0.4f;
	var wheelAlpha : float = 0.4f;
}

function ScrollKart(loadOut : LoadOut, finalHeight : float)
{
	loadOut.scrolling = true;

	var startTime = Time.time;
	
	while(Time.time - startTime < 0.15f)
	{
		loadOut.kartChangeHeight = Mathf.Lerp(0,finalHeight,(Time.time - startTime)/0.15f);
		loadOut.kartAlpha = Mathf.Lerp(0.4f,1f,(Time.time - startTime)/0.15f);
		yield;
	}
	
	loadOut.kartChangeHeight = finalHeight;
	loadOut.kartAlpha = 1f;
	
	loadOut.kart -= Mathf.Sign(finalHeight);
	
	loadOut.kart = ClampRound(loadOut.kart,0,gd.Karts.Length);
			
	loadOut.kartChangeHeight = 0;
	loadOut.kartAlpha = 0.4f;
	
	loadOut.scrolling = false;
}

function ScrollWheel(loadOut : LoadOut, finalHeight : float)
{
	loadOut.scrolling = true;

	var startTime = Time.time;
	
	while(Time.time - startTime < 0.15f)
	{
		loadOut.wheelChangeHeight = Mathf.Lerp(0,finalHeight,(Time.time - startTime)/0.15f);
		loadOut.wheelAlpha = Mathf.Lerp(0.4f,1f,(Time.time - startTime)/0.15f);
		yield;
	}
	
	loadOut.wheelChangeHeight = finalHeight;
	loadOut.wheelAlpha = 1f;
	
	loadOut.wheel -= Mathf.Sign(finalHeight);
	
	loadOut.wheel = ClampRound(loadOut.wheel,0,gd.Wheels.Length);
	
	loadOut.wheelChangeHeight = 0;
	loadOut.wheelAlpha = 0.4f;
	
	loadOut.scrolling = false;
}



function ClampRound(val : float,min : float,max : float)
{
	while(val >= max)
		val -= (max-min);
	
	while(val < min)
		val += (max-min);
		
	return val;
}