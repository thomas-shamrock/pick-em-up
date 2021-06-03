using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryItem {

	private string name;
	private Sprite iconSprite;
	private string walletAddress;

	public InventoryItem(string name, Sprite iconSprite, string walletAddress) {
		this.name = name;
		this.iconSprite = iconSprite;
		this.walletAddress = walletAddress;
	}

	public string Name {
		get {
			return this.name;
		}
		set {
			this.name = value;
		}
	}

	public Sprite IconSprite {
		get {
			return this.iconSprite;
		}
		set {
			this.iconSprite = value;
		}
	}

	public string WalletAddress {
		get {
			return this.walletAddress;
		}
		set {
			this.walletAddress = value;
		}
	}
}
