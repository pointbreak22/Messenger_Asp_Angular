using System.Collections.Concurrent;
using ChatApi.Data;
using ChatApi.DTOs;
using ChatApi.Extensions;
using ChatApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace ChatApi.Hubs;

[Authorize]
public class ChatHub: Hub
{
    public static readonly ConcurrentDictionary<string, OnlineUserDto> onlineUsers = new();
    private readonly AppDbContext _context;

    private readonly UserManager<AppUser> _userManager;

    // Конструктор
    public ChatHub(UserManager<AppUser> userManager, AppDbContext context)
    {
        _userManager = userManager;
        _context = context;
    }

    public override async Task OnConnectedAsync()
    {
        var httpContext = Context.GetHttpContext();
        var receiverId = httpContext?.Request.Query["senderId"].ToString();
        var userName = Context.User!.Identity!.Name!;
        var currentUser = await _userManager.FindByEmailAsync(userName);
        var connectionId = Context.ConnectionId;
        if (onlineUsers.ContainsKey(userName))
        {
            onlineUsers[userName].ConnectionId = connectionId;            

        }
        else
        {
            var user = new OnlineUserDto
            {
                ConnectionId = connectionId,
                UserName = userName,
                ProfilePicture = currentUser!.ProfileImage,
                FullName = currentUser.FullName


            };
            onlineUsers.TryAdd(userName, user);
            await Clients.AllExcept(connectionId).SendAsync("Notify", currentUser);
            

        }

        if (!string.IsNullOrEmpty(receiverId))
        {
            await LoadMessages(receiverId);
        }
        
        await Clients.All.SendAsync("OnlineUsers", await GetAllUsers());

    } 
    public async Task LoadMessages(string recipientId, int pageNumber = 1)
    {
        int pagesize = 10;
        var username = Context.User!.Identity!.Name;
        var currentUser = await _userManager.FindByNameAsync(username!);

        if (currentUser is null)
        {
               return; 
        }

        List<MessageResponseDto> messages = await _context.Messages.Where(x =>
                x.ReceiverId == currentUser!.Id && x.SenderId == recipientId || x.SenderId == currentUser!.Id
                && x.ReceiverId == recipientId).OrderByDescending(x => x.CreatedDate)
            .Skip((pageNumber - 1) * pagesize).Take(pagesize).OrderBy(x => x.CreatedDate)
            .Select(x => new MessageResponseDto
            {
                Id = x.Id,
                Content = x.Content,
                CreatedDate = x.CreatedDate,
                ReceiverId = x.ReceiverId,
                SenderId = x.SenderId
            }).ToListAsync();

        foreach (var message in messages)
        {
            var msg = await _context.Messages.FirstOrDefaultAsync(x=>x.Id==message.Id);
            if (msg != null && msg.ReceiverId == currentUser.Id)
            {
                msg.IsRead = true;
                await _context.SaveChangesAsync();
                
            }
        }

        await Clients.User(currentUser.Id).SendAsync("ReceiveMessageList", messages);
    }
    public async Task SendMessage(MessageRequestDto message)
    {
        var senderId = Context.User!.Identity!.Name;
        var recipientId = message.ReceiverId;
        var newMsg = new Message
        {
            Sender = await _userManager.FindByNameAsync(senderId!),
            Receiver = await _userManager.FindByIdAsync(recipientId!),
            IsRead = false,
            CreatedDate = DateTime.UtcNow,
            Content = message.Content
        };
        _context.Messages.Add(newMsg);
        await _context.SaveChangesAsync();

        await Clients.User(recipientId!).SendAsync("ReceiveNewMessage",newMsg);
        
    }

    public async Task NotifyTyping(string recipientUserName)
    {
        var senderUserName = Context.User!.Identity!.Name;
        if (senderUserName is null)
        {
            return;
        }

        var connectionId = onlineUsers.Values.FirstOrDefault(x => x.UserName == recipientUserName)?.ConnectionId;
        if (connectionId != null)
        {
            await Clients.Client(connectionId).SendAsync("NotifyTypingToUser",senderUserName);
       
            
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var username = Context.User!.Identity!.Name;
        onlineUsers.TryRemove(username!, out _);
        await Clients.All.SendAsync("OnlineUsers", await GetAllUsers());
        
        


    }

    private async Task<IEnumerable<OnlineUserDto>> GetAllUsers()
    {
        var username = Context.User!.GetUserName();
        var onlineUsersSet = new HashSet<string>(onlineUsers.Keys);
        var users = await _userManager.Users.Select(u => new OnlineUserDto
        {
            Id = u.Id,
            UserName = u.UserName,
            FullName = u.FullName,
            ProfilePicture = u.ProfileImage,
            IsOnline = onlineUsersSet.Contains(u.UserName!),
            UnreadCount = _context.Messages.Count(x => x.ReceiverId == username && x.SenderId == u.Id && !x.IsRead)
        }).OrderByDescending(u => u.IsOnline).ToListAsync();

        return users;
    }
}