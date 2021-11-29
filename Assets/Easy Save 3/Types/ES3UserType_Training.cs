using System;
using UnityEngine;

namespace ES3Types
{
	[UnityEngine.Scripting.Preserve]
	[ES3PropertiesAttribute("type", "training", "duration", "cost", "dateToTrain", "moveToTrain")]
	public class ES3UserType_Training : ES3ScriptableObjectType
	{
		public static ES3Type Instance = null;

		public ES3UserType_Training() : base(typeof(Training)){ Instance = this; priority = 1; }


		protected override void WriteScriptableObject(object obj, ES3Writer writer)
		{
			var instance = (Training)obj;
			
			writer.WriteProperty("type", instance.type, ES3Internal.ES3TypeMgr.GetES3Type(typeof(Training.Type)));
			writer.WriteProperty("training", instance.training, ES3Type_string.Instance);
			writer.WriteProperty("duration", instance.duration, ES3Type_int.Instance);
			writer.WriteProperty("cost", instance.cost, ES3Type_int.Instance);
			writer.WriteProperty("dateToTrain", instance.dateToTrain, ES3Internal.ES3TypeMgr.GetES3Type(typeof(Game.GameDate)));
			writer.WritePropertyByRef("moveToTrain", instance.moveToTrain);
		}

		protected override void ReadScriptableObject<T>(ES3Reader reader, object obj)
		{
			var instance = (Training)obj;
			foreach(string propertyName in reader.Properties)
			{
				switch(propertyName)
				{
					
					case "type":
						instance.type = reader.Read<Training.Type>();
						break;
					case "training":
						instance.training = reader.Read<System.String>(ES3Type_string.Instance);
						break;
					case "duration":
						instance.duration = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "cost":
						instance.cost = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "dateToTrain":
						instance.dateToTrain = reader.Read<Game.GameDate>();
						break;
					case "moveToTrain":
						instance.moveToTrain = reader.Read<Move>();
						break;
					default:
						reader.Skip();
						break;
				}
			}
		}
	}


	public class ES3UserType_TrainingArray : ES3ArrayType
	{
		public static ES3Type Instance;

		public ES3UserType_TrainingArray() : base(typeof(Training[]), ES3UserType_Training.Instance)
		{
			Instance = this;
		}
	}
}