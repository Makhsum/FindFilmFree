using Telegram.Bot;
using Telegram.Bot.Types;

namespace FindFilmFree.Application.Abstraction.Interfaces;

public interface IAdminService
{
    Task HandleAdminCommands(ITelegramBotClient client, Update update);
    
    Task AddFilmAsync(ITelegramBotClient botClient,Update update);
    Task AddFilmNameAsync(ITelegramBotClient botClient,Update update);
    Task AddFilmNumberAsync(ITelegramBotClient botClient,Update update);
    Task AddFilmLinkAsync(ITelegramBotClient botClient,Update update);
    Task UpdateFilmData(ITelegramBotClient botClient, Update update);

}