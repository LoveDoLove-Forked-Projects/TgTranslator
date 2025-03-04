using System;
using Flurl;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using TgTranslator.Data.Options;

namespace TgTranslator.Services;

public class TelegramWebhooksRegistrar : IStartupFilter
{
    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) =>
        builder =>
        {
            using (var scope = builder.ApplicationServices.CreateScope())
            {
                var client = scope.ServiceProvider.GetRequiredService<TelegramBotClient>();
                var domain = scope.ServiceProvider.GetRequiredService<IOptions<TelegramOptions>>().Value.WebhooksDomain;

                client.DeleteWebhook()
                    .GetAwaiter().GetResult();

                client
                    .SetWebhook(domain.AppendPathSegments("api", "bot"),
                        allowedUpdates:
                        [
                            UpdateType.Message,
                            UpdateType.CallbackQuery,
                            UpdateType.InlineQuery,
                            UpdateType.ChatMember,
                            UpdateType.MyChatMember,
                            UpdateType.EditedMessage
                        ],
                        dropPendingUpdates: true).GetAwaiter().GetResult();

                var commandsManager = scope.ServiceProvider.GetRequiredService<CommandsManager>();
                commandsManager.SetDefaultCommands()
                    .ConfigureAwait(false)
                    .GetAwaiter()
                    .GetResult();

                var me = client.GetMe()
                    .ConfigureAwait(false)
                    .GetAwaiter()
                    .GetResult();

                Static.Username = me.Username;
                Static.BotId = me.Id;
            }

            next(builder);
        };
}