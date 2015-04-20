#pragma strict

var state : int;

var cursorSpeed : float = 2.5;
var rotSpeed : float = 15;

var Platforms : Transform[];

var hidden : boolean;

private var choice : LoadOut[];
private var ready : boolean[];
private var kartSelected : boolean[];
private var cursorPosition : Vector2[];

var inputLock : boolean[];
private var choicesPerColumn : int;

private var loadedCharacter : int[];
private var loadedHat : int[];
private var loadedKart : int[];
private var loadedWheel : int[];
private var loadedModels : Transform[];

private var gd : CurrentGameData;
private var im : InputManager;
private var sm : Sound_Manager;
private var iconWidth : float;

private var ConfirmSound : AudioClip;
private var BackSound : AudioClip;

function Start () {

gd = GameObject.Find("GameData").GetComponent(CurrentGameData);
im = GameObject.Find("GameData").GetComponent(InputManager);
sm = GameObject.Find("Sound System").GetComponent(Sound_Manager); 

ConfirmSound = Resources.Load("Music & Sounds/SFX/confirm",AudioClip);
BackSound = Resources.Load("Music & Sounds/SFX/back",AudioClip);

hiddenFloat = -(iconWidth*6f);
iconWidth = (Screen.width/2f)/5;

choice = new LoadOut[4];
loadedModels = new Transform[4];

ResetEverything();

for(var i : int = 0; i < 4;i++){
choice[i] = new LoadOut();

loadedCharacter[i] = -1;
loadedHat[i] = -1;
loadedKart[i] = -1;
loadedWheel[i] = -1;

}

state = 0;

yield WaitForSeconds(0.5);

}

function FixedUpdate(){
if(!hidden){

for(var i : int = 0;i < im.c.Length;i++){

if(state == 0){

if(loadedCharacter[i] != choice[i].character){

var oldRot0 : Quaternion;

if(loadedModels[i] != null){
oldRot0 = loadedModels[i].rotation;
Destroy(loadedModels[i].gameObject);
}else
oldRot0 = Quaternion.identity;

if(gd.Characters[choice[i].character].Unlocked != UnlockedState.Locked){
loadedModels[i] = Instantiate(gd.Characters[choice[i].character].CharacterModel_Standing,Platforms[i].FindChild("Spawn").position,oldRot0);
loadedModels[i].GetComponent.<Rigidbody>().isKinematic = true;
}

loadedCharacter[i] = choice[i].character;

}

if(loadedModels[i] != null)
loadedModels[i].Rotate(Vector3.up,-im.c[i].GetInput("Rotate") * Time.fixedDeltaTime * rotSpeed);

}

if(state == 1){

if(loadedHat[i] != choice[i].hat){

var oldRot1 : Quaternion = loadedModels[i].rotation;

if(loadedModels[i] != null)
Destroy(loadedModels[i].gameObject);

loadedModels[i] = Instantiate(gd.Characters[choice[i].character].CharacterModel_Standing,Platforms[i].FindChild("Spawn").position,oldRot1);
loadedModels[i].GetComponent.<Rigidbody>().isKinematic = true;

if(gd.Hats[choice[i].hat].Unlocked == true && gd.Hats[choice[i].hat].Model != null){

var HatObject = Instantiate(gd.Hats[choice[i].hat].Model,loadedModels[i].position,Quaternion.identity);

HatObject.position = loadedModels[i].GetComponent(CharacterSkeleton).HatHolder.position;
HatObject.rotation = loadedModels[i].GetComponent(CharacterSkeleton).HatHolder.rotation;
HatObject.parent = loadedModels[i].GetComponent(CharacterSkeleton).HatHolder;

}

loadedHat[i] = choice[i].hat;

}

if(loadedModels[i] != null)
loadedModels[i].Rotate(Vector3.up,-im.c[i].GetInput("Rotate") * Time.fixedDeltaTime * rotSpeed);

}

if(state == 2){

if(loadedKart[i] != choice[i].kart || loadedWheel[i] != choice[i].wheel){

if(loadedModels[i] != null){
var oldRot2 : Quaternion = loadedModels[i].rotation;
Destroy(loadedModels[i].gameObject);
}

var km = gd.transform.GetComponent(KartMaker);

loadedModels[i] = km.SpawnKart(KartType.Display,Platforms[i].FindChild("Spawn").position + Vector3.up,oldRot2,choice[i].kart,choice[i].wheel,choice[i].character,choice[i].hat);

loadedKart[i] = choice[i].kart;
loadedWheel[i] = choice[i].wheel;

}

if(loadedModels[i] != null)
loadedModels[i].Rotate(Vector3.up,-im.c[i].GetInput("Rotate") * Time.fixedDeltaTime * rotSpeed);

}

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

}

var hiddenFloat : float = 0;
private var animated : boolean;
var scrollTime : float = 0.5f;

function HideTitles(hide : boolean)
{

var startTime = Time.realtimeSinceStartup;

var toScroll : float;
var fromScroll : float;

if(hide)
{
toScroll = -(iconWidth*6f);
fromScroll = 10;
}
else
{
toScroll = 10;
fromScroll = -(iconWidth*6f);
}

while(Time.realtimeSinceStartup-startTime  < scrollTime){
hiddenFloat = Mathf.Lerp(fromScroll,toScroll,(Time.realtimeSinceStartup-startTime)/scrollTime);
yield;
}

if(hide)
hiddenFloat = -(iconWidth*6f);
else
hiddenFloat = 10;

}

function OnGUI () {

var avg = ((Screen.height + Screen.width)/2f)/30f;

var submitBool : boolean;
var cancelBool : boolean;

GUI.skin = Resources.Load("Font/Menu", GUISkin);

GUI.skin.label.fontSize = avg;
//GUI.skin.customStyles[4].fontSize = avg;

if(hidden)
{
if(!animated){
HideTitles(true);
animated = true;
}
}
else
{
if(animated)
{
HideTitles(false);
animated = false;
}
}

GUI.skin = Resources.Load("GUISkins/Main Menu",GUISkin);

var stateTexture : Texture2D;

iconWidth = (Screen.width/2f)/5;

var Heightratio : float = ((Screen.width/3f)/1000f)*200f;

var BoardTexture = Resources.Load("UI Textures/GrandPrix Positions/Backing2",Texture2D);
var BoardHeight : float = iconWidth*6f + 10;
var BoardRect = Rect(hiddenFloat + 10,Screen.height/2f - BoardHeight/2f + Heightratio,iconWidth*5f + 20 ,BoardHeight - Heightratio);
var startHeight = Screen.height/2f - BoardHeight/2f + 10 + Heightratio;

GUI.DrawTexture(BoardRect,BoardTexture);

if(!hidden){

if(state == 0){ //Character Select

stateTexture = Resources.Load("UI Textures/New Character Select/char",Texture2D);

if(Input.GetJoystickNames().Length+1 > im.c.Length && im.c.Length < 4)
{
OutLineLabel(Rect(Screen.width/2f,Screen.height - (avg*2f),Screen.width/2f,avg),"Multiple Controllers Detected",2);
OutLineLabel(Rect(Screen.width/2f,Screen.height - avg,Screen.width/2f,avg),"Press Start to Join",2);
}

var characterCounter : int;

for(var i : int = 0; i < choicesPerColumn;i++){
for(var j : int = 0; j < 5;j++){

if((i*5) + j < gd.Characters.Length){

var iconRect : Rect = Rect(hiddenFloat + 20 + (j*iconWidth),startHeight + (i*iconWidth),iconWidth,iconWidth);

var icon : Texture2D;
if(gd.Characters[characterCounter].Unlocked != UnlockedState.Locked)
icon = gd.Characters[characterCounter].Icon;
else
icon = Resources.Load("UI Textures/Character Icons/question_mark",Texture2D);

GUI.DrawTexture(iconRect,icon);

for(var playerCount : int = 0; playerCount < im.c.Length; playerCount++)
if(im.c[playerCount].inputName == "Key_" && im.MouseIntersects(iconRect) && !ready[playerCount] )
choice[playerCount].character = characterCounter;

characterCounter += 1;

}else{
break;
}

}
}

//Render Cursor
for(var c : int = 0; c < im.c.Length;c++){

if(!hidden){
var submitInput : float = im.c[c].GetInput("Submit");
submitBool = (submitInput != 0);

var cancelInput : float = im.c[c].GetInput("Cancel");
cancelBool = (cancelInput != 0);
}

var selectedchar = choice[c].character;

var CharacterSelection : Vector2 = Vector2(selectedchar%5,selectedchar/5);

cursorPosition[c] = Vector2.Lerp(cursorPosition[c],CharacterSelection,Time.deltaTime*cursorSpeed);

var CursorRect : Rect = Rect(hiddenFloat + 20 + cursorPosition[c].x * iconWidth,startHeight + cursorPosition[c].y * iconWidth,iconWidth,iconWidth);
var CursorTexture : Texture2D = Resources.Load("UI Textures/Cursors/Cursor_"+c,Texture2D);
GUI.DrawTexture(CursorRect,CursorTexture);

if(im.c[c].GetInput("Horizontal") == 0 && im.c[c].GetInput("Vertical") == 0)
inputLock[c] = false;

//Get new Input
if(ready[c] == false){
if(im.c[c].GetInput("Horizontal") != 0 && inputLock[c] == false){

inputLock[c] = true;

var hinput = Mathf.Sign(im.c[c].GetInput("Horizontal"));

var iconsInRow : int;
if((CharacterSelection.y+1)*5 <= gd.Characters.length)
iconsInRow = 5;
else
iconsInRow = (gd.Characters.Length-(CharacterSelection.y*5));

if(choice[c].character+hinput < (CharacterSelection.y*5) + iconsInRow && choice[c].character+hinput >= (CharacterSelection.y*5))
choice[c].character += hinput;
else{

if(choice[c].character+hinput >= (CharacterSelection.y*5))
choice[c].character = (CharacterSelection.y)*5;
else
choice[c].character = (CharacterSelection.y)*5 + iconsInRow - 1;

}

}

if(im.c[c].GetInput("Vertical") != 0 && inputLock[c] == false){

inputLock[c] = true;

var vinput = -Mathf.Sign(im.c[c].GetInput("Vertical"));

if(vinput > 0){
if(CharacterSelection.y == choicesPerColumn-1 || choice[c].character + 5 >= gd.Characters.Length)
choice[c].character = (vinput*(choice[c].character%5));
else 
choice[c].character += (vinput*5);
}

if(vinput < 0){

if(CharacterSelection.y == 0){

if(choice[c].character >= (gd.Characters.Length%5))
choice[c].character -= 5;

choice[c].character += (vinput*(gd.Characters.Length%5));

}else{
choice[c].character += (vinput*5);
}

}
}


if(submitBool && gd.Characters[choice[c].character].Unlocked != UnlockedState.Locked){

if(gd.Characters[choice[c].character].selectedSound != null)
sm.PlaySFX(gd.Characters[choice[c].character].selectedSound);

ready[c] = true;
}

if(cancelBool){
Resetready();
transform.GetComponent(Main_Menu).Return();
hidden = true;
}

}else{

if(cancelBool){
ready[c] = false;
}

}

choice[c].character = NumClamp(choice[c].character,0,gd.Characters.Length);

}
}


////////////////////////////////////////////////////////////////////////////////////Pointless Divider//////////////////////////////////////////////////////////////////////////


if(state == 1){ //Hat Select

im.allowedToChange = false;

stateTexture = Resources.Load("UI Textures/New Character Select/hat",Texture2D);

var hatCounter : int;

for(i = 0; i < choicesPerColumn;i++){
for(j = 0; j < 5;j++){

if((i*5) + j < gd.Hats.Length){

var haticonRect : Rect = Rect(20 + (j*iconWidth),startHeight + (i*iconWidth),iconWidth,iconWidth);

var haticon : Texture2D;
if(gd.Hats[hatCounter].Unlocked == true)
haticon = gd.Hats[hatCounter].Icon;
else
haticon = Resources.Load("UI Textures/Character Icons/question_mark",Texture2D);

GUI.DrawTexture(haticonRect,haticon);

for(var hatCount : int = 0; hatCount < im.c.Length; hatCount++)
if(im.c[hatCount].inputName == "Key_" && im.MouseIntersects(haticonRect) && !ready[hatCount] )
choice[hatCount].hat = hatCounter;

hatCounter += 1;

}else{
break;
}

}
}

//Render Cursor
for(c = 0; c < im.c.Length;c++){

if(!hidden){
submitInput = im.c[c].GetInput("Submit");
submitBool = (submitInput != 0);

cancelInput = im.c[c].GetInput("Cancel");
cancelBool = (cancelInput != 0);
}

var selectedhat = choice[c].hat;

var HatSelection : Vector2 = Vector2(selectedhat%5,selectedhat/5);

cursorPosition[c] = Vector2.Lerp(cursorPosition[c],HatSelection,Time.deltaTime*cursorSpeed);

CursorRect = Rect(20 + cursorPosition[c].x * iconWidth,startHeight + cursorPosition[c].y * iconWidth,iconWidth,iconWidth);
CursorTexture = Resources.Load("UI Textures/Cursors/Cursor_"+c,Texture2D);
GUI.DrawTexture(CursorRect,CursorTexture);


//Get new Input
if(ready[c] == false){
if(im.c[c].GetInput("Horizontal") != 0 && inputLock[c] == false){

inputLock[c] = true;

hinput = Mathf.Sign(im.c[c].GetInput("Horizontal"));

if((HatSelection.y+1)*5 <= gd.Hats.length)
iconsInRow = 5;
else
iconsInRow = (gd.Hats.Length-(HatSelection.y*5));

if(choice[c].hat +hinput < (HatSelection.y*5) + iconsInRow && choice[c].hat+hinput >= (HatSelection.y*5))
choice[c].hat += hinput;
else{

if(choice[c].hat+hinput >= (HatSelection.y*5))
choice[c].hat = (HatSelection.y)*5;
else
choice[c].hat = (HatSelection.y)*5 + iconsInRow - 1;

}

}

if(im.c[c].GetInput("Vertical") != 0 && inputLock[c] == false){

inputLock[c] = true;

vinput = -Mathf.Sign(im.c[c].GetInput("Vertical"));

if(vinput > 0){
if(HatSelection.y == choicesPerColumn-1 || choice[c].hat + 5 >= gd.Hats.Length)
choice[c].hat = (vinput*(choice[c].hat%5));
else 
choice[c].hat += (vinput*5);
}

if(vinput < 0){

if(HatSelection.y == 0){

if(choice[c].hat >= (gd.Hats.Length%5))
choice[c].hat -= 5;

choice[c].hat += (vinput*(gd.Hats.Length%5));

}else{
choice[c].hat += (vinput*5);
}

}
}


if(submitBool && gd.Hats[choice[c].hat].Unlocked == true){
ready[c] = true;
}

if(cancelBool){
Resetready();
state = 0;
}

}else{

if(cancelBool){
ready[c] = false;
}
}

choice[c].hat = NumClamp(choice[c].hat,0,gd.Hats.Length);

if(im.c[c].GetInput("Horizontal") == 0 && im.c[c].GetInput("Vertical") == 0)
inputLock[c] = false;

}
}

if(state == 2){

for(i = 0; i < im.c.Length;i++){

if(im.c.Length == 1)
kartSelect(i,0);

if(im.c.Length == 2)
kartSelect(i,i+1);

if(im.c.Length > 2)
kartSelect(i,3 + i);

}
}

if(state == 3){

gd.currentChoices = choice;
hidden = true;

}

if(stateTexture != null){

var ratio : float = (Screen.width/3f)/stateTexture.width;

GUI.DrawTexture(Rect(hiddenFloat + 10,10,Screen.width/3f,stateTexture.height*ratio),stateTexture,ScaleMode.ScaleToFit);

}

if(im.c.Length > 0){
var allReady : boolean = true;

for( i = 0; i < im.c.Length;i++)
if(ready[i] == false)
allReady = false;

if(allReady){
Resetready();
state += 1;
}
}else
Resetready();

if(submitBool)
sm.PlaySFX(ConfirmSound);

if(cancelBool)
sm.PlaySFX(BackSound);

}
}

function kartSelect(c : int,pos : int){

iconWidth = (Screen.width/2f)/5;
var Heightratio : float = ((Screen.width/3f)/1000f)*200f;
var BoardHeight : float = iconWidth*6f + 10 - Heightratio;

//0 = full screen, 1 = 2 player (vertical) top, 2 = 2 player (vertical) bottom, 3 = 4 player (top left), 4  = 4 player (top right), 5  = 4 player (bottom left), 6 = 4 player (bottom right)
var areaRect : Rect;
if(pos == 0)
areaRect = Rect(20,10 + Heightratio/2f + Screen.height/2f - BoardHeight/2f,iconWidth*5f,BoardHeight - 20);

if(pos == 1)
areaRect = Rect(20,10 + Heightratio/2f + (Screen.height/2f - BoardHeight/2f),iconWidth*5f ,BoardHeight/2f - 20);

if(pos == 2)
areaRect = Rect(20,10 + Heightratio/2f + Screen.height/2f,iconWidth*5f,BoardHeight/2f - 20);

if(pos == 3)
areaRect = Rect(20,10 + Heightratio/2f + (Screen.height/2f - BoardHeight/2f),(iconWidth*5f + 20)/2f  - 20,BoardHeight/2f - 20);

if(pos == 4)
areaRect = Rect(20 + (iconWidth*5f + 20)/2f,10 + Heightratio/2f + (Screen.height/2f - BoardHeight/2f),(iconWidth*5f + 20)/2f  - 20,BoardHeight/2f - 20);

if(pos == 5)
areaRect = Rect(20,10 + Heightratio/2f + Screen.height/2f ,(iconWidth*5f + 20)/2f  - 20,BoardHeight/2f - 20);

if(pos == 6)
areaRect = Rect(20 + (iconWidth*5f + 20)/2f,10 + Heightratio/2f + Screen.height/2f ,(iconWidth*5f + 20)/2f  - 20,BoardHeight/2f - 20);


GUI.BeginGroup(areaRect);

var kartIcon : Texture2D = gd.Karts[choice[c].kart].Icon;
var wheelIcon : Texture2D = gd.Wheels[choice[c].wheel].Icon;

var idealWidth : float = areaRect.width/2f;
var nRatio : float = idealWidth/kartIcon.width;
var idealheight = kartIcon.height * nRatio;

var kartRect : Rect = Rect(0,areaRect.height/2f - idealheight/2f,idealWidth,idealheight);
var wheelRect : Rect = Rect(areaRect.width/2f,areaRect.height/2f - idealheight/2f,idealWidth,idealheight);

GUI.DrawTexture(kartRect,kartIcon,ScaleMode.ScaleToFit);
GUI.DrawTexture(wheelRect,wheelIcon,ScaleMode.ScaleToFit);

var kartUpRect : Rect = Rect(0,areaRect.height/2f - idealheight/2f - idealheight/4f,idealWidth,idealheight/4f);
var kartDownRect : Rect = Rect(0,areaRect.height/2f + idealheight/2f,idealWidth,idealheight/4f);
GUI.DrawTexture(kartUpRect,Resources.Load("UI Textures/New Main Menu/Up_Arrow",Texture2D),ScaleMode.ScaleToFit);
GUI.DrawTexture(kartDownRect,Resources.Load("UI Textures/New Main Menu/Down_Arrow",Texture2D),ScaleMode.ScaleToFit);

var wheelUpRect : Rect = Rect(areaRect.width/2f,areaRect.height/2f - idealheight/2f - idealheight/4f,idealWidth,idealheight/4f);
var wheelDownRect : Rect = Rect(areaRect.width/2f,areaRect.height/2f + idealheight/2f,idealWidth,idealheight/4f);
//GUI.DrawTexture(wheelUpRect,Resources.Load("UI Textures/New Main Menu/Up_Arrow",Texture2D),ScaleMode.ScaleToFit);
//GUI.DrawTexture(wheelDownRect,Resources.Load("UI Textures/New Main Menu/Down_Arrow",Texture2D),ScaleMode.ScaleToFit);

var kartUp = im.MouseIntersects(Rect(areaRect.x+kartUpRect.x,areaRect.y+kartUpRect.y,kartUpRect.width,kartUpRect.height));
var kartDown = im.MouseIntersects(Rect(areaRect.x+kartDownRect.x,areaRect.y+kartDownRect.y,kartDownRect.width,kartDownRect.height));
var wheelUp = im.MouseIntersects(Rect(areaRect.x+wheelUpRect.x,areaRect.y+wheelUpRect.y,wheelUpRect.width,wheelUpRect.height));
var wheelDown = im.MouseIntersects(Rect(areaRect.x+wheelDownRect.x,areaRect.y+wheelDownRect.y,wheelDownRect.width,wheelDownRect.height));

if(kartUp && im.c[c].inputName == "Key_" && Input.GetMouseButton(0))
	choice[c].kart = NumClamp(choice[c].kart + 1,0,gd.Karts.Length);

if(kartDown && im.c[c].inputName == "Key_" && Input.GetMouseButton(0))
	choice[c].kart = NumClamp(choice[c].kart - 1,0,gd.Karts.Length);
	
if(wheelUp && im.c[c].inputName == "Key_" && Input.GetMouseButton(0))
	choice[c].wheel = NumClamp(choice[c].wheel + 1,0,gd.Wheels.Length);

if(wheelDown && im.c[c].inputName == "Key_" && Input.GetMouseButton(0))
	choice[c].wheel = NumClamp(choice[c].wheel - 1,0,gd.Wheels.Length);	

//Render Cursor
var CursorTexture : Texture2D = Resources.Load("UI Textures/Cursors/Cursor_"+c,Texture2D);

if(!kartSelected[c])
GUI.DrawTexture(kartRect,CursorTexture);
else
GUI.DrawTexture(wheelRect,CursorTexture);

GUI.EndGroup();

var submitInput : float = im.c[c].GetInput("Submit");
var submitBool = (submitInput != 0);

var cancelInput : float = im.c[c].GetInput("Cancel");
var cancelBool = (cancelInput != 0);

if(submitBool)
sm.PlaySFX(ConfirmSound);

if(cancelBool)
sm.PlaySFX(BackSound);

if(im.c[c].GetInput("Horizontal") == 0 && im.c[c].GetInput("Vertical") == 0)
inputLock[c] = false;

if(im.c[c].GetInput("Vertical") != 0 && inputLock[c] == false && ready[c] == false){

inputLock[c] = true;

var vinput = -Mathf.Sign(im.c[c].GetInput("Vertical"));

if(!kartSelected[c]){

choice[c].kart -= vinput;
choice[c].kart = NumClamp(choice[c].kart,0,gd.Karts.Length);

}else{

choice[c].wheel -= vinput;
choice[c].wheel = NumClamp(choice[c].wheel,0,gd.Wheels.Length);

}
}

if(submitBool && !kartUp && !kartDown && !wheelUp && !wheelDown){

if(kartSelected[c] == false)
kartSelected[c] = true;
else
ready[c] = true;

}

if(cancelBool){
if(ready[c] == true)
ready[c] = false;
else{
if(kartSelected[c])
kartSelected[c] = false;
else{
Resetready();
state = 1;
}
}

}

}



function Resetready(){

for(var i : int = 0; i < 4;i++){
ready[i] = false;
kartSelected[i] = false;
loadedCharacter[i] = -1;
loadedHat[i] = -1;
loadedKart[i] = -1;
loadedWheel[i] = -1;
}

}

function ResetEverything(){

state = 2;

ready = new boolean[4];
inputLock = new boolean[4];
kartSelected = new boolean[4];
cursorPosition = new Vector2[4];

loadedCharacter = new int[4];
loadedHat = new int[4];
loadedKart = new int[4];
loadedWheel = new int[4];

gd = GameObject.Find("GameData").GetComponent(CurrentGameData);

var counter : int;

for(var i : int = 0; i < gd.Characters.Length;i++){
if(i%5 == 0)
choicesPerColumn += 1;
}

}

class LoadOut{

var character : int = 0;
var hat : int = 0;
var wheel : int= 0;
var kart : int = 0;

}

function NumClamp(val : int,min : int,max : int){

while(val > max-1)
val -= (max-min);

while(val < min)
val += (max-min);


return val;

}

function OutLineLabel(pos : Rect, text : String,Distance : float){
OutLineLabel(pos,text,Distance,Color.black);
}

function OutLineLabel(pos : Rect, text : String,Distance : float,Colour : Color){
Distance = Mathf.Clamp(Distance,1,Mathf.Infinity);

var style = new GUIStyle(GUI.skin.GetStyle("Label"));
style.normal.textColor = Colour;
GUI.Label(Rect(pos.x+Distance,pos.y,pos.width,pos.height),text,style);
GUI.Label(Rect(pos.x,pos.y+Distance,pos.width,pos.height),text,style);
GUI.Label(Rect(pos.x-Distance,pos.y,pos.width,pos.height),text,style);
GUI.Label(Rect(pos.x,pos.y-Distance,pos.width,pos.height),text,style);
var nstyle = new GUIStyle(GUI.skin.GetStyle("Label"));
nstyle.normal.textColor.a = Colour.a;
GUI.Label(pos,text,nstyle);

}