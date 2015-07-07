#pragma strict
import System.IO;
import System.Collections.Generic;

var data : List.<inputData>;
var posData : List.<Vector3>;
var rotData : List.<Quaternion>;
var velData : List.<Vector3>;

var locked : boolean = true;
var reading : boolean;
var processingData : boolean;

var ks : kartScript;
var ki : kartItem;

var dataCount : int = 0;
var posCount : int = 0;
var rotCount : int = 0;
var velCount : int = 0;

private var updateTime : float;

function Awake()
{
	ks = transform.GetComponent(kartScript);
	ki = transform.GetComponent(kartItem);
	
	data = new List.<inputData>();
	posData = new List.<Vector3>();
	rotData = new List.<Quaternion>();
	velData = new List.<Vector3>();
	
	LoadInputs();//Delete later
	
}

function FixedUpdate ()
{
	if(!locked && !processingData)
	{
	
		if(reading)
		{
		
			var t : float = data[dataCount].throttle;
			var s : float = data[dataCount].steer;
			
			ks.throttle = t;
			ks.steer = s;
			ks.drift = data[dataCount].drift;
			ki.input = data[dataCount].item;
			
			if(dataCount%60 == 0)
			{		
				transform.position = posData[posCount];
				transform.rotation = rotData[posCount];
				GetComponent.<Rigidbody>().velocity = velData[posCount];
				posCount++;
				rotCount++;
			}	
			
		}
		else
		{
			//Write Data
			data.Add(new inputData(ks.throttle,ks.steer,ks.drift,ki.input));
			
			if(dataCount%60 == 0)
			{
				posData.Add(transform.position);
				rotData.Add(transform.rotation);
				velData.Add(GetComponent.<Rigidbody>().velocity);
				posCount++;
				rotCount++;
			}
			
		}
		
		dataCount++;
	
	}	
}

function LoadInputs()
{
	processingData = true;
	
	data = new List.<inputData>();
	posData = new List.<Vector3>();
	rotData = new List.<Quaternion>();
	velData = new List.<Vector3>();
	
	dataCount = 0;
	posCount = 0;
	rotCount = 0;
	
	var sr = new File.OpenText(Application.persistentDataPath + "ReplayTest.ycr");
	
	var input: String = "";
	
    while (true) 
    {
        input = sr.ReadLine();  
        
        if (input == null) 
        {
        	break; 
        }
        else
        {
        	switch(input[0])
        	{
        		case 'd':
	        		input = input.Substring(1);
		        	var words : String[] = input.Split(";"[0]);
		        	var floatData : float[] = new float[4];
		        	
		        	for(var i : int = 0; i < words.Length; i++)
		        	{
		        		floatData[i] = float.Parse(words[i]);
		        	}
		        	
		        	if(floatData[2] == 0)
		        	{
		        		if(floatData[3] == 0)
		        			data.Add(new inputData(floatData[0],floatData[1],false,false));
	        			else
	        				data.Add(new inputData(floatData[0],floatData[1],false,true));
	        		}
		        	else
		        	{
		        		if(floatData[3] == 0)
		        			data.Add(new inputData(floatData[0],floatData[1],true,false));
	        			else
	        				data.Add(new inputData(floatData[0],floatData[1],true,true));
	        		}
		    		
		    		dataCount++;
	    		break;
	    		case 'p':
    				input = input.Substring(1);
	    			words = input.Split(":"[0]);
		        	floatData = new float[3];
		        	
		        	for(i = 0; i < words.Length; i++)
		        	{
		        		floatData[i] = float.Parse(words[i]);
		        	}
		        	
		        	posData.Add(Vector3(floatData[0],floatData[1],floatData[2]));
	    			posCount ++;
				break;
				case 'r':
					input = input.Substring(1);
	    			words = input.Split(":"[0]);
		        	floatData = new float[3];
		        	
		        	for(i = 0; i < words.Length; i++)
		        	{
		        		floatData[i] = float.Parse(words[i]);
		        	}
		        	
		        	rotData.Add(Quaternion.Euler(floatData[0],floatData[1],floatData[2]));
	    			rotCount ++;
				break;
				case 'v':
    				input = input.Substring(1);
	    			words = input.Split(":"[0]);
		        	floatData = new float[3];
		        	
		        	for(i = 0; i < words.Length; i++)
		        	{
		        		floatData[i] = float.Parse(words[i]);
		        	}
		        	
		        	velData.Add(Vector3(floatData[0],floatData[1],floatData[2]));
				break;
    		}
        }
    }

    
    sr.Close();
	
	Debug.Log("Loaded " + data.Count + " lines from " + Application.persistentDataPath + "ReplayTest.ycr!");
	Debug.Log("Loaded " + dataCount + " inputs & " + posCount + " position updates!");
	
	dataCount = 0;
	posCount = 0;
	rotCount = 0;
	
	processingData = false;
	reading = true;
	
}

function SaveInputs()
{
	processingData = true;
	dataCount = 0;
	posCount = 0;
	rotCount = 0;
	
  	var sw : StreamWriter = new StreamWriter(Application.persistentDataPath + "ReplayTest.ycr");
	
	sw.WriteLine("//Yogscart Replay Data");
	sw.WriteLine("//Well Done! You can open a .txt file :P Don't be a dick and change your replay though ;)");
	
	for(var i = 0; i < data.Count;i++)
	{
		sw.WriteLine("d" + data[i].ToString());
		
		if(i%60 == 0)
		{
			sw.WriteLine("p" + posData[posCount].x + ":" + posData[posCount].y + ":" + posData[posCount].z);
			sw.WriteLine("r" + rotData[posCount].eulerAngles.x + ":" + rotData[posCount].eulerAngles.y + ":" + rotData[posCount].eulerAngles.z);
			sw.WriteLine("v" + velData[posCount].x + ":" + velData[posCount].y + ":" + velData[posCount].z);
			posCount++;
			rotCount++;
		}
		
	}
	
	sw.Flush();
    sw.Close();
    
    Debug.Log("Saved " + data.Count + " lines to " + Application.persistentDataPath + "ReplayTest.ycr!");
	
	
	dataCount = 0;
	posCount = 0;
	rotCount = 0;
	processingData = false;
}

class inputData extends System.ValueType
{
	var throttle : float;
	var steer : float;
	var drift : boolean;
	var item : boolean;
	
	function inputData(t : float, s : float, d : boolean, i : boolean)
	{
		throttle = t;
		steer = s;
		drift = d;
		item = i;
	}
	
	function ToString()
	{
		if(drift)
		{
			if(item)
				return throttle.ToString() + ";" + steer.ToString() + ";1;1";
			else
				return throttle.ToString() + ";" + steer.ToString() + ";1;0";
		}
		else
		{
			if(item)
				return throttle.ToString() + ";" + steer.ToString() + ";0;1";
			else
				return throttle.ToString() + ";" + steer.ToString() + ";0;0";
		}
	}
}