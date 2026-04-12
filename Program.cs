using Npgsql;
using Support_Bot.CREDENTIALS_CLASSES;
using Support_Bot.DB_INTERFACES;
using Support_Bot.DB_METHODS;
using Support_Bot.DB_MODELS;
using System.Data.Common;
using Discord;
using Discord.WebSocket;
using Support_Bot.DISCORDBOT;
using Support_Bot.SENDTXT;
using Support_Bot.MODELS_FOR_DISCORD_BOT;
using Support_Bot.DISCORDBOT.DISCORD_MAIN;
using Support_Bot.DISCORDBOT.INTERFACES_DISCORD;
using Microsoft.EntityFrameworkCore;
using Support_Bot.DB_METHODS.ContextAndHelpers;

var builder = WebApplication.CreateBuilder(args);

DotNetEnv.Env.Load();
Console.WriteLine(DB_Connection.ConnectionString);
Secret.Secret_Key = Environment.GetEnvironmentVariable("SECRET");
DISCORD_CRED.DISCORDTOKEN = Environment.GetEnvironmentVariable("DISCORDBOTTOKEN");
DB_Connection.ConnectionString = Environment.GetEnvironmentVariable("DBconnection");

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ContextForDB>(options=>options.UseNpgsql(DB_Connection.ConnectionString));
Console.WriteLine(DB_Connection.ConnectionString);

builder.Services.AddHttpClient<DISCORD_START>();
builder.Services.AddScoped<IDB_TICKER_CREATED, DB_ADD_TICKET_INFO>();
builder.Services.AddScoped<IaddDBcontent, DB_add_Content>();

builder.Services.AddScoped<IdiscordSendTXT, DISCORD_SEND_TXT>();
builder.Services.AddScoped<IdiscordGetTXT,DB_DISCORD_GET_TXT>();
builder.Services.AddScoped<IhandleTicketCreation, DISCORD_HANDLE_TICKET_CREATION>();
builder.Services.AddHostedService<BACKGROUND_ADD_MESSAGE_TO_DB>();
builder.Services.AddScoped<IcheckActivity,DB_CHECK_ACTIVITY>();
builder.Services.AddScoped<ISetStatus,SetStatusDB>();

var config = new DiscordSocketConfig
{
    MessageCacheSize = 3000,

    GatewayIntents =
        GatewayIntents.Guilds |
        GatewayIntents.GuildMessages |
        GatewayIntents.DirectMessages |
        GatewayIntents.MessageContent |
        GatewayIntents.GuildMembers |
        GatewayIntents.GuildMessageReactions
};

builder.Services.AddSingleton(new DiscordSocketClient(config));
builder.Services.AddHostedService<DISCORD_START>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
