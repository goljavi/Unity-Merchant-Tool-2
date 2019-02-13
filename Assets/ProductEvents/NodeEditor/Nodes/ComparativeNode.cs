using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ComparativeNode : BaseNode, INeedsChildren {

	public BaseNode[] children = new BaseNode[2]; //Los nodos hijos a los que se va a elegir
	private int[] childrenIDs = new int[2]; //IDs de los hijos para futura asignacion

	//Tipo de comparacion activa
	public enum ComparisonType { Float, Int, Bool }
	private ComparisonType activeType;
	private ComparisonType previousType;

	//Operacion comparativa activa
	public enum ComparisonOperator { Equals, NotEqual, Lesser, LesserEquals, Greater, GreaterEquals }
	private ComparisonOperator activeOperator;
	private readonly string[] OperatorEnumVisualized = { "==", "!=", "<", "<=", ">", ">=" };

	//Valores con los que se van a comparar
	private string[] parameterNames = new string[2] { "", "" };
	private Parameters parameterSource;
	private int parameterPos1;
	private int parameterPos2;

	//Constantes
	private readonly GUIStyle centeredLabel = new GUIStyle { alignment = TextAnchor.MiddleCenter,  };

	//propiedades
	public override string GetNodeType { get { return "Comparison"; } }
	public Parameters ParameterSource { get { return parameterSource; } set { parameterSource = value; } }

	public override void DrawNode() {
		//typo de comparacion
		activeType = (ComparisonType)EditorGUILayout.EnumPopup(activeType);

		if (parameterSource == null) {
			Debug.Log("No parameters data");
			return;
		}
		//Que tipo de comparacion dibujar
		switch (activeType)
		{
			case ComparisonType.Float:
				ShowFloatNode();
				break;
			case ComparisonType.Int:
				ShowIntNode();
				break;
			case ComparisonType.Bool:
				ShowBoolNode();
				break;
		}
		//Para setear a parametros default
		if (previousType != activeType) previousType = activeType;
	}


	//Metodos para la visualicacion de cada opcion
	#region Types Display

	private void ShowFloatNode() {
		//Setear valores por defecto si se cambio de tipo
		if (previousType != activeType)
		{
			parameterNames[0] = parameterSource.DefaultFloatName;
			parameterNames[1] = parameterSource.DefaultFloatName;
			parameterPos1 = 0;
			parameterPos2 = 0;
		}
		//Crea un nuevo parametro si no existe ninguno de este tipo
		if(parameterSource.FloatParametersNames.Count == 0)
		{
			ParameterSource.AddFloat("Default Float");
		}
		//primer parametro
		parameterPos1 = EditorGUILayout.Popup(parameterPos1,parameterSource.FloatParametersNames.ToArray());
		//if(parameterSource.FloatParametersNames[parameterPos1] == null)
		parameterNames[0] = parameterSource.FloatParametersNames[parameterPos1];
		//Crea una caja con el operador seleccionable
		Rect OperatorRect = EditorGUILayout.GetControlRect();
		GUI.Box(OperatorRect, GUIContent.none);
		activeOperator = (ComparisonOperator)EditorGUI.Popup(OperatorRect,(int)activeOperator,
			OperatorEnumVisualized,centeredLabel);
		//Segundo parametro
		parameterPos2 = EditorGUILayout.Popup(parameterPos2, parameterSource.FloatParametersNames.ToArray());
		parameterNames[1] = parameterSource.FloatParametersNames[parameterPos2];
	}

	private void ShowIntNode() {
		//Setear valores por defecto si se cambio de tipo
		if (previousType != activeType)
		{
			parameterNames[0] = parameterSource.DefaultIntName;
			parameterNames[1] = parameterSource.DefaultIntName;
			parameterPos1 = 0;
			parameterPos2 = 0;
		}
		//Crea un nuevo parametro si no existe ninguno de este tipo
		if (parameterSource.IntParametersNames.Count == 0)
		{
			ParameterSource.AddInt("Default Int");
		}
		//primer parametro
		parameterPos1 = EditorGUILayout.Popup(parameterPos1, parameterSource.IntParametersNames.ToArray());
		parameterNames[0] = parameterSource.IntParametersNames[parameterPos1];
		//Crea una caja con el operador seleccionable
		Rect OperatorRect = EditorGUILayout.GetControlRect();
		GUI.Box(OperatorRect, GUIContent.none);
		activeOperator = (ComparisonOperator)EditorGUI.Popup(OperatorRect, (int)activeOperator,
			OperatorEnumVisualized, centeredLabel);
		//Segundo parametro
		parameterPos2 = EditorGUILayout.Popup(parameterPos2, parameterSource.IntParametersNames.ToArray());
		parameterNames[1] = parameterSource.IntParametersNames[parameterPos2];
	}

	private void ShowBoolNode() {
		//Setear valores por defecto si se cambio de tipo
		if (previousType != activeType)
		{
			parameterNames[0] = parameterSource.DefaultBoolName;
			parameterNames[1] = parameterSource.DefaultBoolName;
			parameterPos1 = 0;
			parameterPos2 = 0;
		}
		//Crea un nuevo parametro si no existe ninguno de este tipo
		if (parameterSource.BoolParametersNames.Count == 0)
		{
			ParameterSource.AddBool("Default Bool");
		}
		EditorGUILayout.GetControlRect();
		parameterPos1 = EditorGUILayout.Popup(parameterPos1, parameterSource.BoolParametersNames.ToArray());
		parameterNames[0] = parameterSource.BoolParametersNames[parameterPos1];
	}


	#endregion Types Display

	//Obtencion del nodo correspondiente al resultado. 
	//Tambien se puede acceder con un booleano externo (si por alguna razon se quisiera)
	public BaseNode GetResult(bool successful) {
		if (!HasChildren())
		{
			throw new System.Exception("One or both children nodes are missing. Make sure to assign them properly.");
		}
		return successful ? children[0] : children[1];
	}
	//Como se usara por default.
	public BaseNode GetResult(Parameters parameters) {
		return GetResult(Compare(parameters));
	}

	//Asignacion de hijos correspondiente a verdadero(0) o falso(1). Si es negativo busca si tiene el ID de child
	//De INeedsChildren
	public void AssignChild(BaseNode child, int childPosition) {
		//si no es un valor definido
		if (!ValidIndex(childPosition))
		{
			//positivo y fuera de rango, error
			if (childPosition > 1)
				throw new System.Exception("Invalid child position. Child must be 0(true) or 1(false), or negative to attempt automatic assignment");

			//Asigna hijo si lo tiene guardado de inicializacion
			if (child.id == childrenIDs[0])
				children[0] = child;
			if (child.id == childrenIDs[1])
				children[1] = child;

			//Si el valor es valido, asignarlo. Para cuando se agrega manualmente.
		} else
		{
			if (children[childPosition] != null)
				children[childPosition].parents.Remove(this);

			children[childPosition] = child;
		}
	}

	//Tiene hijos validos. INeedsChildren
	public bool HasChildren() {
		return (children[0] != null && children[1] != null);
	}

	//forma alternativa de setear hijo, mas directo para asignacion manual.
	public BaseNode SetChild(BaseNode node, bool correspondingCase) {
		int i = correspondingCase ? 0 : 1;
		children[i] = node;
		return this;
	}

	//Ejecuta la comparacion correspondiente
	public bool Compare(Parameters parameters) {
		switch (activeType)
		{
			case ComparisonType.Float:
				return CompareFloat(parameters);
			case ComparisonType.Int:
				return CompareInt(parameters);
			case ComparisonType.Bool:
				return parameters.GetBool(parameterNames[0]);
			default:
				Debug.LogWarning("Invalid Comparison Type Enum at " + this.ToString());
				return false;
		}
	}

	//Comparacion de floats
	private bool CompareFloat(Parameters parameters) {

		float float1 = parameters.GetFloat(parameterNames[0]);
		float float2 = parameters.GetFloat(parameterNames[1]);
		switch (activeOperator)
		{
			case ComparisonOperator.Equals:
				return float1 == float2;
			case ComparisonOperator.NotEqual:
				return !(float1 == float2);
			case ComparisonOperator.Lesser:
				return float1 < float2;
			case ComparisonOperator.LesserEquals:
				return float1 <= float2;
			case ComparisonOperator.Greater:
				return float1 > float2;
			case ComparisonOperator.GreaterEquals:
				return float1 >= float2;
			default:
				Debug.LogWarning("Invalid Comparison Operator Enum at " + this.ToString());
				return false;
		}
	}

	//comparacion de ints
	private bool CompareInt(Parameters parameters) {
		float int1 = parameters.GetInt(parameterNames[0]);
		float int2 = parameters.GetInt(parameterNames[1]);
		switch (activeOperator)
		{
			case ComparisonOperator.Equals:
				return int1 == int2;
			case ComparisonOperator.NotEqual:
				return !(int1 == int2);
			case ComparisonOperator.Lesser:
				return int1 < int2;
			case ComparisonOperator.LesserEquals:
				return int1 <= int2;
			case ComparisonOperator.Greater:
				return int1 > int2;
			case ComparisonOperator.GreaterEquals:
				return int1 >= int2;
			default:
				Debug.LogWarning("Invalid Comparison Operator Enum at " + this.ToString());
				return false;
		}
	}

	private bool ValidIndex(int value) { if (value == 0 || value == 1) return true; else return false; }

	//Asignacion y obtencion de data
	public override BaseNode SetNodeData(string data) {
		ComparativeNodeData converted = JsonUtility.FromJson<ComparativeNodeData>(data);
		childrenIDs = converted.childrenIDs;
		activeType = converted.comparisonType;
		activeOperator = converted.comparisonOperator;
		parameterNames = converted.parameterName;
		return this;
	}

	public override string GetNodeData() {
		return JsonUtility.ToJson(new ComparativeNodeData
		{

			childrenIDs = new int[2] {
				children[0] ==null ? -1 : children[0].id,
				children[1] == null ? -1 : children[1].id
			},
			comparisonType = activeType,
			comparisonOperator = activeOperator,
			parameterName = parameterNames
		});
	}

	public override BaseNode SetReference(ProductEventEditor value) {
		BaseNode baseReturn = base.SetReference(value);
		parameterSource = reference.FileParameters;
		return baseReturn;
	}

	public override bool CanTransitionTo(BaseNode node) {
		List<string> types = new List<string> {
			"Product" };

		return types.Contains(node.GetNodeType);
	}

}

//Datos del nodo.
[System.Serializable]
public struct ComparativeNodeData {
	public int[] childrenIDs;
	public ComparativeNode.ComparisonType comparisonType;
	public ComparativeNode.ComparisonOperator comparisonOperator;
	public string[] parameterName;
}


