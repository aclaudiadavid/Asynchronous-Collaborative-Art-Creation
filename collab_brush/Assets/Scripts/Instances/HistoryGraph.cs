using System;
using System.Collections.Generic;
using UnityEngine;

public class HistoryGraph
{
	public int number_nodes;
	public List<int> roots;
	public Dictionary<int, Node> nodes;

	public HistoryGraph()
	{
		this.number_nodes = 0;
		this.roots = new List<int>();
		this.nodes = new Dictionary<int, Node>();
	}

	public int getNewID()
	{
		return this.nodes.Count;
	}


	public Node getNode(int id)
	{
		return nodes[id];
	}

	public int getNumNodes() {
		return number_nodes;
	}

	// Call this to obtain a list of all root nodes of the graph, so that you can build a tree from them
	public List<Node> getRoots()
	{
		List<Node> roots = new List<Node>();
		foreach (int id in this.roots)
		{
			roots.Add(nodes[id]);
		}
		return roots;
	}

	public bool isRoot(int id)
	{
		return this.roots.Contains(id);
	}

	public Node createNewNode(int parentID)
	{
		int newID = getNewID();
		this.nodes.Add(newID, new Node(newID));
		this.nodes[parentID].addChild(newID);
		this.number_nodes++;

		return getNode(newID);
	}

	public Node createNewRoot()
	{
		int newID = getNewID();
		this.nodes.Add(newID, new Node(newID));
		roots.Add(newID);
		this.number_nodes++;
	
		return getNode(newID);
	}

	public int getMaxDepth(int id, int depth)
	{
		int maxDepth = depth;
		foreach (int childID in this.nodes[id].children)
		{
			int childDepth = getMaxDepth(childID, depth + 1);
			if (childDepth > maxDepth)
			{
				maxDepth = childDepth;
			}
		}
		return maxDepth;
	}

	public int getMaxDepth()
	{
		int maxDepth = 0;
		foreach (int rootID in this.roots)
		{
			int rootDepth = getMaxDepth(rootID, 0);
			if (rootDepth > maxDepth)
			{
				maxDepth = rootDepth;
			}
		}
		return maxDepth;
	}

	public float getRequiredLength() {
		int depth = getMaxDepth();
		return depth * 3f;
	}

	public float getRequiredLength(int id) {
		int depth = getMaxDepth(id, 0);
		return depth * 3f;
	}
}