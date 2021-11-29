using System;
using UnityEngine;

namespace ES3Types
{
	[UnityEngine.Scripting.Preserve]
	[ES3PropertiesAttribute("description", "cooldown", "accuracy", "staminaDamage", "balanceDamage", "mindDamage", "healthDamage", "keyPhysicalAttribute", "keyTechnicalAttribute", "limb", "moveType", "moveWeaponType", "modifiers")]
	public class ES3UserType_Move : ES3ScriptableObjectType
	{
		public static ES3Type Instance = null;

		public ES3UserType_Move() : base(typeof(Move)){ Instance = this; priority = 1; }


		protected override void WriteScriptableObject(object obj, ES3Writer writer)
		{
			var instance = (Move)obj;
			
			writer.WriteProperty("description", instance.description, ES3Type_string.Instance);
			writer.WriteProperty("cooldown", instance.cooldown, ES3Type_int.Instance);
			writer.WriteProperty("accuracy", instance.accuracy, ES3Type_int.Instance);
			writer.WriteProperty("staminaDamage", instance.staminaDamage, ES3Type_int.Instance);
			writer.WriteProperty("balanceDamage", instance.balanceDamage, ES3Type_int.Instance);
			writer.WriteProperty("mindDamage", instance.mindDamage, ES3Type_int.Instance);
			writer.WriteProperty("healthDamage", instance.healthDamage, ES3Type_int.Instance);
			writer.WriteProperty("keyPhysicalAttribute", instance.keyPhysicalAttribute, ES3Type_string.Instance);
			writer.WriteProperty("keyTechnicalAttribute", instance.keyTechnicalAttribute, ES3Type_string.Instance);
			writer.WriteProperty("limb", instance.limb, ES3Internal.ES3TypeMgr.GetES3Type(typeof(CoverShooter.Limb)));
			writer.WriteProperty("moveType", instance.moveType, ES3Internal.ES3TypeMgr.GetES3Type(typeof(MoveType)));
			writer.WriteProperty("moveWeaponType", instance.moveWeaponType, ES3Internal.ES3TypeMgr.GetES3Type(typeof(MoveWeaponType)));
			writer.WriteProperty("modifiers", instance.modifiers, ES3Internal.ES3TypeMgr.GetES3Type(typeof(System.Collections.Generic.List<System.String>)));
		}

		protected override void ReadScriptableObject<T>(ES3Reader reader, object obj)
		{
			var instance = (Move)obj;
			foreach(string propertyName in reader.Properties)
			{
				switch(propertyName)
				{
					
					case "description":
						instance.description = reader.Read<System.String>(ES3Type_string.Instance);
						break;
					case "cooldown":
						instance.cooldown = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "accuracy":
						instance.accuracy = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "staminaDamage":
						instance.staminaDamage = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "balanceDamage":
						instance.balanceDamage = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "mindDamage":
						instance.mindDamage = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "healthDamage":
						instance.healthDamage = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "keyPhysicalAttribute":
						instance.keyPhysicalAttribute = reader.Read<System.String>(ES3Type_string.Instance);
						break;
					case "keyTechnicalAttribute":
						instance.keyTechnicalAttribute = reader.Read<System.String>(ES3Type_string.Instance);
						break;
					case "limb":
						instance.limb = reader.Read<CoverShooter.Limb>();
						break;
					case "moveType":
						instance.moveType = reader.Read<MoveType>();
						break;
					case "moveWeaponType":
						instance.moveWeaponType = reader.Read<MoveWeaponType>();
						break;
					case "modifiers":
						instance.modifiers = reader.Read<System.Collections.Generic.List<System.String>>();
						break;
					default:
						reader.Skip();
						break;
				}
			}
		}
	}


	public class ES3UserType_MoveArray : ES3ArrayType
	{
		public static ES3Type Instance;

		public ES3UserType_MoveArray() : base(typeof(Move[]), ES3UserType_Move.Instance)
		{
			Instance = this;
		}
	}
}