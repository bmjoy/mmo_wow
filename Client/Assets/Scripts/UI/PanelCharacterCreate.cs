using UnityEngine;
using UnityEngine.UI;

public class PanelCharacterCreate : MonoBehaviour
{
    [SerializeField]
    private InputField inputFieldName = null;
    [SerializeField]
    private Button buttonConfirm = null;
    [SerializeField]
    private Button buttonCancel = null;

    private void Start()
    {
        this.buttonConfirm.onClick.AddListener(this.OnButtonConfirm);
        this.buttonCancel.onClick.AddListener(this.OnButtonCancel);
    }

    private void OnButtonConfirm()
    {
        if(string.IsNullOrEmpty(inputFieldName.text))
        {
            UIManager.Instance.ShowMessage(PanelMessage.MessageType.Confirm, "角色名为空", "请输入角色名");
        }

        UIManager.Instance.CharacterCreate(inputFieldName.text);
    }

    private void OnButtonCancel()
    {

    }
}
