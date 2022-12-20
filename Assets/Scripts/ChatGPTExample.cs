using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class ChatGPTExample : MonoBehaviour {
    // Replace with your own OpenAI API key
    private string API_KEY = "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";

    // The text completion API endpoint
    private string API_ENDPOINT = "https://api.openai.com/v1/completions";

    // The text prompt for the ChatGPT model
    private string prompt = "Hello, how are you today?";

    void Start() {
        // Set up the request payload
        var request = new UnityWebRequest(API_ENDPOINT, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes("{\"prompt\": \"" + prompt + "\", \"model\": \"text-davinci-002\"}");
        request.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + API_KEY);

        // Send the request and log the response
        StartCoroutine(SendRequest(request));
    }

    IEnumerator SendRequest(UnityWebRequest request) {
        yield return request.SendWebRequest();

        if (request.isNetworkError || request.isHttpError) {
            Debug.LogError(request.error);
        }
        else {
            string response = request.downloadHandler.text;
            Debug.Log("Response: " + response);

            // Parse the response to get the text answer from the ChatGPT model
            string textAnswer = ParseResponse(response);
            Debug.Log("Text answer: " + textAnswer);
        }
    }

    string ParseResponse(string response) {
        // Add code here to parse the response and extract the text answer from the ChatGPT model
        // You can use a JSON library such as Newtonsoft.Json to parse the response
        return "";
    }
}