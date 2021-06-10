using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RPGCharacterController;
using Enjin.SDK.Core;

public class Inventory : MonoBehaviour {
	public Image[] icons;
	public EnjinWallet enjinWallet;

	private InventoryItem[] inventoryItems;

	// Use this for initialization
	void Start () {
		inventoryItems = new InventoryItem[icons.Length];

		RPGMotor rpgMotor = this.gameObject.GetComponent<RPGMotor>();
		rpgMotor.OnItemPickedUp += RpgMotor_OnItemPickedUp;
	}
	
	// Update is called once per frame
	void Update () {
	}

	void RpgMotor_OnItemPickedUp (GameObject itemGameObject)
	{
		ItemStatus itemStatus = itemGameObject.GetComponent<ItemStatus>();
		if (!itemStatus.hasBeenPickedUp)			
		{
			itemStatus.hasBeenPickedUp = true;
			string itemName = itemGameObject.name.Replace ("(Clone)", "");
			Sprite sprite = Resources.Load<Sprite> ("Sprites/" + itemName);
			//TODO: find out how to assign the walletAddress
			string walletAddress = "";
			this.Add (new InventoryItem (itemName, sprite, walletAddress));
			Debug.Log ("Picked up " + itemName);
			enjinWallet.GetItem(itemName);
			Destroy (itemGameObject);
		}
	}

	void Add(InventoryItem item) {
		for (int i = 0; i < this.inventoryItems.Length; i++) {
			if (this.inventoryItems[i] == null) {
				this.inventoryItems[i] = item;
				this.icons[i].sprite = item.IconSprite;
				Color tempColor = this.icons[i].color;
				tempColor.a = 0.5f;
				this.icons[i].color = tempColor;
				break;
			}
		}
	}
}
