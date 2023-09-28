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

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager Instance = null;
    public NetCoreServer.TcpClient tcpClient = null;

    private string account;
    private byte[] passwordHash;
    private byte[] M2;
    public BigInteger SessionKey { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        tcpClient = new NetCoreServer.TcpClient("127.0.0.1", 3724);
        tcpClient.OnClientConnected += OnClientConnected;
        tcpClient.OnClientDataReceived += OnClientDataReceived;
        tcpClient.OnClientDisconnected += OnClientDisconnected;
        tcpClient.OnClientError += OnClientError;

        tcpClient.ConnectAsync();
    }

    public void OnClientConnected()
    {
        Debug.Log("OnClientConnected");
    }

    public void OnClientDataReceived(byte[] buffer, long offset, long size)
    {
        if (size > 0)
        {
            AuthCmd clientOpcodes = (AuthCmd)buffer[0];
            switch (clientOpcodes)
            {
                case AuthCmd.AUTH_LOGON_CHALLENGE:
                    {
                        OnLoginResponse(buffer, offset, size);
                    }
                    break;
                case AuthCmd.AUTH_LOGON_PROOF:
                    {
                        OnLogonProofResponse(buffer, offset, size);
                    }
                    break;
                case AuthCmd.REALM_LIST:
                    {
                        OnRealmListResponse(buffer, offset, size);
                    }
                    break;
            }
        }
        Debug.Log("OnClientDataReceived");
    }

    public void OnClientError(SocketError error)
    {
        Debug.LogFormat("OnClientError:{0}", error.ToString());
    }

    public void OnClientDisconnected()
    {
        Debug.Log("OnClientDisconnected");
    }

    public void OnApplicationQuit()
    {
        if (null != tcpClient)
        {
            tcpClient.DisconnectAsync();
            tcpClient.OnClientConnected += OnClientConnected;
            tcpClient.OnClientDataReceived += OnClientDataReceived;
            tcpClient.OnClientDisconnected += OnClientDisconnected;
            tcpClient.OnClientError += OnClientError;
        }
    }

    public void OnLogin(string account, string password)
    {
        Debug.Log("OnLogin");
        this.account = account.ToUpper();
        string hashStr = string.Format("{0}:{1}", this.account, password);
        passwordHash = Network.Security.HashAlgorithm.SHA1.Hash(Encoding.ASCII.GetBytes(hashStr.ToUpper()));

        AuthLogonChallengeRequest authLogonChallengeRequest = new AuthLogonChallengeRequest(this.account);
        authLogonChallengeRequest.WritePacketData();
        tcpClient.SendAsync(authLogonChallengeRequest.GetData());
    }

    public void OnLoginResponse(byte[] buffer, long offset, long size)
    {
        Debug.Log("OnLoginResponse");
        byte[] data = new byte[size];
        Array.Copy(buffer, data, size);
        InPacket inPacket = new InPacket(data);
        AuthLogonChallengeResponse authLogonChallengeResponse = new AuthLogonChallengeResponse(inPacket);
        authLogonChallengeResponse.Read();
        if (authLogonChallengeResponse.error != AuthResult.WOW_SUCCESS)
        {
            Debug.LogFormat("OnLoginResponse:{0}", authLogonChallengeResponse.error.ToString());
        }

        BigInteger x = Network.Security.HashAlgorithm.SHA1.Hash(authLogonChallengeResponse.salt.ToCleanByteArray(), this.passwordHash).ToBigInteger();

        RandomNumberGenerator rand = RandomNumberGenerator.Create();

        BigInteger A;
        BigInteger a;

        do
        {
            byte[] randBytes = new byte[19];
            rand.GetBytes(randBytes);
            a = randBytes.ToBigInteger();

            A = authLogonChallengeResponse.g.ModPow(a, authLogonChallengeResponse.n);
        } while (A.ModPow(1, authLogonChallengeResponse.n) == 0);

        BigInteger u = Network.Security.HashAlgorithm.SHA1.Hash(A.ToCleanByteArray(), authLogonChallengeResponse.b.ToCleanByteArray()).ToBigInteger();
        BigInteger k = new BigInteger(3);
        BigInteger S = (authLogonChallengeResponse.b - k * authLogonChallengeResponse.g.ModPow(x, authLogonChallengeResponse.n)).ModPow(a + u * x, authLogonChallengeResponse.n);

        byte[] keyHash;
        byte[] sData = S.ToCleanByteArray();
        byte[] keyData = new byte[40];
        byte[] temp = new byte[16];

        // take every even indices byte, hash, store in even indices
        for (int i = 0; i < 16; ++i)
            temp[i] = sData[i * 2];
        keyHash = Network.Security.HashAlgorithm.SHA1.Hash(temp);
        for (int i = 0; i < 20; ++i)
            keyData[i * 2] = keyHash[i];

        // do the same for odd indices
        for (int i = 0; i < 16; ++i)
            temp[i] = sData[i * 2 + 1];
        keyHash = Network.Security.HashAlgorithm.SHA1.Hash(temp);
        for (int i = 0; i < 20; ++i)
            keyData[i * 2 + 1] = keyHash[i];

        SessionKey = keyData.ToBigInteger();

        // XOR the hashes of N and g together
        byte[] gNHash = new byte[20];

        byte[] nHash = Network.Security.HashAlgorithm.SHA1.Hash(authLogonChallengeResponse.n.ToCleanByteArray());
        for (int i = 0; i < 20; ++i)
            gNHash[i] = nHash[i];

        byte[] gHash = Network.Security.HashAlgorithm.SHA1.Hash(authLogonChallengeResponse.g.ToCleanByteArray());
        for (int i = 0; i < 20; ++i)
            gNHash[i] ^= gHash[i];

        // hash username
        byte[] userHash = Network.Security.HashAlgorithm.SHA1.Hash(Encoding.ASCII.GetBytes(account));

        // our proof
        byte[] m1Hash = Network.Security.HashAlgorithm.SHA1.Hash
        (
            gNHash,
            userHash,
            authLogonChallengeResponse.salt.ToCleanByteArray(),
            A.ToCleanByteArray(),
            authLogonChallengeResponse.b.ToCleanByteArray(),
            SessionKey.ToCleanByteArray()
        );

        M2 = Network.Security.HashAlgorithm.SHA1.Hash(A.ToCleanByteArray(), m1Hash, keyData);

        this.OnAuthLogonProofRequest(A, m1Hash);
    }

    public void OnAuthLogonProofRequest(BigInteger a, byte[] hash)
    {
        Debug.Log("OnAuthLogonProofRequest");
        AuthLogonProofRequest authLogonProofRequest = new AuthLogonProofRequest(a, hash);
        authLogonProofRequest.WritePacketData();
        tcpClient.SendAsync(authLogonProofRequest.GetData());
    }

    public void OnLogonProofResponse(byte[] buffer, long offset, long size)
    {
        Debug.Log("OnLogonProofResponse");
        byte[] data = new byte[size];
        Array.Copy(buffer, data, size);
        InPacket inPacket = new InPacket(data);

        AuthLogonProofResponse authLogonProofResponse = new AuthLogonProofResponse(inPacket);
        authLogonProofResponse.Read();

        bool isSame = authLogonProofResponse.m2 != null && authLogonProofResponse.m2.Length == 20;
        for (int i = 0; i < authLogonProofResponse.m2.Length && isSame; ++i)
        {
            if (!(isSame = authLogonProofResponse.m2[i] == M2[i]))
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
        }
    }

    public void OnRealmListRequest()
    {
        Debug.Log("OnRealmListRequest");
        RealmListRequest realmListRequest = new RealmListRequest();
        realmListRequest.WritePacketData();
        tcpClient.SendAsync(realmListRequest.GetData());
    }

    public void OnRealmListResponse(byte[] buffer, long offset, long size)
    {
        Debug.Log("OnRealmListResponse");
        byte[] data = new byte[size];
        Array.Copy(buffer, data, size);
        InPacket inPacket = new InPacket(data);

        RealmListResponse realmListResponse = new RealmListResponse(inPacket);
        realmListResponse.Read();

        if(0 < realmListResponse.Realms.Count)
        {

        }
    }
}
