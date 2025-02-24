using Application.DTOs;
using Core.Parameters;
using Application.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IPaymentService
    {
        Task<PaymentResponse> ProcessAsync(TransactionRequestDto transactionRequestDto);
    }
}
