using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node
{
    public int id;
    public List<int> children;

    //public string name;

    public Node(int id) {
        this.id = id;
        this.children = new List<int>();
    }

    public int getId() {
        return id;
    }

    public void addChild(int id)
	{
		this.children.Add(id);
	}
}
