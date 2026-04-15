# Discord Support Bot
 
> ⚠️ **Old project** — created at the early stage of my learning (about 6-7 months ago as of April 13, 2026).
 
A Discord support ticket bot built with **C# / ASP.NET Core**, integrated with **PostgreSQL** for ticket logging and transcripts.

---
 
## Demo
[![Watch the video](https://img.youtube.com/vi/F5HW-D4LpPY/maxresdefault.jpg)](https://www.youtube.com/watch?v=eDz-OpAqNkY)


# Running locally
 
## Requirements
 
- .NET 8 SDK
- Postgres
- A Discord bot token
 
---
 
## Step 1 — Clone the repo
 
```bash
git clone https://github.com/your-username/your-repo.git
```
 
---
 
## Step 2 — Create a `.env` file
 
In the project root, create a file named `.env`:
 
```env
SECRET=any_random_string_you_choose
DISCORDBOTTOKEN=your_discord_bot_token_here
DBconnection=Host=localhost;Port=5432;Database=supportbot;Username=postgres;Password=your_password
```
 
> Never commit this file. It is already listed in `.gitignore`.
 
---
 
## Step 3 — Start the database
 
Start the database locally or in your docker container
 
---
 
## Step 4 — Set your Discord server IDs
 
Open `DISCORD_CREDENTIALS/DISCORD_CRED.cs` and replace the hardcoded values with your own:
 
```csharp
public static ulong GuildID = YOUR_SERVER_ID;
public static List<ulong> DISCORD_SUPPORT_ROLES = new List<ulong> { ROLE_1, ROLE_2 };
public static ulong ADMINROLE = YOUR_ADMIN_ROLE_ID;
public static ulong SUPPORT_CATEGORY = 
public static ulong CHANNEL_FOR_STARTMESSAGE = 
public static ulong Channel_FOR_LOGS = 
public static ulong ROLE_FOR_TICKETPING = 
public static ulong CHANNEL_FOR_TRANSCRIPTS = 
public static string ServerBannerURL = 
public static string ServerName = 
```
 
To find IDs in Discord: `Settings → Advanced → Developer Mode → ON`, then right-click any server/channel/role and choose **Copy ID**.
 
---
 
## Step 5 — Apply database migrations
 
```bash
dotnet ef database update
```

---
 
## Step 6 — Run the bot
 
```bash
dotnet run
```
 
If everything is set up correctly, you will see in the console:
 
```
Bot is ready
```
 
And the bot will come online in your Discord server.
 
---
