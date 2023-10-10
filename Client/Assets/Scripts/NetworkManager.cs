using Framework.Constants;
using Game.Networking;
using Game.Networking.Packets;
using Network.Security;
using System;
using System.Net.Sockets;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class NetworkManager : MonoSingleton<NetworkManager>
{
    private class Message
    {
        public AuthCmd cmd;
        public byte[] message;
    }

    private NetCoreServer.TcpClient tcpClient = null;

    private string account;
    private byte[] passwordHash;
    private byte[] m2;
    private BigInteger sessionKey;



    private Queue<Message> messages = new Queue<Message>();
    private Queue<Message> tempMessages = new Queue<Message>();


    private void Start()
    {
        this.tcpClient = new NetCoreServer.TcpClient("127.0.0.1", 3724);
        this.tcpClient.OnClientConnected += OnClientConnected;
        this.tcpClient.OnClientDataReceived += OnClientDataReceived;
        this.tcpClient.OnClientDisconnected += OnClientDisconnected;
        this.tcpClient.OnClientError += OnClientError;

        this.tcpClient.ConnectAsync();

        StartCoroutine(DispatchMessage());
    }

    public void OnClientConnected()
    {
        Debug.Log("OnClientConnected");
    }

    public void OnClientDataReceived(byte[] buffer, long offset, long size)
    {
        Debug.Log("OnClientDataReceived");

        if (size > 0)
        {
            AuthCmd clientOpcodes = (AuthCmd)buffer[0];
            Message message = new Message();
            message.cmd = clientOpcodes;
            message.message = new byte[size];
            Array.Copy(buffer, offset, message.message, 0, size);
            this.messages.Enqueue(message);
        }       
    }

    private IEnumerator DispatchMessage()
    {
        do
        {
            if (0 != messages.Count)
            {
                lock (messages)
                {
                    while (0 < messages.Count)
                    {
                        Message message = messages.Dequeue();
                        tempMessages.Enqueue(message);
                    }
                }

                while (0 < tempMessages.Count)
                {
                    Message message = tempMessages.Dequeue();
                    switch (message.cmd)
                    {
                        case AuthCmd.AUTH_LOGON_CHALLENGE:
                            {
                                OnLoginResponse(message.message);
                            }
                            break;
                        case AuthCmd.AUTH_LOGON_PROOF:
                            {
                                OnLogonProofResponse(message.message);
                            }
                            break;
                        case AuthCmd.REALM_LIST:
                            {
                                OnRealmListResponse(message.message);
                            }
                            break;
                    }
                }
            }

            yield return new WaitForEndOfFrame();
        } while (true);
    }


    public void OnClientError(SocketError error)
    {
        switch (error)
        {
            case SocketError.ConnectionRefused:
                {
                    UIManager.Instance.ShowMessage(PanelMessage.MessageType.Confirm, error.ToString(), "无法连接到服务器");
                }
                break;
        }
        Debug.LogFormat("OnClientError:{0}", error.ToString());
    }

    public void OnClientDisconnected()
    {
        Debug.Log("OnClientDisconnected");
        UIManager.Instance.ShowMessage(PanelMessage.MessageType.Confirm, "连接已断开", "无法连接到服务器");
    }

    public void OnApplicationQuit()
    {
        if (null != this.tcpClient)
        {
            this.tcpClient.DisconnectAsync();
            this.tcpClient.OnClientConnected -= OnClientConnected;
            this.tcpClient.OnClientDataReceived -= OnClientDataReceived;
            this.tcpClient.OnClientDisconnected -= OnClientDisconnected;
            this.tcpClient.OnClientError -= OnClientError;
        }
    }

    public void Login(string account, string password)
    {
        if (!this.tcpClient.IsConnected)
        {
            Debug.Log("Reconnect");
            this.tcpClient.ConnectAsync();
            return;
        }
        Debug.Log("Login");
        this.account = account.ToUpper();
        string hashStr = string.Format("{0}:{1}", this.account, password);
        this.passwordHash = Network.Security.HashAlgorithm.SHA1.Hash(Encoding.ASCII.GetBytes(hashStr.ToUpper()));

        AuthLogonChallengeRequest authLogonChallengeRequest = new AuthLogonChallengeRequest(this.account);
        authLogonChallengeRequest.WritePacketData();
        this.tcpClient.SendAsync(authLogonChallengeRequest.GetData());
    }

    public void OnLoginResponse(byte[] buffer)
    {
        Debug.Log("OnLoginResponse");
        InPacket inPacket = new InPacket(buffer);
        AuthLogonChallengeResponse authLogonChallengeResponse = new AuthLogonChallengeResponse(inPacket);
        authLogonChallengeResponse.Read();
        if (authLogonChallengeResponse.error != AuthResult.WOW_SUCCESS)
        {
            Debug.LogFormat("OnLoginResponse:{0}", authLogonChallengeResponse.error.ToString());
            UIManager.Instance.ShowMessage(PanelMessage.MessageType.Confirm, "登录错误", authLogonChallengeResponse.error.ToString());
            return;
        }

        BigInteger y;
        BigInteger a;
        BigInteger k = new BigInteger(3);

        BigInteger b = authLogonChallengeResponse.b;
        BigInteger g = authLogonChallengeResponse.g;
        BigInteger n = authLogonChallengeResponse.n;

        BigInteger x = Network.Security.HashAlgorithm.SHA1.Hash(authLogonChallengeResponse.salt.ToCleanByteArray(), this.passwordHash).ToBigInteger();

        RandomNumberGenerator rand = RandomNumberGenerator.Create();

        do
        {
            byte[] randBytes = new byte[19];
            rand.GetBytes(randBytes);
            a = randBytes.ToBigInteger();

            y = g.ModPow(a, n);
        } while (y.ModPow(1, n) == 0);

        BigInteger u = Network.Security.HashAlgorithm.SHA1.Hash(y.ToCleanByteArray(), b.ToCleanByteArray()).ToBigInteger();

        // compute session key
        BigInteger s = ((b + k * (n - g.ModPow(x, n))) % n).ModPow(a + u * x, n);
        byte[] keyHash;
        byte[] sData = s.ToCleanByteArray();
        if (sData.Length < 32)
        {
            byte[] tmpBuffer = new byte[32];
            System.Buffer.BlockCopy(sData, 0, tmpBuffer, 32 - sData.Length, sData.Length);
            sData = tmpBuffer;
        }

        byte[] keyData = new byte[40];
        byte[] temp = new byte[16];

        // take every even indices byte, hash, store in even indices
        for (int i = 0; i < 16; ++i)
        {
            temp[i] = sData[i * 2];
        }
        keyHash = Network.Security.HashAlgorithm.SHA1.Hash(temp);
        for (int i = 0; i < 20; ++i)
        {
            keyData[i * 2] = keyHash[i];
        }

        // do the same for odd indices
        for (int i = 0; i < 16; ++i)
        {
            temp[i] = sData[i * 2 + 1];
        }
        keyHash = Network.Security.HashAlgorithm.SHA1.Hash(temp);
        for (int i = 0; i < 20; ++i)
        {
            keyData[i * 2 + 1] = keyHash[i];
        }

        BigInteger currentKey = keyData.ToBigInteger();

        // XOR the hashes of N and g together
        byte[] gNHash = new byte[20];

        byte[] nHash = Network.Security.HashAlgorithm.SHA1.Hash(n.ToCleanByteArray());
        for (int i = 0; i < 20; ++i)
        {
            gNHash[i] = nHash[i];
        }

        byte[] gHash = Network.Security.HashAlgorithm.SHA1.Hash(g.ToCleanByteArray());
        for (int i = 0; i < 20; ++i)
        {
            gNHash[i] ^= gHash[i];
        }

        // hash username
        byte[] userHash = Network.Security.HashAlgorithm.SHA1.Hash(Encoding.ASCII.GetBytes(this.account.ToUpper()));

        // our proof
        byte[] m1Hash = Network.Security.HashAlgorithm.SHA1.Hash
        (
            gNHash,
            userHash,
            authLogonChallengeResponse.salt.ToCleanByteArray(),
            y.ToCleanByteArray(),
            b.ToCleanByteArray(),
            currentKey.ToCleanByteArray()
        );

        // expected proof for server
        this.m2 = Network.Security.HashAlgorithm.SHA1.Hash(y.ToCleanByteArray(), m1Hash, keyData);

        this.OnAuthLogonProofRequest(y, m1Hash);
    }

    public void OnAuthLogonProofRequest(BigInteger a, byte[] hash)
    {
        Debug.Log("OnAuthLogonProofRequest");
        AuthLogonProofRequest authLogonProofRequest = new AuthLogonProofRequest(a, hash);
        authLogonProofRequest.WritePacketData();
        this.tcpClient.SendAsync(authLogonProofRequest.GetData());
    }

    public void OnLogonProofResponse(byte[] buffer)
    {
        Debug.Log("OnLogonProofResponse");
        InPacket inPacket = new InPacket(buffer);

        AuthLogonProofResponse authLogonProofResponse = new AuthLogonProofResponse(inPacket);
        authLogonProofResponse.Read();

        if (authLogonProofResponse.error != AuthResult.WOW_SUCCESS)
        {
            Debug.LogFormat("OnLogonProofResponse Error:{0}", authLogonProofResponse.error.ToString());
            switch (authLogonProofResponse.error)
            {
                case AuthResult.WOW_FAIL_UNKNOWN_ACCOUNT:
                    {
                        UIManager.Instance.ShowMessage(PanelMessage.MessageType.Confirm, "用户名错误", authLogonProofResponse.error.ToString());
                    }
                    break;
                case AuthResult.WOW_FAIL_INCORRECT_PASSWORD:
                    {
                        UIManager.Instance.ShowMessage(PanelMessage.MessageType.Confirm, "密码错误", authLogonProofResponse.error.ToString());
                    }
                    break;
            }
            return;
        }

        bool isSame = authLogonProofResponse.m2 != null && authLogonProofResponse.m2.Length == 20;
        for (int i = 0; i < authLogonProofResponse.m2.Length && isSame; ++i)
        {
            if (!(isSame = authLogonProofResponse.m2[i] == this.m2[i]))
            {
                break;
            }
        }

        if (isSame)
        {
            this.OnRealmListRequest();
        }
        else
        {
            Debug.LogErrorFormat("[Error]:{0}", "Proof did not match...");
            UIManager.Instance.ShowMessage(PanelMessage.MessageType.Confirm, "登录错误", "Proof did not match...");
        }
    }

    public void OnRealmListRequest()
    {
        Debug.Log("OnRealmListRequest");
        RealmListRequest realmListRequest = new RealmListRequest();
        realmListRequest.WritePacketData();
        this.tcpClient.SendAsync(realmListRequest.GetData());
    }

    public void OnRealmListResponse(byte[] buffer)
    {
        Debug.Log("OnRealmListResponse");
        InPacket inPacket = new InPacket(buffer);

        RealmListResponse realmListResponse = new RealmListResponse(inPacket);
        realmListResponse.Read();

        if (0 >= realmListResponse.Realms.Count)
        {
            Debug.Log("无法获取游戏服务器列表");
            UIManager.Instance.ShowMessage(PanelMessage.MessageType.Confirm, "获取服务器列表错误", "无法获取游戏服务器列表");
            return;
        }
        UIManager.Instance.ShowServerList(realmListResponse.Realms);
    }

    public bool ConnectToRealm(WorldServerInfo realm)
    {
        //bool b = await this.gameClient.ConnectToRealm(realm);
        //if (b)
        //{
        //    //登录游戏服务器成功
        //    GetCharacters();
        //}
        //else
        //{
        //    return false;
        //}

        return true;
    }

    public bool CharacterCreate(string name)
    {
        //bool b = await this.gameClient.CharacterCreate(name);
        //if (b)
        //{
        //    GetCharacters();
        //}
        //else
        //{
        //    UIManager.Instance.ShowMessage(PanelMessage.MessageType.Confirm, "创建角色失败", "创建角色失败");
        //}
        return true;
    }

    public bool CharacterDelete(ulong GUID)
    {
        //bool b = await this.gameClient.CharacterDelete(GUID);
        //if (b)
        //{
        //    GetCharacters();
        //}
        //else
        //{
        //    UIManager.Instance.ShowMessage(PanelMessage.MessageType.Confirm, "删除角色失败", "删除角色失败");
        //}
        return true;
    }
}
