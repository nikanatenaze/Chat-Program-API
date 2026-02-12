using ChatAppAPI.Models;
using Microsoft.AspNetCore.SignalR;

namespace ChatAppAPI.Hubs
{
    public class MainHub : Hub
    {
        // Baisic operations
        public override async Task OnConnectedAsync()
        {
            string userId = Context.UserIdentifier ?? Context.ConnectionId;
            await Groups.AddToGroupAsync(Context.ConnectionId, "AllUsers");
            await Clients.Caller.SendAsync("Connected", $"Welcome {userId}!");
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "AllUsers");
            await base.OnDisconnectedAsync(exception);
        }

        public async Task JoinGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            await Clients.Group(groupName).SendAsync("GroupNotification", $"{Context.ConnectionId} joined {groupName}");
        }

        public async Task LeaveGroup(string groupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            await Clients.Group(groupName).SendAsync("GroupNotification", $"{Context.ConnectionId} left {groupName}");
        }

        // Chat operations
        public async Task JoinChat(int chatId)
        {
            await Groups.AddToGroupAsync(
                Context.ConnectionId,
                $"chat-{chatId}"
            );
        }

        public async Task LeaveChat(int chatId)
        {
            await Groups.RemoveFromGroupAsync(
                Context.ConnectionId,
                $"chat-{chatId}"
            );
        }

        // Chat users operations
        public async Task JoinChatUsers(int userId)
        {
            await Groups.AddToGroupAsync(
                Context.ConnectionId,
                $"chat-users-{userId}"
            );
        }

        public async Task LeaveChatUsers(int userId)
        {
            await Groups.RemoveFromGroupAsync(
                Context.ConnectionId,
                $"chat-users-{userId}"
            );
        }
    }
}
