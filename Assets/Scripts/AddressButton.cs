using UnityEngine;
using System.Collections;

public class AddressButton : MonoBehaviour {

	private VOAddress address;

	public void SetAddress(VOAddress newAddress){
		address = newAddress;
	}

	public void OnAddressBtnClick(){
		Debug.Log("address btn " + address.AddressLine);
		LoginManager.Instance.SetSelectedMatch(address);
	}
}
