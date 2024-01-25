using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace GDPanda.BanterForge
{
    /// <summary>
    /// Physical representatio
    /// </summary>
    [CreateAssetMenu(menuName = "Dialogue System/New Character", fileName = "Character", order = 0)]
    public class Character : ScriptableObject
    {
        public Color CharacterNodeColor;
        
        private Character _instantiatedCharacter;

        public Character InstantiatedCharacter
        {
            get => _instantiatedCharacter;
            set => _instantiatedCharacter = value;
        }

        public string Name;
        
        [Range(1,10)]
        public int VoiceType;

        [HideInInspector]
        public string HappyAnimationName;
        [HideInInspector]
        public string SadAnimationName;
        [HideInInspector]
        public string SuprisedAnimationNamee;
        [HideInInspector]
        public string AngryAnimationName;
        [HideInInspector]
        public string NormalAnimationName;

        public TMP_FontAsset overrideFont;
        public Color overrideTextColor;
        
        public string GetSpriteFromEmotion(Emotion emotion)
        {
            switch (emotion)
            {
                case Emotion.happy:
                    return HappyAnimationName;
                case Emotion.sad:
                    return SadAnimationName;
                case Emotion.suprised:
                    return SuprisedAnimationNamee;
                case Emotion.angry:
                    return AngryAnimationName;
                default:
                case Emotion.normal:
                    return NormalAnimationName;
            }
        }

        private Animator _animator;

        public Animator Animator
        {
            get => _animator;
            set => _animator = value;
        }

        /*public Character Instantiate(CharacterHolder holder)
        {
            var instantiatedCharacter = ScriptableObject.Instantiate(this);
            instantiatedCharacter._characterHolder = holder;
            // _instantiatedCharacter = instantiatedCharacter;
            return instantiatedCharacter;
        }*/
        
        public string GetVariable(string variableName)
        {
            variableName = variableName.ToLower();
            return variableName switch
            {
                "name" => Name,
                /*"personality" => CharacterPersonality.Personality.ToString(),
                "patience" => CharacterPatience.Patience.ToString(),
                "goal" => CharacterGoal.Goal.ToString(),
                "destination" => GetTownDestination(),*/
                "payment" => null,
                _ => null
            };
        }
    }
}