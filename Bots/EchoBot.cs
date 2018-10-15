using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Knnithyanand.Conversational.Samples.Models;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Azure;
using Microsoft.Bot.Configuration;
using Microsoft.Bot.Schema;

namespace Knnithyanand.Conversational.Samples.Bots
{
    public class EchoBot : IBot
    {
        private readonly AzureBlobStorage dataStore;

        private readonly EchoBotAccessors _accessors;

        public EchoBot(BotServices botService, EchoBotAccessors accessors)
        {
            _accessors = accessors ?? throw new System.ArgumentNullException(nameof(accessors));
            var blobStorage = botService.GetConnectedService(ServiceTypes.BlobStorage, "bot-data") as BlobStorageService;
            dataStore = new AzureBlobStorage(blobStorage.ConnectionString, blobStorage.Container);
        }

        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (turnContext.Activity.Type is ActivityTypes.Message)
            {
                string inputText = turnContext.Activity.Text;

                var oldState = await _accessors.CounterState.GetAsync(turnContext, () => new CounterState());
                var newState = new CounterState { TurnCount = oldState.TurnCount + 1 };
                await _accessors.CounterState.SetAsync(turnContext, newState);

                string[] keys = { "msgs" };
                var responseTemplates = dataStore.ReadAsync<ResponseTemplate>(keys).Result?.FirstOrDefault().Value;
                if (responseTemplates == null)
                {
                    responseTemplates = new ResponseTemplate()
                    {
                        Templates = new[] {
                        "Hi {0}",
                        "Hey {0}, How you doing?",
                        "Welcome {0}, I am {1}",
                        "Hi {0}, I just wanted to say {2}"
                        }
                    };
                }
                var changes = new System.Collections.Generic.Dictionary<string, object>();
                changes.Add("msgs", responseTemplates);
                await dataStore.WriteAsync(changes, cancellationToken);

                var variables = new[] { "Guest User", "Demo Bot", "Hello" };

                await turnContext.SendActivityAsync(responseTemplates.GetMessage(variables));
            }
        }
    }
}
