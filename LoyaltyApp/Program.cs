using LoyaltyApp.Data;
using LoyaltyApp.Models;
using Microsoft.EntityFrameworkCore;

namespace LoyaltyApp
{
    class Program
    {
        static void Main(string[] args)
        {
            SeedDatabase();

            RunMainMenu();
        }

        static void SeedDatabase()
        {
            using (var dbContext = new ApplicationDbContext())
            {
                if (!dbContext.Clients.Any())
                {
                    Console.WriteLine("База данных пуста. Добавляем тестовые данные...");

                    var client1 = new Client
                    {
                        FullName = "Иван Тестовый",
                        Email = "test1@example.com",
                        PhoneNumber = "111-111-111",
                        RegistrationDate = DateTime.UtcNow
                    };

                    var client2 = new Client
                    {
                        FullName = "Анна Тестовая",
                        Email = "test2@example.com",
                        PhoneNumber = "222-222-222",
                        RegistrationDate = DateTime.UtcNow
                    };

                    client1.LoyaltyCards.Add(new LoyaltyCard
                    {
                        CardNumber = "CARD-001",
                        IssueDate = DateTime.UtcNow
                    });

                    dbContext.Clients.Add(client1);
                    dbContext.Clients.Add(client2);

                    dbContext.SaveChanges();

                    Console.WriteLine("Тестовые данные успешно добавлены!");
                    Console.WriteLine("Нажмите любую клавишу для продолжения...");
                    Console.ReadKey();
                }
            }
        }

        static void RunMainMenu()
        {
            using (var dbContext = new ApplicationDbContext())
            {
                while (true)
                {
                    Console.Clear();
                    Console.WriteLine("--- Система Управления Программой Лояльности ---");
                    Console.WriteLine("Главное меню:");
                    Console.WriteLine("1. Добавить нового клиента");
                    Console.WriteLine("2. Посмотреть всех клиентов");
                    Console.WriteLine("0. Выход");
                    Console.Write("\nВведите ваш выбор: ");

                    string choice = Console.ReadLine();

                    switch (choice)
                    {
                        case "1":
                            AddNewClient(dbContext);
                            break;
                        case "2":
                            ViewAllClients(dbContext);
                            break;
                        case "0":
                            Console.WriteLine("Выход из программы...");
                            return;
                        default:
                            Console.WriteLine("Неверный выбор. Пожалуйста, попробуйте снова.");
                            Console.ReadKey();
                            break;
                    }
                }
            }
        }

        static void AddNewClient(ApplicationDbContext dbContext)
        {
            Console.Clear();
            Console.WriteLine("--- Добавление нового клиента ---");

            Console.Write("Введите полное имя: ");
            string fullName = Console.ReadLine()?.Trim();
            if (string.IsNullOrEmpty(fullName))
            {
                Console.WriteLine("Имя не может быть пустым. Отмена операции.");
                Console.ReadKey();
                return;
            }

            Console.Write("Введите email: ");
            string email = Console.ReadLine()?.Trim();
            if (string.IsNullOrEmpty(email) || !email.Contains("@"))
            {
                Console.WriteLine("Некорректный email. Отмена операции.");
                Console.ReadKey();
                return;
            }

            Console.Write("Введите номер телефона: ");
            string phoneNumber = Console.ReadLine()?.Trim();
            if (string.IsNullOrEmpty(phoneNumber))
            {
                Console.WriteLine("Номер телефона не может быть пустым. Отмена операции.");
                Console.ReadKey();
                return;
            }

            var newClient = new Client
            {
                FullName = fullName,
                Email = email,
                PhoneNumber = phoneNumber,
                RegistrationDate = DateTime.UtcNow
            };

            // Автоматически добавляем карту лояльности при регистрации
            newClient.LoyaltyCards.Add(new LoyaltyCard
            {
                CardNumber = $"CARD-{DateTime.UtcNow:yyyyMMddHHmmss}",
                IssueDate = DateTime.UtcNow
            });

            dbContext.Clients.Add(newClient);
            dbContext.SaveChanges();

            Console.WriteLine($"\nКлиент '{fullName}' успешно добавлен!");
            Console.WriteLine("Карта лояльности выдана с номером: " + newClient.LoyaltyCards.First().CardNumber);
            Console.WriteLine("Нажмите любую клавишу для возврата в меню...");
            Console.ReadKey();
        }

        static void ViewAllClients(ApplicationDbContext dbContext)
        {
            Console.Clear();
            Console.WriteLine("--- Список всех клиентов ---");

            var clients = dbContext.Clients
                .Include(c => c.LoyaltyCards)
                .ToList();

            if (!clients.Any())
            {
                Console.WriteLine("Клиенты не найдены.");
                Console.WriteLine("Нажмите любую клавишу для возврата в меню...");
                Console.ReadKey();
                return;
            }

            foreach (var client in clients)
            {
                Console.WriteLine($"\nID: {client.Id}");
                Console.WriteLine($"Имя: {client.FullName}");
                Console.WriteLine($"Email: {client.Email}");
                Console.WriteLine($"Телефон: {client.PhoneNumber}");
                Console.WriteLine($"Дата регистрации: {client.RegistrationDate:dd.MM.yyyy HH:mm}");

                if (client.LoyaltyCards.Any())
                {
                    Console.WriteLine("Карты лояльности:");
                    foreach (var card in client.LoyaltyCards)
                    {
                        Console.WriteLine($"  - Номер: {card.CardNumber}, Выдана: {card.IssueDate:dd.MM.yyyy HH:mm}");
                    }
                }
                else
                {
                    Console.WriteLine("Карты лояльности: Нет");
                }

                Console.WriteLine(new string('-', 50));
            }

            Console.WriteLine("\nНажмите любую клавишу для возврата в меню...");
            Console.ReadKey();
        }
    }
}
