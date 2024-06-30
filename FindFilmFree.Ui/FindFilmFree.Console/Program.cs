using FindFilmFree.Application.Configerations;
using FindFilmFree.Application.Services;
using FindFilmFree.Infrastructure.Database;
using Microsoft.Extensions.Configuration;
using Telegram.Bot;

IConfiguration configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json",optional:false,reloadOnChange:true).Build();
string connectionString = configuration.GetConnectionString("SqliteConnection");
string telegramBotToken = configuration["TelegramBot:token"];

long ownerId = 0;
bool ownerIdParse = long.TryParse(configuration["OwnerId"], out ownerId);
if (ownerIdParse&&telegramBotToken!=null&&connectionString!=null)
{
    DatabaseContext context = new DatabaseContext(connectionString);
    UnitOfWork unitOfWork = new UnitOfWork(context);
    TelegramBotClient client = new TelegramBotClient(telegramBotToken);

    BotTelegramService botTelegramService = new BotTelegramService(client,unitOfWork:unitOfWork,ownerId);
    await botTelegramService.StartAsync();
}

