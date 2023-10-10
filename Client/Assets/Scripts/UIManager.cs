using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoSingleton<UIManager>
{
    [SerializeField]
    private PanelMessage panelMessage = null;
    [SerializeField]
    private PanelLogin panelLogin = null;
    [SerializeField]
    private PanelLoading panelLoading = null;
    [SerializeField]
    private PanelServerList panelServerList = null;
    [SerializeField]
    private PanelCharacterList panelCharacterList = null;
    [SerializeField]
    private PanelCharacterCreate panelCharacterCreate = null;

    private Queue<PanelMessage.Message> messages = new Queue<PanelMessage.Message>();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(null != panelMessage && !panelMessage.gameObject.activeSelf)
        {
            if(0 < messages.Count)
            {
                PanelMessage.Message message = messages.Dequeue();
                this.panelMessage.gameObject.SetActive(true);
                this.panelMessage.ShowMessage(message);
            }
        }
    }

    public void ShowMessage(PanelMessage.MessageType _messageType, string _title, string _content)
    {
        PanelMessage.Message message = new PanelMessage.Message
        {
            messageType = _messageType,
            title = _title,
            content = _content
        };
        this.messages.Enqueue(message);
    }

    public void ShowLoading(bool show)
    {
        this.panelLoading.gameObject.SetActive(show);
    }

    public void Login(string account, string password)
    {
        NetworkManager.Instance.Login(account, password);

        UIManager.Instance.ShowLoading(true);
    }

    public void ShowServerList(List<WorldServerInfo> worldServerInfos)
    {
        this.ShowLoading(false);
        this.panelLogin.gameObject.SetActive(false);
        this.panelServerList.gameObject.SetActive(true);
        this.panelServerList.ShowServerList(worldServerInfos);
    }

    public void ShowCharacterList(List<Character> characters)
    {
        this.ShowLoading(false);
        this.panelServerList.gameObject.SetActive(false);
        this.panelCharacterList.gameObject.SetActive(true);
        this.panelCharacterList.ShowCharacterList(characters);
    }

    public void ShowCharacterCreate()
    {
        this.panelCharacterList.gameObject.SetActive(false);
        this.panelCharacterCreate.gameObject.SetActive(true);
    }

    public void CharacterCreate(string name)
    {
        this.ShowLoading(true);
        NetworkManager.Instance.CharacterCreate(name);
    }

    public void CharacterDelete(ulong GUID)
    {
        this.ShowLoading(true);
        NetworkManager.Instance.CharacterDelete(GUID);
    }
}
