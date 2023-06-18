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
}