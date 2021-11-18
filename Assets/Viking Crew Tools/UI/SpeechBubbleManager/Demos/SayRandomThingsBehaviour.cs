using UnityEngine;
using SpeechBubbleManager = VikingCrew.Tools.UI.SpeechBubbleManager;

namespace VikingCrewDevelopment.Demos{
    public class SayRandomThingsBehaviour : MonoBehaviour {
        [Multiline]
        public string[] thingsToSay = new string[] { "Hello world" };
        [Header("Leave as null if you just want center of character to emit speechbubbles")]
        public Transform mouth;
        public float timeBetweenSpeak = 5f;
        public bool doTalkOnYourOwn = true;
        private float timeToNextSpeak;
        // Use this for initialization
        void Start() {
            timeToNextSpeak = timeBetweenSpeak;
        }

        // Update is called once per frame
        void Update() {
            timeToNextSpeak -= Time.deltaTime;

            if (doTalkOnYourOwn && timeToNextSpeak  <= 0 && thingsToSay.Length > 0)
                SaySomething();
        }

        public void SaySomething() {
            string message = thingsToSay[Random.Range(0, thingsToSay.Length)];
            SaySomething(message);
        }

        public void SaySomething(string message) {
            SaySomething(message, SpeechBubbleManager.Instance.GetRandomSpeechbubbleType());
        }

        public void SaySomething(string message, SpeechBubbleManager.SpeechbubbleType speechbubbleType) {
            if (mouth == null)
                SpeechBubbleManager.Instance.AddSpeechBubble(transform, message, speechbubbleType);
            else
                SpeechBubbleManager.Instance.AddSpeechBubble(mouth, message, speechbubbleType);

            timeToNextSpeak = timeBetweenSpeak;
        }
        
    }
}