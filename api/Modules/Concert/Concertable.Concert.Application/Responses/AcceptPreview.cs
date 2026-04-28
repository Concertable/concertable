using System.Text.Json.Serialization;
using Concertable.Concert.Application.Enums;

namespace Concertable.Concert.Application.Responses;

internal record AcceptPreview(PaymentTiming Timing, IPaymentAmount Amount, PayeeSummary Payee);

internal record PayeeSummary(string Name, string? Email);

[JsonDerivedType(typeof(FlatPayment), "flat")]
[JsonDerivedType(typeof(DoorSharePayment), "doorShare")]
[JsonDerivedType(typeof(GuaranteedDoorPayment), "guaranteedDoor")]
internal interface IPaymentAmount { }

internal record FlatPayment(decimal Amount) : IPaymentAmount;
internal record DoorSharePayment(decimal ArtistPercent) : IPaymentAmount;
internal record GuaranteedDoorPayment(decimal Guarantee, decimal ArtistPercent) : IPaymentAmount;
