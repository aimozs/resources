using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RegisterDetails : MonoBehaviour {

	public string male;
	public string dob;
	public string firstname;
	public string lastname;
	public string school;
	public string schoolcode;
	public string teacher;
	public string guardian_firstname;
	public string guardian_lastname;
	public string guardian_dob;
	public string guardian_title;
	public string relationship;
	public string address;
	public string address_suburb;
	public string address_state;
	public string address_postcode;
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
	
	public RegisterDetails(
		string newMale,
		string newDob, 
		string newFirstname, 
		string newLastname, 
		string newSchool, 
		string newAddress, 
		string newSuburb, 
		string newState, 
		string newPostcode, 
		string newPhone, 
		string newEmail, 
		string newPassword){
			male = newMale;
			dob = newDob;
			firstname = newFirstname;
			lastname = newLastname;
			school = newSchool;
			address = newAddress;
			address_suburb = newSuburb;
			address_state = newState;
			address_postcode = newPostcode;
			phone = newPhone;
			email = newEmail;
			password = newPassword;
			schoolcode = "1";
			teacher = "t";
			guardian_firstname = "gfn";
			guardian_lastname = "gln";
			guardian_dob = "1982-11-22";
			guardian_title = "mr";
			relationship = "parent";
		}
}
