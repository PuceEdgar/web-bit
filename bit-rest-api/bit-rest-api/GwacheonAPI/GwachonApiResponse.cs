using System.Text.Json.Nodes;

namespace BITRestAPI.GwacheonAPI
{
	public class GwachonApiResponse
	{
		public Response response { get; set; }
	}

	public class Response
	{
		public string comMsgHeader { get; set; }
		public MsgHeader msgHeader { get; set; }
		public MsgBody msgBody { get; set; }
	}

	public class MsgBody
	{
		public JsonNode busArrivalList { get; set; }
	}

	public class MsgHeader
	{
		public string queryTime { get; set; }
		public int resultCode { get; set; }
		public string resultMessage { get; set; }
	}

	public class BusArrivalList
	{
		public object crowded1 { get; set; }
		//public object crowded2 { get; set; }
		public string flag { get; set; }
		public object locationNo1 { get; set; }
		//public string locationNo2 { get; set; }
		public object lowPlate1 { get; set; }
		//public string lowPlate2 { get; set; }
		public string plateNo1 { get; set; }
		//public string plateNo2 { get; set; }
		public object predictTime1 { get; set; }
		//public string predictTime2 { get; set; }
		public object remainSeatCnt1 { get; set; }
		//public object remainSeatCnt2 { get; set; }
		public int routeDestId { get; set; }
		public string routeDestName { get; set; }
		public int routeId { get; set; }
		public object routeName { get; set; }
		public int routeTypeCd { get; set; }
		public int staOrder { get; set; }
		public int stationId { get; set; }
		public string stationNm1 { get; set; }
		//public string stationNm2 { get; set; }
		public object taglessCd1 { get; set; }
		//public string taglessCd2 { get; set; }
		public int turnSeq { get; set; }
		public object vehId1 { get; set; }
		//public string vehId2 { get; set; }
		public int? predictTimeSec1 { get; set; }
		public int? stateCd1 { get; set; }
	}
}
