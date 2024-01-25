using System.Collections;
using System.Linq;
using GDPanda.BanterForge;
using UnityEngine;
using UnityEngine.Events;

namespace TMPro
{
    public enum Emotion
    {
        happy,
        sad,
        suprised,
        angry,
        normal
    };

    [System.Serializable]
    public class EmotionEvent : UnityEvent<Emotion>
    {
    }

    [System.Serializable]
    public class ActionEvent : UnityEvent<string>
    {
    }

    [System.Serializable]
    public class TextRevealEvent : UnityEvent<char>
    {
    }

    [System.Serializable]
    public class DialogueEvent : UnityEvent
    {
    }

    public class TMP_Animated : TextMeshProUGUI
    {
        [SerializeField] private float speed = 10;
        public EmotionEvent onEmotionChange;
        public ActionEvent onAction;

        //Use for audio stuff!
        public TextRevealEvent onTextReveal;
        public DialogueEvent onDialogueFinish;

        private DialogueManager _dialogueManager => DialogueManager.GetInstance();

        private bool _inSkip;
        private float _cachedSpeed;

        public void SetOtherSpeed(float value)
        {
            _cachedSpeed = speed;
            speed = value;
            _inSkip = true;
        }

        public void ResetSpeed()
        {
            if (_cachedSpeed < 1)
                _cachedSpeed = 15;

            speed = _cachedSpeed;
            _cachedSpeed = 15;
            _inSkip = false;
        }

        public void PrepareText(string newText)
        {
            var currentCharacter = _dialogueManager.CurrentSpeaker;
            var preparedText = string.Empty;

            // split the whole text into parts based off the <> tags 
            // even numbers in the array are text, odd numbers are tags
            string[] subTexts = newText.Split('<', '>');

            // textmeshpro still needs to parse its built-in tags, so we only include noncustom tags
            string displayText = "";
            for (int i = 0; i < subTexts.Length; i++)
            {
                if (i % 2 == 0)
                {
                    displayText += subTexts[i];
                }
                else
                {
                    if (ShouldInsertText(subTexts[i].Replace(" ", "")))
                    {
                        var subText = subTexts[i];
                        var variableName = subText.Split('=')[1];
                        var variableOutput = currentCharacter.GetVariable(variableName);

                        var textToReplace = variableOutput;
                        displayText += textToReplace;
                    }
                    else
                    {
                        var textToAdd = subTexts[i]/*.ToLower()*/;
                        displayText += "<" + textToAdd + ">";
                    }
                }
            }

            // send that string to textmeshpro and hide all of it, then start reading
            preparedText = displayText;

            ReadText(preparedText);
            // if (tag.StartsWith("charvar="))
            // {
            //     var variableName = tag.Split('=')[1];
            //     var variableOutput = currentCharacter.GetVariable(variableName);
            //     return variableOutput;
            // }
        }

        public void ReadText(string newText)
        {
            text = string.Empty;
            // split the whole text into parts based off the <> tags 
            // even numbers in the array are text, odd numbers are tags
            string[] subTexts = newText.Split('<', '>');

            // textmeshpro still needs to parse its built-in tags, so we only include noncustom tags
            string displayText = "";
            for (int i = 0; i < subTexts.Length; i++)
            {
                if (i % 2 == 0)
                    displayText += subTexts[i];
                else if (!IsCustomTag(subTexts[i].Replace(" ", "")))
                    displayText += $"<{subTexts[i]}>";
            }

            // send that string to textmeshpro and hide all of it, then start reading
            text = displayText;
            maxVisibleCharacters = 0;
            
            StartCoroutine(Read());

            IEnumerator Read()
            {
                int subCounter = 0;
                int visibleCounter = 0;
                while (subCounter < subTexts.Length)
                {
                    // if 
                    if (subCounter % 2 == 1)
                    {
                        yield return EvaluateTag(subTexts[subCounter].Replace(" ", ""));
                    }
                    else
                    {
                        while (visibleCounter < subTexts[subCounter].Length)
                        {
                            onTextReveal.Invoke(subTexts[subCounter][visibleCounter]);
                            visibleCounter++;
                            maxVisibleCharacters++;
                            yield return new WaitForSeconds(1f / speed);
                        }

                        visibleCounter = 0;
                    }

                    subCounter++;
                }
                
                yield return null;

                onDialogueFinish.Invoke();
            }
        }

        private bool IsCustomTag(string tag)
        {
            return tag.StartsWith("speed=") || tag.StartsWith("pause=") || tag.StartsWith("emotion=") ||
                   tag.StartsWith("action") || tag.StartsWith("charvar=") || tag.StartsWith("charvars=");
        }

        private bool ShouldInsertText(string tag)
        {
            return tag.StartsWith("charvar=") || tag.StartsWith("charvars=");
        }

        private WaitForSeconds EvaluateTag(string tag)
        {
            if (tag.Length <= 0)
                return null;

            if (tag.StartsWith("speed="))
            {
                speed = _inSkip ? float.Parse(tag.Split('=')[1]) : speed;
            }
            else if (tag.StartsWith("pause="))
            {
                if (!_inSkip)
                    return new WaitForSeconds(float.Parse(tag.Split('=')[1]));
            }
            else if (tag.StartsWith("emotion="))
            {
                onEmotionChange.Invoke((Emotion)System.Enum.Parse(typeof(Emotion), tag.Split('=')[1]));
            }
            else if (tag.StartsWith("action="))
            {
                onAction.Invoke(tag.Split('=')[1]);
            }

            return null;
        }
    }
}