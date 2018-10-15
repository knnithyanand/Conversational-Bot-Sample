using Microsoft.Bot.Configuration;
using Microsoft.ApplicationInsights;
using System.Linq;
using System;

namespace Knnithyanand.Conversational.Samples
{
    /// <summary>
    /// Represents references to external services.
    ///
    /// For example, LUIS services are kept here as a singleton.  This external service is configured
    /// using the <see cref="BotConfiguration"/> class.
    /// </summary>
    /// <seealso cref="https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection?view=aspnetcore-2.1"/>
    /// <seealso cref="https://www.luis.ai/home"/>
    public class BotServices
    {
        public BotConfiguration BotConfig { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BotServices"/> class.
        /// </summary>
        /// <param name="botConfiguration">The <see cref="BotConfiguration"/> instance for the bot.</param>
        public BotServices(BotConfiguration botConfiguration)
        {
            this.BotConfig = botConfiguration;
        }

        /// <summary>
        /// Gets the set of the Authentication Connection Name for the Bot application.
        /// </summary>
        /// <remarks>The Authentication Connection Name  should not be modified while the bot is running.</remarks>
        /// <value>
        /// A string based on configuration in the .bot file.
        /// </value>
        public string AuthConnectionName { get; }

        public ConnectedService GetConnectedService(string serviceType, string name = "")
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return BotConfig.Services.FirstOrDefault(s => s.Type == serviceType)
                    ?? throw new Exception($"Please configure your service of type '{serviceType}' in your .bot file.");
            }
            return BotConfig.Services.FirstOrDefault(s => s.Type == serviceType && s.Name == name)
                ?? throw new Exception($"Please configure your service '{name}' of type '{serviceType}' in your .bot file.");

        }

        /// <summary>
        /// Gets the set of AppInsights Telemetry Client used.
        /// </summary>
        /// <remarks>The AppInsights Telemetry Client should not be modified while the bot is running.</remarks>
        /// <value>
        /// A <see cref="TelemetryClient"/> client instance created based on configuration in the .bot file.
        /// </value>
        public TelemetryClient TelemetryClient { get; }

    }
}