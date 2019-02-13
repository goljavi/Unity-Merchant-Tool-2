using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface INeedsChildren{
	void AssignChild(BaseNode child, int childPosition = -1);
	bool HasChildren();
}

