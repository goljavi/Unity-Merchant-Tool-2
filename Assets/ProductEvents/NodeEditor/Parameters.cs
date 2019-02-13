using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parameters {

	private SortedDictionary<string, int> intParameters = new SortedDictionary<string, int>();
	private SortedDictionary<string, float> floatParameters = new SortedDictionary<string, float>();
	private SortedDictionary<string, bool> boolParameters = new SortedDictionary<string, bool>();

	//Lista de nombres de parametros de cada tipo
	public List<string> IntParametersNames {
		get
		{
			return new List<string>(intParameters.Keys);
		}
	}
	public List<string> FloatParametersNames {
		get
		{
			return new List<string>(floatParameters.Keys);
		}
	}
	public List<string> BoolParametersNames {
		get
		{
			return new List<string>(boolParameters.Keys);
		}
	}

	public enum ParameterType { Int, Float, Bool };

	//Devolver el valor dado el nombre del parametro
	//Si se necesita verificar si se obtuvo un valor valido, usar el que tiene "out bool"
	//int
	public int GetInt(string parameterName, out bool success) {
		if (intParameters.ContainsKey(parameterName))
		{
			success = true;
			return intParameters[parameterName];
		} else
		{
			success = false;
			return default(int);
		}
	}
	public int GetInt(string parameterName) {
		bool voidVar;
		return (GetInt(parameterName, out voidVar));
	}

	//float
	public float GetFloat(string parameterName, out bool success) {
		if (floatParameters.ContainsKey(parameterName))
		{
			success = true;
			
			return floatParameters[parameterName];
		} else
		{
			success = false;
			return default(float);
		}
	}
	public float GetFloat(string parameterName) {
		bool voidVar;
		return GetFloat(parameterName, out voidVar);
	}

	//bool
	public bool GetBool(string parameterName, out bool success) {
		if (boolParameters.ContainsKey(parameterName))
		{
			success = true;
			return boolParameters[parameterName];
		} else
		{
			success = false;
			return default(bool);
		}
	}
	public bool GetBool(string parameterName) {
		bool voidVar;
		return (GetBool(parameterName, out voidVar));
	}

	//Devuelve un parametro predeterminado, si existen
	public string DefaultIntName {
		get
		{
			if (IntParametersNames.Count > 0)
				return IntParametersNames[0];
			return "";
		}
	}
	public string DefaultFloatName {
		get
		{
			if (FloatParametersNames.Count > 0)
				return FloatParametersNames[0];
			return "";
		}
	}
	public string DefaultBoolName {
		get
		{
			if (BoolParametersNames.Count > 0)
				return BoolParametersNames[0];
			return "";
		}
	}

	//Crear un parametro
	public void AddInt(string name) {
		if (name != null && name!= "" && !intParameters.ContainsKey(name))
			intParameters.Add(name, 0);
	}
	public void AddFloat(string name) {
		if (name != null && name != "" && !floatParameters.ContainsKey(name))
			floatParameters.Add(name, 0f);
	}
	public void AddBool(string name) {
		if (name != null && name != "" && !boolParameters.ContainsKey(name))
			boolParameters.Add(name, false);
	}

	//Asignar un parametro
	public void SetInt(string name, int value) {
		if (intParameters.ContainsKey(name))
			intParameters[name] = value;
	}
	public void Setfloat(string name, float value) {
		if (floatParameters.ContainsKey(name))
			floatParameters[name] = value;
	}
	public void SetBool(string name, bool value) {
		if (boolParameters.ContainsKey(name))
			boolParameters[name] = value;
	}

	//Renombrar un parametro
	public void RenameParameter(string oldName, string newName, ParameterType type) {
		switch (type)
		{
			case ParameterType.Int:
				if (intParameters.ContainsKey(oldName))
				{
					intParameters[newName] = intParameters[oldName];
					intParameters.Remove(oldName);
				}
				break;
			case ParameterType.Float:
				if (floatParameters.ContainsKey(oldName))
				{
					floatParameters[newName] = floatParameters[oldName];
					floatParameters.Remove(oldName);
				}
				break;
			case ParameterType.Bool:
				if (boolParameters.ContainsKey(oldName))
				{
					boolParameters[newName] = boolParameters[oldName];
					boolParameters.Remove(oldName);
				}
				break;
		}
	}

	//Borrar parametros
	public void DeleteParameter(string name, ComparativeNode.ComparisonType type) {
		switch (type)
		{
			case ComparativeNode.ComparisonType.Float:
				floatParameters.Remove(name);
				break;
			case ComparativeNode.ComparisonType.Int:
				intParameters.Remove(name);
				break;
			case ComparativeNode.ComparisonType.Bool:
				boolParameters.Remove(name);
				break;
		}
	}


	//Obtener data serializada
	public ParametersData GetData() {
		//int
		List<string> intN = new List<string>();
		List<int> intV = new List<int>();
		foreach (var item in intParameters)
		{
			intN.Add(item.Key);
			intV.Add(item.Value);
		}

		//float
		List<string> floatN = new List<string>();
		List<float> floatV = new List<float>();
		foreach (var item in floatParameters)
		{
			floatN.Add(item.Key);
			floatV.Add(item.Value);
		}

		//bool
		List<string> boolN = new List<string>();
		List<bool> boolV = new List<bool>();
		foreach (var item in boolParameters)
		{
			boolN.Add(item.Key);
			boolV.Add(item.Value);
		}

		return new ParametersData
		{
			intNames = intN,
			intValues = intV,
			floatNames = floatN,
			floatValues = floatV,
			boolNames = boolN,
			boolValues = boolV
		};
	}

	//Obtener data como Json
	public string GetJsonData() {
		return JsonUtility.ToJson(GetData());
	}

	//asignar data a partir de la clase de data
	public Parameters SetData(string data) {
		ParametersData converted = JsonUtility.FromJson<ParametersData>(data);
		SetData(converted);
		return this;
	}

	//Asignar data a partir de data serializada
	public Parameters SetData(ParametersData data) {
        if (data.intNames == null) return this;
		//ints
		intParameters.Clear();
		int c = data.intNames.Count <= data.intValues.Count ?
			data.intNames.Count : data.intValues.Count; //uses minimum count in case of disparity
		for (int i = 0; i < c; i++)
			intParameters.Add(data.intNames[i], data.intValues[i]);

		//floats
		floatParameters.Clear();
		c = data.floatNames.Count <= data.floatValues.Count ?
			data.floatNames.Count : data.floatValues.Count; //uses minimum count in case of disparity
		for (int i = 0; i < c; i++)
			floatParameters.Add(data.floatNames[i], data.floatValues[i]);

		//bools
		boolParameters.Clear();
		c = data.boolNames.Count <= data.boolValues.Count ?
			data.boolNames.Count : data.boolValues.Count; //uses minimum count in case of disparity
		for (int i = 0; i < c; i++)
			boolParameters.Add(data.boolNames[i], data.boolValues[i]);
		return this;
	}
}

[System.Serializable]
public class ParametersData {
	public List<string> intNames;
	public List<int> intValues;
	public List<string> floatNames;
	public List<float> floatValues;
	public List<string> boolNames;
	public List<bool> boolValues;
}