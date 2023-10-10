using UnityEngine;
using UnityEngine.UI;


public class PanelLogin : MonoBehaviour
{
    [SerializeField]
    private Button buttonLogin = null;
    [SerializeField]
    private InputField inputFieldAccount = null;
    [SerializeField]
    private InputField inputFieldPassword = null;

    private void Start()
    {
        this.buttonLogin.onClick.AddListener(this.OnButtonLogin);
    }

    private void OnButtonLogin()
    {
        if(null == inputFieldAccount || null == inputFieldPassword)
        {
            Debug.LogError("need bind input field!");
        }

        if(string.IsNullOrEmpty(inputFieldAccount.text))
        {
            UIManager.Instance.ShowMessage(PanelMessage.MessageType.Confirm, "�û���Ϊ��", "�������û���");
            return;
        }

        if(string.IsNullOrEmpty(inputFieldPassword.text))
        {
            UIManager.Instance.ShowMessage(PanelMessage.MessageType.Confirm, "����Ϊ��", "����������");
            return;
        }

        UIManager.Instance.Login(inputFieldAccount.text, inputFieldPassword.text);      
    }
}
