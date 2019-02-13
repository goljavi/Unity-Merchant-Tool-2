using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Esta es una clase abstracta, lo que significa que no se pueden generar BaseNodes, pero nos sirve para que 
 * cada nodo que herede de BaseNode, herede toda la funcionalidad y podamos tratar a todos los tipos de nodo 
 * como "BaseNode" por polimorfismo hasta que realmente necesitemos saber que tipo de nodo es */
public abstract class BaseNode {
	public int id;

    public Color color;
    public Color defaultColor;

	//Guardamos los nodos que sean padres de este nodo
	public List<BaseNode> parents = new List<BaseNode>();

	//Estas variables las pide GUI.Window()
	public Rect windowRect;
	public string windowTitle;

	//Guardo la referencia a la ventana de nodos en caso de que el nodo necesite llamar a algún metodo de esta
	public ProductEventEditor reference;

	//Transiciones posibles
	public List<string> validTransitions;

	//Para obtener el tipo de nodo
	public abstract string GetNodeType { get; }

	public abstract bool CanTransitionTo(BaseNode node);

	//Estas funciones son llamadas por la ventana de nodos para que cada nodo dibuje su contenido y sus conexiones
	public virtual void DrawNode() { }

	//predeterminada
	public virtual void DrawConnection() {
		if (parents.Count > 0)
		{
			foreach (var parent in parents)
			{
				if (parent != null) ProductEventEditor.DrawNodeConnection(parent.windowRect, windowRect,
					true, Color.white);
			}
		}
	}

	public virtual Color GetBackgroundColor() { return Color.white; }

    /* Cada nodo sabe cual es la data que contiene y por lo tanto tiene que saber como serializarla y deserializarla
     * ya que necesitamos un medio generico por el cual información que de otra forma, es extremadamente variada
     * Por lo tanto al setear u obtener la información que contiene dentro el nodo, la misma se pasa a través de JSON */
    public virtual BaseNode SetNodeData(string data) { return this; }
	public virtual string GetNodeData() { return ""; }

	//Builder
	public virtual BaseNode SetWindowRect(Rect value) {
		windowRect = value;
		return this;
	}

	public virtual BaseNode SetWindowTitle(string value) {
		windowTitle = value;
		return this;
	}

	public virtual BaseNode SetReference(ProductEventEditor value) {
		reference = value;
		return this;
	}

	public virtual BaseNode SetId(int value) {
		id = value;
		return this;
	}

	public virtual BaseNode SetParent(BaseNode value) {
		parents.Add(value);
		return this;
	}
}
