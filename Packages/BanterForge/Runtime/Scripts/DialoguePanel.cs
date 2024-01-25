using System;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace GDPanda.BanterForge
{
    public class DialoguePanel : MonoBehaviour
    {
        [Header("Variables")]
        [SerializeField]
        private float _timeToDestroy;

        //Prefab for choice buttons
        public DialogueChoiceButton ChoiceButtonPrefab;
        
        //Default font for dialogue text and name
        [SerializeField]
        private TMP_FontAsset _defaultFont;
        
        //Default dialogue text color
        [SerializeField]
        private Color _defaultColor;
        
        [Header("References")]
        //Parent transform to spawn dialogue choices
        public Transform ChoicesParent;
        
        //TMP component for name text
        [SerializeField]
        private TMP_Animated _nameText;

        //TMP component for dialogue text
        [SerializeField]
        private TMP_Animated _dialogueText;

        //Image components for dialogue image and its shadow (should be colored black)
        [SerializeField]
        private Image _image, _imageShadow;
        
        //Image that shows when dialogue can be interacted with/continued
        [SerializeField]
        private GameObject _interactImage;
        
        private Animator _animator;
        private Animator _currentDialogueAnimator;

        private TMP_Animated _dialogueTextAnimated;
        
        private static readonly int DialogueActive = Animator.StringToHash("DialogueActive");
        
        #region Callbacks

        public static Action OnEndOfDialogueLineCallback;
        public static Action<char> OnCharacterRevealCallback;

        #endregion
        
        public void OnEnable()
        {
            _animator = GetComponentInChildren<Animator>();
            
            _nameText.SetText(string.Empty);
            _dialogueText.SetText(string.Empty);
            _defaultColor = _dialogueText.color;
            _dialogueText.onTextReveal.AddListener(OnCharReveal);
            _dialogueText.onDialogueFinish.AddListener(OnEndOfDialogueLine);
            _dialogueText.onAction.AddListener(DialogueActions.OnDialogueAction);
            
            DialogueManager.OnEmotionChange -= OnEmotionChange;
            DialogueManager.OnEmotionChange += OnEmotionChange;
        }

        private void OnDisable()
        {
            DialogueManager.OnEmotionChange -= OnEmotionChange;
        }

        public void SetupDialoguePanel()
        {
            _animator.SetBool(DialogueActive, true);
            
            SetNameText(string.Empty);
            SetDialogueText(string.Empty);
            
            SetNameFont();
            SetDialogueFont();
            
            SetInteractImageActive(false);

            //TODO: Hook up with current Char anim
            // _image.sprite = _lines.Peek().Character.GetSpriteFromEmotion(Emotion.normal);
            // _imageShadow.sprite = _lines.Peek().Character.GetSpriteFromEmotion(Emotion.normal);
        }

        public void EndDialoguePanel()
        {
            Invoke(nameof(DestroyPanel), _timeToDestroy);
            _animator.SetBool(DialogueActive, false);
            
            SetNameText(string.Empty);
            SetDialogueText(string.Empty);
            
            SetInteractImageActive(false);
        }

        private void DestroyPanel()
        {
            DialogueManager.GetInstance().CanContinue = true;
            Destroy(gameObject);
        }
        
        public void SetInteractImageActive(bool isActive)
        {
            _interactImage.SetActive(isActive);
        }

        #region TMP Variable setters (Fonts, colors, text etc)

        public void SetNameFont()
        {
            _nameText.font = _defaultFont;
            _nameText.UpdateFontAsset();
        }

        public void SetNameFont(TMP_FontAsset font)
        {
            if (!font)
            {
                SetNameFont();
                return;
            }
            
            _nameText.font = font;
            _nameText.UpdateFontAsset();
        }

        public void SetNameColor(Color color)
        {
            var colorToSet = color == Color.clear ? _defaultColor : color;
            _nameText.color = colorToSet;
        }
        
        public void SetNameText(string text)
        {
            _nameText.SetText(text);
        }
        
        public void SetDialogueFont()
        {
            _dialogueText.font = _defaultFont;
            _dialogueText.UpdateFontAsset();
        }
        
        public void SetDialogueFont(TMP_FontAsset font)
        {
            if (!font)
            {
                SetDialogueFont();
                return;
            }
            
            _dialogueText.font = font;
            _dialogueText.UpdateFontAsset();
        }

        public void SetDialogueColor(Color color)
        {
            var colorToSet = color == Color.clear ? _defaultColor : color;
            _dialogueText.color = colorToSet;
        }
        
        public void SetDialogueText(string text)
        {
            if (text == string.Empty)
            {
                _dialogueText.SetText(text);
                return;
            }
            _dialogueText.PrepareText(text);
            // _dialogueText.SetText(text);
        }
        
        #endregion

        #region TMP Animated methods

        public void ResetDialogueSpeed()
        {
            _dialogueText.ResetSpeed();
        }
        
        public void SetDialogueTextSpeed(int speed)
        {
            _dialogueText.SetOtherSpeed(speed);
        }

        #endregion
        
        public void OnEmotionChange(Emotion emotion)
        {
            ChangeSprite(emotion);

            var emotionString = emotion.ToString();
            
            emotionString = char.ToUpper(emotionString[0]) + emotionString.Substring(1);
            
            _animator.SetTrigger(emotionString);
        }

        private void ChangeSprite(Emotion emotion)
        {
            var currentSpeaker = DialogueManager.GetInstance().CurrentSpeaker;
            if (!currentSpeaker)
                return;
            
            var animationNameRef = currentSpeaker.GetSpriteFromEmotion(emotion);
            // Debug.Log($"{currentSpeaker} changed mood to {emotion}");
            
            //TODO: Hook up with current Char anim
            // _image.sprite = sprite;
            // _imageShadow.sprite = sprite;
        }
        
        #region Callbacks from this component

        private void OnCharReveal(char revealedCharacter)
        {
            revealedCharacter = char.ToLower(revealedCharacter);
            
            OnCharacterRevealCallback?.Invoke(revealedCharacter);
        }

        private void OnEndOfDialogueLine()
        {
            OnEndOfDialogueLineCallback?.Invoke();
        }

        #endregion
    }
}