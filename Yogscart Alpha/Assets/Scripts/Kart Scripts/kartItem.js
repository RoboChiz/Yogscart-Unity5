#pragma strict

private var gd : CurrentGameData;
private var ki : kartInfo;
private var kaI : kartInput;
private var pf : Position_Finding;
private var im : InputManager;

var heldPowerUp : int = -1;//Used to reference the item in Current Game Data
var iteming : boolean; //True is player is getting / has an item

private var shield : Transform;
private var sheilding : boolean;

private var renderItem : Texture2D;
private var renderItemHeight : int;
private var GUIAlpha : float = 0;

function Awake()
{
	//Access the scripts needed for proper iteming
	ki = transform.GetComponent(kartInfo);
	kaI = transform.GetComponent(kartInput);
	pf = transform.GetComponent(Position_Finding);
	gd = GameObject.Find("GameData").GetComponent(CurrentGameData);
	im = GameObject.Find("GameData").GetComponent(InputManager);
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
	if(heldPowerUp != -1 && gd.PowerUps[heldPowerUp].type == ItemType.UsableAsShield)
	{
		shield = Instantiate(gd.PowerUps[heldPowerUp].Model,transform.position - (transform.forward *2f),transform.rotation);
		shield.parent = transform;
		shield.GetComponent.<Rigidbody>().isKinematic = true;
		
	}
}

@RPC
function DropShield()
{
	if(heldPowerUp != -1 && gd.PowerUps[heldPowerUp].type == ItemType.UsableAsShield)
	{
	
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
		heldPowerUp--;
		
		GetComponent.<NetworkView>().group = 5; //Set RPC Group to item group
		GetComponent.<NetworkView>().RPC("RecieveItem",RPCMode.All,-1);
		yield;
		GetComponent.<NetworkView>().RPC("RecieveItem",RPCMode.AllBuffered,heldPowerUp);
		GetComponent.<NetworkView>().group = 0; //Set RPC Group back to default group
	}
	else
	{
		//Nothing left to do turn off items
		heldPowerUp = -1;
		iteming = false;
		
		//Clear the item for everyone else
		if(GetComponent.<NetworkView>().isMine)
			Network.RemoveRPCsInGroup(5); //Group 5 is used for Item RPCS
			
	}
	
}

function OnTriggerEnter (other : Collider) {
	if(other.tag == "Crate" && !iteming && GetComponent.<NetworkView>().isMine){
		decidePowerUp();
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
		
		Debug.Log("Generated " + gd.PowerUps[nItem].Name);
		
	}
	else
	{
		//Random item for testing purposes
		nItem = Random.Range(0,gd.PowerUps.Length);
	}
	
	
	GetComponent.<NetworkView>().group = 5; //Set RPC Group to item group
	GetComponent.<NetworkView>().RPC("RecieveItem",RPCMode.AllBuffered,nItem);
	GetComponent.<NetworkView>().group = 0; //Set RPC Group back to default group
	
}

function FixedUpdate()
{
	if(GetComponent.<NetworkView>().isMine)
	{
	
		if(heldPowerUp != -1)
		{
			if(gd.PowerUps[heldPowerUp].type != ItemType.UsableAsShield)
			{
				var itemKey = im.c[kaI.InputNum].GetMenuInput("Use Item");
				
				if(itemKey)
				{
					GetComponent.<NetworkView>().RPC("UseItem",RPCMode.All);
				}
			}
			else
			{
				if(im.c[kaI.InputNum].GetInput("Use Item") != 0)
				{
					if(!sheilding)
					{
						GetComponent.<NetworkView>().group = 5; //Set RPC Group to item group
						GetComponent.<NetworkView>().RPC("UseShield",RPCMode.AllBuffered);
						GetComponent.<NetworkView>().group = 0; //Set RPC Group back to default group
						
						sheilding = true;
					}
				}
				else
				{
					if(sheilding)
					{
						GetComponent.<NetworkView>().RPC("DropShield",RPCMode.All);
						sheilding = false;
					}
				}
			}
		}
		
		if(sheilding && shield == null)
		{
			GetComponent.<NetworkView>().RPC("EndItemUse",RPCMode.All);
			sheilding = false;
		}
	}
}

function OnGUI () 
{
	if(GetComponent.<NetworkView>().isMine)
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

		if(heldPowerUp != -1)
			renderItem = gd.PowerUps[heldPowerUp].Icon;

		if(renderItem != null){
			var ItemRect : Rect = Rect(5,5+renderItemHeight,size - 10,size- 10);
			GUI.DrawTexture(ItemRect,renderItem);
		}

		GUI.EndGroup();

		GUI.DrawTexture(FrameRect,frame);
		
		if(iteming)
			GUIAlpha = Mathf.Lerp(GUIAlpha,256,Time.deltaTime*5f);
		else
			GUIAlpha = Mathf.Lerp(GUIAlpha,0,Time.deltaTime*5f);
	}
}
