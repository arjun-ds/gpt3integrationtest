using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using OpenAI_API;
using OpenAI_API.Chat;
using System;
using OpenAI_API.Models;

public class OpenAIController : MonoBehaviour
{
    public TMP_Text textField;
    public TMP_InputField inputField;
    public Button okButton;

    private OpenAIAPI api;
    private List<ChatMessage> messages;

    // Start is called before the first frame update
    void Start()
    {
        api = new OpenAIAPI(Environment.GetEnvironmentVariable("OPENAI_API_KEY", EnvironmentVariableTarget.User));

        StartConversation();
        okButton.onClick.AddListener(() => GetResponse());
    }

    private void StartConversation()
    {
        messages = new List<ChatMessage> {
            new ChatMessage(ChatMessageRole.System, "Respond like Donald Trump. Keep your responses short and to the point.")
        };

        inputField.text = ""; //clears input field text
        string startString = "Donald Trump appears before you. What would you like to say?"; //starter prompt for user
        textField.text = startString;
        Debug.Log(startString);
    }

    private async void GetResponse()
    {
        if (inputField.text.Length < 1)
        {
            return; //so that user doesnt submit empty message
        }

        // Disable the OK Button just to make sure
        okButton.enabled = false;

        // Fill user message from input field on okButton click
        ChatMessage userMessage = new ChatMessage();
        userMessage.Role = ChatMessageRole.User;
        userMessage.Content = inputField.text;

        if (userMessage.Content.Length > 100) //preserve token length & prevents accidentally sending really long message
        {
            //Limit messages to 100 characters
            userMessage.Content = userMessage.Content.Substring(0, 100);
        }
        Debug.Log(string.Format("{0}: {1}", userMessage.rawRole, userMessage.Content));

        // Add message to the list of messages that were sent back and forth
        messages.Add(userMessage);

        // Update text field with user message
        textField.text = string.Format("You: {0}", userMessage.Content); // updates upper text box to say inputted message while sending message to gpt and waiting for response

        // Clears input field
        inputField.text = "";

        // Send entire chat to OpenAI to get next message:
        var chatResult = await api.Chat.CreateChatCompletionAsync(new ChatRequest() // async chat completion request to openai servers 
        { 
            Model = Model.ChatGPTTurbo, // what model we use
            Temperature = 0.1, //temperature (between 0 and 1) -- how creative responses are / how incorrect willing to be. More towards 1 = wildly wrong but creative
            MaxTokens = 50,
            Messages = messages  // list of messages. gives model entire script up until this point 
        });

        // Get response message
        ChatMessage responseMessage = new ChatMessage();
        responseMessage.Role = chatResult.Choices[0].Message.Role;
        responseMessage.Content = chatResult.Choices[0].Message.Content;
        Debug.Log(string.Format("{0}: {1}", responseMessage.rawRole, responseMessage.Content));

        //Add response to list of messages
        messages.Add(responseMessage);

        // Update text field with response
        textField.text = string.Format("You: {0}\n\nTrump: {1}", userMessage.Content, responseMessage.Content);

        // Re-enable OK Button
        okButton.enabled = true;
    }
}