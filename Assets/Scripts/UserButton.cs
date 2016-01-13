using UnityEngine;
using System.Collections;

public class UserButton : MonoBehaviour {

	private User user;

	void Start(){
		user = GetComponent<User>();
	}

	public void OnUserBtnClick(){
		Debug.Log("user btn " + user.firstname);
		LoginManager.Instance.LoginExistingUser(user);
	}
}
