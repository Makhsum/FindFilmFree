using System.Globalization;
using System.Resources;
using FindFilmFree.Application.Abstraction.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using User = FindFilmFree.Domain.Models.User;

namespace FindFilmFree.Application.Services;

public class BotTelegramService
{
    private readonly ITelegramBotClient _botClient;
    private readonly IUnitOfWork _unitOfWork;
    private  readonly long _adminChatId;
    private User _user = default;
    private AdminService _adminService = default;
    private ModerService _moderService = default;
    private bool sendMessage = false;
    private string[] moderCommands = { }; 
  
    ResourceManager resourceManager = new ResourceManager("FindFilmFree.Application.Resources.Language", typeof(BotTelegramService).Assembly);

    
    public BotTelegramService(ITelegramBotClient client,IUnitOfWork unitOfWork,long ownerId)
    {
        _botClient = client;
        _unitOfWork = unitOfWork;
        _adminService = new AdminService(unitOfWork, ownerId);
        _moderService = new ModerService(unitOfWork);
        _adminChatId = ownerId;
    }
    
    public async Task StartAsync()
    {
        CancellationTokenSource cts = new();
        ReceiverOptions receiverOptions = new();
        _botClient.StartReceiving(
            updateHandler: HandleUpdateAsync,
            pollingErrorHandler: HandlePollingErrorAsync,
            receiverOptions: receiverOptions,
            cancellationToken: cts.Token
        );
        var commands = new[]
        {
            new BotCommand { Command = "/start", Description = "Начать с начала" },
            new BotCommand { Command = "/lang", Description = "Выбрать язык" },
            new BotCommand { Command = "/referal", Description = "Реферальная ссылка" },
            new BotCommand { Command = "/friendscount", Description = "Кол-во друзей" },
            // Добавьте сюда другие команды
        };
        
        await _botClient.SetMyCommandsAsync(commands);
        
        var me = await _botClient.GetMeAsync();
        Console.WriteLine($"Start listening for @{me.Username}");
        Console.ReadLine();
        cts.Cancel();
    }

    private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update,
        CancellationToken cancellationToken)
    {
       
        List<Task> tasks = new List<Task>();
       
        
        if (update.Type == UpdateType.CallbackQuery)
        {
            _user = await _unitOfWork.Users.GetByTelegramIdAsync(update.CallbackQuery.Message.Chat.Id);
            if (_user!=null)
            {
                if (update.CallbackQuery.Message.Chat.Id==_adminChatId)
                {
                   tasks.Add(_adminService.HandleAdminCommands(botClient,update));
                }

                if (_user.IsActive)
                {
                    tasks.Add(  _moderService.HandleAdminCommands(botClient, update));
                }
            }
            var callbackquery = update.CallbackQuery;
            try
            {
                tasks.Add(HandleCallBackQueryAsync(callbackquery));
            }
            catch (Exception ex)
            {
                await _botClient.SendTextMessageAsync(_adminChatId, ex.Message);
                if (ex.Message.Contains("bot was blocked by the user"))
                {
                    //  await UserDeactive(update.CallbackQuery.Message.Chat.Id);
                    tasks.Add(UserDeactive(update.CallbackQuery.Message.Chat.Id));
                }
                await botClient.SendTextMessageAsync(callbackquery.Message.Chat.Id,
                    $"{resourceManager.GetString("error",new CultureInfo("default"))}");
            }
        }
        if (update.Message is not { } message)
            return;
        // Only process text messages
        if (message.Text is not { } messageText)
            return;
        CultureInfo culture = CultureInfo.InvariantCulture;
        var chatId = message.Chat.Id;
        var userr = await _unitOfWork.Users.GetByTelegramIdAsync(chatId);
        if (userr!=null)
        {
            culture = new CultureInfo(userr.Language);
            if (userr.IsModer)
            {
                tasks.Add( _moderService.HandleAdminCommands(botClient,update));
            }
        }
        tasks.Add(HandleCommands(update));
        
        if (chatId==_adminChatId)
        {
           
            tasks.Add( _adminService.HandleAdminCommands(botClient,update));
        }

      await  Task.WhenAll(tasks);

    }


    private async Task HandleCallBackQueryAsync(CallbackQuery callbackQuery)
        {
            CultureInfo culture = new CultureInfo("default");
            var chatId = callbackQuery.Message.Chat.Id;
            var user = await _unitOfWork.Users.GetByTelegramIdAsync(chatId);
            
            if (user!=null)
            {
                if (chatId==_adminChatId)
                {
                    
                }
                  culture = new CultureInfo(user.Language);
            }
            if (await IsMemberOfChannel(chatId))
            {
                var messageId = callbackQuery.Message.MessageId; 
                var data = callbackQuery.Data;
            try
            {
               
                switch (data)
                {
                    case "chatMember":
                        if (await IsMemberOfChannel(chatId))
                        {
                            
                            await _botClient.SendTextMessageAsync(chatId,
                                $"{resourceManager.GetString("congratulationMember",culture)}");
                            await _botClient.SendTextMessageAsync(chatId,
                                "\u2764\ufe0f");
                        }
                        
                        break;
                   
                   case "ru":
                     var selectedruuser =await   _unitOfWork.Users.GetByTelegramIdAsync(chatId);
                     if (selectedruuser!=null)
                     {
                         selectedruuser.Language = "ru";
                         culture = new CultureInfo("ru");
                         await _unitOfWork.CompleteAsync();
                         await _botClient.SendTextMessageAsync(chatId,
                             resourceManager.GetString("languageChanged", culture));
                     }
                     break;
                    case "default":
                        var selectedenuser =await   _unitOfWork.Users.GetByTelegramIdAsync(chatId);
                        if (selectedenuser!=null)
                        {
                            selectedenuser.Language = "default";
                            culture = new CultureInfo("default");
                            await _unitOfWork.CompleteAsync();
                            await _botClient.SendTextMessageAsync(chatId,
                                resourceManager.GetString("languageChanged", culture));
                        }
                        break;
                    case "de":
                        var selecteddeuser =await   _unitOfWork.Users.GetByTelegramIdAsync(chatId);
                        if (selecteddeuser!=null)
                        {
                            selecteddeuser.Language = "de";
                            culture = new CultureInfo("de");
                            await _unitOfWork.CompleteAsync();
                            await _botClient.SendTextMessageAsync(chatId,
                                resourceManager.GetString("languageChanged", culture));
                        }
                        break;
                    default:
                        try
                        {
                           
                            
                           
                           
                            
                        }
                        catch (Exception e)
                        {
                            
                            await _botClient.SendTextMessageAsync(chatId, $"{resourceManager.GetString("error",culture)}");
                            await _botClient.SendTextMessageAsync(_adminChatId, e.Message);
                        }
                       
                        break;
                }
            }
            catch (Exception e)
            {
                if (e.Message.Contains("bot was blocked by the user"))
                {
                    await UserDeactive(chatId);
                }

                await _botClient.SendTextMessageAsync(_adminChatId, e.Message);
                Console.WriteLine(e.Message);
                return;
            }
            }
           
        }
    
    
      private async Task HandleCommands(Update update)
        {
            ReplyMarkupBase keyboardMarkup = new ReplyKeyboardRemove();
            var chatId = update.Message.Chat.Id;
            var messageText = update.Message.Text;
            CultureInfo culture = new CultureInfo("default");
            var userr = await _unitOfWork.Users.GetByTelegramIdAsync(chatId);
            if (userr!=null)
            {
                culture = new CultureInfo(userr.Language);
           
               
            } 
            try
                {
                    
                    if (messageText.StartsWith("/start"))
                    {
                        if (chatId==_adminChatId)
                        {
                            
                            keyboardMarkup =   new ReplyKeyboardMarkup(new[]
                            {
                                new KeyboardButton[] { "/sendMsg" },
                                new KeyboardButton[] { "/addfilm" },
                                new KeyboardButton[] { "/getusersinfo" },
                                new KeyboardButton[] { "/setModer" },
                                new KeyboardButton[] { "/deleteModer" },
                                new KeyboardButton[] { "/updateFilmData" },
                            })
                            {
                                ResizeKeyboard = true
                            };
                        }
                        
                        else if (userr!=null&& userr.IsModer)
                        {
                            keyboardMarkup =   new ReplyKeyboardMarkup(new[]
                            {
                                new KeyboardButton[] { "/addfilm" },
                                new KeyboardButton[] { "/updateFilmData" },
                                
                            })
                            {
                                ResizeKeyboard = true
                            };
                        }
                        await _botClient.SendTextMessageAsync(chatId, $"{resourceManager.GetString("hiMsg", culture)} {update.Message.Chat.Username??update.Message.Chat.FirstName} {resourceManager.GetString("startMsg", culture)} \ud83c\udf7f",replyMarkup:keyboardMarkup);
                        if (await IsUserExist(chatId))
                        {
                            var user = await _unitOfWork.Users.GetByTelegramIdAsync(chatId) ;
                            if (!user.IsActive)
                            {
                                user.IsActive = true;
                                await _unitOfWork.CompleteAsync();
                            }
                            return;
                        }
                        else
                        {
                            User user = new User();
                            user.TelegramId = update.Message.Chat.Id;
                            user.UserName = update.Message.Chat.Username ?? " ";
                            user.Name = update.Message.Chat.FirstName ?? " ";
                            user.LastName = update.Message.Chat.LastName ?? " ";
                            user.IsActive = true;
                            user.ReferrerId = update.Message.Chat.Id;
                            user.Language = "default";
                            user.IsModer = false;
                            long referrerId = 0;

                            if (messageText.Length > 7)
                            {
                                var referrerString = messageText.Substring(7);
                                if (long.TryParse(referrerString, out referrerId))
                                {
                                    var frindUser = await _unitOfWork.Users.GetByTelegramIdAsync(referrerId);
                                    if (frindUser!=null)
                                    {
                                        if (user.ReferrerId!=referrerId)
                                        {
                                            user.FriendReferrerId = referrerId;
                                            await _botClient.SendTextMessageAsync(referrerId, $"{user.UserName} {resourceManager.GetString("registered",culture)}");
                                        }
                                       
                                    }
                                    
                                 
                                }
                            }
                            await _unitOfWork.Users.AddAsync(user);
                            await _unitOfWork.CompleteAsync();
                            return;
                          // 
                        }
                    
                        

                       
                    }
                    switch (update.Message.Text)
                    {
                        
                       case "/lang":
                           InlineKeyboardMarkup inlineKeyboard = new(new[]
                           {
                               // first row
                               new []
                               {
                                   InlineKeyboardButton.WithCallbackData(text: "en - \ud83c\uddec\ud83c\udde7", callbackData: "default"),
                                   InlineKeyboardButton.WithCallbackData(text: "de - \ud83c\udde9\ud83c\uddea", callbackData: "de"),
                               },
                               // second row
                               new []
                               {
                                   InlineKeyboardButton.WithCallbackData(text: "ru - 🇷🇺", callbackData: "ru"),
                               },
                           });
                           await _botClient.SendTextMessageAsync(chatId,
                               $"{resourceManager.GetString("chooseLanguage", culture)}",replyMarkup:inlineKeyboard);
                            break;
                       case "/referal":
                           var me = await _botClient.GetMeAsync();
                           await _botClient.SendTextMessageAsync(chatId, $"{resourceManager.GetString("refLink",culture)} https://t.me/{me.Username}?start={chatId}");
                           break;
                       case "/friendscount":
                           var users = await _unitOfWork.Users.GetAllAsync();
                           var friendsCount = users.Where(u => u.FriendReferrerId == chatId).Count();
                           await _botClient.SendTextMessageAsync(chatId, $"{resourceManager.GetString("friendsCount",culture)} {friendsCount}");
                           break;
                       case "/help":
                           await _botClient.SendTextMessageAsync(chatId,$"{resourceManager.GetString("defaultMsg",culture)}");
                           break;
                       default:
                            if (chatId==_adminChatId)return;
                            if (userr!=null)
                            {
                                if (userr.IsModer)
                                {
                                    return;
                                }
                            }
                            if (await IsMemberOfChannel(update.Message.Chat.Id))
                            {
                                int filmNumber = 0;
                                bool intParse = int.TryParse(update.Message.Text, out filmNumber);
                                if (intParse)
                                {
                                      var film =  await  _unitOfWork.Films.GetByNumber(filmNumber);
                                      if (film!=null)
                                      {
                                          string filmDate = 
                                              $"<b>Name</b>: {film.Name}\n" +
                                              $"<b>Number</b>: {film.Number}\n" +
                                              $"<b>Link</b>: {film.Link}";
                                          await _botClient.SendTextMessageAsync(chatId, filmDate, parseMode: ParseMode.Html);
                                          return;
                                      }
                                      else
                                      {
                                          await _botClient.SendTextMessageAsync(chatId, $"{resourceManager.GetString("numbernotexist",culture)}");
                                          return;
                                      }
                                    
                                }
                                else
                                {
                                    await _botClient.SendTextMessageAsync(chatId,$"{resourceManager.GetString("error",culture)}");
                                    return;
                                }

                               
                            }

                            break;

                    }
                }
                catch (Exception e)
                {
                    if (e.Message.Contains("bot was blocked by the user"))
                    {
                        await UserDeactive(chatId);
                    }
                    await _botClient.SendTextMessageAsync(_adminChatId, e.Message);
                }
            
        }

        private List<List<ReplyKeyboardMarkup>> CreateKeyboard(string[] commands)
        {
            List<List<ReplyKeyboardMarkup>> _keyboard = new List<List<ReplyKeyboardMarkup>>();


            foreach (var co in commands)
            {
                _keyboard.Add(new List<ReplyKeyboardMarkup>(){new ReplyKeyboardMarkup(co)});
            }

            return _keyboard;
        }
    
    private Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception,CancellationToken cancellationToken)
    {
        var errorMessage = exception switch
        {
            ApiRequestException apiRequestException
                => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };
        

        Console.WriteLine(errorMessage);
        return Task.CompletedTask;
    }
    
    private async Task<bool> IsMemberOfChannel(long chatId)
    {
        try
        {
            CultureInfo cultureInfo = new CultureInfo("default");
            var user = await _unitOfWork.Users.GetByTelegramIdAsync(chatId);
            if (user!=null)
            {
                cultureInfo = new CultureInfo(user.Language);
            }
            InlineKeyboardMarkup inlineKeyboard = new(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithUrl(
                        text: $"{resourceManager.GetString("subscribe_to_the_channel",cultureInfo)}",
                        url: "https://t.me/painfull_truth"),
                    InlineKeyboardButton.WithCallbackData(text: $"{resourceManager.GetString("check_your_subscription",cultureInfo)}", "chatMember")
                }

            });
            var chatMember = await _botClient.GetChatMemberAsync("@painfull_truth", chatId);

            if (chatMember.Status == ChatMemberStatus.Creator ||
                chatMember.Status == ChatMemberStatus.Member)
            {
                return true;
            }

            await _botClient.SendTextMessageAsync(chatId,
                $"{resourceManager.GetString("subscribePls",cultureInfo)}", replyMarkup: inlineKeyboard);

        }
        catch (Exception e)
        {
            await _botClient.SendTextMessageAsync(_adminChatId, e.Message);
            if (e.Message.Contains("bot was blocked by the user"))
            {
                await UserDeactive(chatId);
            }

        }

        return false;


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
    private async Task<bool> IsUserExist(long userId)
    {
        var user = await _unitOfWork.Users.GetByTelegramIdAsync(userId);
        if (user != null)
        {
            return true;
        }

        return false;

    }
    
}