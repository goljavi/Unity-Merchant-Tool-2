using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/* Este script que se encarga de recibir el archivo de tipo "ProductEventMap" 
 * y hacer de interfaz entre el sistema de nodos (servicio) y el script del programador (cliente) */
public class DialogueBehavior : MonoBehaviour {
	//Uso un serializefield para que unity muestre el campo para que diseñador dropee su archivo de mapa de nodos
	[SerializeField]
	private ProductEventMap dialog;

	//Aca guardo el primer "ProductEventObject" que haya, es decir, el primer dialogo que apunta al Start
	private ProductEventObject first;

	/* En esta lista se guardan los objetos "ProductEventObject" que se consiguen procesando la 
     * información de los nodos y es lo que se le entrega al programador para armar la UI */
	private List<ProductEventObject> dialogueObjects;

	//Acá se guarda la referencia de una opción al siguiente ProductEventObject
	private Dictionary<int, ProductEventObject> optionIdToNextDialogue;

	//Aca recibira una copia de los parametros que utilizara para hacer checkeos con el nodo comparativo
	private Parameters parameters;

    public Parameters ParameterData { get { return parameters; } }

	public UnityEvent[] functions;

	void Start() {
		dialogueObjects = new List<ProductEventObject>();
		optionIdToNextDialogue = new Dictionary<int, ProductEventObject>();
		int start = 0;
		foreach (var node in dialog.nodes)
		{
			if (node.windowTitle == "Start")
			{
				//Si el titulo del nodo es "Start" guardo su id
				start = node.id;
			}
		}
		parameters = new Parameters().SetData(dialog.parameters);

	}

}