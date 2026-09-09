using System;
using UnityEngine;

namespace Platformer.Dialogue
{
    [Serializable]
    public class DialogueData
    {
        public string npcName = "Aldeano";
        public Sprite avatar;
        [TextArea(2, 5)]
        public string[] lines;
        public int rewardCoins = 0;
        public string rewardMessage = "";
    }
}
