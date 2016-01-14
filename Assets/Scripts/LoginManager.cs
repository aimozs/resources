using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using MiniJSON;
using System.Globalization;
using System.Text.RegularExpressions;


//[RequireComponent (typeof (User))]
[RequireComponent (typeof (LoginDetails))]
[RequireComponent (typeof (RegisterDetails))]
public class LoginManager : MonoBehaviour {
	
	public enum urlEnum {registration, forgotPassword};
	
	public bool debugLogin;
	public bool live = false;

	public int limit = 3;
	
	public GameObject LoginUI;
	
	public GameObject LoginScreen;
	public GameObject Feedback;
	public GameObject RegisterScreen;
	public GameObject notifScreen;
	public GameObject SwapScreenButton;
	public GameObject LogoutBtn;
	
	public GameObject usernameGO;
	public GameObject passwordGO;
	public GameObject loginGO;

	public GameObject sexGO;
	public GameObject maleGO;
	public GameObject femaleGO;
	public GameObject dobYearGO;
	public GameObject dobMonthGO;
	public GameObject dobDayGO;

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

	public GameObject addressMatchGO;
	public GameObject addressPrefab;

	public GameObject schoolMatchGO;
	public GameObject schoolPrefab;

	public GameObject usersGO;
	public GameObject userPrefab;

	public GameObject notifPrefab;

	public LoginDetails newLoginDetails;
	public RegisterDetails newRegisterDetails;
	public User newUser;
	public User currentUser;

	public WWWForm form;

	private bool _fetchAddress = true;
	private bool _fetchSchool = true;

	private string kleberKey;
	private string schoolCode;

	
	private string stagingURL = "http://racstaging.2and2.com.au/";
	private string liveURL = "https://littlelegends.rac.com.au/";
	private string URLLogin = "api/login";
	private string APIRegistration = "api/users";
	private string URLRegistration = "registration";
	private string URLForgotPassword = "forgotpassword";
	private string URLKleberKey = "kleberkey";
	private string URLSchool = "schools?search=";
	private string URLFetchAddress;
	
	private string username;
	private string password;
	private string jsonString;
	private Dictionary<string, string> headers = new Dictionary<string, string>();
	public List<VOAddress> addresses = new List<VOAddress>();

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

		GenerateHeaders();

		DisplayScreen(addressMatchGO, false);
		DisplayScreen(usersGO, false);

		if(live){
			URLLogin = liveURL + URLLogin;
			URLKleberKey = liveURL + "api/" + URLKleberKey;
			APIRegistration = liveURL + APIRegistration;
			URLRegistration = liveURL + URLRegistration;
			URLForgotPassword = liveURL + "api/" + URLForgotPassword;
			URLSchool = liveURL + URLSchool;
		} else {
			URLLogin = stagingURL + URLLogin;
			URLKleberKey = stagingURL + "api/" + URLKleberKey;
			APIRegistration = stagingURL + APIRegistration;
			URLRegistration = stagingURL + "website/" + URLRegistration;
			URLForgotPassword = stagingURL + "api/" + URLForgotPassword;
			URLSchool = stagingURL + "api/" + URLSchool;
		}
		
		DisplayScreen(LoginScreen, true);
		DisplayScreen(RegisterScreen, false);
		DisplayScreen(notifScreen, false);
		DisplayScreen(Feedback, false);
		
		if(Debug.isDebugBuild){
			Debug.Log ("login: " + URLLogin);
			Debug.Log ("api registration: " + APIRegistration);
			Debug.Log ("registration link: " + URLRegistration);
			Debug.Log ("api password: " + URLForgotPassword);
			Debug.Log ("kleberkey: " + URLKleberKey);

		}

		LoadUsers();
	}

	void LoadUsers(){
		if(debugLogin)
			Debug.Log("Loading from playerprefs");

		int numUsers = PlayerPrefs.GetInt("NumUsers", 0);
		Debug.Log(numUsers);

		if(numUsers > 0){
			DisplayScreen(usersGO, true);
			SwapSigninLogout();
			for(int u = 1; u < numUsers; u++){
				Debug.Log(u);
				string loadedUser = PlayerPrefs.GetString("user" + u);
				Debug.Log(loadedUser);

				Dictionary<string, object> newUser = (Dictionary<string, object>)MiniJSON.Json.Deserialize(loadedUser);
	//			Debug.Log(newUser["id"]);
	//			foreach(KeyValuePair<string, object> kvp in newUser){
	//				Debug.Log(kvp.Key);
	//			}
				CreateUserFromDict(newUser);
			}
		}
	}

	void SwapSigninLogout(){
		if(SwapScreenButton.activeInHierarchy) {
			SwapScreenButton.SetActive(false);
			LogoutBtn.SetActive(true);
		} else {
			SwapScreenButton.SetActive(true);
			LogoutBtn.SetActive(false);
		}
	}

	public void LogUserOut(){
		PlayerPrefs.DeleteAll();
		PlayerPrefs.Save();

		DeleteListChildren(usersGO);
		SwapSigninLogout();
	}

	void GenerateHeaders(){
		headers.Add("Content-Type", "application/json");
		headers.Add("Accept", "application/json");
	}
	
	//GOTOS
	public void GoToRegistration() {
		Application.OpenURL(URLRegistration);
	}
	
	public void AskForgotPassword() {
		string accountEmail = usernameGO.GetComponent<InputField>().text;
		if(accountEmail != ""){
			StartCoroutine(AskForNewPassowrd(accountEmail));
		} else {
			Notify("Email shouldnt be empty");
		}
	}

	IEnumerator AskForNewPassowrd(string accountEmail){
		newLoginDetails.email = usernameGO.GetComponent<InputField>().text;
		newLoginDetails.password = passwordGO.GetComponent<InputField>().text;

		jsonString = JsonUtility.ToJson(newLoginDetails);
		System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();
		byte[] json = encoding.GetBytes(jsonString);

		Debug.Log("json forgotten email" + jsonString);
		WWW www = new WWW(URLForgotPassword, json, headers);
		yield return www;

		if(www.error != null)
			Notify("Email not found");
	}

	void Notify(string message){
		if(debugLogin)
			Debug.Log("Notifying " + message);
		GameObject notifGO = Instantiate(notifPrefab);
		notifGO.transform.SetParent(notifScreen.transform, false);
		notifGO.GetComponentInChildren<Text>().text = message;
		DisplayScreen(notifScreen, true);
		Destroy(notifGO, 5f);

		if(notifGO.transform.childCount == 0)
			DisplayScreen(notifScreen, false);

	}

	
	//On Clicks
	public void OnLoginBtnClick() {
		newLoginDetails.email = usernameGO.GetComponent<InputField>().text;
		newLoginDetails.password = passwordGO.GetComponent<InputField>().text;

		jsonString = JsonUtility.ToJson(newLoginDetails);


		if(debugLogin)
			Debug.Log("JSON that will be sent" + jsonString);

		StartCoroutine(Login());
	}

	public void OnRegisterBtnClick(){
		
		if(newPasswordGO.GetComponent<InputField>().text != passwordConfirmationGO.GetComponent<InputField>().text && emailGO.GetComponent<InputField>().text!= ""){
			DisplayScreen(Feedback, true);
			return;
		}

		PopulateRegistration();

		jsonString = JsonUtility.ToJson(newRegisterDetails);
		DisplayScreen(Feedback, false);

		Debug.Log(_fetchAddress);

		if(_fetchAddress){
			StartCoroutine(DoVerify());
		} else {
		
			StartCoroutine(Register());
		}
	}

	IEnumerator Register () {
		if(debugLogin)
			Debug.Log("Registering");


		System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();
		if(debugLogin)
			Debug.Log("JSON created from form" + jsonString);

		byte[] json = encoding.GetBytes(jsonString);

		WWW register = new WWW(APIRegistration, json, headers);
		yield return register;

		string errorCode = "400";

		if(register.error == null) {

			errorCode = "201";

			RegisterGO.GetComponent<Image>().color = Color.green;

			CreateUserFromAnswer(register.text);

		} else {
			if(register.error.Length >= 3)
				errorCode = register.error.Substring(0,3);

			DisplayScreen(Feedback, true);
			
			if(Debug.isDebugBuild)
				Debug.LogWarning (register.error + ": " + register.text);
		}
		
		if(Debug.isDebugBuild || debugLogin)
			Debug.Log("Answer from " + APIRegistration + ": " + errorCode + register.text);
	}

	public void HideAddressSuggestions(){
		DeleteListChildren(addressMatchGO);
		_fetchAddress = true;
	}


	IEnumerator RetreiveKleberKey(){

		WWW www = new WWW(URLKleberKey);
		yield return www;

		Dictionary<string, object> answerDict = MiniJSON.Json.Deserialize(www.text) as Dictionary<string, object>;

		Dictionary<string, object> dtResponse = (Dictionary<string, object>)answerDict["DtResponse"];

		List<object> result = (List<object>)dtResponse["Result"];

		Dictionary<string, object> itemDict = (Dictionary<string, object>)result[0];

		kleberKey = (string)itemDict["TemporaryRequestKey"];

		if(debugLogin)
			Debug.Log(kleberKey);
	}

	public void FetchSchool(){
		if(schoolGO.GetComponent<InputField>().text.Length > 2){
			if(_fetchSchool){
				string search = schoolGO.GetComponent<InputField>().text;
				Debug.Log("address " + search);

				StartCoroutine(FetchSchoolData(search));
			}

		} else {
			_fetchSchool = true;
		}
	}

	IEnumerator FetchSchoolData(string search){
		string URLSchoolSearch = URLSchool + search;

		if(debugLogin)
			Debug.Log("URL fetching " + URLSchoolSearch);

		WWW www = new WWW(URLSchoolSearch);
		yield return www;

		List<object> schoolMatches = MiniJSON.Json.Deserialize(www.text) as List<object>;

		foreach(Dictionary<string, object> schoolMatch in schoolMatches){
			DisplayScreen(schoolMatchGO, true);

			GameObject newSchool = Instantiate(schoolPrefab);
			newSchool.transform.SetParent(schoolMatchGO.transform, false);
			newSchool.GetComponent<SchoolButton>().SetSchool(schoolMatch["name"].ToString(), schoolMatch["code"].ToString());
		}

	}

	public void FetchAddress(){
		if(kleberKey == null)
			StartCoroutine(RetreiveKleberKey());
		else if(addressGO.GetComponent<InputField>().text.Length > 2 ){

			if(_fetchAddress){
				string address = addressGO.GetComponent<InputField>().text;
				Debug.Log("address " + address);

				URLFetchAddress = "https://kleber.datatoolscloud.net.au/KleberWebService/DtKleberService.svc/ProcessQueryStringRequest?Method=DataTools.Capture.Address.Predictive.AuPaf.SearchAddress&AddressLine=" + EncodeToURI(address) + "&ResultLimit=" + Instance.limit + "&RequestKey=" + EncodeToURI(kleberKey) + "&OutputFormat=json";

				StartCoroutine(FetchAddressData());
			}

		} else {
			_fetchAddress = true;
		}
	}

	IEnumerator FetchAddressData(){
		if(debugLogin)
			Debug.Log("URL fetching " + URLFetchAddress);

		WWW www = new WWW(URLFetchAddress);
		yield return www;

		Debug.Log(www.text);

		Dictionary<string, object> answerDict = MiniJSON.Json.Deserialize(www.text) as Dictionary<string, object>;

		Dictionary<string, object> dtResponse = (Dictionary<string, object>)answerDict["DtResponse"];

		if(dtResponse["Result"] != null){

			List<object> results = (List<object>)dtResponse["Result"];

			addresses.Clear();
			DeleteListChildren(addressMatchGO);

			foreach(object result in results){
				Dictionary<string, object> match = (Dictionary<string, object>)result;

				VOAddress voAddress = new VOAddress((string)match["RecordId"], (string)match["AddressLine"], (string)match["Locality"], (string)match["State"], (string)match["Postcode"]);

				addresses.Add(voAddress);
			}

			DisplayMatchList();

		} else {

			DisplayScreen(addressMatchGO, false);

			if(debugLogin)
				Debug.Log("There's no matching addresses");
		}
	}

	IEnumerator DoVerify(){
		string jsonv = "https://kleber.datatoolscloud.net.au/KleberWebService/DtKleberService.svc/ProcessQueryStringRequest?Method=DataTools.Verify.Address.AuPaf.VerifyAddress&AddressLine1=" + EncodeToURI(addressGO.GetComponent<InputField>().text) + "&Locality=" + EncodeToURI(suburbGO.GetComponent<InputField>().text) + "&State=" + EncodeToURI(stateGO.GetComponent<Dropdown>().captionText.text) + "&Postcode=" + EncodeToURI(postcodeGO.GetComponent<InputField>().text) + "&RequestKey=" + EncodeToURI(kleberKey) + "&OutputFormat=json";
		if(debugLogin)
			Debug.Log("Verifying with " + addressGO.GetComponent<InputField>().text + "\n" + jsonv);


		WWW verify = new WWW(jsonv);
		yield return verify;

		Debug.Log(verify.text);

		if(verify.error == null){
			Dictionary<string, object> answerDict = MiniJSON.Json.Deserialize(verify.text) as Dictionary<string, object>;

			Dictionary<string, object> dtResponse = (Dictionary<string, object>)answerDict["DtResponse"];

			if(dtResponse["Result"] != null){
				List<object> results = (List<object>)dtResponse["Result"];
				if(results.Count == 1) {
					Dictionary<string, object> match = (Dictionary<string, object>)results[0];

					if((string)match["AddressLine"] == ""){

						if(debugLogin)
							Debug.Log("verify address line empty");

						StartCoroutine(DoRepair());

					} else {

						VOAddress voAddress = new VOAddress("none", (string)match["AddressLine"], (string)match["Locality"], (string)match["State"], (string)match["Postcode"]);

						SetSelectedAddress(voAddress);

						StartCoroutine(Register());
					}

				} else {
					Debug.Log("NOT EQUAL TO ONE");
				}

			} else {
				if(debugLogin)
					Debug.Log("verify result equal null");
				StartCoroutine(DoRepair());

			}
		} else {
			if(debugLogin)
				Debug.Log("verify error" + verify.text);
			StartCoroutine(DoRepair());
		}
	}

	IEnumerator DoRepair(){
		if(debugLogin)
			Debug.Log("Repairing");

		WWW repair = new WWW("https://kleber.datatoolscloud.net.au/KleberWebService/DtKleberService.svc/ProcessQueryStringRequest?Method=DataTools.Repair.Address.AuPaf.RepairAddress&AddressLine1=" + EncodeToURI(addressGO.GetComponent<InputField>().text) + "&Locality=" + EncodeToURI(suburbGO.GetComponent<InputField>().text) + "&State=" + EncodeToURI(stateGO.GetComponent<Dropdown>().captionText.text) + "&Postcode=" + EncodeToURI(postcodeGO.GetComponent<InputField>().text) + "&RequestKey=" + EncodeToURI(kleberKey) + "&OutputFormat=json");
		yield return repair;

		if(repair.error == null){
			Dictionary<string, object> answerDict = MiniJSON.Json.Deserialize(repair.text) as Dictionary<string, object>;

			Dictionary<string, object> dtResponse = (Dictionary<string, object>)answerDict["DtResponse"];

			if(dtResponse["Result"] != null){
				List<object> results = (List<object>)dtResponse["Result"];
				if(results.Count == 1) {
					Dictionary<string, object> match = (Dictionary<string, object>)results[0];

					if((string)match["AddressLine"] == ""){

						StartCoroutine(Register());

					} else {

						VOAddress voAddress = new VOAddress("none", (string)match["AddressLine"], (string)match["Locality"], (string)match["State"], (string)match["Postcode"]);

						SetSelectedAddress(voAddress);

						StartCoroutine(Register());
					}
				}
			} else {
				if(debugLogin)
					Debug.Log("repair result equal null");
			}
		} else {
			if(debugLogin)
				Debug.Log("repair erro " + repair.text);
		}
	}

	void DisplayMatchList(){
		DisplayScreen(addressMatchGO, true);

		foreach(VOAddress address in addresses){
			GameObject newAddress = Instantiate(addressPrefab);
			newAddress.transform.SetParent(addressMatchGO.transform, false);
			newAddress.GetComponentInChildren<Text>().text = address.AddressLine + "\n" + address.Locality + "\n" + address.Postcode + " " + address.State;
			newAddress.GetComponent<AddressButton>().SetAddress(address);
		}
	}

	public void SetSelectedSchool(string _schoolName, string _schoolCode){
		_fetchSchool = false;

		schoolGO.GetComponent<InputField>().text = _schoolName;
		schoolCode = _schoolCode;
		Debug.Log("setting school data: " + schoolGO.GetComponent<InputField>().text + schoolCode);

		DeleteListChildren(schoolMatchGO);
	}

	public void SetSelectedAddress(VOAddress selectedAddress){

		_fetchAddress = false;

		addressGO.GetComponent<InputField>().text = selectedAddress.AddressLine;
		suburbGO.GetComponent<InputField>().text = selectedAddress.Locality;
		postcodeGO.GetComponent<InputField>().text = selectedAddress.Postcode;
		switch(selectedAddress.State){
		case "ACT":
			stateGO.GetComponent<Dropdown>().value = 0;
			break;
		case "QLD":
			stateGO.GetComponent<Dropdown>().value = 1;
			break;
		case "NSW":
			stateGO.GetComponent<Dropdown>().value = 2;
			break;
		case "NT":
			stateGO.GetComponent<Dropdown>().value = 3;
			break;
		case "VIC":
			stateGO.GetComponent<Dropdown>().value = 4;
			break;
		case "TAZ":
			stateGO.GetComponent<Dropdown>().value = 5;
			break;
		case "WA":
			stateGO.GetComponent<Dropdown>().value = 6;
			break;
		}

		string URLRetrieveAddress = "https://kleber.datatoolscloud.net.au/KleberWebService/DtKleberService.svc/ProcessQueryStringRequest?Method=DataTools.Capture.Address.Predictive.AuPaf.RetrieveAddress&RecordId=" + EncodeToURI(selectedAddress.RecordId) + "&AddressLine=" + EncodeToURI(selectedAddress.AddressLine) + "&Locality=" + EncodeToURI(selectedAddress.Locality) + "&State=" + EncodeToURI(selectedAddress.State) + "&Postcode=" + EncodeToURI(selectedAddress.Postcode) + "&RequestKey=" + EncodeToURI(kleberKey) + "&OutputFormat=json";
		StartCoroutine(RetrieveAddressData(URLRetrieveAddress));

		DeleteListChildren(addressMatchGO);
	}


	IEnumerator RetrieveAddressData(string urlRetrieveAddress){
		if(debugLogin)
			Debug.Log("URL retreive address " + urlRetrieveAddress);

		WWW www = new WWW(urlRetrieveAddress);
		yield return www;

		Debug.Log(www.text);

		Dictionary<string, object> answerDict = MiniJSON.Json.Deserialize(www.text) as Dictionary<string, object>;

		Dictionary<string, object> dtResponse = (Dictionary<string, object>)answerDict["DtResponse"];

	}

	void DeleteListChildren(GameObject GO){
		int childs = GO.transform.childCount;
		if(childs > 0){
			
	        for (int i = childs - 1; i >= 0; i--) {
				Debug.Log(i + ": " + GO.transform.GetChild(i).name);
				Destroy(GO.transform.GetChild(i).gameObject);
	        }
        }
		DisplayScreen(GO, false);
	}

	IEnumerator DeleteListChildrenDelayed(GameObject GO){
		yield return new WaitForSeconds(1f);
		int childs = GO.transform.childCount;
		if(childs > 0){
			
	        for (int i = childs - 1; i >= 0; i--) {
				Debug.Log(i + ": " + GO.transform.GetChild(i).name);
				Destroy(GO.transform.GetChild(i).gameObject);
	        }
        }
		DisplayScreen(GO, false);
	}

	public string EncodeToURI(string value) {
		return System.Uri.EscapeDataString(value);
	}

	public byte[] EncodeToByte(string value) {
		System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();
		byte[] bytes = encoding.GetBytes(value);
		return bytes;
	}
	
	public void ResetFeedback(){
		passwordGO.GetComponent<InputField>().text = "";
		DisplayScreen(Feedback, false);
	}

	public void PopulateRegistration(){
		newRegisterDetails.male = maleGO.GetComponent<Toggle>().isOn ? "1" : "0";
		newRegisterDetails.dob = dobYearGO.GetComponent<InputField>().text + "-" + dobMonthGO.GetComponent<InputField>().text + "-" + dobDayGO.GetComponent<InputField>().text;
		newRegisterDetails.firstname = firstnameGO.GetComponent<InputField>().text;
		newRegisterDetails.lastname = lastnameGO.GetComponent<InputField>().text;
		newRegisterDetails.school = schoolGO.GetComponent<InputField>().text;

		newRegisterDetails.address = addressGO.GetComponent<InputField>().text;
		newRegisterDetails.address_suburb = suburbGO.GetComponent<InputField>().text;
		newRegisterDetails.address_state = stateGO.GetComponent<Dropdown>().captionText.text;
		newRegisterDetails.address_postcode = postcodeGO.GetComponent<InputField>().text;

		newRegisterDetails.phone = phoneGO.GetComponent<InputField>().text;
		newRegisterDetails.email = emailGO.GetComponent<InputField>().text;
		newRegisterDetails.password = newPasswordGO.GetComponent<InputField>().text;

		newRegisterDetails.schoolcode = schoolCode;
		newRegisterDetails.teacher = "t";
		newRegisterDetails.guardian_firstname = "gfn";
		newRegisterDetails.guardian_lastname = "gln";
		newRegisterDetails.guardian_dob = "1982-11-22";
		newRegisterDetails.guardian_title = "mr";
		newRegisterDetails.relationship = "parent";
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

		if(kleberKey == null)
			StartCoroutine(RetreiveKleberKey());
	}

	public void ClearFormAndData(){
		usernameGO.GetComponent<InputField>().text = "";
		passwordGO.GetComponent<InputField>().text = "";
		PlayerPrefs.DeleteAll();
		PlayerPrefs.Save();
	}

	public void LoginExistingUser(User user){
		Debug.Log("login that user: " + user.firstname);
	}

	IEnumerator Login () {
		if(debugLogin)
			Debug.Log("Logging in");

		DisplayScreen(Feedback, false);
		WWW www = new WWW(URLLogin, EncodeToByte(jsonString), headers);
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

				DisplayScreen(usersGO, true);

				foreach(Dictionary<string, object> user in users) {
					CreateUserFromDict(user);
				}

				SwapSigninLogout();

			} else {

				CreateUserFromAnswer(www.text);
				loginGO.GetComponentInChildren<Image>().color = Color.green;
			}
				
		} else {
		
			DisplayScreen(Feedback, true);
			
			if(Debug.isDebugBuild)
				Debug.LogWarning (www.error + ": " + www.text);
		}
	}

	public void CreateUserFromDict(Dictionary<string, object> answerDict) {
		User[] users = GameObject.FindObjectsOfType<User>();

		bool exist = false;

		foreach(User user in users){
			if(user.id == (int)(long)answerDict["id"])
				exist = true;
		}

		if(!exist){
			GameObject newUser = Instantiate(userPrefab) as GameObject;
			newUser.transform.SetParent(usersGO.transform, false);
			newUser.GetComponentInChildren<Text>().text = CultureInfo.CurrentCulture.TextInfo.ToTitleCase((string)answerDict["firstname"]);
			newUser.AddComponent<User>();
			newUser.GetComponentInChildren<User>().id = (int)(long)answerDict["id"];
			newUser.GetComponentInChildren<User>().email = (string)answerDict["email"];
			newUser.GetComponentInChildren<User>().firstname = (string)answerDict["firstname"];
			SaveUser(newUser.GetComponent<User>());
//			newUser = gameObject.AddComponent<User>();
//			newUser.SetupUser((int)(long)answerDict["id"], (string)answerDict["email"], (string)answerDict["firstname"]);
//			SaveUser(newUser);
		}

	}

	public void CreateUserFromAnswer(string answer){
		Dictionary<string, object> answerDict = MiniJSON.Json.Deserialize(answer) as Dictionary<string, object>;
		CreateUserFromDict(answerDict);
	}

	public void SaveUser(User newUser){
		

		int numUsers = PlayerPrefs.GetInt("NumUsers", 0);
		numUsers++;
		string jsonUser = JsonUtility.ToJson(newUser);

		if(debugLogin)
			Debug.Log("Saving user " + (int)newUser.id + " as the " + numUsers + "th user");

		PlayerPrefs.SetString("user" + numUsers, jsonUser);
		PlayerPrefs.SetInt("NumUsers", numUsers);

//		PlayerPrefs.SetInt("user" + newUser.id.ToString(), (int)newUser.id);
//		PlayerPrefs.SetString("email", newUser.email);
//		PlayerPrefs.SetString("firstname" + newUser.id.ToString(), newUser.firstname);

		PlayerPrefs.Save();
	}

}