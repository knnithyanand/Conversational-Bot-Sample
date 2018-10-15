using System;
using Knnithyanand.Conversational.Samples.Models;
using Microsoft.Bot.Builder;

namespace Knnithyanand.Conversational.Samples.Bots
{
    public class EchoBotAccessors
    {
        public ConversationState ConversationState { get; }
        
        public UserState UserState { get; }

        public EchoBotAccessors(ConversationState conversationState, UserState userState)
        {
            ConversationState = conversationState ?? throw new ArgumentNullException(nameof(conversationState));
            UserState = userState ?? throw new ArgumentNullException(nameof(userState));
        }

        public IStatePropertyAccessor<CounterState> CounterState { get; set; }

    }
}