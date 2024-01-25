using System;
using System.Collections;
using System.Collections.Generic;
using GDPanda.BanterForge.Tree;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GDPanda.BanterForge
{
    public class DialogueChoiceButton : MonoBehaviour
    {
        private DialogueManager _dialogueManager;

        private Node node;
        // private void Start()
        // {
        //     Initialize();
        // }

        public void Initialize(Node choice)
        {
            if (choice == null)
            {
                Destroy(gameObject);
                return;
            }

            node = choice;

            // var dialogueNode = node as DialogueNode;
            // if(dialogueNode)
            
            _dialogueManager = FindObjectOfType<DialogueManager>();

            var tmp = GetComponentInChildren<TextMeshProUGUI>();
            tmp.SetText(node.nodeName);

            var button = GetComponent<Button>();
            button.onClick.AddListener(() =>
            {
                _dialogueManager.DialogueChoice(node);
            });
        }
    }
}