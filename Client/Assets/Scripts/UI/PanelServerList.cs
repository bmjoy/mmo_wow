using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PanelServerList : MonoBehaviour
{
    [SerializeField]
    private ToggleGroup toggleGroup = null;
    [SerializeField]
    private ServerItem serverItemPrefab = null;
    [SerializeField]
    private Button buttonLogin = null;

    private List<ServerItem> serverItems = new List<ServerItem>();

    private uint currentID = 1;
    private List<WorldServerInfo> worldServerInfos = null;

    private void Start()
    {
        this.buttonLogin.onClick.AddListener(OnButtonLogin);
    }

    private void OnButtonLogin()
    {
        WorldServerInfo worldServerInfo = null;
        foreach(var info in this.worldServerInfos)
        {
            if(info.Id == this.currentID)
            {
                worldServerInfo = info;
                break;
            }
        }
        UIManager.Instance.ShowLoading(true);

        NetworkManager.Instance.ConnectToRealm(worldServerInfo);
    }

    public void OnSelectID(uint id)
    {
        this.currentID = id;
    }

    public void ShowServerList(List<WorldServerInfo> worldServerInfos)
    {
        if(0 < serverItems.Count)
        {
            for(int i = 0; i < serverItems.Count; ++i)
            {
                GameObject.Destroy(serverItems[i].gameObject);
            }
            serverItems.Clear();
        }

        this.worldServerInfos = worldServerInfos;

        for (int i = 0; i < worldServerInfos.Count; ++i)
        {
            ServerItem serverItem = GameObject.Instantiate<ServerItem>(this.serverItemPrefab);
            serverItem.gameObject.SetActive(true);
            serverItem.Init(worldServerInfos[i].Id, worldServerInfos[i].Name, worldServerInfos[i].Load.ToString(), worldServerInfos[i].Load.ToString(), this);
            serverItem.ToggleGroup(this.toggleGroup);
            serverItem.transform.SetParent(this.toggleGroup.transform,false);
            this.serverItems.Add(serverItem);
        }
    }
}
