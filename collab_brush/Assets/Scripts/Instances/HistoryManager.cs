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
	[SerializeField] private Texture2D m_LoadingImageTexture;
	[SerializeField] private Texture2D m_UnknownImageTexture;
	private const string DATA_PATH = "SaveHistory/graph.json";
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

	public void saveHistoryGraph()
	{
		string path = Path.Combine(Application.dataPath, DATA_PATH);
		string jsonString = JsonConvert.SerializeObject(historyGraph);
		File.WriteAllText(path, jsonString);
	}

	public void loadHistoryGraph()
	{
		string path = Path.Combine(Application.dataPath, DATA_PATH);
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

	public void drawGraph() {
		List<Node> roots = historyGraph.getRoots();
		int i;
		Vector3 pos = GameObject.Find("Camera (eye)").transform.position;
		Quaternion rot = GameObject.Find("Camera (eye)").transform.rotation;
		GameObject cameraInstantPosition = new GameObject("cameraInstantPosition");
		cameraInstantPosition.transform.position = pos;
		cameraInstantPosition.transform.rotation = rot;
		Vector3 front = GameObject.Find("Camera (eye)").transform.forward;

		Vector3 finalPos = cameraInstantPosition.transform.position + front * 3 - GameObject.Find("Camera (eye)").transform.right * 3;
		for (i = 0; i < roots.Count; i++) {
			finalPos += front * 3;
			GameObject root = Instantiate(nodeprf, finalPos, cameraInstantPosition.transform.rotation);
			root.GetComponentInChildren<TextMeshPro>().SetText("Instance " + roots[i].getId() + "\n"+ File.GetCreationTime(@"C:\Users\ursin\Documents\Open Brush\Sketches\Untitled_" + roots[i].getId() + ".tilt"));
			blocks.Add(root);
			nodeBlocks.Add(root.GetComponentInChildren<LoadSketchButton>());
			setSketchID(root, roots[i].getId());
			createSons(roots[i], finalPos);
		}
		InitializeSketchSet();
		createLine();
		awake = true;
	}

	public void createSons(Node node, Vector3 parentPosition) {
		Vector3 sonPosition;
		Vector3 edgePosition;
		Vector3 pos = GameObject.Find("Camera (eye)").transform.position;
		Vector3 front = GameObject.Find("Camera (eye)").transform.forward;
		Vector3 right = GameObject.Find("Camera (eye)").transform.right;
		Quaternion rot = GameObject.Find("Camera (eye)").transform.rotation;
		int i;
		for (i = 0; i < node.children.Count; i++) {
			if (node.children.Count == 1) {
				edgePosition = parentPosition + right * 1.7f;

				sonPosition = edgePosition + right * 1.7f;
			}
			else {
				if (i % 2 == 0) {
					if (i > 0) {
						edgePosition = parentPosition + right * 1.7f + new Vector3(0, 2.5f * (i/2), 0);
					}
					else {
						edgePosition = parentPosition + right * 1.7f + new Vector3(0, 0.5f, 0);
					}
					sonPosition = edgePosition + right * 1.7f + new Vector3(0, 1, 0);			
				}
				else {
					if (i > 1) {
						edgePosition = parentPosition + right * 1.7f + new Vector3(0, -0.5f * -((i/2)-2.5f), 0);
					}
					else {
						edgePosition = parentPosition + right * 1.7f + new Vector3(0, -0.5f, 0);
					}
					sonPosition = edgePosition + right * 1.7f + new Vector3(0, -1, -0.1f);
				}											
			}
			GameObject save = Instantiate(nodeprf, sonPosition, rot);
			save.GetComponentInChildren<TextMeshPro>().SetText("Instance " + node.children[i] + "\n"+ File.GetCreationTime(@"C:\Users\ursin\Documents\Open Brush\Sketches\Untitled_" + node.children[i] + ".tilt"));
			blocks.Add(save);
			nodeBlocks.Add(save.GetComponentInChildren<LoadSketchButton>());
			setSketchID(save, node.children[i]);

			GameObject edge = Instantiate(edgeprf, parentPosition, Quaternion.identity);
			edge.GetComponentInChildren<LineRenderer>().SetPosition(1, sonPosition-parentPosition);
			blocks.Add(edge);	
			createSons(historyGraph.getNode(node.children[i]), sonPosition);
		} 
	}

	public void destroyGraph() {
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
