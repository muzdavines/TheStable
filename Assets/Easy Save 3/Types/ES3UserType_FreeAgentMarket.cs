using System;
using UnityEngine;

namespace ES3Types
{
	[UnityEngine.Scripting.Preserve]
	[ES3PropertiesAttribute("market", "marketSave")]
	public class ES3UserType_FreeAgentMarket : ES3ObjectType
	{
		public static ES3Type Instance = null;

		public ES3UserType_FreeAgentMarket() : base(typeof(FreeAgentMarket)){ Instance = this; priority = 1; }


		protected override void WriteObject(object obj, ES3Writer writer)
		{
			var instance = (FreeAgentMarket)obj;
			
			writer.WriteProperty("market", instance.market, ES3Internal.ES3TypeMgr.GetES3Type(typeof(System.Collections.Generic.List<Character>)));
			writer.WriteProperty("marketSave", instance.marketSave, ES3Internal.ES3TypeMgr.GetES3Type(typeof(System.Collections.Generic.List<CharacterSave>)));
		}

		protected override void ReadObject<T>(ES3Reader reader, object obj)
		{
			var instance = (FreeAgentMarket)obj;
			foreach(string propertyName in reader.Properties)
			{
				switch(propertyName)
				{
					
					case "market":
						instance.market = reader.Read<System.Collections.Generic.List<Character>>();
						break;
					case "marketSave":
						instance.marketSave = reader.Read<System.Collections.Generic.List<CharacterSave>>();
						break;
					default:
						reader.Skip();
						break;
				}
			}
		}

		protected override object ReadObject<T>(ES3Reader reader)
		{
			var instance = new FreeAgentMarket();
			ReadObject<T>(reader, instance);
			return instance;
		}
	}


	public class ES3UserType_FreeAgentMarketArray : ES3ArrayType
	{
		public static ES3Type Instance;

		public ES3UserType_FreeAgentMarketArray() : base(typeof(FreeAgentMarket[]), ES3UserType_FreeAgentMarket.Instance)
		{
			Instance = this;
		}
	}
}