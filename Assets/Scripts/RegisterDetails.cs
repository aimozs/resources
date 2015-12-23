using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RegisterDetails : MonoBehaviour {

	public string sex;
	public string dob;
	public string firstname;
	public string lastname;
	public string school;
	public string address;
	public string suburb;
	public string state;
	public string postcode;
	public string phone;
	public string email;
	public string password;
	
	private static RegisterDetails instance;
	public static RegisterDetails Instance {
		get {
			if (instance == null) {
				instance = GameObject.FindObjectOfType<RegisterDetails>();
				DontDestroyOnLoad(instance);
			}
			return instance;
		}
	}
	
	public RegisterDetails(string newSex, string newDob, string newFirstname, string newLastname, string newSchool, string newAddress, string newSuburb, string newState, string newPostcode, string newPhone, string newEmail, string newPassword){
		firstname = newFirstname;
		email = newEmail;
		password = newPassword;
	}

}
