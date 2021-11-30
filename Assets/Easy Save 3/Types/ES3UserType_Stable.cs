using System;
using UnityEngine;

namespace ES3Types
{
	[UnityEngine.Scripting.Preserve]
	[ES3PropertiesAttribute("stableNameList", "stableName", "reputation", "alignment", "favor", "warlord", "heroes", "heroesSave", "coaches", "buildings", "contracts", "availableTrainings", "availableTrainingsSave", "finance", "inventory", "activeContract", "leagueLevel")]
	public class ES3UserType_Stable : ES3ObjectType
	{
		public static ES3Type Instance = null;

		public ES3UserType_Stable() : base(typeof(Stable)){ Instance = this; priority = 1; }


		protected override void WriteObject(object obj, ES3Writer writer)
		{
			var instance = (Stable)obj;
			
			writer.WriteProperty("stableNameList", Stable.stableNameList, ES3Type_StringArray.Instance);
			writer.WriteProperty("stableName", instance.stableName, ES3Type_string.Instance);
			writer.WriteProperty("reputation", instance.reputation, ES3Type_int.Instance);
			writer.WriteProperty("alignment", instance.alignment, ES3Type_float.Instance);
			writer.WriteProperty("favor", instance.favor, ES3Type_intArray.Instance);
			writer.WritePropertyByRef("warlord", instance.warlord);
			writer.WriteProperty("heroes", instance.heroes, ES3Internal.ES3TypeMgr.GetES3Type(typeof(System.Collections.Generic.List<Character>)));
			writer.WriteProperty("heroesSave", instance.heroesSave, ES3Internal.ES3TypeMgr.GetES3Type(typeof(System.Collections.Generic.List<CharacterSave>)));
			writer.WriteProperty("coaches", instance.coaches, ES3Internal.ES3TypeMgr.GetES3Type(typeof(System.Collections.Generic.List<Character>)));
			writer.WriteProperty("buildings", instance.buildings, ES3Internal.ES3TypeMgr.GetES3Type(typeof(System.Collections.Generic.List<StableBuilding>)));
			writer.WriteProperty("contracts", instance.contracts, ES3Internal.ES3TypeMgr.GetES3Type(typeof(System.Collections.Generic.List<MissionContract>)));
			writer.WriteProperty("availableTrainings", instance.availableTrainings, ES3Internal.ES3TypeMgr.GetES3Type(typeof(System.Collections.Generic.List<Training>)));
			writer.WriteProperty("availableTrainingsSave", instance.availableTrainingsSave, ES3Internal.ES3TypeMgr.GetES3Type(typeof(System.Collections.Generic.List<TrainingSave>)));
			writer.WriteProperty("finance", instance.finance, ES3Internal.ES3TypeMgr.GetES3Type(typeof(Finance)));
			writer.WriteProperty("inventory", instance.inventory, ES3Internal.ES3TypeMgr.GetES3Type(typeof(System.Collections.Generic.List<Item>)));
			writer.WritePropertyByRef("activeContract", instance.activeContract);
			writer.WriteProperty("leagueLevel", instance.leagueLevel, ES3Type_int.Instance);
		}

		protected override void ReadObject<T>(ES3Reader reader, object obj)
		{
			var instance = (Stable)obj;
			foreach(string propertyName in reader.Properties)
			{
				switch(propertyName)
				{
					
					case "stableNameList":
						Stable.stableNameList = reader.Read<System.String[]>(ES3Type_StringArray.Instance);
						break;
					case "stableName":
						instance.stableName = reader.Read<System.String>(ES3Type_string.Instance);
						break;
					case "reputation":
						instance.reputation = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "alignment":
						instance.alignment = reader.Read<System.Single>(ES3Type_float.Instance);
						break;
					case "favor":
						instance.favor = reader.Read<System.Int32[]>(ES3Type_intArray.Instance);
						break;
					case "warlord":
						instance.warlord = reader.Read<Warlord>();
						break;
					case "heroes":
						instance.heroes = reader.Read<System.Collections.Generic.List<Character>>();
						break;
					case "heroesSave":
						instance.heroesSave = reader.Read<System.Collections.Generic.List<CharacterSave>>();
						break;
					case "coaches":
						instance.coaches = reader.Read<System.Collections.Generic.List<Character>>();
						break;
					case "buildings":
						instance.buildings = reader.Read<System.Collections.Generic.List<StableBuilding>>();
						break;
					case "contracts":
						instance.contracts = reader.Read<System.Collections.Generic.List<MissionContract>>();
						break;
					case "availableTrainings":
						instance.availableTrainings = reader.Read<System.Collections.Generic.List<Training>>();
						break;
					case "availableTrainingsSave":
						instance.availableTrainingsSave = reader.Read<System.Collections.Generic.List<TrainingSave>>();
						break;
					case "finance":
						instance.finance = reader.Read<Finance>();
						break;
					case "inventory":
						instance.inventory = reader.Read<System.Collections.Generic.List<Item>>();
						break;
					case "activeContract":
						instance.activeContract = reader.Read<MissionContract>();
						break;
					case "leagueLevel":
						instance.leagueLevel = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					default:
						reader.Skip();
						break;
				}
			}
		}

		protected override object ReadObject<T>(ES3Reader reader)
		{
			var instance = new Stable();
			ReadObject<T>(reader, instance);
			return instance;
		}
	}


	public class ES3UserType_StableArray : ES3ArrayType
	{
		public static ES3Type Instance;

		public ES3UserType_StableArray() : base(typeof(Stable[]), ES3UserType_Stable.Instance)
		{
			Instance = this;
		}
	}
}