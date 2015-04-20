#pragma strict

var MasterVolume : int = 100;
var MusicVolume : int = 100;
var SFXVolume : int = 100;
//var DialogueVolume : int = 100;

var fadeTime : float = 0.5f;

private var lastMav : float = -1;

private var mSource : AudioSource;
private var mbeingUsed : boolean;

private var sfxSource : AudioSource;
private var dSource : AudioSource;

function Awake ()
{

MasterVolume = Mathf.Clamp(PlayerPrefs.GetInt("MAV",100),0,100);
MusicVolume = Mathf.Clamp(PlayerPrefs.GetInt("MV",50),0,100);
SFXVolume = Mathf.Clamp(PlayerPrefs.GetInt("SFXV",100),0,100);
//DialogueVolume = Mathf.Clamp(PlayerPrefs.GetInt("DV",100),0,100);

mSource = transform.FindChild("Music").GetComponent.<AudioSource>();
sfxSource = transform.FindChild("SFX").GetComponent.<AudioSource>();
//dSource = transform.FindChild("Dialogue").audio;

mSource.loop = true;

}

function Update ()
{
var mav : float = MasterVolume/100f;
var mv : float = MusicVolume/100f;
var sfxv : float = SFXVolume/100f;
//var dv : float = DialogueVolume/100f;

if(!mbeingUsed && (mSource.volume != mv || lastMav != mav))
{
PlayerPrefs.SetInt("MAV",MasterVolume);
PlayerPrefs.SetInt("MV",MusicVolume);
mSource.volume = mav * mv;
}

if(sfxSource.volume != sfxv || lastMav != mav)
{
PlayerPrefs.SetInt("MAV",MasterVolume);
PlayerPrefs.SetInt("SFXV",SFXVolume);
sfxSource.volume = mav * sfxv;
}
/*if(dSource.volume != dv)
dSource.volume = dv;*/

lastMav = mav;

}

function PlaySFX(nMusic : AudioClip)
{
	if(sfxSource != null)
	{
		sfxSource.PlayOneShot(nMusic,1f);
	}
}

function PlayMusic(nMusic : AudioClip)
{
	if(mSource != null)
	{
		//Wait for current track swap to finish
		while(mbeingUsed)
		yield;

		mbeingUsed = true;

		var finalVolume = mSource.volume;

		if(mSource.isPlaying)
		yield TransitionVolume(0);

		mSource.Stop();
		mSource.clip = nMusic;
		mSource.Play();

		yield TransitionVolume(finalVolume);

		mbeingUsed = false;
	}
}

function StopMusic()
{
//Wait for current track swap to finish
while(mbeingUsed)
yield;

mbeingUsed = true;

if(mSource.isPlaying)
yield TransitionVolume(0);

mSource.Stop();

mbeingUsed = false;

}

function TransitionVolume(endVolume : float)
{
var startTime : float = Time.realtimeSinceStartup;
var startVolume : float = mSource.volume;

while((Time.realtimeSinceStartup-startTime) < fadeTime){
mSource.volume = Mathf.Lerp(startVolume,endVolume,(Time.realtimeSinceStartup-startTime)/fadeTime);
yield;
}

}
