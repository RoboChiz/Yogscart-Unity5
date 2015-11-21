#pragma strict

static public class MasterMsgTypes
{

	public enum NetworkEvent
	{
		DateMessage
	}
	
	//server to client IDs
	static public function get DateMessageID() : short {return dateMessageID;};  
    static public function set DateMessageID(value : short) {};
    static private var dateMessageID : short = 150;
	
	//Message used to tell client what the date is
	public class DateMessage extends MessageBase {
		
		public var dateString: String;
		
		public function DateMessage(pDate : String)
		{
			dateString = pDate;
		}
		
		public function DateMessage()
		{
			dateString = "";
		}
		
	}
}