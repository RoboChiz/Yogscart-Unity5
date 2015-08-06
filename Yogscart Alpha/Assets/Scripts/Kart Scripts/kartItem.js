

#pragma strict

private var gd : CurrentGameData;
private var ki : kartInfo;
private var kaI : kartInput;
private var pf : Position_Finding;
private var im : InputManager;
private var sm : Sound_Manager;
private var ks : kartScript;

var heldPowerUp : int = -1;//Used to reference the item in Current Game Data
var iteming : boolean; //True is player is getting / has an item

private var shield : Transform;
private var sheilding : boolean;

 var renderItem : Texture2D;
 var renderItemHeight : int;
private var GUIAlpha : float = 0;

private var online : boolean;
private var mine : boolean;
private var aiControlled : boolean;

private var spinning : boolean;
var locked : boolean = true;

var input : boolean;
var inputLock : boolean;
var inputDirection : float;

function Awake()
{
	//Access the scripts needed for proper iteming
	ks = transform.GetComponent(kartScript);
	ki = transform.GetComponent(kartInfo);
	kaI = transform.GetComponent(kartInput);
	pf = transform.GetComponent(Position_Finding);
	gd = GameObject.Find("GameData").GetComponent(CurrentGameData);
	im = GameObject.Find("GameData").GetComponent(InputManager);
	sm = GameObject.Find("Sound System").GetComponent(Sound_Manager); 	
}

function Start()
{
	if((Network.isClient || Network.isServer))
		online = true;
	else
		online = false;
	
	if(online)
		mine = GetComponent.<NetworkView>().isMine;
		
}

//Informs all clients that this kart has recieved an item
@RPC
function RecieveItem(item : int)
{
	heldPowerUp = item;
	iteming = true;
}

//Makes the local version of the kart use an item, the effect should appear the same on all clients
@RPC
function UseItem()
{
	if(heldPowerUp != -1)
	{
	
		var item : Transform = Instantiate(gd.PowerUps[heldPowerUp].Model,transform.position - (transform.forward *2f),transform.rotation);
		item.parent = transform;
		
		EndItemUse();
		
	}
}

@RPC
function UseShield()
{
	if(heldPowerUp != -1 && gd.PowerUps[heldPowerUp].usableShield)
	{
		shield = Instantiate(gd.PowerUps[heldPowerUp].Model,transform.position - (transform.forward *2f),transform.rotation);
		shield.parent = transform;
		shield.GetComponent.<Rigidbody>().isKinematic = true;
		
	}
}

@RPC
function DropShield(dir : float)
{
	if(heldPowerUp != -1 && gd.PowerUps[heldPowerUp].usableShield)
	{
	
		inputDirection = dir;
	
		shield.parent = null;
		shield.GetComponent.<Rigidbody>().isKinematic = false;
		shield = null;
		
		
		EndItemUse();
		
	}
}

@RPC
function EndItemUse()
{
	if(gd.PowerUps[heldPowerUp].MultipleUses)
	{
		
		var tempPowerUp = heldPowerUp - 1;
		
		if(!online)
			RecieveItem(tempPowerUp);
		else if(mine)
		{
			GetComponent.<NetworkView>().group = 5; //Set RPC Group to item group
			GetComponent.<NetworkView>().RPC("RecieveItem",RPCMode.All,-1);
			yield;
			GetComponent.<NetworkView>().RPC("RecieveItem",RPCMode.AllBuffered,tempPowerUp);
			GetComponent.<NetworkView>().group = 0; //Set RPC Group back to default group
		}
		
	}
	else
	{
		//Nothing left to do turn off items
		heldPowerUp = -1;
		iteming = false;
		
		//Clear the item for everyone else
		if(online && mine)
			Network.RemoveRPCsInGroup(5); //Group 5 is used for Item RPCS
			
	}
	
}

function OnTriggerEnter (other : Collider) {
	if(other.tag == "Crate" && !iteming && (!online || mine)){
		StartCoroutine("decidePowerUp");
		iteming = true;
	}
}

function decidePowerUp()
{

	var nItem : int;
		
	if(pf != null)
	{
		var totalChance : int = 0;
		
		for(var i : int = 0; i < gd.PowerUps.Length; i++)
			totalChance += gd.PowerUps[i].likelihood[pf.position];
			
		var randomiser = Random.Range(0,totalChance);
		
		for(var j : int = 0; j <gd.PowerUps.Length; j++)
		{
		
			randomiser -= gd.PowerUps[j].likelihood[pf.position];
			
			if(randomiser <= 0)
			{
				nItem = j;
				break;
			}
			
		}
		
	}
	else
	{
		//Random item for testing purposes
		nItem = Random.Range(0,gd.PowerUps.Length);
	}
	
	yield RollItem(nItem);
	
	if(online)
	{
		GetComponent.<NetworkView>().group = 5; //Set RPC Group to item group
		GetComponent.<NetworkView>().RPC("RecieveItem",RPCMode.AllBuffered,nItem);
		GetComponent.<NetworkView>().group = 0; //Set RPC Group back to default group
	}
	else
	{
		RecieveItem(nItem);
	}
	
}

function FixedUpdate()
{
	
	if(transform.GetComponent(kartInput) != null)
	{
		input = im.c[kaI.InputNum].GetInput("Use Item") != 0;
		
		if(im.c[kaI.InputNum].inputName != "Key_")
		{
			inputDirection = im.c[kaI.InputNum].GetInput("Vertical");
		}
	}
	
	if(input == false)
		inputLock = false;

	if((!online && ! aiControlled) || mine)
	{
	
		if(heldPowerUp != -1)
		{
			if(!gd.PowerUps[heldPowerUp].usableShield)
			{
				var itemKey = input && !inputLock && !locked;
				
				if(itemKey)
				{
					if(online)
						GetComponent.<NetworkView>().RPC("UseItem",RPCMode.All);
					else
						UseItem();
					
					inputLock = true;
				}
			}
			else
			{
				if(input)
				{
					if(!sheilding)
					{
						if(online)
						{
							GetComponent.<NetworkView>().group = 5; //Set RPC Group to item group
							GetComponent.<NetworkView>().RPC("UseShield",RPCMode.AllBuffered);
							GetComponent.<NetworkView>().group = 0; //Set RPC Group back to default group
						}
						else
						{
							UseShield();
						}
						
						sheilding = true;
					}
				}
				else
				{
					if(sheilding)
					{
						if(online)
							GetComponent.<NetworkView>().RPC("DropShield",RPCMode.All,inputDirection);
						else
							DropShield(inputDirection);
							
						sheilding = false;
					}
				}
			}
		}
		
		if(sheilding && shield == null)
		{		
			sheilding = false;
			EndItemUse();
		}
	}
}

function RollItem(item : int)
{

	spinning = true;
	
	if(!aiControlled)
	{
		sm.PlaySFX(Resources.Load("Music & Sounds/SFX/Powerup",AudioClip));
	
		var size = Screen.width/8f;
		renderItemHeight = -size ;

		var counter : int = 0;
		var startTime : float = Time.timeSinceLevelLoad;

		while((Time.timeSinceLevelLoad-startTime) < 1.7){

		renderItem = gd.PowerUps[counter].Icon;

		yield Scroll();

		if(counter+1<gd.PowerUps.Length)
		counter += 1;
		else
		counter = 0;

		}

		renderItem = gd.PowerUps[item].Icon;
		
		sm.PlaySFX(Resources.Load("Music & Sounds/SFX/Powerup2",AudioClip));
		yield Stop();
		
	}
	else
	{
		yield WaitForSeconds(2f);
	}
	
	spinning = false;		
	
}

function Scroll(){
	var size = Screen.width/8f;
	var nstartTime : float = Time.timeSinceLevelLoad;
	
	while((Time.timeSinceLevelLoad-nstartTime) < 0.2 ){
		renderItemHeight = Mathf.Lerp(-size,size,(Time.timeSinceLevelLoad-nstartTime)/0.2);
	yield;
	}
	
	renderItemHeight = size;
	
}

function Stop(){
	
	var size = Screen.width/8f;
	var nstartTime : float = Time.timeSinceLevelLoad;
	while((Time.timeSinceLevelLoad-nstartTime) < 0.2 ){
		renderItemHeight = Mathf.Lerp(-size,0,(Time.timeSinceLevelLoad-nstartTime)/0.2);
	yield;
	}
	
	renderItemHeight = 0;
	
}


function OnGUI () 
{

	if(transform.GetComponent(Racer_AI) != null)
		aiControlled = true;

	if((!online && ! aiControlled) || mine)
	{
		GUI.color = Color32(255,255,255,GUIAlpha);
		
		var frame : Texture2D = Resources.Load("UI Textures/Power Ups/item frame",Texture2D);
		var FrameRect : Rect;
		var size : float = Screen.width/8f;
		
		if(ki != null){

			if(ki.screenPos != ScreenType.Full)
				size = Screen.width/16f;

			if(ki.screenPos == ScreenType.Full)
				FrameRect = Rect(Screen.width - 20 - size,20,size,size);

			if(ki.screenPos == ScreenType.TopLeft)
				FrameRect = Rect(Screen.width/2f - 20 - size,20,size,size);

			if(ki.screenPos == ScreenType.TopRight || ki.screenPos == ScreenType.Top)
				FrameRect = Rect(Screen.width - 20 - size,20,size,size);

			if(ki.screenPos == ScreenType.BottomLeft)
				FrameRect = Rect(Screen.width/2f - 20 - size,20 + Screen.height/2f,size,size);

			if(ki.screenPos == ScreenType.BottomRight || ki.screenPos == ScreenType.Bottom)
				FrameRect = Rect(Screen.width - 20 - size,20 + Screen.height/2f,size,size);

		}
		else
		{
			FrameRect = Rect(Screen.width - 20 - size,20,size,size);
		}
		
		GUI.BeginGroup(FrameRect);

		if(!spinning && heldPowerUp != -1)
			renderItem = gd.PowerUps[heldPowerUp].Icon;

		if(renderItem != null){
			var ItemRect : Rect = Rect(5,5+renderItemHeight,size - 10,size- 10);
			GUI.DrawTexture(ItemRect,renderItem);
		}

		GUI.EndGroup();

		GUI.DrawTexture(FrameRect,frame);
		
		if(iteming && !locked)
			GUIAlpha = Mathf.Lerp(GUIAlpha,256,Time.deltaTime*5f);
		else
			GUIAlpha = Mathf.Lerp(GUIAlpha,0,Time.deltaTime*5f);
	}
}
