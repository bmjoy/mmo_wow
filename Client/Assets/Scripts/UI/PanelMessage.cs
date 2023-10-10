using UnityEngine;
using UnityEngine.UI;

public class PanelMessage : MonoBehaviour
{
    [SerializeField]
    private Text textTitle = null;
    [SerializeField]
    private Text textContent = null;
    [SerializeField]
    private Button buttonConfirm = null;
    [SerializeField]
    private Button buttonCancel = null;

    public enum MessageType
    {
        Confirm,
        Cancel,
        Both,
    }

    public class Message
    {
        public string title;
        public string content;
        public MessageType messageType;
    }

    private void Start()
    {
        this.buttonConfirm.onClick.AddListener(this.OnButtonConfirm);
        this.buttonCancel.onClick.AddListener(this.OnButtonCancel);
    }


    public void ShowMessage(Message message)
    {
        this.textTitle.text = message.title;
        this.textContent.text = message.content;
        switch(message.messageType)
        {
            case MessageType.Confirm:
                {
                    this.buttonConfirm.gameObject.SetActive(true);
                    this.buttonCancel.gameObject.SetActive(false);
                }
                break;

            case MessageType.Cancel:
                {
                    this.buttonConfirm.gameObject.SetActive(false);
                    this.buttonCancel.gameObject.SetActive(true);
                }
                break;

            case MessageType.Both:
                {
                    this.buttonConfirm.gameObject.SetActive(true);
                    this.buttonCancel.gameObject.SetActive(true);
                }
                break;
        }
    }

    private void OnButtonConfirm()
    {
        this.gameObject.SetActive(false);
        UIManager.Instance.ShowLoading(false);
    }

    private void OnButtonCancel()
    {
        this.gameObject.SetActive(false);
        UIManager.Instance.ShowLoading(false);
    }
}
