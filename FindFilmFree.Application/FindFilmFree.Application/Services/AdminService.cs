using System.Globalization;
using System.Resources;
using FindFilmFree.Application.Abstraction.Interfaces;
using FindFilmFree.Domain.Models;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace FindFilmFree.Application.Services;

public class AdminService:IMainModerService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly long _adminId;
    private Film _film = default;
    ResourceManager resourceManager = new ResourceManager("FindFilmFree.Application.Resources.Language", typeof(AdminService).Assembly);
    private CultureInfo _cultureInfo = new CultureInfo("default");
    private int filmNumber = 0;
    private Dictionary<string,bool> commands = new Dictionary<string, bool>()
    {
        {"SendAllUsersTextMessageAsync",false},
        {"AddFilmNameAsync",false},
        {"AddFilmNumberAsync",false},
        {"AddFilmLinkAsync",false},
        {"SetModerAsync",false},
        {"DeleteModerAsync",false},
        {"changeName",false},
        {"updateFilmData",false},
        {"changeNumber",false},
        {"changeLink",false},
        {"deleteFilm",false},
    };

    public AdminService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
    public AdminService(IUnitOfWork unitOfWork,long adminId)
    {
        _unitOfWork = unitOfWork;
        _adminId = adminId;
    }

    public async Task HandleAdminCommands(ITelegramBotClient client, Update update)
    {
        if (update.Type == UpdateType.CallbackQuery)
        {
            await HandleCallBackQuery(client,update.CallbackQuery);
        }
        var chatId = update.Message.Chat.Id;
        var admin = await _unitOfWork.Users.GetByTelegramIdAsync(chatId);
        
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
        else if (commands["SetModerAsync"])
        {
           await SetModerAsync(client,update);
        } 
        
        else if (commands["DeleteModerAsync"])
        {
           await DeleteModerAsync(client,update);
        }
        else if (commands["changeName"])
        {
           await ChangeName(client,update);
        }
        else if (commands["changeNumber"])
        {
           await ChangeNumber(client,update);
        }
        else if (commands["changeLink"])
        {
           await ChangeLink(client,update);
        }else if (commands["deleteFilm"])
        {
           await DeleteFilm(client,update);
        }
        else if (commands["DeleteModerAsync"])
        {
           await DeleteModerAsync(client,update);
        }
        
        
        else if (commands["updateFilmData"])
        {
            bool numberParse = int.TryParse(update.Message.Text, out filmNumber);
            if (numberParse)
            {
                var film = await _unitOfWork.Films.GetByNumber(filmNumber);
                if (film!=null)
                {
                    string message = $"<b>FilmId</b>: {film.FilmId}\n" +
                                     $"<b>FilmName</b>: {film.Name}\n" +
                                     $"<b>FilmNumber</b>: {film.Number}\n" +
                                     $"<b>FilmLink</b>: {film.Link}";
                    await client.SendTextMessageAsync(update.Message.Chat.Id, message, parseMode: ParseMode.Html);
                    await UpdateFilmData(client, update);
                }
                else
                {
                    await client.SendTextMessageAsync(update.Message.Chat.Id, "Film Not Found");
                }
            }
           
        }
        switch (messageText)
        {
               
            case "/sendMsg":
                if (chatId== _adminId)
                {
                    commands["SendAllUsersTextMessageAsync"] = true;
                    await client.SendTextMessageAsync(update.Message.Chat.Id,
                        $"{resourceManager.GetString("sentMessage", _cultureInfo)}");
                    
                }
                break;
            case "/updateFilmData":
                
                    commands["updateFilmData"] = true;
                    await client.SendTextMessageAsync(update.Message.Chat.Id,
                        $"send Film number");
                break;
               
            case "/addfilm":
              await  AddFilmAsync(client,update);
                break;
            case "/setModer":
                if (chatId == _adminId)
                {
                    commands["SetModerAsync"] = true;
                    await client.SendTextMessageAsync(update.Message.Chat.Id,
                        $"{resourceManager.GetString("sentModerUserName", _cultureInfo)}");
                }

                break; 
            case "/deleteModer":
                if (chatId == _adminId)
                {
                    commands["DeleteModerAsync"] = true;
                    await client.SendTextMessageAsync(update.Message.Chat.Id,
                        $"{resourceManager.GetString("sentModerUserName", _cultureInfo)}");
                }

                break;
            case "/getusersinfo":
                if (chatId == _adminId)
                {
                    await GetUsersCount(client);
                }
                break;
        }
    }

    public async Task SendAllUsersTextMessageAsync(ITelegramBotClient botClient, string text)
    {
        long erorrId = 0;
        int msgCount = 0;


        var userCount = await _unitOfWork.Users.GetUsersCount();
            
            
           
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
                            await botClient.SendTextMessageAsync(_adminId, ex.Message);
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

    public async Task AddFilmAsync(ITelegramBotClient botClient,Update update)
    {
        _film = new Film();
        commands["AddFilmNameAsync"] = true;
        await botClient.SendTextMessageAsync(update.Message.Chat.Id, $"{resourceManager.GetString("addFilmName",_cultureInfo)}");
    }

    public async Task AddFilmNameAsync(ITelegramBotClient botClient, Update update)
    {
        _film.Name = update.Message.Text;
        await botClient.SendTextMessageAsync(update.Message.Chat.Id, $"{resourceManager.GetString("nameAdded",_cultureInfo)}");
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
            await botClient.SendTextMessageAsync(update.Message.Chat.Id, $"{resourceManager.GetString("numberAdded",_cultureInfo)}");
            commands["AddFilmNumberAsync"] = false;
            commands["AddFilmLinkAsync"] = true;
        }
       
        
    }

    public async Task AddFilmLinkAsync(ITelegramBotClient botClient, Update update)
    {
        _film.FilmId = Guid.NewGuid();
        _film.Link = update.Message.Text;
        await _unitOfWork.Films.AddAsync(_film);
        await _unitOfWork.CompleteAsync();
        await botClient.SendTextMessageAsync(update.Message.Chat.Id, $"{resourceManager.GetString("linkAdded",_cultureInfo)}");
        string message = $"<b>FilmId</b>: {_film.FilmId}\n" +
                         $"<b>FilmName</b>: {_film.Name}\n" +
                         $"<b>FilmNumber</b>: {_film.Number}\n" +
                         $"<b>FilmLink</b>: {_film.Link}";
        await botClient.SendTextMessageAsync(update.Message.Chat.Id, message, parseMode: ParseMode.Html);
        commands["AddFilmLinkAsync"] = false;
        
    }

    public async Task UpdateFilmData(ITelegramBotClient botClient, Update update)
    {
        InlineKeyboardMarkup inlineKeyboard = new(new[]
        {
            // first row
            new []
            {
                InlineKeyboardButton.WithCallbackData(text: "Change Name \ud83d\udcdb", callbackData: "changeName"),
                InlineKeyboardButton.WithCallbackData(text: "Change Number #\ufe0f\u20e3", callbackData: "changeNumber"),
            },
            // second row
            new []
            {
                InlineKeyboardButton.WithCallbackData(text: "Change Link \ud83d\udd17", callbackData: "changeLink"),
                InlineKeyboardButton.WithCallbackData(text: "Delete ❌", callbackData: "deleteFilm"),
            },
        });
        await botClient.SendTextMessageAsync(update.Message.Chat.Id, "choose", replyMarkup: inlineKeyboard);
    }


    public async Task SetModerAsync(ITelegramBotClient botClient, Update update)
    {
        var user = await _unitOfWork.Users.GetByUserName(update.Message.Text);
        if (user!=null)
        {
            user.IsModer = true;
            await _unitOfWork.CompleteAsync();
            await botClient.SendTextMessageAsync(update.Message.Chat.Id, "Enjoy!");
            await botClient.SendTextMessageAsync(user.TelegramId, "Moder status Updated /start to more information!");
        }
        else
        {
            await botClient.SendTextMessageAsync(update.Message.Chat.Id, "User not exist!");
        }
        commands["SetModerAsync"] = false;
    }
    public async Task DeleteModerAsync(ITelegramBotClient botClient, Update update)
    {
        var user = await _unitOfWork.Users.GetByUserName(update.Message.Text);
        if (user!=null)
        {
            user.IsModer = false;
            await _unitOfWork.CompleteAsync();
            await botClient.SendTextMessageAsync(update.Message.Chat.Id, "Enjoy!");
            await botClient.SendTextMessageAsync(user.TelegramId, "Moder status Updated /start to more information!");
        }
        else
        {
            await botClient.SendTextMessageAsync(update.Message.Chat.Id, "User not exist!");
        }
        commands["DeleteModerAsync"] = false;
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

    public async Task ChangeName(ITelegramBotClient botClient, Update update)
    {
        var film = await _unitOfWork.Films.GetByNumber(filmNumber);
        if (film!=null)
        {
            film.Name = update.Message.Text;
            await _unitOfWork.CompleteAsync();
            await botClient.SendTextMessageAsync(update.Message.Chat.Id, "Enjoy!");
           
        }
        else
        {
            await botClient.SendTextMessageAsync(update.Message.Chat.Id, "Film Not Found");
        }
        commands["changeName"] = false;
        
    }
    public async Task ChangeNumber(ITelegramBotClient botClient, Update update)
    {
        var film = await _unitOfWork.Films.GetByNumber(filmNumber);
        if (film!=null)
        {
            int newNumber = 0;
            bool intParse = int.TryParse(update.Message.Text, out newNumber);
            if (intParse)
            {
                film.Number = newNumber;
                await _unitOfWork.CompleteAsync();
                await botClient.SendTextMessageAsync(update.Message.Chat.Id, "Enjoy!");
            }
            else
            {
                await botClient.SendTextMessageAsync(update.Message.Chat.Id, "Отправьте только номер!");
            }
           
            
        }
        else
        {
            await botClient.SendTextMessageAsync(update.Message.Chat.Id, "Film Not Found");
        }
        commands["changeNumber"] = false;
    }
    public async Task ChangeLink(ITelegramBotClient botClient, Update update)
    {
        var film = await _unitOfWork.Films.GetByNumber(filmNumber);
        if (film!=null)
        {
            film.Link = update.Message.Text;
            await _unitOfWork.CompleteAsync();
            await botClient.SendTextMessageAsync(update.Message.Chat.Id, "Enjoy!");
            
        }
        else
        {
            await botClient.SendTextMessageAsync(update.Message.Chat.Id, "Film Not Found");
        }
        commands["changeLink"] = false;
    }
    public async Task DeleteFilm(ITelegramBotClient botClient, Update update)
    {
        var film = await _unitOfWork.Films.GetByNumber(filmNumber);
        if (film!=null)
        {
           bool delete = await _unitOfWork.Films.DeleteByNumber(filmNumber);
           if (delete)
           {
               await _unitOfWork.CompleteAsync();
               await botClient.SendTextMessageAsync(update.Message.Chat.Id, "Enjoy!");
           }
           else
           {
               await botClient.SendTextMessageAsync(update.Message.Chat.Id, "Error!");
           }
          
            
        }
        else
        {
            await botClient.SendTextMessageAsync(update.Message.Chat.Id, "Film Not Found");
        }

        commands["deleteFilm"] = false;

    }

    public async Task HandleCallBackQuery(ITelegramBotClient client,CallbackQuery callbackQuery)
    {
        var date = callbackQuery.Data;
        var chatId = callbackQuery.Message.Chat.Id;
        switch (date)
        {
            case "changeName":
                commands["changeName"] = true;
                await client.SendTextMessageAsync(chatId, "Send a  new Name");
                break; 
            case "changeNumber":
                commands["changeNumber"] = true;
                await client.SendTextMessageAsync(chatId, "Send a  new Number");
                break;  
            case "changeLink":
                commands["changeLink"] = true;
                await client.SendTextMessageAsync(chatId, "Send a  new Link");
                break;
            case "deleteFilm":
                commands["deleteFilm"] = true;
                await client.SendTextMessageAsync(chatId, "Send a delete to Delete!");
                break;
            default:
                await Task.CompletedTask;
                break;
        }
    }
    
    
    
    

  
}