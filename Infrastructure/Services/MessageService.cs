using Core.Interfaces;
using Application.DTOs;
using Application.Interfaces;
using Application.Mappers;
using Core.Entities;
using Core.Parameters;
using Application.Responses;

namespace Infrastructure.Services
{
    public class MessageService : IMessageService
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IMessageRepository messageRepository;
        private readonly ICurrentUserService currentUserService;
        private readonly TimeProvider timeProvider;

        public MessageService(
            IUnitOfWork unitOfWork,
            IMessageRepository messageRepository,
            ICurrentUserService currentUserService,
            TimeProvider timeProvider)
        {
            this.unitOfWork = unitOfWork;
            this.messageRepository = messageRepository;
            this.currentUserService = currentUserService;
            this.timeProvider = timeProvider;

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
                SentDate = timeProvider.GetUtcNow().DateTime,
                Read = false
            };

            await messageRepository.AddAsync(message);
        }

        public async Task SendAndSaveAsync(int fromUserId, int toUserId, string action, int actionId, string content)
        {
            var messageRepository = unitOfWork.GetRepository<Message>();

            var message = new Message
            {
                Content = content,
                FromUserId = fromUserId,
                ToUserId = toUserId,
                Action = action,
                ActionId = actionId,
                SentDate = timeProvider.GetUtcNow().DateTime,
                Read = false
            };

            await messageRepository.AddAsync(message);
            await messageRepository.SaveChangesAsync();
        }

        public async Task<Pagination<MessageDto>> GetForUserAsync(IPageParams pageParams)
        {
            var user = await currentUserService.GetAsync();
            var messages = await messageRepository.GetByUserIdAsync(user.Id, pageParams);

            return new Pagination<MessageDto>(
                messages.Data.Select(m => m.ToDto()),
                messages.TotalCount,
                messages.PageNumber,
                messages.PageSize);
        }

        public async Task<MessageSummaryDto> GetSummaryForUser()
        {
            var pageParams = new PageParams { PageNumber = 1, PageSize = 5 };

            var user = await currentUserService.GetAsync();
            var messages = await messageRepository.GetByUserIdAsync(user.Id, pageParams);
            var unreadCount = await messageRepository.GetUnreadCountByUserIdAsync(user.Id);

            var pagination = new Pagination<MessageDto>(
                messages.Data.Select(m => m.ToDto()),
                messages.TotalCount,
                messages.PageNumber,
                messages.PageSize
            );
            return new MessageSummaryDto(pagination, unreadCount);
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
