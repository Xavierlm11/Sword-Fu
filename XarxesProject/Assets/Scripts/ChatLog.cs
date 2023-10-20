using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text;
using TMPro;

public class ChatLog : MonoBehaviour
{
    //lo suyo seria q para comunicarse el server su nickname default fuera server
    // y en la pestaña de server se viera lo nicknames de la gente cnectada

    [SerializeField]
    List<Message> messageLog = new List<Message>();

    [SerializeField]
    private int maxMessages = 30; // a parrtir de 30 mensaje en el chat se empieza a borrar el mensaje mas antiguo

    [SerializeField]
    private Transform chatPanel; //no se como hacer q funcione el scroll
   
    private Transform chatPanelTransform; //no se como hacer q funcione el scroll

    
   // [SerializeField]
   // private TMP_InputField text; 
    
    [SerializeField]
    private GameObject textObject;

    void Start()
    {
        chatPanelTransform = chatPanel;
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            LogMessageToChat("amogas","sus");
        }
    }

    public void LogMessageToChat(string nick, string text)
    {
        Message newMessage = new Message();

        if (messageLog.Count >= maxMessages)
        {
            Destroy(messageLog[0].uiText.gameObject);
            messageLog.Remove(messageLog[0]);
        }

        newMessage.nickname = nick;
        newMessage.messageText = text;

        GameObject newTextObj = Instantiate(textObject, chatPanelTransform);

        newMessage.uiText = newTextObj.GetComponent<TMP_Text>();

        newMessage.uiText.text = newMessage.nickname + ": " + newMessage.messageText;

        messageLog.Add(newMessage);
    }
}

[System.Serializable]
public class Message
{
    public string nickname;
    public string messageText;
    public TMP_Text uiText;
}