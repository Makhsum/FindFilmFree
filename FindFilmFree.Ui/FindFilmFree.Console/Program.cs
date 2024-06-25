using FindFilmFree.Application.Configerations;
using FindFilmFree.Application.Services;
using FindFilmFree.Infrastructure.Database;
using Telegram.Bot;

DatabaseContext context = new DatabaseContext();
UnitOfWork unitOfWork = new UnitOfWork(context);
TelegramBotClient client = new TelegramBotClient("7313876264:AAH2HTIILQ1sgR0oklbZCBIO1JOQNNxCChc");

BotTelegramService botTelegramService = new BotTelegramService(client,unitOfWork:unitOfWork);
await botTelegramService.StartAsync();