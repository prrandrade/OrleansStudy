namespace Grains.States
{
    using System;
    using System.Collections.Generic;

    [Serializable]
    public class ConversationState
    {
        public List<ConversationPhraseState> Phrases { get; set; }
    }

    public class ConversationPhraseState
    {
        public DateTime DateTime { get; set; }

        public string Phrase { get; set; }
    }
}
