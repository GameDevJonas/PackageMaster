using System;
using System.Reflection;
using GDPanda.Data;
using UnityEngine;

namespace GDPanda.BanterForge
{
    public class DialogueActions : Singleton<DialogueActions>
    {
        public static void OnDialogueAction(string actionString)
        {
            var instance = GetInstance();
            Type thisType = instance.GetType();

            string methodName = actionString.Split("(")[0];
            /*string[] parameters = Array.Empty<string>(); 
            for (int characterIndex = 0; characterIndex < actionString.Length; characterIndex++)
            {
                var character = actionString[characterIndex];
                if(character == '(') //Entering params
                    continue;
            }*/
            
            MethodInfo theMethod = thisType.GetMethod(methodName);
            if (theMethod == null)
            {
                Debug.LogError("Trying to invoke dialogue action on null method!");
                return;
            }
            
            theMethod.Invoke(instance, null);
        }
    }
}