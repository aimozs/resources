using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class VOAddress {

	public string RecordId;
	public string AddressLine;
	public string Locality;
	public string State;
	public string Postcode;

	public VOAddress(string recordId, string addressLine, string locality, string state, string postcode){
		RecordId = recordId;
		AddressLine = addressLine;
		Locality = locality;
		State = state;
		Postcode = postcode;
	}

}
