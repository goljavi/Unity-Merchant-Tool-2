using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISetableFloat {
	void SetFloat(int id, float value);
}

public interface ISetableInt {
	void SetInt(int id, int value);
}

public interface ISetableBool {
	void SetBool(int id, bool value);
}

public interface ISetableValues : ISetableFloat, ISetableInt, ISetableBool { }
