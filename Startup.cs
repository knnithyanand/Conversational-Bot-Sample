using System;
using System.Linq;
using Knnithyanand.Conversational.Samples.Bots;
using Knnithyanand.Conversational.Samples.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Azure;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Configuration;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Knnithyanand.Conversational.Samples
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        private string _environmentName { get; set; }

        public Startup(IHostingEnvironment env)
        {
            _environmentName = env.EnvironmentName.ToLower();

            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            // Load the connected services from .bot file.
            var botFilePath = Configuration.GetSection("botFilePath")?.Value;
            var botFileSecret = Configuration.GetSection("botFileSecret")?.Value;
            var botConfig = BotConfiguration.Load(botFilePath ?? @".\BotConfiguration.bot", botFileSecret);
            services.AddSingleton(sp => botConfig ?? throw new InvalidOperationException($"The .bot config file could not be loaded."));

            // Initializes your bot service clients and adds a singleton that your Bot can access through dependency injection.
            var connectedServices = new BotServices(botConfig);
            services.AddSingleton(connectedServices);

            // Initialize Bot State
            var blobStorage = connectedServices.GetConnectedService(ServiceTypes.BlobStorage, "bot-state") as BlobStorageService;

            var dataStore = new AzureBlobStorage(blobStorage.ConnectionString, blobStorage.Container);
            var userState = new UserState(dataStore);
            var conversationState = new ConversationState(dataStore);
            var botStateSet = new BotStateSet(userState, conversationState);

            services.AddSingleton(dataStore);
            services.AddSingleton(userState);
            services.AddSingleton(conversationState);
            services.AddSingleton(botStateSet);

            // Add UserProfile Accessor
            services.AddSingleton(new EchoBotAccessors(conversationState, userState)
            {
                CounterState = userState.CreateProperty<CounterState>($"{nameof(EchoBotAccessors)}.{nameof(CounterState)}"),
            });

            services.AddBot<EchoBot>(options =>
            {
                var service = botConfig.Services.FirstOrDefault(s => s.Type == ServiceTypes.Endpoint && s.Name == _environmentName);
                if (!(service is EndpointService endpointService))
                {
                    throw new InvalidOperationException($"The .bot file does not contain an endpoint with name '{_environmentName}'.");
                }

                options.CredentialProvider = new SimpleCredentialProvider(endpointService.AppId, endpointService.AppPassword);

                // Typing Middleware (automatically shows typing when the bot is responding/working)
                var typingMiddleware = new ShowTypingMiddleware();
                options.Middleware.Add(typingMiddleware);

                options.State.Add(conversationState);
                options.State.Add(userState);

                // Save State Middleware (automatically saves user and conversation state to storage)
                options.Middleware.Add(new AutoSaveStateMiddleware(userState, conversationState));

            });
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app">Application Builder.</param>
        /// <param name="env">Hosting Environment.</param>
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseDefaultFiles()
                .UseStaticFiles()
                .UseBotFramework();
        }
    }
}
