using Application.DTOs;
using Application.Interfaces;
using AutoMapper;
using Core.Entities;
using Core.Parameters;
using Application.Responses;
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
        private readonly ICurrentUserService currentUserService;
        private readonly IMapper mapper;

        public MessageService(
            IUnitOfWork unitOfWork, 
            IMessageRepository messageRepository,
            ICurrentUserService currentUserService,
            IMapper mapper)
        {
            this.unitOfWork = unitOfWork;
            this.messageRepository = messageRepository;
            this.currentUserService = currentUserService;
            this.mapper = mapper;
        }

        public async Task SendAsync(int fromUserId, int toUserId, string action, int actionId, string content)
        {
            var messageRepository = unitOfWork.GetRepository<Message>();

            var message = new Message
            {
                Content = content,
                FromUserId = fromUserId,
                ToUserId = toUserId,
                Action = action,
                ActionId = actionId,
                SentDate = DateTime.UtcNow,
                Read = false
            };

            await messageRepository.AddAsync(message);
            await messageRepository.SaveChangesAsync();
        }


        public async Task<PaginationResponse<MessageDto>> GetForUserAsync(PaginationParams? pageParams)
        {
            var user = await currentUserService.GetAsync();
            var messages = await messageRepository.GetByUserIdAsync(user.Id, pageParams);

            var messagesDto = mapper.Map<IEnumerable<MessageDto>>(messages.Data);
            return new PaginationResponse<MessageDto>(
                messagesDto,
                messages.TotalCount,
                messages.PageNumber,
                messages.PageSize);
        }

        public async Task<MessageSummaryDto> GetSummaryForUser()
        {
            var pageParams = new PaginationParams() { PageNumber = 1, PageSize = 5 };

            var user = await currentUserService.GetAsync();
            var messages = await messageRepository.GetByUserIdAsync(user.Id, pageParams);
            var unreadCount = await messageRepository.GetUnreadCountByUserIdAsync(user.Id);

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

        public async Task<int> GetUnreadCountForUserAsync()
        {
            var user = await currentUserService.GetAsync();
            return await messageRepository.GetUnreadCountByUserIdAsync(user.Id);
        }

        public async Task MarkAsReadAsync(List<int> ids)
        {
            await messageRepository.MarkAsReadAsync(ids);
        }
    }
}
