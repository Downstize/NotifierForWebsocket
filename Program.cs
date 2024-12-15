using EasyNetQ;
using Microsoft.AspNetCore.SignalR.Client;
using DoctorAppointmentWebApi.Messages;

var rabbitMqConnectionString = "amqp://guest:guest@localhost:5672";

var connection = new HubConnectionBuilder()
    .WithUrl("https://localhost:7049/appointmentHub")
    .Build();

await connection.StartAsync();
Console.WriteLine("SignalR connected.");

using var bus = RabbitHutch.CreateBus(rabbitMqConnectionString);

Console.WriteLine("Connected to RabbitMQ.");

bus.PubSub.Subscribe<AppointmentMessage>("AppointmentSubscriber", async message =>
{
    Console.WriteLine($"Received message for Patient: {message.PatientFullName}, Status: {message.Status}");
    
    await connection.InvokeAsync("SendAppointmentNotification", message);
    Console.WriteLine("Initial notification sent.");
    
    await Task.Delay(7000);
    
    var random = new Random();
    var isRejected = random.Next(1, 101) <= 25;
    
    message.Status = isRejected ? "ваша запись отклонена" : "ваша запись подтверждена";
    
    await connection.InvokeAsync("SendAppointmentNotification", message);
    Console.WriteLine($"Updated notification sent with status: {message.Status}");
});

Console.WriteLine("Press [enter] to exit.");
Console.ReadLine();