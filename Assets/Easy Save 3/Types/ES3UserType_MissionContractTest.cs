using System;
using UnityEngine;

namespace ES3Types
{
	[UnityEngine.Scripting.Preserve]
	[ES3PropertiesAttribute("minHeroes", "maxHeroes", "attributeReq", "attributeReqAmount", "attributeReq2", "attributeReqAmount2")]
	public class ES3UserType_MissionContractTest : ES3ScriptableObjectType
	{
		public static ES3Type Instance = null;

		public ES3UserType_MissionContractTest() : base(typeof(MissionContractTest)){ Instance = this; priority = 1; }


		protected override void WriteScriptableObject(object obj, ES3Writer writer)
		{
			var instance = (MissionContractTest)obj;
			
			writer.WriteProperty("minHeroes", instance.minHeroes, ES3Type_int.Instance);
			writer.WriteProperty("maxHeroes", instance.maxHeroes, ES3Type_int.Instance);
			writer.WriteProperty("attributeReq", instance.attributeReq, ES3Type_string.Instance);
			writer.WriteProperty("attributeReqAmount", instance.attributeReqAmount, ES3Type_int.Instance);
			writer.WriteProperty("attributeReq2", instance.attributeReq2, ES3Type_string.Instance);
			writer.WriteProperty("attributeReqAmount2", instance.attributeReqAmount2, ES3Type_int.Instance);
		}

		protected override void ReadScriptableObject<T>(ES3Reader reader, object obj)
		{
			var instance = (MissionContractTest)obj;
			foreach(string propertyName in reader.Properties)
			{
				switch(propertyName)
				{
					
					case "minHeroes":
						instance.minHeroes = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "maxHeroes":
						instance.maxHeroes = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "attributeReq":
						instance.attributeReq = reader.Read<System.String>(ES3Type_string.Instance);
						break;
					case "attributeReqAmount":
						instance.attributeReqAmount = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "attributeReq2":
						instance.attributeReq2 = reader.Read<System.String>(ES3Type_string.Instance);
						break;
					case "attributeReqAmount2":
						instance.attributeReqAmount2 = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					default:
						reader.Skip();
						break;
				}
			}
		}
	}


	public class ES3UserType_MissionContractTestArray : ES3ArrayType
	{
		public static ES3Type Instance;

		public ES3UserType_MissionContractTestArray() : base(typeof(MissionContractTest[]), ES3UserType_MissionContractTest.Instance)
		{
			Instance = this;
		}
	}
}