using UnityEngine;
using UnityEngine.UI;

public class ServerItem : MonoBehaviour
{
    [SerializeField]
    private Text textName = null;
    [SerializeField]
    private Text textCharacter = null;
    [SerializeField]
    private Text textLoad = null;
    [SerializeField]
    private Toggle toggle = null;

    private PanelServerList panelServerList = null;

    private uint id = 0;

    private void Start()
    {
        this.toggle.onValueChanged.AddListener(this.OnToogleValue);
    }

    private void OnToogleValue(bool selected)
    {
        if(selected)
        {
            this.panelServerList.OnSelectID(this.id);
        }
    }

    public void Init(uint id, string name, string character, string load, PanelServerList panelServerList)
    {
        this.id = id;
        this.textName.text = name;
        this.textCharacter.text = character;
        this.textLoad.text = load;
        this.panelServerList = panelServerList;
    }

    public void ToggleGroup(ToggleGroup toggleGroup)
    {
        this.toggle.group = toggleGroup;
    }
}
