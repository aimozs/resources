using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SchoolButton : MonoBehaviour {

	private string _schoolName;
	private string _schoolCode;

	public void SetSchool(string schoolName, string schoolCode){
		_schoolName = schoolName;
		_schoolCode = schoolCode;
		GetComponentInChildren<Text>().text = _schoolName;
	}

	public void OnSchoolBtnClick(){
		Debug.Log("school btn " + _schoolName);
		LoginManager.Instance.SetSelectedSchool(_schoolName, _schoolCode);
	}
}
