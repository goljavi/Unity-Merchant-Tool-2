using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

/* Este script que se encarga de recibir el archivo de tipo "ProductEventMap", deserializarlo, mostrar
 * la interfaz de nodos, permitir la edición de los mismos y la serialización al guardar los mismos. */
public class ProductEventEditor : EditorWindow {

    #region VARIABLES
    //Aca se guardan las variables de la toolbar
    private GUIStyle myStyle;
    private GUIStyle parametersTextStyle;
    private float toolbarHeight = 100;
    private bool changesMade;

    //Variables para el paneo
    Vector2 _scrollPos;
    Vector2 _scrollStartPos;

    //Variables para la grilla
    private UnityEditor.Graphs.Graph graph;
    private GraphGUITest graphGUI;

    class GraphGUITest : UnityEditor.Graphs.GraphGUI
    {
    }

    //Acá se guardan los nodos que se muestran en la ventana. Esta lista se muestra en cada OnGUI() -> DrawNodes()
    List<BaseNode> _nodes = new List<BaseNode>();

    //Acá se guarda la posición del mouse dentro de la ventana. Se guarda en cada OnGUI()
    Vector3 _mousePosition;

    /* Acá se guardan los ultimo nodos en los cual se hizo click derecho y click izquierdo (según corresponde)
     * Esto sirve para poder identificar que nodo quiso seleccionar la persona a la hora de realizar una acción
     * Se guardan en la función OnGUI() -> UserInput()
     */
    BaseNode _lastRightClickedNode;
    BaseNode _lastLeftClickedNode;

    //Bool para saber si el nodo está focuseado
    bool _focusedNode;

    //Interactive connection arrow
    bool interactiveConnectionModeActive;
    BaseNode interactiveConnectionOriginNode;
    bool interactiveConnecitonComparativeState; //solo setear false para false connections

    //Contiene la referencia al archivo en el cual se está serializando la información
    ProductEventMap _assetFile;

    /* Contiene una lista con todos los ID de todos los nodos, generados al azar. 
     * Se usa unicamente con la intención de que no se repitan nunca los ID asignados a los nodos.
     * Se le asigand ID's a los nodos para poder serializarlos, ya que el sistema de unity no permite
     * Que un nodo hijo contenga a su nodo padre (podria darse una recursión infinita)
     * Se actualiza con la función GetNewId() 
     */
    List<int> _idList = new List<int>();

	//Estos son los parametros que los nodos comparativos van a usar. 
	//Los valores que guarda son los definidos por defecto
	//En runtime va a usar una copia de esta clase para no modificar el archivo.
	//TODO: Implemetar en esta clase
	Parameters _fileParameters = new Parameters();

	public Parameters FileParameters {get{return _fileParameters;}}

	//Tipo de parametros a mostrar
    private ComparativeNode.ComparisonType activeType;

	//Nombre del proximo parametro a crear
    private string parameterAsignationName;

    //En este enum están todas las posibles acciones a las que se puede llamar
    //haciendo click derecho en el editor ya sea en un nodo individual o no.
    public enum UserActions
    {
        addProductNode,
        deleteNode,
        addConnection,
	    addComparisonNode,
	    addConnectionAsFalse,
        resetScroll
    }
    #endregion

    #region SERIALIZADO E INTERPRETE
    /* En esta función se deserealiza el archivo "ProductEventMap". Se llama desde el archivo
     * ProductEventMapEditor Ese archivo se encarga de mostrar el botón para abrir la ventana de 
     * nodos de ese archivo en particular, por lo tanto es el que contiene la referencia al archivo.
     * En esta función se agarra la lista de "ProductEventMapSerializedObject" que contiene cada nodo en
     * un modo que no es legible por el editor de nodos pero que sirve para que le guste al sistema de
     * serializado de unity (el sistema no admite clases abstractas ni recursión). Esta función hace de 
     * interprete, pasando de ProductEventMapSerializedObject a el tipo de nodo que corresponda 
     * (basenode, startnode, endnode, etc)
     */
    public void LoadAssetFile(ProductEventMap assetFile)
    {
        changesMade = false;
        _assetFile = assetFile;

		//Se carga la data de los parametros
		_fileParameters.SetData(_assetFile.parameters);

        //Se borran las listas en caso de que haya información anterior no deseada
        _nodes.Clear();
        _idList.Clear();

        if (assetFile.nodes.Count == 0)
        {
            AddNode<StartNode>(new Rect(300, 400, 100, 100), GetNewId());
            SaveAssetFile();
            return;
        }

        //Interpreto en base al título que clase de nodo se guardó y lo genero en la ventana
        foreach (ProductEventMapSerializedObject item in assetFile.nodes)
        {
            //Guardo el ID de cada uno en la lista de ID's, para que al crear nuevos no se superpongan
            _idList.Add(item.id);

			switch (item.nodeType)
			{
				case "Start":
					AddNode<StartNode>(item.windowRect, item.id);
					break;
				case "Product":
                    /* En este caso "AddProductNode" devuelve el nodo que crea, por lo tanto, 
				* utilizando el nodo que devuelve puedo usar su función SetNodeData() y pasarle la varialbe
				* jsonObject para que el nodo se encargue de interpretarla y rellenar el contenido del nodo */
                    AddNode<ProductNode>(item.windowRect, item.id).SetNodeData(item.data);
					break;
                case "Comparison":
                    AddNode<ComparativeNode>(item.windowRect, item.id).SetNodeData(item.data);
                    break;
            }
        }

        /* Una vez que están todos creados por separado les asigno sus padres a cada uno
         * Por cada "ProductEventMapSerializedObject" (detro de el archivo serializado)
         */
        foreach (ProductEventMapSerializedObject item in assetFile.nodes)
        {
            //Por cada Nodo (no serializado, sino dentro del editor)
            foreach (var node in _nodes)
            {
                /* Busco la coincidencia, es decir que estoy parado en el mismo nodo
                 * tanto en el foreach de ProductEventMapSerializedObject como en el de los Nodos
                 */
                if (node.id == item.id)
                {
                    /* Si hay coincidencia recorro cada parentId del ProductEventMapSerializedObject
                     * Ya que lo necesito para luego buscar la coincidencia entre el ID y el nodo 
                     * generado y así finalmente asignarle el padre a su hijo
                     */
                    foreach (var parentId in item.parentIds)
                    {
                        /* Vuelvo a hacer un recorrido de cada uno de los nodos ya 
                         * generados para encontrar el nodo que contenga el parentId
                         */
                        foreach (var n in _nodes)
                        {
                            if (n.id == parentId)
                            {
                                /* Si hubo coincidencia seteo al nodo que se encontró recorriendo los parentId del objeto serializado
                                 * como padre del nodo que se encontró recorriendo los id del objeto serializado
                                 */
                                node.SetParent(n);

								if(n is INeedsChildren)
								{
									((INeedsChildren)n).AssignChild(node);
								}
                            }
                        }
                    }
                }
            }
        }

    }

    //Acá se convierte cada Nodo (guardado en la variable _nodes) a "ProductEventMapSerializedObject" 
    //y se guarda en una lista en el objeto serializado (de tipo "ProductEventMap")
    public void SaveAssetFile()
    {
		//Guarda la data de parametros
		if(_fileParameters != null) _assetFile.parameters = _fileParameters.GetData();

        //Borro cualquier información previamente guardada en el archivo
        _assetFile.nodes.Clear();

        //Por cada nodo
        foreach (var node in _nodes)
        {
            //Genero una lista de ID's de los padres del nodo
            List<int> parentsIds = new List<int>();
            foreach(var parent in node.parents)
            {
                if(parent != null) parentsIds.Add(parent.id);
            }

			//Genero el ProductEventMapSerializedObject y lo agrego a la lista
			_assetFile.nodes.Add(
				new ProductEventMapSerializedObject() {
					id = node.id,
					parentIds = parentsIds,
					windowRect = node.windowRect,
					windowTitle = node.windowTitle,
					data = node.GetNodeData(),
					nodeType = node.GetNodeType
                }
            );
        }

        //Esto no sé bien que hace pero se solucionó un bug usandolo.
        EditorUtility.SetDirty(_assetFile);

        changesMade = false;
    }
    #endregion

    #region DIBUJADO DE LOS NODOS Y REGISTRO DE INPUT

    private void OnEnable()
    {
        graph = CreateInstance<UnityEditor.Graphs.Graph>();
        graphGUI = CreateInstance<GraphGUITest>();
        graphGUI.graph = graph;
    }

    //Es el update del EditorWindow
    private void OnGUI()
    {

        //Logeo la posición del mouse
        Event e = Event.current;
        _mousePosition = e.mousePosition;

        //Registro si hizo click izquierdo o derecho
        UserInput(e);

        graphGUI.BeginGraphGUI(this, new Rect(0f, 0f, position.width, position.height));

        graphGUI.EndGraphGUI();

        DrawInteractiveConnection(e);

        //Dibujo los nodos sobre la ventana
        DrawNodes();

        //Dibujo la Toolbar despues de los nodos para que los tape
        DrawToolbar();

        DrawParameters();

        PaintNode();
    }

    void DrawInteractiveConnection(Event e)
    {
        //Interactive connection mode. Dibujo flecha interactiva para conectar nodos.
        if (interactiveConnectionModeActive && interactiveConnectionOriginNode != null)
        {
            var mouseRect = new Rect(_mousePosition, Vector2.zero);
            DrawNodeConnection(interactiveConnectionOriginNode.windowRect, mouseRect, false, Color.yellow);

            if (e.type == EventType.MouseDown && e.button == 0)
            {
                if (_lastLeftClickedNode == null) return;
                if (interactiveConnecitonComparativeState == true)
                {
                    AddConnection(interactiveConnectionOriginNode, _lastLeftClickedNode);
                }
                else
                {
                    AddFalseConnection(interactiveConnectionOriginNode, _lastLeftClickedNode);
                }
                interactiveConnectionModeActive = false;
                interactiveConnectionOriginNode = null;
            }
        }
    }

    void DrawToolbar()
    {
        //Estos son los valores del GUIStyle
        var mySelf = GetWindow<ProductEventEditor>();
        mySelf.myStyle = new GUIStyle();
        mySelf.myStyle.fontSize = 20;
        mySelf.myStyle.alignment = TextAnchor.MiddleCenter;

        //Esta es la Toolbar con el titulo y el boton
        EditorGUI.DrawRect(new Rect(0, 0, position.width, toolbarHeight), Color.gray);
        EditorGUILayout.BeginVertical(GUILayout.Height(100));
        EditorGUILayout.LabelField("Product Event Editor", myStyle, GUILayout.Height(50));
        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();

        if (changesMade) GUI.backgroundColor = Color.cyan;
        else GUI.backgroundColor = Color.white;

        if (GUILayout.Button("Save map", GUILayout.Width(150), GUILayout.Height(30)))
        {
            //Guardo la información registrada hasta el momento
            SaveAssetFile();
        }

        GUI.backgroundColor = Color.white;
        if (GUILayout.Button("Discard changes", GUILayout.Width(150), GUILayout.Height(30)))
        {
            //Cargo la información registrada hasta el momento
            LoadAssetFile(_assetFile);
        }

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
    }

    void DrawParameters()
    {
        //Style de parameters
        var mySelf = GetWindow<ProductEventEditor>();
        mySelf.myStyle = new GUIStyle();
        mySelf.myStyle.fontSize = 13;
        mySelf.myStyle.alignment = TextAnchor.MiddleCenter;

        //Estas es la ventana de parameters
        Rect paramsRect = new Rect(0, 100, 200, position.height - 100);
        EditorGUILayout.BeginHorizontal(GUILayout.Width(paramsRect.width));
        EditorGUI.DrawRect(paramsRect, new Color32(155, 155 ,155, 255));
        EditorGUILayout.BeginVertical(GUILayout.Height(100));
        EditorGUILayout.LabelField("Parameters", myStyle, GUILayout.Height(50));


        //Selector de tipo de parametros
        EditorGUILayout.LabelField("Type selector:", myStyle, GUILayout.Height(50));
        activeType = (ComparativeNode.ComparisonType)EditorGUILayout.EnumPopup(activeType, GUILayout.Width(paramsRect.width - 10));

        //Funciones que ejecuta segun el tipo
        switch (activeType)
        {
            case ComparativeNode.ComparisonType.Float:
                ShowParametersFloat(paramsRect.width);
                break;
            case ComparativeNode.ComparisonType.Int:
                ShowParametersInt(paramsRect.width);
                break;
            case ComparativeNode.ComparisonType.Bool:
                ShowParametersBool(paramsRect.width);
                break;
            default:
                break;
        }


        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();
    }

    //Metodo que muestra los parametros tipo float
    void ShowParametersFloat(float paramsWidth)
    {
        EditorGUILayout.BeginHorizontal(GUILayout.Width(paramsWidth -10));
        if (GUILayout.Button("Create", GUILayout.Width(80), GUILayout.Height(20)))
        {
            FileParameters.AddFloat(parameterAsignationName);
            parameterAsignationName = "";
		}

        parameterAsignationName = EditorGUILayout.TextField(parameterAsignationName);
        EditorGUILayout.EndHorizontal();


        foreach (var item in FileParameters.FloatParametersNames)
        {

            FileParameters.Setfloat(item,EditorGUILayout.FloatField(item, FileParameters.GetFloat(item), GUILayout.Width(paramsWidth-10)));
            if (GUILayout.Button("Delete", GUILayout.Width(60), GUILayout.Height(20)))
            {
				_fileParameters.DeleteParameter(item, ComparativeNode.ComparisonType.Float);
            }
        }
    }
    //Metodo que muestra los parametros tipo INT
    void ShowParametersInt(float paramsWidth)
    {
        EditorGUILayout.BeginHorizontal(GUILayout.Width(paramsWidth - 20));
        if (GUILayout.Button("Create", GUILayout.Width(80), GUILayout.Height(20)))
        {
            FileParameters.AddInt(parameterAsignationName);
			parameterAsignationName = "";
		}

        parameterAsignationName = EditorGUILayout.TextField(parameterAsignationName);
        EditorGUILayout.EndHorizontal();

        foreach (var item in FileParameters.IntParametersNames)
        {
            FileParameters.SetInt(item, EditorGUILayout.IntField(item, FileParameters.GetInt(item), GUILayout.Width(paramsWidth - 10)));
            if (GUILayout.Button("Delete", GUILayout.Width(60), GUILayout.Height(20)))
            {
				_fileParameters.DeleteParameter(item, ComparativeNode.ComparisonType.Int);
			}
        }
    }

    //Metodo que muestra los parametros tipo Bool
    void ShowParametersBool(float paramsWidth)
    {
        EditorGUILayout.BeginHorizontal(GUILayout.Width(paramsWidth - 20));
        if (GUILayout.Button("Create", GUILayout.Width(80), GUILayout.Height(20)))
        {
            FileParameters.AddBool(parameterAsignationName);
			parameterAsignationName = "";
        }

        parameterAsignationName = EditorGUILayout.TextField(parameterAsignationName);
        EditorGUILayout.EndHorizontal();

        foreach (var item in FileParameters.BoolParametersNames)
        {
            FileParameters.SetBool(item, EditorGUILayout.Toggle(item, FileParameters.GetBool(item), GUILayout.Width(paramsWidth - 10)));
            if (GUILayout.Button("Delete", GUILayout.Width(60), GUILayout.Height(20)))
            {
				_fileParameters.DeleteParameter(item, ComparativeNode.ComparisonType.Bool);
			}
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="comparativeState">True = Default. False = para false connections (Comaparative node)</param>
    private void BeginInteractiveConnectionMode(bool comparativeState = true)
    {
        interactiveConnectionModeActive = true;
        interactiveConnectionOriginNode = _lastRightClickedNode;
        interactiveConnecitonComparativeState = comparativeState;
    }

    //Se encarga de dibujar los nodos sobre la ventana
    void DrawNodes()
    {
        BeginWindows();

        /* Cada tipo de nodo puede tener su preferencia de como dibujar las conexiones entre el y su padre
         * así que recorro todos los nodos y les pido a cada uno que se encargue de dibujar la conexión entre el y su padre */
        foreach (BaseNode n in _nodes)
        {
            n.DrawConnection();
        }

        //Dibujo el nodo sobre la ventana. Le seteo id, Rect, Title y seteo 
        //a DrawNodeWindow como la función para dibujar las cosas internas

        for (int i = 0; i < _nodes.Count; i++)
        {
            GUI.backgroundColor = _nodes[i].GetBackgroundColor();
            _nodes[i].windowRect = GUI.Window(i, _nodes[i].windowRect, DrawNodeWindow, _nodes[i].windowTitle);         
        }

        EndWindows();
    }

    //Esta función dibuja el contenido interno del nodo
    void DrawNodeWindow(int id)
    {
        /* Cada tipo de nodo puede tener su preferencia de mostrar dentro del mismo
         * así que le pido al nodo que se encargue de dibujar y mostrar su contenido */
        _nodes[id].DrawNode();

        //Esta función hace que el nodo se pueda mover con el mouse
        GUI.DragWindow();
    }

    //Registra el input del mouse del user
    void UserInput(Event e)
    {
        //Si el evento fue de tipo "MouseDrag"
        if (e.type == EventType.MouseDrag)
        {
            //Por cada nodo mostrado en ventana
            for (int i = 0; i < _nodes.Count; i++)
            {
                if (e.button == 2)
                {
                    Panning(e);
                }
            }
        }

        //Si el evento fue de tipo "MouseDown (click)
        if (e.type == EventType.MouseDown)
        {
            var clickedOnNode = false;
            _focusedNode = false;

            //Por cada nodo mostrado en ventana
            for (int i = 0; i < _nodes.Count; i++)
            {               

                if (e.button == 2)
                {
                    _scrollStartPos = e.mousePosition;
                }
                
                //Si el mouse se encontraba en el rectangulo del nodo
                if (_nodes[i].windowRect.Contains(e.mousePosition))
                {
                    //Hizo click en un nodo!
                    clickedOnNode = true;
                    _focusedNode = true;

                    //Si hizo click izquierdo
                    if (e.button == 0)
                    {
                        //Logeo a ese nodo como el ultimo en el que se hizo click izquierdo
                        _lastLeftClickedNode = _nodes[i];                       
                    }
                    //Si hizo click derecho
                    else if (e.button == 1)
                    {
                        //Logeo a ese nodo como el ultimo en el que se hizo click derecho
                        _lastRightClickedNode = _nodes[i];
                    }
                    break;
                }
            }

            //Si hizo click derecho llamo a la función RightClick
            if(e.button == 1)
            {
                RightClick(e, clickedOnNode);
            }
        }
    }

    //FUNCION PARA QUE PANEÉ
    void Panning(Event e)
    {
        Vector2 diff = e.mousePosition - _scrollStartPos;
        diff *= 1; //"Sensibilidad del paneo"
        _scrollStartPos = e.mousePosition;
        _scrollPos += diff;

        for (int i = 0; i < _nodes.Count; i++) //Redibuja los nodos cuando se mueve el mouse
        {
            BaseNode b = _nodes[i];
            b.windowRect.x += diff.x;
            b.windowRect.y += diff.y;
        }
    }

    //FUNCIÓN PARA RESETEAR EL SCROLL
    void ResetScroll()
    {
        for (int i = 0; i < _nodes.Count; i++)
        {
            BaseNode b = _nodes[i];
            b.windowRect.x -= _scrollPos.x;
            b.windowRect.y -= _scrollPos.y;
        }
        _scrollPos = Vector2.zero;
    }


    //FUNCIÓN PARA PINTAR LOS NODOS
    void PaintNode()
    {
        for (int i = 0; i < _nodes.Count; i++)
        {
            BaseNode b = _nodes[i];
            if (_lastLeftClickedNode == b && _focusedNode == true)
            {
                b.color = Color.cyan;
            }
            else
            {
                b.color = b.defaultColor;
            }
        }
    }

    //Esta función se encarga de llamar a las funciones que hacen los menues contextuales
    void RightClick(Event e, bool clickedOnWindow)
    {
        if (clickedOnWindow)
        {
            //Si clickeó en un nodo, el menú contextual al que debe llamar es el de editar nodo
            ModifyNode(e);
        }
        else
        {
            AddNewNode(e);
        }
    }

    //Acá se genera el menu contextual si no se hizo click derecho en ningún nodo
    void AddNewNode(Event e)
    {
        GenericMenu menu = new GenericMenu();
        
        //AddItem pide una función y un parametro para pasarle si se hace click en el item
        menu.AddItem(new GUIContent("Add Product Node"), false, ContextMenuActions, UserActions.addProductNode);
		menu.AddItem(new GUIContent("Add Comparison"), false, ContextMenuActions, UserActions.addComparisonNode);

        menu.AddSeparator("");
        menu.AddItem(new GUIContent("Reset Scroll"), false, ContextMenuActions, UserActions.resetScroll);

        menu.ShowAsContext();
        e.Use();
    }

    //Acá se generan los menues contextuales según el nodo en el cual se hizo click derecho
    void ModifyNode(Event e)
    {
        GenericMenu menu = new GenericMenu();
		if(_lastRightClickedNode is ProductNode)
        {
		    menu.AddItem(new GUIContent("Add Connection"), false, ContextMenuActions, UserActions.addConnection);
			menu.AddItem(new GUIContent("Delete"), false, ContextMenuActions, UserActions.deleteNode);
        }
        else if (_lastRightClickedNode is StartNode)
        {
            menu.AddItem(new GUIContent("Add Connection"), false, ContextMenuActions, UserActions.addConnection);
        }
        else if (_lastRightClickedNode is ComparativeNode)
		{
			menu.AddItem(new GUIContent("Add Connection for True"), false, ContextMenuActions, UserActions.addConnection);
			menu.AddItem(new GUIContent("Add Connection for False"), false, ContextMenuActions, UserActions.addConnectionAsFalse);
			menu.AddItem(new GUIContent("Delete"), false, ContextMenuActions, UserActions.deleteNode);
		}

        menu.ShowAsContext();
        e.Use();
    }
    #endregion

    #region CREADO Y BORRADO DE NODOS / CONEXIONES ENTRE NODOS

    //Esta función es llamada por los items de los menues contextuales
    void ContextMenuActions(object o)
    {
        //Como GenericMenu.AddItem() pide una función que devuelva void y reciba un object, 
        //hay que upcastear de object al tipo de variable u objeto que estas queriendo usar.
        UserActions a = (UserActions)o;

        switch (a)
        {
            case UserActions.addProductNode:
                AddNode<ProductNode>(new Rect(_mousePosition.x, _mousePosition.y, 300, 250), GetNewId());
                break;
            case UserActions.addConnection:
                BeginInteractiveConnectionMode();
                break;
            case UserActions.deleteNode:
                DeleteNode();
                break;
			case UserActions.addComparisonNode:
				AddNode<ComparativeNode>(new Rect(_mousePosition.x, _mousePosition.y, 180, 100), GetNewId());
				break;
			case UserActions.addConnectionAsFalse:
                BeginInteractiveConnectionMode(false);
				break;

            case UserActions.resetScroll:
                ResetScroll();
                break;
        }

        NotifyChangesWereMade();
    }

    //Borra el nodo seleccionado
    public void DeleteNode()
    {
        //Agarro el ultimo nodo en el que hice click derecho
        var target = _lastRightClickedNode;

        //Borro todas las referencias del nodo en sus nodos hijo
        RemoveParentReferencesInChildNodes(target);

        //Borro el id de la lista de id's
        _idList.Remove(target.id);

        //Borro el nodo de la lista de nodos
        _nodes.Remove(target);
    }

    //Genera una conexión entre dos nodos preexistentes
    public void AddConnection()
    {
        if (_lastRightClickedNode == null || _lastLeftClickedNode == null) return;

		/* Ya que hay nodos que no pueden tener ciertos tipos de padre (el nodo respuesta no puede 
         * tener otro nodo respuesta como padre) chequeo que la conexión que se intente hacer sea válida */
		
		#region Checkeo legacy, para referencia
		//if ((_lastLeftClickedNode is StartNode && _lastRightClickedNode is ProductNode)
  //          || (_lastLeftClickedNode is ProductNode && _lastRightClickedNode is OptionNode)
  //          || (_lastLeftClickedNode is OptionNode && _lastRightClickedNode is ProductNode)
  //          || (_lastLeftClickedNode is EndNode && _lastRightClickedNode is OptionNode)
		//	|| (_lastLeftClickedNode is ComparativeNode && _lastRightClickedNode is OptionNode))

		#endregion
		if(_lastLeftClickedNode.CanTransitionTo(_lastRightClickedNode))
		{
			/* Si la conexión es válida seteo al ultimo nodo en el cual se hizo click 
             * izquierdo como el padre del ultimo nodo en el que se hizo click derecho */
			_lastRightClickedNode.SetParent(_lastLeftClickedNode);
			if(_lastLeftClickedNode is INeedsChildren)
			{
				((INeedsChildren)_lastLeftClickedNode).AssignChild(_lastRightClickedNode,0);
			}
        }   
    }

    public void AddConnection(BaseNode origin, BaseNode target)
    {
        if (target == null || origin == null) return;

        /* Ya que hay nodos que no pueden tener ciertos tipos de padre (el nodo respuesta no puede 
         * tener otro nodo respuesta como padre) chequeo que la conexión que se intente hacer sea válida */

        #region Checkeo legacy, para referencia
        //if ((origin is StartNode && target is ProductNode)
        //          || (origin is ProductNode && target is OptionNode)
        //          || (origin is OptionNode && target is ProductNode)
        //          || (origin is EndNode && target is OptionNode)
        //	|| (origin is ComparativeNode && target is OptionNode))

        #endregion
        if (origin.CanTransitionTo(target))
        {
            /* Si la conexión es válida seteo al ultimo nodo en el cual se hizo click 
             * izquierdo como el padre del ultimo nodo en el que se hizo click derecho */
            target.SetParent(origin);
            if (origin is INeedsChildren)
            {
                ((INeedsChildren)origin).AssignChild(target, 0);
            }
        }
    }

    public void AddFalseConnection() {
		if (_lastRightClickedNode == null || _lastLeftClickedNode == null) return;

		if (_lastLeftClickedNode.CanTransitionTo(_lastRightClickedNode))
		{
			/* Si la conexión es válida seteo al ultimo nodo en el cual se hizo click 
             * izquierdo como el padre del ultimo nodo en el que se hizo click derecho */
			_lastRightClickedNode.SetParent(_lastLeftClickedNode);
			if (_lastLeftClickedNode is INeedsChildren)
			{
				((INeedsChildren)_lastLeftClickedNode).AssignChild(_lastRightClickedNode, 1);
			}
		}
	}

    public void AddFalseConnection(BaseNode origin, BaseNode target)
    {
        if (target == null || origin == null) return;

        if (origin.CanTransitionTo(target))
        {
            /* Si la conexión es válida seteo al ultimo nodo en el cual se hizo click 
             * izquierdo como el padre del ultimo nodo en el que se hizo click derecho */
            target.SetParent(origin);
            if (origin is INeedsChildren)
            {
                ((INeedsChildren)origin).AssignChild(target, 1);
            }
        }
    }

    public bool NodeExists(BaseNode node)
    {
        return _nodes.Contains(node);
    }

	//Crea el nodo genericamente
	public TNode AddNode<TNode>(Rect rect, int id, BaseNode parent = null) where TNode : BaseNode, new() {
		TNode node = new TNode();
			node.SetWindowRect(rect).SetWindowTitle(node.GetNodeType).SetId(id).SetReference(this);
		//No asignar padres innecesarios al Start o hijos al Comparison
		if (parent != null && !(node.GetType() == typeof(StartNode) || (parent.GetNodeType == "Comparison")))
			node.SetParent(parent);
		if (node.GetType() == typeof(StartNode)||(parent != null && parent.GetNodeType == "Comparison")) node.SetParent(null);
		_nodes.Add(node);
		return node;
	}

    //Metodo Helper que es llamado desde los nodos para crear la conexión entre ellos y sus padres
    public static void DrawNodeConnection(Rect start, Rect end, bool left, Color curveColor)
    {
        Vector3 lerpVector = Vector3.Lerp(start.center, end.center, 0.1f);
        lerpVector.z = -10f;
        Handles.color = curveColor;
        Handles.DrawLine(start.center, end.center);
        Handles.ArrowHandleCap(0, lerpVector, Quaternion.LookRotation((end.center - start.center).normalized, Vector3.forward), 100f, EventType.Repaint);
    }

    //Borra la referencia de un nodo en sus nodos hijos
    public void RemoveParentReferencesInChildNodes(BaseNode target)
    {
        //Busco a sus nodos hijo
        var childNodes = GetChildNodes(target);

        //Recorro a cada uno de ellos y les remuevo la referencia de su padre
        foreach (var node in childNodes)
        {
            node.parents.Remove(target);
        }
    }

    //Genera una lista con todos los nodos que contienen la referencia padre de otro nodo
    public List<BaseNode> GetChildNodes(BaseNode n)
    {
        List<BaseNode> childs = new List<BaseNode>();

        //Recorro cada nodo
        foreach (var node in _nodes)
        {
            //Recorro cada nodo padre en cada nodo
            foreach (var parent in node.parents)
            {
                //Si hay coincidencia entre el nodo padre y el nodo de referencia, lo agrego a la lista
                if (parent == n)
                {
                    childs.Add(node);
                }
            }
        }

        return childs;
    }

    /* Genera un ID al azar y se fija que no esté creado. Si está creado 
     * genera otro, si no lo está lo guarda en la lista y lo devuelve */
    public int GetNewId()
    {
        int randomNumber;
        do{ randomNumber = Random.Range(0, 10000); } while (_idList.Contains(randomNumber));
        _idList.Add(randomNumber);
        return randomNumber;
    }

    public void NotifyChangesWereMade()
    {
        changesMade = true;
    }
    #endregion
}
