using UnityEngine;
using UnityEngine.UI;

public class CharacterItem : MonoBehaviour
{
    [SerializeField]
    private Text textName = null;
    [SerializeField]
    private Text textLevel = null;
    [SerializeField]
    private Toggle toggle = null;

    private ulong id = 0;
    private PanelCharacterList panelCharacterList = null;

    private void Start()
    {
        this.toggle.onValueChanged.AddListener(this.OnToggle);
    }

    private void OnToggle(bool selected)
    {
        if(selected)
        {
            this.panelCharacterList.OnSelectID(this.id);
        }
    }

    public void Init(ulong id, string name, string level,PanelCharacterList panelCharacterList)
    {
        this.id = id;
        this.textName.text = name;
        this.textLevel.text = level;
        this.panelCharacterList = panelCharacterList;
    }


    public void ToggleGroup(ToggleGroup toggleGroup)
    {
        this.toggle.group = toggleGroup;
    }
}
