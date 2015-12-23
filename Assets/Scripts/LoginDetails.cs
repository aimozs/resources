using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LoginDetails : MonoBehaviour {

	public string email;
	public string password;
	
	private static LoginDetails _me;
	public static LoginDetails Instance {
		get {
			if (_me == null) {
				_me = GameObject.FindObjectOfType<LoginDetails>();
				DontDestroyOnLoad(_me);
			}
			return _me;
		}
	}
	
	public LoginDetails(string newEmail, string newPassword){
		email = newEmail;
		password = newPassword;
	}

}
