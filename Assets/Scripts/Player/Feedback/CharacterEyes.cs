using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class CharacterEyes : MonoBehaviour
{
	public static readonly List<string> ShapekeyList = new List<string>
    {	"Eyes_Annoyed",
    	"Eyes_Blink",
    	"Eyes_Cry",
    	"Eyes_Dead",
    	"Eyes_Excited",
    	"Eyes_Happy",
    	"Eyes_LookDown",
    	"Eyes_LookIn",
    	"Eyes_LookOut",
    	"Eyes_LookUp",
    	"Eyes_Rabid",
    	"Eyes_Sad",
    	"Eyes_Shrink",
    	"Eyes_Sleep",
    	"Eyes_Spin",
    	"Eyes_Squint",
    	"Eyes_Trauma",
    	"Sweat_L",
    	"Sweat_R",
    	"Teardrop_L",
    	"Teardrop_R"
    };

    // Display as a dropdown on editor to select an option from ShapekeyList
    [ShapekeyDropdown]
    [SerializeField] private string currentEye;
    
	[SerializeField] private bool changeOnUpdate;

	private Animator animator;
	void Start()
	{
		animator = GetComponent<Animator>();
		SetEye(currentEye);
	}

	public void SetEye(string eyeType)
	{
		currentEye = eyeType;
		animator.Play(currentEye);
	}

	private void Update()
	{
		if (changeOnUpdate)
			animator.Play(currentEye);
	}
}

#if UNITY_EDITOR

public class ShapekeyDropdownAttribute : PropertyAttribute { }

[CustomPropertyDrawer(typeof(ShapekeyDropdownAttribute))]
public class ShapekeyDropdownDrawer : PropertyDrawer
{
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		var list = CharacterEyes.ShapekeyList;

		int index = Mathf.Max(0, list.IndexOf(property.stringValue));
		index = EditorGUI.Popup(position, label.text, index, list.ToArray());

		property.stringValue = list[index];
	}
}
#endif
