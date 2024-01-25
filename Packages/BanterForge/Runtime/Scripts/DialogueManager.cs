using System;
using System.Collections.Generic;
using System.Linq;
using GDPanda.BanterForge.Tree;
// using FMODUnity;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace GDPanda.BanterForge
{
    public class DialogueManager : Data.Singleton<DialogueManager>
    {
        //The test button for continuing/starting dialogue
        public Button _buttonTest;

        [HideInInspector]
        public bool CanContinue;
        
        [SerializeField]
        private GameObject _dialoguePanelPrefab;

        [SerializeField]
        private Character _fallbackCharacter;
        
        private DialogueTreeRunner _currentTreeRunner;

        private List<GameObject> _instantiatedChoices = new();

        private DialoguePanel _instantiatedPanel;

        private bool _canSkip;
        private bool _choicesActive;
        private float _textDelay;
        
        private bool _autoNextLine = false;

        private Character _currentSpeaker;

        private bool _noInteractionDelay;

        public Character CurrentSpeaker => _currentSpeaker;
        public bool StartedDialogue => _currentTreeRunner && _instantiatedPanel;

        #region Callbacks

        public Action EndOfDialogue;
        
        public static Action<Emotion> OnEmotionChange;

        #endregion

        #region Debug

        [SerializeField]
        private bool _debugEnabled = false;

        public static void Log(string log, Object context = null)
        {
            var instance = DialogueManager._instance;
            if(instance && !instance._debugEnabled)
                return;
            
            context ??= instance;
            
            Debug.Log(log, context);
        }

        public static void LogError(string log, Object context = null)
        {
            var instance = DialogueManager._instance;
            if(!instance._debugEnabled)
                return;
            
            context ??= instance;
            
            Debug.LogError(log, context);
        }
        
        public static void LogWarning(string log, Object context = null)
        {
            var instance = DialogueManager._instance;
            if(!instance._debugEnabled)
                return;
            
            context ??= instance;
            
            Debug.LogWarning(log, context);
        }
        
        #endregion
        
        private void Start()
        {
            _canSkip = false;
            CanContinue = false;
            _buttonTest.GetComponentInChildren<TextMeshProUGUI>().text = "Start Dialogue";

            DialoguePanel.OnEndOfDialogueLineCallback -= OnEndOfDialogueLineCallback;
            DialoguePanel.OnEndOfDialogueLineCallback += OnEndOfDialogueLineCallback;
        }

        private void OnDisable()
        {
            if(_currentTreeRunner)
                _currentTreeRunner.ResetTree();
            
            DialoguePanel.OnEndOfDialogueLineCallback -= OnEndOfDialogueLineCallback;
        }

        public void DoDialogueRunner(DialogueTreeRunner runner)
        {
            if(_currentTreeRunner && _currentTreeRunner != runner)
                return;

            _currentTreeRunner = runner;
            
            if(!StartedDialogue)
                StartDialogue();
            
            if(!CanContinue || !_noInteractionDelay)
                return;

            Invoke(nameof(DelayedInteraction), .1f);

            if (_canSkip)
            {
                _instantiatedPanel.SetDialogueTextSpeed(100);
                return;
            }
            
            var node = _currentTreeRunner.ProgressTree();
            if (!node)
            {
                EndDialogue();
                return;
            }
            

            var dialogueNode = node as DialogueNode;
            if (dialogueNode)
            {
                DoDialogueLine(dialogueNode.dialogueContent);
                
                if (!dialogueNode.child)
                {
                    //Should show end dialogue things
                    _buttonTest.GetComponentInChildren<TextMeshProUGUI>().text = "End Dialogue";
                }
                else
                {
                    //Should show continue dialogue things
                    _buttonTest.GetComponentInChildren<TextMeshProUGUI>().text = "Continue Dialogue";
                }
                return;
            }

            var choiceNode = node as ChoiceNode; 
            if (choiceNode)
            {
                if(!_choicesActive)
                    SetChoiceParentActive(true, choiceNode.children);
                
                
                return;
            }
            
            _currentTreeRunner = null;
        }

        public void AnimationReady()
        {
            CanContinue = true;
            
            // ContinueDialogue();
            // DoLine(_lines.Peek());
            
            DoDialogueRunner(_currentTreeRunner);
            Invoke(nameof(DelayedInteraction), .1f);
        }
        
        private void StartDialogue()
        {
            if (!_instantiatedPanel)
                InstantiatePanel();
            
            _noInteractionDelay = false;
            
            SetChoiceParentActive(false, null);
            
            //TODO: Should send in specifics on animations etc
            _instantiatedPanel.SetupDialoguePanel();
            
            //TODO: If special font in dialogue, set after started dialogue
            // _instantiatedPanel.SetNameTextFont();
            // _instantiatedPanel.SetDialogueTextFont();
            
            _currentTreeRunner.ResetTree();
            
            OnEmotionChange?.Invoke(Emotion.normal);
        }
        
        private void DoDialogueLine(DialogueContent content)
        {
            if (!_instantiatedPanel)
            {
                StartDialogue();
            }
            
            _instantiatedPanel.SetInteractImageActive(false);
            _canSkip = true;

            var characterSpeaker = content.character;
            if (!characterSpeaker)
                characterSpeaker = _fallbackCharacter;
            
            var font = characterSpeaker.overrideFont;
            var color = characterSpeaker.overrideTextColor;
            
            //TODO: Also set name vars?
            _instantiatedPanel.SetDialogueFont(font);
            _instantiatedPanel.SetDialogueColor(color);
            
            _instantiatedPanel.ResetDialogueSpeed();

            var character = characterSpeaker;
            
            var nameText = character.Name;
            
            var dialogueText = content.dialogueLine;

            _autoNextLine = content.autoNextLine;

            var prevCharacter = _currentSpeaker;
            _currentSpeaker = character;
            if (character != prevCharacter)
            {
                OnEmotionChange?.Invoke(Emotion.normal);
            }
            
            //TODO: Find a smort way to add gameobject/instantiated character animations
            //FIND INSTANTIATED CHAR
            /*_currentDialogueAnimator = character.Animator ? character.Animator : null;
            if (_currentDialogueAnimator)
            {
                SetTalkerAnimatorOn();
                _dialogueText.onDialogueFinish.AddListener(SetTalkerAnimatorOff);
            }*/

            _instantiatedPanel.SetNameText(nameText);
            _instantiatedPanel.SetDialogueText(dialogueText);
            // _dialogueText.PrepareText(dialogueText);
            
        }

        private void InstantiatePanel()
        {
            var panelGameObject = Instantiate(_dialoguePanelPrefab, transform);
            _instantiatedPanel = panelGameObject.GetComponent<DialoguePanel>();
        }

        private void DelayedInteraction()
        {
            _noInteractionDelay = true;
        }
        
        public void DialogueChoice(Node dialogueChoice)
        {
            _currentTreeRunner.OnPlayerMadeDecision(dialogueChoice.connectedIndex);
            
            SetChoiceParentActive(false, null);
            _choicesActive = false;
            
            _instantiatedPanel.SetNameText(string.Empty);
            _instantiatedPanel.SetDialogueText(string.Empty);

            _buttonTest.GetComponentInChildren<TextMeshProUGUI>().text = "Continue Dialogue";
            _currentSpeaker = null;
            
            //TODO: Should send in specifics on animations etc
            _instantiatedPanel.SetupDialoguePanel();
            
            OnEmotionChange?.Invoke(Emotion.normal);

            DoDialogueRunner(_currentTreeRunner);
            Invoke(nameof(DelayedInteraction), .1f);
        }

        private void EndDialogue()
        {
            CanContinue = false;
            
            SetChoiceParentActive(false, null);

            _instantiatedPanel.EndDialoguePanel();
            // _instantiatedPanel = null;
            
            EndOfDialogue?.Invoke();

            _buttonTest.GetComponentInChildren<TextMeshProUGUI>().text = "Start Dialogue";
            
            _currentTreeRunner = null;
            _currentSpeaker = null;
            _noInteractionDelay = false;
            
        }

        private void SetChoiceParentActive(bool setActive, List<Node> choices)
        {
            Transform choicesParent = _instantiatedPanel.ChoicesParent;
            var choicesActive = choicesParent.gameObject.activeInHierarchy;
            
            if (choicesActive == setActive)
            {
                // Debug.Log($"Tried setting choices to {choicesActive}, but it already is");
                return;
            }
            
            if (choicesActive || choices == null || choices.Count <= 0)
            {
                DestroyInstantiatedChoices();
                choicesParent.gameObject.SetActive(false);
                _choicesActive = false;
                return;
            }
            
            DestroyInstantiatedChoices();
            InstantiateChoices(choices);
            
            choicesParent.gameObject.SetActive(true);
            _choicesActive = true;
        }

        private void InstantiateChoices(List<Node> choices)
        {
            DestroyInstantiatedChoices();

            var choicesParent = _instantiatedPanel.ChoicesParent;
            var choiceButtonPrefab = _instantiatedPanel.ChoiceButtonPrefab;
            
            foreach (var choice in choices)
            {
                var instantiatedButton = Instantiate(choiceButtonPrefab, choicesParent);
                instantiatedButton.Initialize(choice);
                _instantiatedChoices.Add(instantiatedButton.gameObject);
            }
        }

        private void DestroyInstantiatedChoices()
        {
            for (int i = 0; i < _instantiatedChoices.Count; i++)
            {
                var choice = _instantiatedChoices[i];
                Destroy(choice);
            }
        }

        private void OnEndOfDialogueLineCallback()
        {
            DialogueManager.Log("Reached end of dialogue line");
            
            _canSkip = false;
            if (_autoNextLine)
            {
                _autoNextLine = false;
                DoDialogueRunner(_currentTreeRunner);
                return;
            }
            
            //TODO: Check how choices work with new system, maybe it needs to wait until some dialogue done?
            /*if (_lines.Count == 0 && _activeElementHolder && _activeElementHolder.Choices.Length > 0)
            {
                SetChoiceParentActive(true);
            }*/

            _instantiatedPanel.SetInteractImageActive(true);
            _autoNextLine = false;
        }

        private void SetTalkerAnimatorOn()
        {
            // _currentDialogueAnimator.SetBool("Talking", true);
        }
        
        private void SetTalkerAnimatorOff()
        {
            /*_dialogueText.onDialogueFinish.RemoveListener(SetTalkerAnimatorOff);
            if(!_currentDialogueAnimator)
                return;
            
            _currentDialogueAnimator.SetBool("Talking", false);
            _currentDialogueAnimator = null;*/
        }
        
        // [SerializeField] 
        // private StudioEventEmitter _popupAudio, _exitAudio, _uiButtonAudio;

        /*public void EnterAudio()
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