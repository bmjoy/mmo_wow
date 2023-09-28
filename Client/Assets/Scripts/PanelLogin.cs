using UnityEngine;
using UnityEngine.UI;

public class PanelLogin : MonoBehaviour
{
    public InputField inputFieldAccount = null;
    public InputField inputFieldPassword = null;
    public Button buttonLogin = null;

    // Start is called before the first frame update
    void Start()
    {
        buttonLogin.onClick.AddListener(OnButtonLogin);
    }

    private void OnButtonLogin()
    {
        if(string.IsNullOrEmpty(inputFieldAccount.text))
        {
            Debug.Log("�û���Ϊ��");
            return;
        }
        if(string.IsNullOrEmpty(inputFieldPassword.text))
        {
            Debug.Log("����Ϊ��");
            return;
        }

        NetworkManager.Instance.OnLogin(inputFieldAccount.text, inputFieldPassword.text);
    }
}
