using API.DTOs;
using API.Extensions;
using API.Interfaces;
using API.Models;
using API.Services;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalR
{
    [Authorize]
    public class MessageHub : Hub
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly IHubContext<PresenceHub> _presenceHub;

        public MessageHub(IUnitOfWork uow, IMapper mapper, IHubContext<PresenceHub> presenceHub)
        {
            _uow = uow;
            _mapper = mapper;
            _presenceHub = presenceHub;
        }

        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext(); //acesso ao httpContext pq para fazer uma conexao ao SignalR/Hub enviamos um http request
            var otherUser = httpContext.Request.Query["user"];  //faz um httpRequest com uma query string "user".. vai dar match com o httpRequest/query do client side(message.service, method createHubConnection())
            //resumo: vai buscar o nome do user que estamos connectado a trocar mensagens

            var groupName = GetGroupName(Context.User.GetUsername(), otherUser);
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            var group = await AddToGroup(groupName);

            await Clients.Group(groupName).SendAsync("UpdatedGroup", group);

            var messages = await _uow.MessageRepository.GetMessageThread(Context.User.GetUsername(), otherUser);

            if (_uow.HasChanges()) await _uow.Complete();

            await Clients.Caller.SendAsync("ReceiveMessageThread", messages); //quando um user se connectar a este messageHub/groupName, vai receber as mensagens
            //via/atraves do SignalR, em vez de fazer uma call à API para ir busca-las.. vai receber o chat entre estes 2 users
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var group = await RemoveFromMessageGroup();
            await Clients.Group(group.Name).SendAsync("UpdatedGroup");
            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(CreateMessageDto createMessageDto)
        {
            var username = Context.User.GetUsername();  //vai buscar APENAS o nome do user/current user q mandou msg

            if (username == createMessageDto.RecipientUsername.ToLower())
                //return BadRequest("You can't send messages to yourself"); como n estou dentro de um API CONTROLLER, n dá para dar return em http responses("BadRequest")
                throw new HubException("You can't send messages to yourself");

            var sender = await _uow.UserRepository.GetUserByNameAsync(username);    //vai buscar o OBJETO AppUser atraves do username, para aceder às props, pq só com username n dá pra aceder !
            var recipient = await _uow.UserRepository.GetUserByNameAsync(createMessageDto.RecipientUsername);

            if (recipient == null) throw new HubException("Not found user");

            var message = new Message
            {
                Sender = sender,
                Recipient = recipient,
                SenderUsername = sender.UserName,
                RecipientUsername = recipient.UserName,
                MessageContent = createMessageDto.MessageContent
            };

            var groupName = GetGroupName(sender.UserName, recipient.UserName);

            var group = await _uow.MessageRepository.GetMessageGroup(groupName);

            if (group.Connections.Any(x => x.Username == recipient.UserName))   //verifica se users estão no msm grupo/chat(se tem chat aberto)
            {
                message.DateRead = DateTime.UtcNow; //marca msg como vista
            }
            else //se ñ tiver com o chat/thread aberto(n tiver connectado ao hub/grupo)
            {
                var connections = await PresenceTracker.GetConnectionsForUser(recipient.UserName);  //ver se o destinatario da msg está onlione
                if (connections != null)
                {
                    await _presenceHub.Clients.Clients(connections).SendAsync("NewMessageReceived", new {username = sender.UserName, knowAs = sender.KnownAs}); //envia msg(toast no Angular/Client side)
                }
            }

            _uow.MessageRepository.AddMessage(message);

            if (await _uow.Complete())
            {
                await Clients.Group(groupName).SendAsync("NewMessage", _mapper.Map<MessageDto>(message));
            }
            else
            {
                throw new HubException("Failed to send message");
            }
        }

        private string GetGroupName(string caller, string receiver) //retorna uma string(GroupName) com os nomes dos 2 users em ordem alfabetica, para o GroupName ser sempre o mesmo e guardar/buscar as mensagens
        {
            var stringCompare = string.CompareOrdinal(caller, receiver) < 0;    //retorna true se caller<receiver (ver o q CompareOrdinal faz/retorna)
            return stringCompare ? $"{caller}-{receiver}" : $"{receiver}-{caller}"; //retorna "caller-receiver" se (caller<receiver) for true, else retorna receiver-caller
        }

        private async Task<Group> AddToGroup(string groupName)
        {
            var group = await _uow.MessageRepository.GetMessageGroup(groupName);
            var connection = new Connection(Context.ConnectionId, Context.User.GetUsername());

            if(group == null)
            {
                group = new Group(groupName);
                _uow.MessageRepository.AddGroup(group);
            }

            group.Connections.Add(connection);

            if (await _uow.Complete()) return group;

            throw new HubException("Failed to add to group");
        }

        private async Task<Group> RemoveFromMessageGroup()
        {
            var group = await _uow.MessageRepository.GetGroupForConnection(Context.ConnectionId);
            var connection = group.Connections.FirstOrDefault(c => c.ConnectionId == Context.ConnectionId);
            _uow.MessageRepository.RemoveConnection(connection);

            if (await _uow.Complete()) return group;

            throw new HubException("Failed to remove from group");
        }
    }
}
