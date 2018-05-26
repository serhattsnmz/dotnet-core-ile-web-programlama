using Newtonsoft.Json;
using Project.Core.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Project.Core.Logger.FileLogger
{
	public class FileLogRepository<T> : ILogRepository<T>
		where T : class, ILog, new()
	{

		private readonly string filePath;

		public FileLogRepository(string _filePath)
		{
			filePath = _filePath;
			if (!File.Exists(filePath))
				File.WriteAllText(filePath, "");
		}
		public List<T> GetList(Func<T, bool> filter = null)
		{
			List<T> json =
				JsonConvert.DeserializeObject<List<T>>(File.ReadAllText(filePath))
				?? new List<T>();

			if (filter != null)
				json = json.Where(filter).ToList();
			return json;
		}

		public void Add(T log)
		{
			List<T> json = 
				JsonConvert.DeserializeObject<List<T>>(File.ReadAllText(filePath))
				?? new List<T>();
			json.Add(log);
			File.WriteAllText(filePath, JsonConvert.SerializeObject(json));
		}

		public void Delete(T log)
		{
			List<T> json =
				JsonConvert.DeserializeObject<List<T>>(File.ReadAllText(filePath))
				?? new List<T>();
			json.Remove(log);
			File.WriteAllText(filePath, JsonConvert.SerializeObject(json));
		}

		public string PrintTModelPropertyAndValue(T tmodelObj)
		{
			string result = "";

			//Getting Type of Generic Class Model
			Type tModelType = tmodelObj.GetType();

			//We will be defining a PropertyInfo Object which contains details about the class property 
			PropertyInfo[] arrayPropertyInfos = tModelType.GetProperties();

			//Now we will loop in all properties one by one to get value
			foreach (PropertyInfo property in arrayPropertyInfos)
			{
				result += property.Name + " : ";
				result += property.GetValue(tmodelObj).ToString();
				result += Environment.NewLine;
			}

			result += "---" + Environment.NewLine;

			return result;
		}
	}
}
