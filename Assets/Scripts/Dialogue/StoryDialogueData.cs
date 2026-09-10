using System;
using UnityEngine;

namespace Platformer.Dialogue
{
    [Serializable]
    public class StoryLine
    {
        public string speakerName;
        public Sprite speakerPortrait;
        [TextArea(3, 6)]
        public string text;
        public float typingSpeed = 0.025f;
    }

    [CreateAssetMenu(fileName = "NewStoryDialogue", menuName = "Story/Story Dialogue Data")]
    public class StoryDialogueData : ScriptableObject
    {
        public string dialogueTitle;
        public StoryLine[] lines;
        public string nextSceneOnComplete = "Village_Scene";
    }
}
