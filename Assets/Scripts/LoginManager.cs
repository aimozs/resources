using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using MiniJSON;
using System.Globalization;


//[RequireComponent (typeof (User))]
[RequireComponent (typeof (LoginDetails))]
[RequireComponent (typeof (RegisterDetails))]
public class LoginManager : MonoBehaviour {
	
	public enum urlEnum {registration, forgotPassword};
	
	public bool debugLogin;
	public bool live = false;
	
	public GameObject LoginUI;
	
	public GameObject LoginScreen;
	public GameObject Feedback;
	public GameObject RegisterScreen;
	public GameObject SwapScreenButton;
	
	public GameObject usernameGO;
	public GameObject passwordGO;
	public GameObject loginGO;

	public GameObject sexGO;
	public GameObject dobGO;
	public GameObject firstnameGO;
	public GameObject lastnameGO;
	public GameObject schoolGO;

	public GameObject addressGO;
	public GameObject suburbGO;
	public GameObject stateGO;
	public GameObject postcodeGO;

	public GameObject phoneGO;
	public GameObject emailGO;
	public GameObject newPasswordGO;
	public GameObject passwordConfirmationGO;
	public GameObject RegisterGO;

	public LoginDetails newLoginDetails;
	public RegisterDetails newRegisterDetails;
	public User newUser;

	public WWWForm form;
	
	private string stagingURL = "http://racstaging.2and2.com.au/";
	private string liveURL = "https://littlelegends.rac.com.au/";
	private string URLLogin = "api/login";
	private string APIRegistration = "api/users";
	private string URLRegistration = "registration";
	private string URLForgotPassword = "forgotpassword";
	
	private string username;
	private string password;
	private string jsonString;

	private static LoginManager instance;
	public static LoginManager Instance {
		get {
			if(instance == null) {
				instance = GameObject.FindObjectOfType(typeof(LoginManager)) as LoginManager;
			}
			return instance;
		}
	}
	
	void Start() {

		newLoginDetails = GetComponent<LoginDetails>();
		newRegisterDetails = GetComponent<RegisterDetails>();

		
		if(live){
			URLLogin = liveURL + URLLogin;
			APIRegistration = liveURL + APIRegistration;
			URLRegistration = liveURL + URLRegistration;
			URLForgotPassword = liveURL + URLForgotPassword;
		} else {
			URLLogin = stagingURL + URLLogin;
			APIRegistration = stagingURL + APIRegistration;
			URLRegistration = stagingURL + "website/" + URLRegistration;
			URLForgotPassword = stagingURL + "website/" + URLForgotPassword;
		}
		
		DisplayScreen(LoginScreen, true);
		DisplayScreen(RegisterScreen, false);
		DisplayScreen(Feedback, false);
		
		if(Debug.isDebugBuild){
			Debug.Log ("login: " + URLLogin);
			Debug.Log ("api registration: " + APIRegistration);
			Debug.Log ("registration link: " + URLRegistration);
			Debug.Log ("password: " + URLForgotPassword);

		}
	}

	
	
	//GOTOS
	public void GoToRegistration() {
		Application.OpenURL(URLRegistration);
	}
	
	public void GoToForgotPassword() {
		Application.OpenURL(URLForgotPassword);
	}
	
	//On Clicks
	public void OnLoginBtnClick() {
		newLoginDetails.email = usernameGO.GetComponent<InputField>().text;
		newLoginDetails.password = passwordGO.GetComponent<InputField>().text;

		jsonString = JsonUtility.ToJson(newLoginDetails);

		if(debugLogin)
			Debug.Log("JSON that will be sent" + jsonString);

		StartCoroutine("Login");
	}

	public void OnRegisterBtnClick(){
		if(newPasswordGO.GetComponent<InputField>().text != passwordConfirmationGO.GetComponent<InputField>().text){
			DisplayScreen(Feedback, true);
			return;
		}

		PopulateRegistration();
		form = new WWWForm();

		if(newRegisterDetails.sex == "Male")
			form.AddField("male", "1");
		else
			form.AddField("female", "1");

		form.AddField("dob", newRegisterDetails.dob);
		form.AddField("firstname", newRegisterDetails.firstname);
		form.AddField("lastname", newRegisterDetails.lastname);
		form.AddField("school", newRegisterDetails.school);
		form.AddField("address", newRegisterDetails.address);
		form.AddField("address_suburb", newRegisterDetails.suburb);
		form.AddField("address_state", newRegisterDetails.state);
		form.AddField("address_postcode", newRegisterDetails.postcode);
		form.AddField("phone", newRegisterDetails.phone);
		form.AddField("email", newRegisterDetails.email);
		form.AddField("password", newRegisterDetails.password);

		form.AddField("guardian_dob", newRegisterDetails.dob);
		form.AddField("guardian_firstname", newRegisterDetails.firstname);
		form.AddField("guardian_lastname", newRegisterDetails.lastname);
		form.AddField("guardian_title", "Mr");
		form.AddField("schoolcode", "");
		form.AddField("teacher", "wow");

		jsonString = JsonUtility.ToJson(newRegisterDetails);
//		Debug.Log(jsonString);

		StartCoroutine(Register());
	}
	
	public void ResetFeedback(){
		passwordGO.GetComponent<InputField>().text = "";
		DisplayScreen(Feedback, false);
	}

	public void PopulateRegistration(){
		newRegisterDetails.sex = sexGO.GetComponent<ToggleGroup>().ActiveToggles().First().gameObject.name;
		newRegisterDetails.dob = dobGO.GetComponent<InputField>().text;
		newRegisterDetails.firstname = firstnameGO.GetComponent<InputField>().text;
		newRegisterDetails.lastname = lastnameGO.GetComponent<InputField>().text;
		newRegisterDetails.school = schoolGO.GetComponent<InputField>().text;

		newRegisterDetails.address = addressGO.GetComponent<InputField>().text;
		newRegisterDetails.suburb = suburbGO.GetComponent<InputField>().text;
		newRegisterDetails.state = stateGO.GetComponent<Dropdown>().captionText.text;
		newRegisterDetails.postcode = postcodeGO.GetComponent<InputField>().text;

		newRegisterDetails.phone = phoneGO.GetComponent<InputField>().text;
		newRegisterDetails.email = emailGO.GetComponent<InputField>().text;
		newRegisterDetails.password = newPasswordGO.GetComponent<InputField>().text;
	}
	
	//Feedback
	void DisplayScreen(GameObject screen, bool on) {
		if(!screen.activeInHierarchy)
			screen.SetActive(true);
			
		if(screen.GetComponent<CanvasGroup>() != null) {
			CanvasGroup screenCanvas = screen.GetComponent<CanvasGroup>();
			screenCanvas.alpha = on ? 1f : 0f;
			screenCanvas.interactable = screenCanvas.blocksRaycasts = on;
		} else {
			if(debugLogin)
				Debug.LogWarning("[LM] Couldnt find canvas group for that screen: " + screen.name);
		}
	}
	
	void RemoveAllScreen () {
		CanvasGroup[] screens = LoginUI.GetComponentsInChildren<CanvasGroup>();
		
		foreach(CanvasGroup screenCanvas in screens){
			screenCanvas.alpha = 0f;
			screenCanvas.interactable = screenCanvas.blocksRaycasts = false;
		}
	}
	
	public void SwapScreen(){
		if(debugLogin)
			Debug.Log("swapping screen");

		if(LoginScreen.GetComponent<CanvasGroup>().alpha == 0f) {
			RemoveAllScreen();
			DisplayScreen(LoginScreen, true);
			SwapScreenButton.GetComponentInChildren<Text>().text = "Sign up";
		} else {
			RemoveAllScreen();
			DisplayScreen(RegisterScreen, true);
			SwapScreenButton.GetComponentInChildren<Text>().text = "Sign in";
		}
	}

	public void ClearFormAndData(){
		usernameGO.GetComponent<InputField>().text = "";
		passwordGO.GetComponent<InputField>().text = "";
		PlayerPrefs.DeleteAll();
		PlayerPrefs.Save();
	}

	IEnumerator Register () {
		if(debugLogin)
			Debug.Log("Registering");

//		Dictionary<string, string> headers = new Dictionary<string, string>();
//		headers.Add("Content-Type", "application/json");
//		headers.Add("Accept", "application/json");
//		System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();
		byte[] json =  form.data;//encoding.GetBytes(jsonString);
		
		DisplayScreen(Feedback, false);
		WWW www = new WWW(APIRegistration, json);
		yield return www;

		Debug.Log(www.error + www.text);
//		string errorCode = "400";
//		if(www.error == null)
//			errorCode = "200";
//		else
//			if(www.error.Length >= 3)
//				errorCode = www.error.Substring(0,3);
//		
//		if(Debug.isDebugBuild || debugLogin)
//			Debug.Log("Answer from " + APIRegistration + ": " + errorCode + www.text);
//		
//		if (errorCode == "200") {
//			List<object> users = MiniJSON.Json.Deserialize(www.text) as List<object>;
//
//			if(users != null){
//
//				if(debugLogin)
//					Debug.Log("Loggedin with more than one kid");
//
//				foreach(Dictionary<string, object> user in users) {
//					CreateUserFromDict(user);
//				}
//
//			} else {
//
//				CreateUserFromAnswer(www.text);
//			}
//				
//		} else {
//		
//			DisplayScreen(Feedback, true);
//			
//			if(Debug.isDebugBuild)
//				Debug.LogWarning (www.error + ": " + www.text);
//		}
	}
	
	IEnumerator Login () {
		if(debugLogin)
			Debug.Log("Logging in");

		Dictionary<string, string> headers = new Dictionary<string, string>();
		headers.Add("Content-Type", "application/json");
		headers.Add("Accept", "application/json");
		
		System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();
		byte[] json = encoding.GetBytes(jsonString);
		
		DisplayScreen(Feedback, false);
		WWW www = new WWW(URLLogin, json, headers);
		yield return www;
		
		string errorCode = "400";
		if(www.error == null)
			errorCode = "200";
		else
			if(www.error.Length >= 3)
				errorCode = www.error.Substring(0,3);
		
		if(Debug.isDebugBuild || debugLogin)
			Debug.Log("Answer from " + URLLogin + ": " + errorCode + www.text);
		
		if (errorCode == "200") {
			List<object> users = MiniJSON.Json.Deserialize(www.text) as List<object>;

			if(users != null){

				if(debugLogin)
					Debug.Log("Loggedin with more than one kid");

				foreach(Dictionary<string, object> user in users) {
					CreateUserFromDict(user);
				}

			} else {

				CreateUserFromAnswer(www.text);
			}
				
		} else {
		
			DisplayScreen(Feedback, true);
			
			if(Debug.isDebugBuild)
				Debug.LogWarning (www.error + ": " + www.text);
		}
	}

	public void CreateUserFromDict(Dictionary<string, object> answerDict) {

		newUser = gameObject.AddComponent<User>();
		newUser.SetupUser((int)(long)answerDict["id"], (string)answerDict["email"], (string)answerDict["firstname"]);
		SaveUser();
		loginGO.GetComponentInChildren<Image>().color = Color.green;
	}

	public void CreateUserFromAnswer(string answer){
		Dictionary<string, object> answerDict = MiniJSON.Json.Deserialize(answer) as Dictionary<string, object>;
		CreateUserFromDict(answerDict);
	}

	public void SaveUser(){
		PlayerPrefs.SetInt("userID", (int)newUser.id);
		PlayerPrefs.SetString("email", newUser.email);
		PlayerPrefs.SetString("firstname", newUser.firstname);
		PlayerPrefs.Save();
	}

}