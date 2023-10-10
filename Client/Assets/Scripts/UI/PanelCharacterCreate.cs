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
            UIManager.Instance.ShowMessage(PanelMessage.MessageType.Confirm, "��ɫ��Ϊ��", "�������ɫ��");
        }

        UIManager.Instance.CharacterCreate(inputFieldName.text);
    }

    private void OnButtonCancel()
    {

    }
}
