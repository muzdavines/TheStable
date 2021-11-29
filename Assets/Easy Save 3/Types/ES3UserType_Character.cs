using System;
using UnityEngine;

namespace ES3Types
{
	[UnityEngine.Scripting.Preserve]
	[ES3PropertiesAttribute("strength", "agility", "reaction", "running", "swordsmanship", "dualwielding", "dodging", "archery", "toughness", "speech", "intelligence", "education", "motivation", "strategist", "economics", "negotiating", "insight", "deception", "intimidation", "lockpicking", "pickpocketing", "trapSetting", "trapDisarming", "pugilism", "martialarts", "melee", "parry", "shieldDefense", "survivalist", "landNavigation", "hunting", "foraging", "herbLore", "camping", "attackMagic", "defenseMagic", "supportMagic", "condition", "sharpness", "health", "maxStamina", "maxBalance", "maxMind", "maxHealth", "knownMoves", "knownMovesSave", "activeMoves", "activeMovesSave", "startingArmor", "startingWeapon", "armor", "weapon", "mat", "contract", "currentTraining", "returnDate", "activeForNextMission", "incapacitated", "modelName", "currentObject", "currentMissionCharacter", "name", "maxHP", "HP", "age")]
	public class ES3UserType_Character : ES3ScriptableObjectType
	{
		public static ES3Type Instance = null;

		public ES3UserType_Character() : base(typeof(Character)){ Instance = this; priority = 1; }


		protected override void WriteScriptableObject(object obj, ES3Writer writer)
		{
			var instance = (Character)obj;
			
			writer.WriteProperty("strength", instance.strength, ES3Type_int.Instance);
			writer.WriteProperty("agility", instance.agility, ES3Type_int.Instance);
			writer.WriteProperty("reaction", instance.reaction, ES3Type_int.Instance);
			writer.WriteProperty("running", instance.running, ES3Type_int.Instance);
			writer.WriteProperty("swordsmanship", instance.swordsmanship, ES3Type_int.Instance);
			writer.WriteProperty("dualwielding", instance.dualwielding, ES3Type_int.Instance);
			writer.WriteProperty("dodging", instance.dodging, ES3Type_int.Instance);
			writer.WriteProperty("archery", instance.archery, ES3Type_int.Instance);
			writer.WriteProperty("toughness", instance.toughness, ES3Type_int.Instance);
			writer.WriteProperty("speech", instance.speech, ES3Type_int.Instance);
			writer.WriteProperty("intelligence", instance.intelligence, ES3Type_int.Instance);
			writer.WriteProperty("education", instance.education, ES3Type_int.Instance);
			writer.WriteProperty("motivation", instance.motivation, ES3Type_int.Instance);
			writer.WriteProperty("strategist", instance.strategist, ES3Type_int.Instance);
			writer.WriteProperty("economics", instance.economics, ES3Type_int.Instance);
			writer.WriteProperty("negotiating", instance.negotiating, ES3Type_int.Instance);
			writer.WriteProperty("insight", instance.insight, ES3Type_int.Instance);
			writer.WriteProperty("deception", instance.deception, ES3Type_int.Instance);
			writer.WriteProperty("intimidation", instance.intimidation, ES3Type_int.Instance);
			writer.WriteProperty("lockpicking", instance.lockpicking, ES3Type_int.Instance);
			writer.WriteProperty("pickpocketing", instance.pickpocketing, ES3Type_int.Instance);
			writer.WriteProperty("trapSetting", instance.trapSetting, ES3Type_int.Instance);
			writer.WriteProperty("trapDisarming", instance.trapDisarming, ES3Type_int.Instance);
			writer.WriteProperty("pugilism", instance.pugilism, ES3Type_int.Instance);
			writer.WriteProperty("martialarts", instance.martialarts, ES3Type_int.Instance);
			writer.WriteProperty("melee", instance.melee, ES3Type_int.Instance);
			writer.WriteProperty("parry", instance.parry, ES3Type_int.Instance);
			writer.WriteProperty("shieldDefense", instance.shieldDefense, ES3Type_int.Instance);
			writer.WriteProperty("survivalist", instance.survivalist, ES3Type_int.Instance);
			writer.WriteProperty("landNavigation", instance.landNavigation, ES3Type_int.Instance);
			writer.WriteProperty("hunting", instance.hunting, ES3Type_int.Instance);
			writer.WriteProperty("foraging", instance.foraging, ES3Type_int.Instance);
			writer.WriteProperty("herbLore", instance.herbLore, ES3Type_int.Instance);
			writer.WriteProperty("camping", instance.camping, ES3Type_int.Instance);
			writer.WriteProperty("attackMagic", instance.attackMagic, ES3Type_int.Instance);
			writer.WriteProperty("defenseMagic", instance.defenseMagic, ES3Type_int.Instance);
			writer.WriteProperty("supportMagic", instance.supportMagic, ES3Type_int.Instance);
			writer.WriteProperty("condition", instance.condition, ES3Type_int.Instance);
			writer.WriteProperty("sharpness", instance.sharpness, ES3Type_int.Instance);
			writer.WriteProperty("health", instance.health, ES3Type_int.Instance);
			writer.WriteProperty("maxStamina", instance.maxStamina, ES3Type_int.Instance);
			writer.WriteProperty("maxBalance", instance.maxBalance, ES3Type_int.Instance);
			writer.WriteProperty("maxMind", instance.maxMind, ES3Type_int.Instance);
			writer.WriteProperty("maxHealth", instance.maxHealth, ES3Type_int.Instance);
			writer.WriteProperty("knownMoves", instance.knownMoves, ES3Internal.ES3TypeMgr.GetES3Type(typeof(System.Collections.Generic.List<Move>)));
			writer.WriteProperty("knownMovesSave", instance.knownMovesSave, ES3Internal.ES3TypeMgr.GetES3Type(typeof(System.Collections.Generic.List<MoveSave>)));
			writer.WriteProperty("activeMoves", instance.activeMoves, ES3Internal.ES3TypeMgr.GetES3Type(typeof(System.Collections.Generic.List<Move>)));
			writer.WriteProperty("activeMovesSave", instance.activeMovesSave, ES3Internal.ES3TypeMgr.GetES3Type(typeof(System.Collections.Generic.List<MoveSave>)));
			writer.WriteProperty("startingArmor", instance.startingArmor, ES3Type_string.Instance);
			writer.WriteProperty("startingWeapon", instance.startingWeapon, ES3Type_string.Instance);
			writer.WritePropertyByRef("armor", instance.armor);
			writer.WritePropertyByRef("weapon", instance.weapon);
			writer.WritePropertyByRef("mat", instance.mat);
			writer.WriteProperty("contract", instance.contract, ES3Internal.ES3TypeMgr.GetES3Type(typeof(EmploymentContract)));
			writer.WritePropertyByRef("currentTraining", instance.currentTraining);
			writer.WriteProperty("returnDate", instance.returnDate, ES3Internal.ES3TypeMgr.GetES3Type(typeof(Game.GameDate)));
			writer.WriteProperty("activeForNextMission", instance.activeForNextMission, ES3Type_bool.Instance);
			writer.WriteProperty("incapacitated", instance.incapacitated, ES3Type_bool.Instance);
			writer.WriteProperty("modelName", instance.modelName, ES3Type_string.Instance);
			writer.WritePropertyByRef("currentObject", instance.currentObject);
			writer.WritePropertyByRef("currentMissionCharacter", instance.currentMissionCharacter);
			writer.WriteProperty("name", instance.name, ES3Type_string.Instance);
			writer.WriteProperty("maxHP", instance.maxHP, ES3Type_int.Instance);
			writer.WriteProperty("HP", instance.HP, ES3Type_int.Instance);
			writer.WriteProperty("age", instance.age, ES3Type_float.Instance);
		}

		protected override void ReadScriptableObject<T>(ES3Reader reader, object obj)
		{
			var instance = (Character)obj;
			foreach(string propertyName in reader.Properties)
			{
				switch(propertyName)
				{
					
					case "strength":
						instance.strength = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "agility":
						instance.agility = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "reaction":
						instance.reaction = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "running":
						instance.running = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "swordsmanship":
						instance.swordsmanship = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "dualwielding":
						instance.dualwielding = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "dodging":
						instance.dodging = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "archery":
						instance.archery = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "toughness":
						instance.toughness = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "speech":
						instance.speech = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "intelligence":
						instance.intelligence = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "education":
						instance.education = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "motivation":
						instance.motivation = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "strategist":
						instance.strategist = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "economics":
						instance.economics = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "negotiating":
						instance.negotiating = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "insight":
						instance.insight = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "deception":
						instance.deception = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "intimidation":
						instance.intimidation = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "lockpicking":
						instance.lockpicking = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "pickpocketing":
						instance.pickpocketing = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "trapSetting":
						instance.trapSetting = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "trapDisarming":
						instance.trapDisarming = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "pugilism":
						instance.pugilism = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "martialarts":
						instance.martialarts = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "melee":
						instance.melee = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "parry":
						instance.parry = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "shieldDefense":
						instance.shieldDefense = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "survivalist":
						instance.survivalist = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "landNavigation":
						instance.landNavigation = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "hunting":
						instance.hunting = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "foraging":
						instance.foraging = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "herbLore":
						instance.herbLore = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "camping":
						instance.camping = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "attackMagic":
						instance.attackMagic = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "defenseMagic":
						instance.defenseMagic = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "supportMagic":
						instance.supportMagic = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "condition":
						instance.condition = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "sharpness":
						instance.sharpness = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "health":
						instance.health = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "maxStamina":
						instance.maxStamina = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "maxBalance":
						instance.maxBalance = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "maxMind":
						instance.maxMind = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "maxHealth":
						instance.maxHealth = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "knownMoves":
						instance.knownMoves = reader.Read<System.Collections.Generic.List<Move>>();
						break;
					case "knownMovesSave":
						instance.knownMovesSave = reader.Read<System.Collections.Generic.List<MoveSave>>();
						break;
					case "activeMoves":
						instance.activeMoves = reader.Read<System.Collections.Generic.List<Move>>();
						break;
					case "activeMovesSave":
						instance.activeMovesSave = reader.Read<System.Collections.Generic.List<MoveSave>>();
						break;
					case "startingArmor":
						instance.startingArmor = reader.Read<System.String>(ES3Type_string.Instance);
						break;
					case "startingWeapon":
						instance.startingWeapon = reader.Read<System.String>(ES3Type_string.Instance);
						break;
					case "armor":
						instance.armor = reader.Read<Armor>();
						break;
					case "weapon":
						instance.weapon = reader.Read<Weapon>();
						break;
					case "mat":
						instance.mat = reader.Read<UnityEngine.Material>(ES3Type_Material.Instance);
						break;
					case "contract":
						instance.contract = reader.Read<EmploymentContract>();
						break;
					case "currentTraining":
						instance.currentTraining = reader.Read<Training>(ES3UserType_Training.Instance);
						break;
					case "returnDate":
						instance.returnDate = reader.Read<Game.GameDate>();
						break;
					case "activeForNextMission":
						instance.activeForNextMission = reader.Read<System.Boolean>(ES3Type_bool.Instance);
						break;
					case "incapacitated":
						instance.incapacitated = reader.Read<System.Boolean>(ES3Type_bool.Instance);
						break;
					case "modelName":
						instance.modelName = reader.Read<System.String>(ES3Type_string.Instance);
						break;
					case "currentObject":
						instance.currentObject = reader.Read<UnityEngine.GameObject>(ES3Type_GameObject.Instance);
						break;
					case "currentMissionCharacter":
						instance.currentMissionCharacter = reader.Read<MissionCharacter>();
						break;
					case "name":
						instance.name = reader.Read<System.String>(ES3Type_string.Instance);
						break;
					case "maxHP":
						instance.maxHP = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "HP":
						instance.HP = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "age":
						instance.age = reader.Read<System.Single>(ES3Type_float.Instance);
						break;
					default:
						reader.Skip();
						break;
				}
			}
		}
	}


	public class ES3UserType_CharacterArray : ES3ArrayType
	{
		public static ES3Type Instance;

		public ES3UserType_CharacterArray() : base(typeof(Character[]), ES3UserType_Character.Instance)
		{
			Instance = this;
		}
	}
}