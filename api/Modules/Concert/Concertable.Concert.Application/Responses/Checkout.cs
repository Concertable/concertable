using System.Text.Json.Serialization;
using Concertable.Concert.Application.Enums;
using Concertable.Payment.Contracts;

namespace Concertable.Concert.Application.Responses;

internal record Checkout(PaymentTiming Timing, IPaymentAmount Amount, PayeeSummary Payee, CheckoutSession Session);

internal record PayeeSummary(string Name, string? Email);

[JsonDerivedType(typeof(FlatPayment), "flat")]
[JsonDerivedType(typeof(DoorSharePayment), "doorShare")]
[JsonDerivedType(typeof(GuaranteedDoorPayment), "guaranteedDoor")]
internal interface IPaymentAmount { }

internal record FlatPayment(decimal Amount) : IPaymentAmount;
internal record DoorSharePayment(decimal ArtistPercent) : IPaymentAmount;
internal record GuaranteedDoorPayment(decimal Guarantee, decimal ArtistPercent) : IPaymentAmount;
