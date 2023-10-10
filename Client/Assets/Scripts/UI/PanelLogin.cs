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
            UIManager.Instance.ShowMessage(PanelMessage.MessageType.Confirm, "用户名为空", "请输入用户名");
            return;
        }

        if(string.IsNullOrEmpty(inputFieldPassword.text))
        {
            UIManager.Instance.ShowMessage(PanelMessage.MessageType.Confirm, "密码为空", "请输入密码");
            return;
        }

        UIManager.Instance.Login(inputFieldAccount.text, inputFieldPassword.text);      
    }
}
