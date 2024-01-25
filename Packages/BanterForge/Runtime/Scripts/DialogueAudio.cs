using System;
using TMPro;
using UnityEngine;

namespace GDPanda.BanterForge
{
    public class DialogueAudio : MonoBehaviour
    {
        private void Start()
        {
            DialoguePanel.OnCharacterRevealCallback -= CharacterRevealAudio;
            DialoguePanel.OnCharacterRevealCallback += CharacterRevealAudio;

            DialogueManager.OnEmotionChange -= OnEmotionChange;
            DialogueManager.OnEmotionChange += OnEmotionChange;
        }

        private void OnDisable()
        {
            DialoguePanel.OnCharacterRevealCallback -= CharacterRevealAudio;
            DialogueManager.OnEmotionChange -= OnEmotionChange;
        }

        private void OnEnable()
        {
            // Lookup();
        }

        private void CharacterRevealAudio(char character)
        {
            string charEvent = "event:/Dialogue/" + character;
            if (ContainsSpecialLetter(character))
                charEvent = "event:/Dialogue/space";
            
            // FMODUnity.RuntimeManager.PlayOneShot(charEvent);
        }
        
        private bool ContainsSpecialLetter(char letter)
        {
            return letter is ' ' or ',' or '=' or '*' or '!' or ':' or ')' or '.';
        }
        
        private void OnEmotionChange(Emotion emotion)
        {
            var currentSpeaker = DialogueManager.GetInstance().CurrentSpeaker;
            if (!currentSpeaker)
                return;
            
            // FMODUnity.RuntimeManager.StudioSystem.setParameterByName("Character", currentSpeaker.VoiceType);
            
            var emotionString = emotion.ToString();
            
            emotionString = char.ToUpper(emotionString[0]) + emotionString.Substring(1);
            
            if (emotionString.Contains("Suprised"))
                emotionString = "Surprised";
            
            /*string emotionEvent = "event:/SetMood/Set" + emotionString;
            FMODUnity.RuntimeManager.PlayOneShot(emotionEvent);*/
        }
        
        /*private FMOD.RESULT Lookup()
       {
           FMOD.RESULT result = RuntimeManager.StudioSystem.getParameterDescriptionByName(_musStateParam, out _musStateParameter);
           return result;
       }*/
        
         /*[SerializeField] 
         private StudioEventEmitter _popupAudio, _exitAudio, _uiButtonAudio;

         public void EnterAudio()
         {
             _popupAudio.Play();
         }

         public void ExitAudio()
         {
             _exitAudio.Play();
         }

         public void UiButtonAudio()
         {
             _uiButtonAudio.Play();
         }*/
    }
}