using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CustomBuilderView : EditorWindow {

	string buildName = Application.productName;

	[MenuItem("JPT Application/Task 1/Custom Build Window")]
	static void OpenBuildWindow(){
		EditorWindow.GetWindow(typeof(CustomBuilderView));
	}

	void OnGUI(){
		
		buildName = EditorGUILayout.TextField("Build Name:", buildName);
	}
}
 