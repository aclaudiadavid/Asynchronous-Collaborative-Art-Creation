using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using TiltBrush;
using TMPro;

public class HistoryManager : MonoBehaviour
{
	public const float SIDE_MAX_LENGTH = 18f;
	public const float DIST_BETWEEN_NODES = 3f;
	public const float PADDING = DIST_BETWEEN_NODES / 2;

	[SerializeField] private Texture2D m_LoadingImageTexture;
	[SerializeField] private Texture2D m_UnknownImageTexture;
	//private const string DATA_PATH = "SaveHistory/graph.json";
	private const string DATA_PATH = "SaveHistory";
	private List<GameObject> blocks;
	private List<LoadSketchButton> nodeBlocks;
	private HistoryGraph historyGraph;
	private Node currentlyOpenNode;
	private GameObject nodeprf;
	private GameObject edgeprf;
	private GameObject lineprf;
	private SketchSet m_SketchSet;
	private bool m_AllIconTexturesAssigned;
	private bool m_AllSketchesAreAvailable;
	private float m_ImageAspect;
	private UnityEngine.XR.InputDevice device;
	private GameObject rightController;

	public bool awake = false;
	public bool isDrawing = false;
	public string room = "0";

	void Awake()
	{
		loadHistoryGraph();
		this.blocks = new List<GameObject>();
		this.nodeBlocks = new List<LoadSketchButton>();
		nodeprf = Resources.Load<GameObject>("InstancePlace");
		edgeprf = Resources.Load<GameObject>("InstanceConnector");
		lineprf = Resources.Load<GameObject>("LineHistory");
	}

	void Update() {
		m_SketchSet = SketchCatalog.m_Instance.GetSet(SketchSetType.User);
		if (awake && m_SketchSet.IsReadyForAccess &&
			(!m_SketchSet.RequestedIconsAreLoaded ||
			!m_AllIconTexturesAssigned || !m_AllSketchesAreAvailable))
		{
			UpdateIcons();
		}
	}

	public void setOpenNode(int id) {
		currentlyOpenNode = historyGraph.getNode(id);
	}

	public bool isLeaf() {
		if (currentlyOpenNode == null) {
			return true;
		}
		else if (!currentlyOpenNode.children.Any()) {
			return true;
		}
		else {
			return false;
		}
	}

	public void createNewSave()
	{
		if (currentlyOpenNode == null) {
			Debug.Log("No node is currently open, creating new root node");
			currentlyOpenNode = historyGraph.createNewRoot();
		}
		else {
			currentlyOpenNode = historyGraph.createNewNode(currentlyOpenNode.id);
		}
		saveHistoryGraph();
	}

	public void updateRoomName(string roomCode) {
		room = roomCode;
		loadHistoryGraph();
		currentlyOpenNode = null;
	}

	public void saveHistoryGraph()
	{
		string path = Path.Combine(Application.dataPath, DATA_PATH, room +".json");
		string jsonString = JsonConvert.SerializeObject(historyGraph);
		File.WriteAllText(path, jsonString);
	}

	public void loadHistoryGraph()
	{
		string path = Path.Combine(Application.dataPath, DATA_PATH, room +".json");
		if (File.Exists(path))
		{
			string jsonString = File.ReadAllText(path);
			historyGraph = JsonConvert.DeserializeObject<HistoryGraph>(jsonString);
		}
		else
		{
			historyGraph = new HistoryGraph();
		}
	}

	public Vector3 getClosestAxis(Vector3 front) {
		// Find the best forward vector (-y, +y, -x, +x)
		Vector3 bestAxis = Vector3.zero;
		float maxDot = 0;
		foreach (Vector3 axis in new Vector3[] { Vector3.left, Vector3.back, Vector3.forward, Vector3.right }) {
			float dot = Vector3.Dot(axis, front);
			if (dot > maxDot) {
				maxDot = dot;
				bestAxis = axis;
			}
		}
		return bestAxis;
	}

	public Vector3 getFirstNodePosition(Vector3 cameraPos, Vector3 cameraFront, Vector3 cameraRight) {
		Vector3 rootPosition;
		float reqLength = historyGraph.getRequiredLength();

		if (reqLength > 2 * SIDE_MAX_LENGTH) {
			// If we need to use at least 2 sides, use all the space in the room
			rootPosition = getClosestAxis(cameraFront) * (SIDE_MAX_LENGTH / 2 + PADDING) - getClosestAxis(cameraRight) * SIDE_MAX_LENGTH / 2;
			rootPosition.y = cameraPos.y;
		}
		else if (reqLength > SIDE_MAX_LENGTH) {
			// If we need to use just one more side, place the graph relative to the room but as close to the center as possible
			rootPosition = getClosestAxis(cameraFront) * ((reqLength - SIDE_MAX_LENGTH) / 2 + PADDING) - getClosestAxis(cameraRight) * SIDE_MAX_LENGTH / 2;
			rootPosition.y = cameraPos.y;
		}
		else {
			// If space is not a problem, put the root in front of the camera
			rootPosition = cameraPos + cameraFront * 3 - cameraRight * reqLength / 2;
		}
		return rootPosition;
	}

	public Vector3 getEffectiveForward(Vector3 cameraFront) {
		Vector3 effectiveForward;
		float reqLength = historyGraph.getRequiredLength();

		if (reqLength > SIDE_MAX_LENGTH) {
			effectiveForward = getClosestAxis(cameraFront);
		}
		else {
			effectiveForward = cameraFront;
		}
		return effectiveForward;
	}

	public Vector3 getEffectiveRight(Vector3 cameraRight) {
		Vector3 effectiveRight;
		float reqLength = historyGraph.getRequiredLength();

		if (reqLength > SIDE_MAX_LENGTH) {
			effectiveRight = getClosestAxis(cameraRight);
		}
		else {
			effectiveRight = cameraRight;
		}
		return effectiveRight;
	}

	public Quaternion getEffectiveRotation(Quaternion cameraRot, Vector3 effectiveForward) {
		Quaternion effectiveRotation;
		float reqLength = historyGraph.getRequiredLength();

		if (reqLength > SIDE_MAX_LENGTH) {
			if (effectiveForward == Vector3.back) {
				effectiveRotation = Quaternion.FromToRotation(Vector3.forward, Vector3.right);
				effectiveRotation = effectiveRotation * effectiveRotation;
			}
			else effectiveRotation = Quaternion.FromToRotation(Vector3.forward, effectiveForward);
		}
		else {
			effectiveRotation = cameraRot;
		}
		return effectiveRotation;
	}

	public void drawGraph() {
		if (awake || isDrawing) {
			return;
		}
		isDrawing = true;
		List<Node> roots = historyGraph.getRoots();
		int i;
		Vector3 pos = GameObject.Find("Camera (eye)").transform.position;
		Quaternion rot = GameObject.Find("Camera (eye)").transform.rotation;
		GameObject cameraInstantPosition = new GameObject("cameraInstantPosition");
		cameraInstantPosition.transform.position = pos;
		cameraInstantPosition.transform.rotation = rot;
		Vector3 front = GameObject.Find("Camera (eye)").transform.forward;
		Vector3 right = GameObject.Find("Camera (eye)").transform.right;


		Vector3 rootPosition = getFirstNodePosition(pos, front, right);
		Vector3 effectiveForward = getEffectiveForward(front);
		Quaternion effectiveRotation = getEffectiveRotation(rot, effectiveForward);
		Vector3 effectiveRight = getEffectiveRight(right);

		//TODO Calcular "finalPos"
		for (i = 0; i < roots.Count; i++) {
			createSons(roots[i], rootPosition, effectiveRotation, effectiveForward, effectiveRight, 0);
			rootPosition += front * 3;
		}
		InitializeSketchSet();
		createLine();
		awake = true;
		isDrawing = false;
	}

	public void createSons(Node node, Vector3 position, Quaternion rot, Vector3 forward, Vector3 right, int depth) {
		Vector3 sonPosition;
		int i;

		GameObject save = Instantiate(nodeprf, position, rot);
		save.GetComponentInChildren<TextMeshPro>().SetText("Instance " + node.getId() + "\n"+ File.GetLastWriteTime(@"C:\Users\ursin\Documents\Open Brush\Sketches\Untitled_" + node.getId() + ".tilt"));
		blocks.Add(save);
		nodeBlocks.Add(save.GetComponentInChildren<LoadSketchButton>());
		setSketchID(save, node.getId());
		
		if (depth % (SIDE_MAX_LENGTH/3) == (SIDE_MAX_LENGTH/3)-1) {
			// Son is going to be on the next side so we want to rotate it
			rot = Quaternion.FromToRotation(Vector3.forward, Vector3.right) * rot;
		}

		for (i = 0; i < node.children.Count; i++) {
			if (node.children.Count == 1) {
				sonPosition = position;
			}
			else {
				if (i % 2 == 0) {
					if (i > 0) sonPosition = position + new Vector3(0, 2.5f * (i/2), 0) + new Vector3(0, 1, 0);	
					else sonPosition = position + new Vector3(0, 0.5f, 0) + new Vector3(0, 1, 0);	
				}
				else {
					if (i > 1) sonPosition = position + new Vector3(0, -0.5f * -((i/2)-2.5f), 0) + new Vector3(0, -1, 0);
					else sonPosition = position + new Vector3(0, -0.5f, 0) + new Vector3(0, -1, 0);
				}
			}
			// Create edge to child before creating child
			GameObject edge = Instantiate(edgeprf, position, Quaternion.identity);
			blocks.Add(edge);
			LineRenderer line = edge.GetComponentInChildren<LineRenderer>();
			if (depth % (SIDE_MAX_LENGTH/3) == (SIDE_MAX_LENGTH/3)-1) {
				// Son is going to be on the next side so we want to add an extra point to the edge
				sonPosition += right * PADDING + -forward * PADDING;
				line.positionCount = 3;
				line.SetPosition(1, right * PADDING);
				line.SetPosition(2, sonPosition-position);
				createSons(historyGraph.getNode(node.children[i]), sonPosition, rot, right, -forward, depth+1);
			}
			else {
				sonPosition += right * DIST_BETWEEN_NODES;
				line.SetPosition(1, sonPosition-position);
				createSons(historyGraph.getNode(node.children[i]), sonPosition, rot, forward, right, depth+1);
			}
		} 
	}

	public void destroyGraph() {
		if (!awake || isDrawing) {
			return;
		}
		awake = false;
		foreach (GameObject i in blocks){
			Destroy(i);
		}
		blocks.Clear();
		nodeBlocks.Clear();
	}

	public void setSketchID(GameObject obj, int id) {
		var button = obj.GetComponentInChildren<LoadSketchButton>();
		button.SketchIndex = id;
		button.SketchSet = SketchCatalog.m_Instance.GetSet(SketchSetType.User);
	}

	List<int> GetIconLoadIndices()
	{
		var ret = new List<int>(); 
		for (int idx = 0; idx < historyGraph.number_nodes; idx++)
		{
			ret.Add(idx);
		}
		return ret;
	}

	private void UpdateIcons()
	{
		m_AllIconTexturesAssigned = true;
		m_AllSketchesAreAvailable = true;
		// Poll sketch catalog until icons have loaded
		foreach (LoadSketchButton icon in nodeBlocks)
		{
			if (icon == null) { continue; }
			int iSketchIndex = icon.SketchIndex;
			if (m_SketchSet.IsSketchIndexValid(iSketchIndex))
			{
				icon.FadeIn = m_SketchSet.GetSketchSceneFileInfo(iSketchIndex).Available ? 1f : 0.5f;

				if (!icon.ThumbnailLoaded)
				{
					Texture2D rTexture = null;
					string[] authors;
					string description;
					if (m_SketchSet.GetSketchIcon(iSketchIndex, out rTexture, out authors, out description))
					{
						if (rTexture != null)
						{
							// Pass through aspect ratio of image so we don't get squished
							// thumbnails from Poly
							m_ImageAspect = (float)rTexture.width / rTexture.height;
							float aspect = m_ImageAspect;
							icon.SetButtonTexture(rTexture, aspect);
						}
						else
						{
							icon.SetButtonTexture(m_UnknownImageTexture);
						}

						// Mark the texture as assigned regardless of actual bits being valid
						icon.ThumbnailLoaded = true;
						List<string> lines = new List<string>();
						lines.Add(icon.Description);

						SceneFileInfo info = m_SketchSet.GetSketchSceneFileInfo(iSketchIndex);
						if (info is PolySceneFileInfo polyInfo &&
							polyInfo.License != VrAssetService.kCreativeCommonsLicense)
						{
							lines.Add(String.Format("© {0}", authors[0]));
							lines.Add("All Rights Reserved");
						}
						else
						{
							// Include primary author in description if available
							if (authors != null && authors.Length > 0)
							{
								lines.Add(authors[0]);
							}
							// Include an actual description
							if (description != null)
							{
								lines.Add(App.ShortenForDescriptionText(description));
							}
						}
						icon.SetDescriptionText(lines.ToArray());
					}
					else
					{
						// While metadata has not finished loading, check if this file is valid
						bool bFileValid = false;
						SceneFileInfo rInfo = m_SketchSet.GetSketchSceneFileInfo(iSketchIndex);
						if (rInfo != null)
						{
							bFileValid = rInfo.Exists;
						}

						// If this file isn't valid, just keep the defaults and move on
						if (!bFileValid)
						{
							icon.SetButtonTexture(m_UnknownImageTexture);
							icon.ThumbnailLoaded = true;
						}
						else
						{
							m_AllIconTexturesAssigned = false;
						}
						if (!rInfo.Available)
						{
							m_AllSketchesAreAvailable = false;
						}
					}
				}
			}
		}
	}

	public void InitializeSketchSet() {
		foreach (LoadSketchButton icon in nodeBlocks)
		{
			icon.SketchSet = m_SketchSet;
		}

		m_SketchSet.RequestOnlyLoadedMetadata(GetIconLoadIndices());
		m_AllIconTexturesAssigned = false;
		m_AllSketchesAreAvailable = false;
		for (int idx = 0; idx < historyGraph.number_nodes; idx++)
		{
			LoadSketchButton icon = nodeBlocks[idx] as LoadSketchButton;
			// Default to loading image
			icon.SetButtonTexture(m_LoadingImageTexture);
			icon.ThumbnailLoaded = false;

			// Init icon according to availability of sketch
			GameObject go = icon.gameObject;
			if (m_SketchSet.IsSketchIndexValid(icon.SketchIndex))
			{
				string sSketchName = m_SketchSet.GetSketchName(icon.SketchIndex);
				FileInfo fi = new FileInfo(@"C:\Users\ursin\Documents\Open Brush\Sketches\"+sSketchName+".tilt");
				var lastmodified = fi.LastWriteTime.ToString();
				icon.SetDescriptionText(sSketchName + "\n" + lastmodified);
				SceneFileInfo info = m_SketchSet.GetSketchSceneFileInfo(icon.SketchIndex);
				if (info.Available)
				{
					m_SketchSet.PrecacheSketchModels(icon.SketchIndex);
				}
				go.SetActive(true);
			}
			else
			{
				go.SetActive(false);
			}
		}
	}
	public void createLine() {
        rightController = GameObject.Find("Controller (brush)");

        GameObject line = Instantiate(lineprf, rightController.transform);
		line.layer = LayerMask.NameToLayer("Selection");
		line.transform.localPosition = new Vector3(0,0,0);
		blocks.Add(line);
	}
}
