using UnityEngine;

namespace GDPanda.BanterForge
{
    public class DialoguePanelSignalReceiver : MonoBehaviour
    {
        public void EnterAudio()
        {
            
        }

        public void ExitAudio()
        {
            
        }
        
        public void AnimationReady()
        {
            DialogueManager.GetInstance().AnimationReady();
        }
    }
}