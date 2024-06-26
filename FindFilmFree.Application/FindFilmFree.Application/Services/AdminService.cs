using System.Globalization;
using System.Resources;
using FindFilmFree.Application.Abstraction.Interfaces;
using FindFilmFree.Domain.Models;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace FindFilmFree.Application.Services;

public class AdminService:IAdminService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly long _adminId;
    private Film _film = default;
    ResourceManager resourceManager = new ResourceManager("FindFilmFree.Application.Resources.Language", typeof(AdminService).Assembly);
    private CultureInfo _cultureInfo = new CultureInfo("default");
    private Dictionary<string,bool> commands = new Dictionary<string, bool>()
    {
        {"SendAllUsersTextMessageAsync",false},
        {"AddFilmNameAsync",false},
        {"AddFilmNumberAsync",false},
        {"AddFilmLinkAsync",false},
    };
    public AdminService(IUnitOfWork unitOfWork,long adminId)
    {
        _unitOfWork = unitOfWork;
        _adminId = adminId;
    }

    public async Task HandleAdminCommands(ITelegramBotClient client, Update update)
    {
        var chatId = update.Message.Chat.Id;
        var admin = await _unitOfWork.Users.GetByTelegramIdAsync(_adminId);
        if (admin!=null)
        {
            _cultureInfo = new CultureInfo(admin.Language);
        }
        var messageText = update.Message.Text;
        if (commands["SendAllUsersTextMessageAsync"])
        {
            await SendAllUsersTextMessageAsync(client, messageText);
        }
        else if (commands["AddFilmNameAsync"])
        {
           await AddFilmNameAsync(client,update);
        }
        else if (commands["AddFilmNumberAsync"])
        {
           await AddFilmNumberAsync(client,update);
        }
        else if (commands["AddFilmLinkAsync"])
        {
           await AddFilmLinkAsync(client,update);
        }
        switch (messageText)
        {
            case "/sendMsg":
                commands["SendAllUsersTextMessageAsync"] = true;
                await client.SendTextMessageAsync(_adminId,
                    $"{resourceManager.GetString("sentMessage", _cultureInfo)}");
                break;
            case "/addfilm":
              await  AddFilmAsync(client);
                break;
            case "/getusersinfo":
              await  GetUsersCount(client);
                break;
        }
    }

    public async Task SendAllUsersTextMessageAsync(ITelegramBotClient botClient, string text)
    {
        long erorrId = 0;
        int msgCount = 0;
        
        
            
            var users = await _unitOfWork.Users.GetAllAsync();
            foreach (var user in users)
            {
                if (user.IsActive)
                {
                  
                    try
                    {
                        erorrId = user.TelegramId;
                        await botClient.SendTextMessageAsync(user.TelegramId, text);
                        msgCount += 1;
                    }
                    catch (Exception ex)
                    {
                       // await botClient.SendTextMessageAsync(_adminId, $"{resourceManager.GetString("sent_messages",_cultureInfo)} {msgCount}");
                        await botClient.SendTextMessageAsync(1635253907, ex.Message);
                        if (ex.Message.Contains("bot was blocked by the user"))
                        {
                            //  await UserDeactive(update.CallbackQuery.Message.Chat.Id);
                            await UserDeactive(erorrId);
                        }
                    }
                  
                }
            }

            await botClient.SendTextMessageAsync(_adminId, $"{resourceManager.GetString("sent_messages",_cultureInfo)} {msgCount}");
            commands["SendAllUsersTextMessageAsync"] = false;
        
        
       
    }

    public async Task AddFilmAsync(ITelegramBotClient botClient)
    {
        _film = new Film();
        commands["AddFilmNameAsync"] = true;
        await botClient.SendTextMessageAsync(_adminId, $"{resourceManager.GetString("addFilmName",_cultureInfo)}");
    }

    public async Task AddFilmNameAsync(ITelegramBotClient botClient, Update update)
    {
        _film.Name = update.Message.Text;
        await botClient.SendTextMessageAsync(_adminId, $"{resourceManager.GetString("nameAdded",_cultureInfo)}");
        commands["AddFilmNameAsync"] = false;
        commands["AddFilmNumberAsync"] = true;
    }

    public async Task AddFilmNumberAsync(ITelegramBotClient botClient, Update update)
    {
        int number = 0;
        bool parse = int.TryParse(update.Message.Text,out number);
        if (parse)
        {
            _film.Number = number;
            await botClient.SendTextMessageAsync(_adminId, $"{resourceManager.GetString("numberAdded",_cultureInfo)}");
            commands["AddFilmNumberAsync"] = false;
            commands["AddFilmLinkAsync"] = true;
        }
       
        
    }

    public async Task AddFilmLinkAsync(ITelegramBotClient botClient, Update update)
    {
        _film.FilmId = Guid.NewGuid();
        _film.Link = update.Message.Text;
        await botClient.SendTextMessageAsync(_adminId, $"{resourceManager.GetString("linkAdded",_cultureInfo)}");
        await _unitOfWork.Films.AddAsync(_film);
        await _unitOfWork.CompleteAsync();
        await botClient.SendTextMessageAsync(_adminId, $"{resourceManager.GetString("linkAdded",_cultureInfo)}");
        string message = $"<b>FilmId</b>: {_film.FilmId}\n" +
                         $"<b>FilmName</b>: {_film.Name}\n" +
                         $"<b>FilmNumber</b>: {_film.Number}\n" +
                         $"<b>FilmLink</b>: {_film.Link}";
        await botClient.SendTextMessageAsync(_adminId, message, parseMode: ParseMode.Html);
        commands["AddFilmLinkAsync"] = false;
        
    }

    private async Task UserDeactive(long id)
    {
        var user = await _unitOfWork.Users.GetByTelegramIdAsync(id);
        if (user != null)
        {
            user.IsActive = false;
            await _unitOfWork.CompleteAsync();
        }

    }
    public async Task GetUsersCount(ITelegramBotClient botClient)
    {
        int deactiveUsers = 0;
        var users = await _unitOfWork.Users.GetAllAsync();
        var usersCount = users.Count();
        foreach (var user in users)
        {
            if (!user.IsActive)
            {
                deactiveUsers += 1;
            }            
        }
        await botClient.SendTextMessageAsync(_adminId, $"Count:{usersCount}\n Active: {usersCount-deactiveUsers} Deactive \ud83d\udc7b : {deactiveUsers}");
    }

  
}