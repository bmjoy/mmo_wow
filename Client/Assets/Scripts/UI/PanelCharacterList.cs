using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PanelCharacterList : MonoBehaviour
{
    [SerializeField]
    private CharacterItem characterItemPrefab = null;
    [SerializeField]
    private ToggleGroup toggleGroup = null;

    [SerializeField]
    private Button buttonCreate = null;
    [SerializeField]
    private Button buttonDelete = null;
    [SerializeField]
    private Button buttonLogin = null;

    private List<CharacterItem> characterItems = new List<CharacterItem>();

    private ulong GUID = 0;

    private void Start()
    {
        this.buttonCreate.onClick.AddListener(this.OnButtonCreate);
        this.buttonDelete.onClick.AddListener(this.OnButtonDelete);
        this.buttonLogin.onClick.AddListener(this.OnButtonLogin);
    }

    public void OnSelectID(ulong GUID)
    {
        this.GUID = GUID;
    }

    private void OnButtonCreate()
    {
        UIManager.Instance.ShowCharacterCreate();
    }

    private void OnButtonDelete()
    {
        UIManager.Instance.CharacterDelete(this.GUID);
    }

    private void OnButtonLogin()
    {

    }


    public void ShowCharacterList(List<Character> characters)
    {
        if(0 < characterItems.Count)
        {
            for(int i = 0; i < characterItems.Count; ++i)
            {
                GameObject.Destroy(characterItems[i].gameObject);
            }

            characterItems.Clear();
        }

        for(int i = 0; i < characters.Count; ++i)
        {
            CharacterItem characterItem = GameObject.Instantiate<CharacterItem>(this.characterItemPrefab);
            characterItem.gameObject.SetActive(true);
            characterItem.Init(characters[i].GUID, characters[i].Name, characters[i].Level.ToString(), this);
            characterItem.ToggleGroup(this.toggleGroup);
            characterItem.transform.SetParent(this.toggleGroup.transform, false);
            if(0 == i)
            {
                this.GUID = characters[i].GUID;
                characterItem.GetComponent<Toggle>().isOn = true;
            }
            this.characterItems.Add(characterItem);
        }
    }
}
