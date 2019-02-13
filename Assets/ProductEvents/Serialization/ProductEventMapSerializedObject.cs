using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Este objeto es un nodo en un modo no legible por el editor de nodos. Sirve para que le guste
 * al sistema de serializado de unity (el sistema no admite clases abstractas ni recursión). Acá se almacenan
 * los nodos sin referencia a sus padres, solo conteniendo el ID de los mismos, y la data también de forma serializada */
[System.Serializable]
public class ProductEventMapSerializedObject {
    public int id;
    public List<int> parentIds = new List<int>();
    public Rect windowRect;
    public string windowTitle;
    public string data;
	public string nodeType;
}
