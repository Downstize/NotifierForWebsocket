using EasyNetQ;
using Microsoft.AspNetCore.SignalR.Client;
using DoctorAppointmentWebApi.Messages;

var rabbitMqConnectionString = "amqp://guest:guest@localhost:5672";

var connection = new HubConnectionBuilder()
    .WithUrl("https://localhost:7049/appointmentHub")
    .Build();

await connection.StartAsync();
Console.WriteLine("SignalR подключен!.");

using var bus = RabbitHutch.CreateBus(rabbitMqConnectionString);

Console.WriteLine("Слушаю RabbitMQ...");

bus.PubSub.Subscribe<AppointmentMessage>("AppointmentSubscriber", async message =>
{
    Console.WriteLine($"Получено сообщение для Пациента: {message.PatientFullName}, Статус: {message.Status}");
    
    await connection.InvokeAsync("SendAppointmentNotification", message);
    Console.WriteLine("Инициализирующее уведомление отправлено.");
    
    await Task.Delay(7000);
    
    var random = new Random();
    var isRejected = random.Next(1, 101) <= 25;
    
    message.Status = isRejected ? "ваша запись отклонена" : "ваша запись подтверждена";
    
    await connection.InvokeAsync("SendAppointmentNotification", message);
    Console.WriteLine($"Обновлённое оповещение было отправлено со статусом: {message.Status}");
});

Console.WriteLine("Нажмите любую кнопку для завершения работы...");
Console.ReadLine();