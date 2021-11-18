using System.Collections;
using UnityEngine;

namespace VikingCrew.Tools.UI {
	public class ScriptedDialogueBehaviour : MonoBehaviour {
        [System.Serializable]
        public class DialogueLine {
            [Tooltip("The transform doing the speaking. This could be a mouth, head or character transform depending on your scene")]
            public Transform speaker;
            [Tooltip("Time to delay from the previous message in the array")]
            public float delay = 2;
            [Multiline, Tooltip("What to say")]
            public string line = "Hello World!";
            [Tooltip("How to say it")]
            public SpeechBubbleManager.SpeechbubbleType speechBubbleType = SpeechBubbleManager.SpeechbubbleType.NORMAL;
        }

        public DialogueLine[] script;
        public bool doRestartAtEnd = true;
        public float bubbleTimeToLive = 3f;
		// Use this for initialization
		private void Start () {
            StartCoroutine(FollowScript());
		}
		
        private IEnumerator FollowScript() {
            int index = 0;

            while (index < script.Length) {
                yield return new WaitForSeconds(script[index].delay);

                SpeechBubbleManager.Instance.AddSpeechBubble(script[index].speaker, script[index].line, script[index].speechBubbleType, bubbleTimeToLive, Color.white, Vector3.zero);
                index++;
                if (doRestartAtEnd && index == script.Length)
                    index = 0;
                
            }
        }
	}
} 