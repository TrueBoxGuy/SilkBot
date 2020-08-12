﻿using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using SilkBot.ServerConfigurations;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SilkBot.Commands.Moderation
{
    public class KickCommand : BaseCommandModule
    {
        [Command("Kick")]
        [HelpDescription("Kick a user! *Note, caller requires moderator permission.*")]
        public async Task Kick(CommandContext ctx, DiscordMember user, [RemainingText] string reason = "Not Given.")
        {
            var bot = await ctx.Guild.GetMemberAsync(ctx.Client.CurrentUser.Id);
            var isBelowRole = false;
            var canKick = ctx.Member.Roles.Any(role => role.Permissions.HasPermission(Permissions.KickMembers));

            if (!canKick)
            {
                await ctx.RespondAsync("Sorry, but you're not allowed to kick people!");
                return;
            }


            if (user.Roles.Any())
                foreach (var role in user.Roles)
                    if (bot.Roles.Last().Position <= user.Roles.Last().Position)
                        isBelowRole = true;


            //var userRole = user.Roles.Last();

            if (isBelowRole)
            {
                var staffRole = "";
                if (user == bot)
                    staffRole = "I can't kick myself!";
                else if (user == ctx.Guild.Owner)
                    staffRole = $"I can't kick {user.Mention}...They're the owner...";
                else if (user.Roles.Last().Permissions.HasPermission(Permissions.KickMembers))
                    staffRole = $"I can't kick {user.Mention}! They're a moderator! ({user.Roles.Last().Mention})";
                else if (user.Roles.Last().Permissions.HasPermission(Permissions.BanMembers))
                    staffRole = $"I can't kick {user.Mention}! They're an admin! ({user.Roles.Last().Mention})";





                var message = await ctx.Client.SendMessageAsync(ctx.Channel,
                                              embed: new DiscordEmbedBuilder()
                                                    .WithAuthor(ctx.Member.DisplayName, "", ctx.Member.AvatarUrl)
                                                    .WithColor(DiscordColor.Red)
                                                    .WithDescription(staffRole)
                                                    .WithFooter("Silk")
                                                    .WithTimestamp(DateTime.UtcNow));



            }
            else
            {


                var embed = new DiscordEmbedBuilder(EmbedGenerator.CreateEmbed(ctx, $"You've been kicked from {ctx.Guild.Name}!", "")).AddField("Reason:", reason);

                await DMCommand.DM(ctx, user, embed);

                await ctx.Guild.BanMemberAsync(user, 0, reason);
                await ctx.Guild.UnbanMemberAsync(user);



                ServerConfigurationManager.LocalConfiguration.TryGetValue(ctx.Guild.Id, out var guildConfig);
                var logChannelID = guildConfig?.LoggingChannel;
                var logChannelValue = logChannelID.HasValue ? logChannelID.Value : ctx.Channel.Id;
                await ctx.Client.SendMessageAsync(await ServerInfo.Instance.ReturnChannelFromID(ctx, logChannelValue),
                    embed: new DiscordEmbedBuilder()
                    .WithAuthor(ctx.Member.DisplayName, "", ctx.Member.AvatarUrl)
                    .WithColor(DiscordColor.SpringGreen)
                    .WithDescription($":boot: Kicked {user.Mention}! (User notified with direct message)")
                    .WithFooter("Silk")
                    .WithTimestamp(DateTime.Now));
            }
        }
    }
}