using System.Globalization;
using System.Resources;
using FindFilmFree.Application.Abstraction.Interfaces;
using FindFilmFree.Domain.Models;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace FindFilmFree.Application.Services;

public class ModerService:IModerService
{
    private readonly IUnitOfWork _unitOfWork;
    // private readonly long _moderId;
    private ResourceManager resourceManager = new ResourceManager("FindFilmFree.Application.Resources.Language", typeof(AdminService).Assembly);
    private Film _film = default;
    private CultureInfo _cultureInfo = new CultureInfo("default");
    private AdminService _adminService = default;
    private Dictionary<string,bool> commands = new Dictionary<string, bool>()
    {
        {"AddFilmNameAsync",false},
        {"AddFilmNumberAsync",false},
        {"AddFilmLinkAsync",false},
        {"changeName",false},
        {"changeNumber",false},
        {"changeLink",false},
        {"updateFilmData",false},
    };
    public ModerService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        _adminService = new AdminService(unitOfWork);
    }

    public async Task HandleAdminCommands(ITelegramBotClient client, Update update)
    {
        await _adminService.HandleAdminCommands(client, update);
    }

   

    public async Task AddFilmAsync(ITelegramBotClient botClient,Update update)
    {
        // _film = new Film();
        // commands["AddFilmNameAsync"] = true;
        // await botClient.SendTextMessageAsync(update.Message.Chat.Id, $"{resourceManager.GetString("addFilmName",_cultureInfo)}");
       await _adminService.AddFilmAsync(botClient, update);
    }

    public async Task AddFilmNameAsync(ITelegramBotClient botClient, Update update)
    {
        await _adminService.AddFilmNameAsync(botClient, update);
        // _film.Name = update.Message.Text;
        // await botClient.SendTextMessageAsync(update.Message.Chat.Id, $"{resourceManager.GetString("nameAdded",_cultureInfo)}");
        // commands["AddFilmNameAsync"] = false;
        // commands["AddFilmNumberAsync"] = true;
    }

    public async Task AddFilmNumberAsync(ITelegramBotClient botClient, Update update)
    {
        await _adminService.AddFilmNumberAsync(botClient, update);
        // int number = 0;
        // bool parse = int.TryParse(update.Message.Text,out number);
        // if (parse)
        // {
        //     _film.Number = number;
        //     await botClient.SendTextMessageAsync(update.Message.Chat.Id, $"{resourceManager.GetString("numberAdded",_cultureInfo)}");
        //     commands["AddFilmNumberAsync"] = false;
        //     commands["AddFilmLinkAsync"] = true;
        // }
       
        
    }

    public async Task AddFilmLinkAsync(ITelegramBotClient botClient, Update update)
    {
        await _adminService.AddFilmLinkAsync(botClient, update);
        // _film.FilmId = Guid.NewGuid();
        // _film.Link = update.Message.Text;
        // await botClient.SendTextMessageAsync(update.Message.Chat.Id, $"{resourceManager.GetString("linkAdded",_cultureInfo)}");
        // await _unitOfWork.Films.AddAsync(_film);
        // await _unitOfWork.CompleteAsync();
        // await botClient.SendTextMessageAsync(update.Message.Chat.Id, $"{resourceManager.GetString("linkAdded",_cultureInfo)}");
        // string message = $"<b>FilmId</b>: {_film.FilmId}\n" +
        //                  $"<b>FilmName</b>: {_film.Name}\n" +
        //                  $"<b>FilmNumber</b>: {_film.Number}\n" +
        //                  $"<b>FilmLink</b>: {_film.Link}";
        // await botClient.SendTextMessageAsync(update.Message.Chat.Id, message, parseMode: ParseMode.Html);
        // commands["AddFilmLinkAsync"] = false;
        
    }

    public  async Task UpdateFilmData(ITelegramBotClient botClient, Update update)
    {
        await _adminService.UpdateFilmData(botClient, update);
    }
}