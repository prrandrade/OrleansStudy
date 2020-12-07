namespace Grains
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Interfaces;
    using Orleans;
    using Orleans.Runtime;
    using States;

    public class ConversationGrain : Grain, IConversationGrain
    {
        private readonly IPersistentState<ConversationState> _conversationState;

        // se estamos usando storages nomeados, o nome do storage deste objeto deve
        // ser o segundo parâmetro do atributo PersistentState
        public ConversationGrain([PersistentState("conversation")] IPersistentState<ConversationState> conversationState)
        {
            _conversationState = conversationState;
        }

        public override async Task OnActivateAsync()
        {
            if (_conversationState.State.Phrases == null)
            {
                _conversationState.State.Phrases ??= new List<ConversationPhraseState>();
                await _conversationState.WriteStateAsync();
            }

            await base.OnActivateAsync();
        }

        public async Task Say(string message)
        {
            DeactivateOnIdle();
            _conversationState.State.Phrases.Add(new ConversationPhraseState { DateTime = DateTime.Now, Phrase = message});
            await _conversationState.WriteStateAsync();
        }

        public async Task<IEnumerable<string>> ShowHistory()
        {
            DeactivateOnIdle();
            await _conversationState.ReadStateAsync();
            return _conversationState.State.Phrases
                .OrderBy(x => x.DateTime)
                .Select(x => $"{x.DateTime} - {x.Phrase}").ToList();
        }

        public async Task EraseHistory()
        {
            DeactivateOnIdle();
            await _conversationState.ClearStateAsync();
        }
    }
}
