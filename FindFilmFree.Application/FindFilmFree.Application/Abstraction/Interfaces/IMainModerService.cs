using Telegram.Bot;
using Telegram.Bot.Types;

namespace FindFilmFree.Application.Abstraction.Interfaces;

public interface IMainModerService:IAdminService
{
    Task SendAllUsersTextMessageAsync(ITelegramBotClient botClient,string text);
    Task GetUsersCount(ITelegramBotClient botClient);
    Task SetModerAsync(ITelegramBotClient botClient, Update update);
    Task DeleteModerAsync(ITelegramBotClient botClient, Update update);
}