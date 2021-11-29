using System;
using UnityEngine;

namespace ES3Types
{
	[UnityEngine.Scripting.Preserve]
	[ES3PropertiesAttribute("playerStable", "otherStables", "leagues", "gameDate", "freeAgentMarket", "contractMarket", "contractMarketSave", "missionContractList", "modifierList")]
	public class ES3UserType_Game : ES3ComponentType
	{
		public static ES3Type Instance = null;

		public ES3UserType_Game() : base(typeof(Game)){ Instance = this; priority = 1;}


		protected override void WriteComponent(object obj, ES3Writer writer)
		{
			var instance = (Game)obj;
			
			writer.WriteProperty("playerStable", instance.playerStable, ES3UserType_Stable.Instance);
			writer.WriteProperty("otherStables", instance.otherStables, ES3Internal.ES3TypeMgr.GetES3Type(typeof(System.Collections.Generic.List<Stable>)));
			writer.WriteProperty("leagues", instance.leagues, ES3Internal.ES3TypeMgr.GetES3Type(typeof(System.Collections.Generic.List<League>)));
			writer.WriteProperty("gameDate", instance.gameDate, ES3Internal.ES3TypeMgr.GetES3Type(typeof(Game.GameDate)));
			writer.WriteProperty("freeAgentMarket", instance.freeAgentMarket, ES3UserType_FreeAgentMarket.Instance);
			writer.WriteProperty("contractMarket", instance.contractMarket, ES3Internal.ES3TypeMgr.GetES3Type(typeof(System.Collections.Generic.List<MissionContract>)));
			writer.WriteProperty("contractMarketSave", instance.contractMarketSave, ES3Internal.ES3TypeMgr.GetES3Type(typeof(System.Collections.Generic.List<MissionContractSave>)));
			writer.WritePropertyByRef("missionContractList", instance.missionContractList);
			writer.WriteProperty("modifierList", instance.modifierList, ES3Internal.ES3TypeMgr.GetES3Type(typeof(System.Collections.Generic.List<MoveModifier>)));
		}

		protected override void ReadComponent<T>(ES3Reader reader, object obj)
		{
			var instance = (Game)obj;
			foreach(string propertyName in reader.Properties)
			{
				switch(propertyName)
				{
					
					case "playerStable":
						instance.playerStable = reader.Read<Stable>(ES3UserType_Stable.Instance);
						break;
					case "otherStables":
						instance.otherStables = reader.Read<System.Collections.Generic.List<Stable>>();
						break;
					case "leagues":
						instance.leagues = reader.Read<System.Collections.Generic.List<League>>();
						break;
					case "gameDate":
						instance.gameDate = reader.Read<Game.GameDate>();
						break;
					case "freeAgentMarket":
						instance.freeAgentMarket = reader.Read<FreeAgentMarket>(ES3UserType_FreeAgentMarket.Instance);
						break;
					case "contractMarket":
						instance.contractMarket = reader.Read<System.Collections.Generic.List<MissionContract>>();
						break;
					case "contractMarketSave":
						instance.contractMarketSave = reader.Read<System.Collections.Generic.List<MissionContractSave>>();
						break;
					case "missionContractList":
						instance.missionContractList = reader.Read<MissionList>();
						break;
					case "modifierList":
						instance.modifierList = reader.Read<System.Collections.Generic.List<MoveModifier>>();
						break;
					default:
						reader.Skip();
						break;
				}
			}
		}
	}


	public class ES3UserType_GameArray : ES3ArrayType
	{
		public static ES3Type Instance;

		public ES3UserType_GameArray() : base(typeof(Game[]), ES3UserType_Game.Instance)
		{
			Instance = this;
		}
	}
}