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
   
    private Transform chatPanelTrans; //no se como hacer q funcione el scroll

    
   // [SerializeField]
   // private TMP_InputField text; 
    
    [SerializeField]
    private GameObject textObject;


    // Start is called before the first frame update
    void Start()
    {
        chatPanelTrans = chatPanel;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space ))
        {
            LogMessageToChat("amogas: "+ "sus");
        }
    }

    public void LogMessageToChat(string text) // usar esta funcion para llamarla desde otros scripts para que escriba en el chat
    {
        Message _newMessage = new Message();

        if (messageLog.Count >= maxMessages)
        {
            Destroy(messageLog[0].textObj.gameObject);
            messageLog.Remove(messageLog[0]);
        }

        _newMessage.text = text;

        GameObject _newTextObj = Instantiate(textObject, chatPanelTrans);

        _newMessage.textObj = _newTextObj.GetComponent<TMP_Text>();

        _newMessage.textObj.text = _newMessage.text;

        messageLog.Add(_newMessage);
    }
}

[System.Serializable]
public class Message
{

    public string text;
    public TMP_Text textObj;
}