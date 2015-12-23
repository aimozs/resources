using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class User : MonoBehaviour {

	public int id;
	public string firstname;
	public string lastname;
	public string email;
	public string dob;
	public string password;

	public User(string newEmail, string newPassword){
		email = newEmail;
		password = newPassword;
	}

	public void SetupUser(int newId, string newEmail, string newFirstname){
		id = newId;
		email = newEmail;
		firstname = newFirstname;
	}

	public void CreateNewUser(string data){
		Debug.Log(data.ToString());
		object user = JsonUtility.FromJson<object>(data.ToString());
		Debug.Log(user);
	}

}
