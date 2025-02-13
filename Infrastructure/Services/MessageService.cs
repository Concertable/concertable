using Application.DTOs;
using Application.Interfaces;
using AutoMapper;
using Core.Entities;
using Core.Parameters;
using Core.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class MessageService : IMessageService
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IMessageRepository messageRepository;
        private readonly IAuthService authService;
        private readonly IMapper mapper;

        public MessageService(
            IUnitOfWork unitOfWork, 
            IMessageRepository messageRepository,
            IAuthService authService,
            IMapper mapper)
        {
            this.unitOfWork = unitOfWork;
            this.messageRepository = messageRepository;
            this.authService = authService;
            this.mapper = mapper;
        }

        public async Task SendAsync(int fromUserId, int toUserId, string action, string content)
        {
            var messageRepository = unitOfWork.GetRepository<Message>();

            var message = new Message
            {
                Content = content,
                FromUserId = fromUserId,
                ToUserId = toUserId,
                Action = action,
                SentDate = DateTime.Now,
                Read = false
            };

            await messageRepository.AddAsync(message);
        }

        public async Task<PaginationResponse<MessageDto>> GetAllForUserAsync(PaginationParams? pageParams)
        {
            var user = await authService.GetCurrentUserAsync();
            var messages = await messageRepository.GetAllForUserAsync(user.Id, pageParams);

            var messagesDto = mapper.Map<IEnumerable<MessageDto>>(messages.Data);
            return new PaginationResponse<MessageDto>(
                messagesDto,
                messages.TotalCount,
                messages.PageNumber,
                messages.PageSize);
        }

        public async Task<MessageSummaryDto> GetSummaryForUser(PaginationParams? pageParams)
        {
            var user = await authService.GetCurrentUserAsync();
            var messages = await messageRepository.GetAllForUserAsync(user.Id, pageParams);
            var unreadCount = await messageRepository.GetUnreadCountForUserAsync(user.Id);

            var messagesDto = mapper.Map<IEnumerable<MessageDto>>(messages.Data);

            return new MessageSummaryDto
            {
                Messages = new PaginationResponse<MessageDto>(
                    messagesDto,
                    messages.TotalCount,
                    messages.PageNumber,
                    messages.PageSize
                ),
                UnreadCount = unreadCount
            };
        }

    }
}
