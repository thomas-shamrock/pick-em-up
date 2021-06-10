using System.Collections.Generic;
using UnityEngine;
//using XLegacyEnjinSDK;
using System;
using System.Collections;
using Enjin.SDK.Core;
using Enjin.SDK.DataTypes;

namespace Enjin.SDK.Core
{
    public class EnjinWallet : MonoBehaviour
    {
        [SerializeField] string platformUrl = "https://kovan.cloud.enjin.io";
        [SerializeField] int projectId;
        [SerializeField] string projectSecret;
        [SerializeField] string playerName;

        
        private string _developerWalletAddress;
        private string _adminAccessToken;
        private string _playerAccessToken;
        private int _playerIdentityId;
        private string _playerLinkingCode;
        private string _playerWalletAddress;

        private void Awake()
        {
            AuthenticateSdkAsAdmin();
            FetchDeveloperWalletAddress();
            StartCoroutine(AuthenticatePlayer(playerName));
        }

        private void AuthenticateSdkAsAdmin()
        {
            /*
             * Authenticate the SDK as an admin.
             *
             * Note: You should not do this in an actual game. Developers need to have a manged server responsible
             * for player authentication and any other operations requiring admin authentication. Exposing the app
             * secret to player clients is a security risk.
             */
            Enjin.StartPlatform(platformUrl, projectId, projectSecret);
            _adminAccessToken = Enjin.AccessToken;
        }

        private void FetchDeveloperWalletAddress()
        {
            /*
             * Get the details of the app the SDK is authenticated for.
             */
            App app = Enjin.GetApp();
            
            // Check if the GetApp query found a result.
            if (Enjin.ServerResponse != ResponseCodes.SUCCESS)
                return;

            /*
             * Check to see if the app has been linked to a wallet. If so,
             * set the developer wallet address to the address of the first
             * wallet result.
             */
            if (app.wallets.Count > 0)
                _developerWalletAddress = app.wallets[0].ethAddress;
            
            Debug.Log($"Developer Wallet Address: {_developerWalletAddress}");
        }

        /*
         * Authenticates the player.
         *
         * Note: This is intended to be handled on a managed authentication server due to the fact that
         * sensitive credentials are involved. Player clients should never handle this themselves.
         */
        public IEnumerator AuthenticatePlayer(string playerName)
        {
            Enjin.IsDebugLogActive = true;

            // Attempt to fetch player from Enjin platform.
            User player = Enjin.GetUser(playerName);
            if (Enjin.ServerResponse == ResponseCodes.NOTFOUND)
            {
                // Create new player if no result found.
                player = Enjin.CreatePlayer(playerName);
            }

            for (int i = 0; i < player.identities.Length; i++)
            {
                Identity identity = player.identities[i];
                
                if (identity.app.id != projectId)
                    continue;
                
                _playerIdentityId = identity.id;
                _playerWalletAddress = identity.wallet.ethAddress;
                _playerLinkingCode = identity.linkingCode;
                break;
            }
            
            Debug.Log($"Player Identity Id: {_playerIdentityId}");
            Debug.Log($"Player Wallet Address: {_playerWalletAddress}");
            Debug.Log($"Player Linking Code: {_playerLinkingCode}");
            
            /*
             * Authenticate the player and cache the access token.
             *
             * Note: Normally you will have your own protocol for clients to communicate with your
             * authentication server using credentials you save in your own database. It is recommended
             * that you generate a unique ID for your users and associate that with their login
             * credentials to avoid exposing their private details to a third party (Enjin).
             */
            _playerAccessToken = Enjin.AuthPlayer(playerName);
            yield return null;
        }

        private void SetAccessToken(bool player = false)
        {
            /*
             * Sets the access token to the admin token by default. If player is set to true the
             * SDK will be set to use the player access token.
             * 
             * Note: In an ideal setup you only ever need to deal with one access token. This is
             * why it is important to have a managed server. The server only needs the app
             * access token whereas player clients would only need the player access token
             * to authenticate the SDK with.
             */
            Enjin.AccessToken = player ? _playerAccessToken : _adminAccessToken;
        }

        IEnumerator MintItem(string itemName, int quantity)
        {
            string itemId = itemName;

            SetAccessToken();
            Enjin.MintFungibleItem(_developerWalletAddress, new string[] {_playerWalletAddress}, itemId, quantity,
                (requestData) => { print("Item Minted::" + itemName); }, true);

            yield return null;
        }


        IEnumerator SendItem(string itemName, int quantiy)
        {
            string itemId = itemName;
            
            SetAccessToken(true);
            Enjin.SendCryptoItemRequest(_playerWalletAddress, itemName, _developerWalletAddress, quantiy,
                (requestData) => { print("Item Sended::" + itemName); }, true);
            SetAccessToken();

            yield return null;
        }


        IEnumerator MeltItem(string itemName, int quantity)
        {
            string itemId = itemName;
            
            SetAccessToken(true);
            Enjin.MeltTokens(_playerWalletAddress, itemId, quantity,
                (requestData) => { print("Item Melted::" + itemName); }, true);
            SetAccessToken();

            yield return null;
        }


        public void GetItem(string name)
        {
            print("Verifying transaction..");
            StartCoroutine(MintItem(name, 1));
        }

        public void ReturnItem(string name)
        {
            print("Verifying transaction..");
            StartCoroutine(MeltItem(name, 1));
        }

        public void SendItemTo(string name)
        {
            print("Verifying transaction..");
            StartCoroutine(SendItem(name, 1));
        }
    }
}