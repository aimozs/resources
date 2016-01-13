using UnityEngine;
using System.Collections;

public class SchoolButton : MonoBehaviour {

	private string _schoolName;
	private string _schoolCode;

	public void SetSchool(string schoolName, string schoolCode){
		_schoolName = schoolName;
		_schoolCode = schoolCode;
	}

	public void OnSchoolBtnClick(){
		Debug.Log("school btn " + _schoolName);
		LoginManager.Instance.SetSelectedSchool(_schoolName, _schoolCode);
	}
}
