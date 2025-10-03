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

        #region Seeding (Начальное заполнение)
        static void SeedDatabase()
        {
            using (var dbContext = new ApplicationDbContext())
            {
                if (!dbContext.Clients.Any())
                {
                    Console.WriteLine("База данных пуста. Добавляем тестовые данные...");

                    var client1 = new Client
                    {
                        FullName = "Иван Тест",
                        Email = "test1@example.com",
                        PhoneNumber = "111-111-111",
                        RegistrationDate = DateTime.UtcNow,
                        // Сразу создаем и присваиваем карту (связь 1-к-1)
                        LoyaltyCard = new LoyaltyCard { CardNumber = "CARD-001", DiscountPercent = 5.0m }
                    };

                    var client2 = new Client
                    {
                        FullName = "Анна Тест",
                        Email = "test2@example.com",
                        PhoneNumber = "222-222-222",
                        RegistrationDate = DateTime.UtcNow,
                        LoyaltyCard = new LoyaltyCard { CardNumber = "CARD-002", DiscountPercent = 3.5m }
                    };

                    dbContext.Clients.Add(client1);
                    dbContext.Clients.Add(client2);

                    var product1 = new Product { Name = "Молоко", Description = "Молоко 3.2%", Price = 75.50m };
                    var product2 = new Product { Name = "Хлеб", Description = "Хлеб Бородинский", Price = 52.00m };
                    var product3 = new Product { Name = "Сыр", Description = "Сыр Гауда 100г", Price = 150.00m };

                    dbContext.Products.AddRange(product1, product2, product3);

                    dbContext.SaveChanges();

                    Console.WriteLine("Тестовые данные успешно добавлены!");
                    Console.WriteLine("Нажмите любую клавишу для продолжения...");
                    Console.ReadKey();
                }
            }
        }
        #endregion

        #region Main Menu
        static void RunMainMenu()
        {
            using (var dbContext = new ApplicationDbContext())
            {
                while (true)
                {
                    Console.Clear();
                    Console.WriteLine("--- Система учета клиентов и карт лояльности ---");
                    Console.WriteLine("1. Добавить нового клиента");
                    Console.WriteLine("2. Посмотреть всех клиентов");
                    Console.WriteLine("3. Добавить товар в каталог");
                    Console.WriteLine("4. Посмотреть каталог товаров");
                    Console.WriteLine("5. Оформить новую покупку");
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
                        case "3":
                            AddNewProduct(dbContext);
                            break;
                        case "4":
                            ViewAllProducts(dbContext);
                            break;
                        case "5":
                            CreateNewPurchase(dbContext);
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
        #endregion

        #region Client Management
        static void AddNewClient(ApplicationDbContext dbContext)
        {
            Console.Clear();
            Console.WriteLine("--- Добавление нового клиента ---");

            Console.Write("Введите ФИО: ");
            string fullName = Console.ReadLine();
            Console.Write("Введите Email: ");
            string email = Console.ReadLine();
            Console.Write("Введите номер телефона: ");
            string phoneNumber = Console.ReadLine();
            Console.Write("Введите процент скидки для новой карты (например, 3.5): ");
            decimal.TryParse(Console.ReadLine(), out decimal discount);

            var newClient = new Client
            {
                FullName = fullName,
                Email = email,
                PhoneNumber = phoneNumber,
                RegistrationDate = DateTime.UtcNow,
                LoyaltyCard = new LoyaltyCard
                {
                    CardNumber = $"CARD-{DateTime.UtcNow.Ticks}", // Уникальный номер на основе времени
                    DiscountPercent = discount
                }
            };

            dbContext.Clients.Add(newClient);
            dbContext.SaveChanges();

            Console.WriteLine($"\nКлиент '{fullName}' успешно добавлен с картой '{newClient.LoyaltyCard.CardNumber}'!");
            Console.ReadKey();
        }

        static void ViewAllClients(ApplicationDbContext dbContext)
        {
            Console.Clear();
            Console.WriteLine("--- Список всех клиентов ---");

            // Загружаем клиентов вместе с их картами лояльности
            var clients = dbContext.Clients.Include(c => c.LoyaltyCard).ToList();

            if (!clients.Any())
            {
                Console.WriteLine("Клиенты не найдены.");
            }
            else
            {
                foreach (var client in clients)
                {
                    Console.WriteLine($"ID: {client.Id} | ФИО: {client.FullName} | Email: {client.Email}");
                    if (client.LoyaltyCard != null)
                    {
                        Console.WriteLine($"  -> Карта: {client.LoyaltyCard.CardNumber}, Скидка: {client.LoyaltyCard.DiscountPercent}%");
                    }
                    else
                    {
                        Console.WriteLine("  -> Карта лояльности не найдена.");
                    }
                    Console.WriteLine(new string('-', 20));
                }
            }

            Console.WriteLine("\nНажмите любую клавишу для возврата в меню...");
            Console.ReadKey();
        }
        #endregion

        #region Product Management
        static void AddNewProduct(ApplicationDbContext dbContext)
        {
            Console.Clear();
            Console.WriteLine("--- Добавление нового товара в каталог ---");

            Console.Write("Введите название товара: ");
            string name = Console.ReadLine();
            Console.Write("Введите описание товара: ");
            string description = Console.ReadLine();
            Console.Write("Введите цену товара (например, 120.50): ");
            decimal.TryParse(Console.ReadLine(), out decimal price);

            var newProduct = new Product { Name = name, Description = description, Price = price };
            dbContext.Products.Add(newProduct);
            dbContext.SaveChanges();

            Console.WriteLine($"\nТовар '{name}' успешно добавлен в каталог!");
            Console.ReadKey();
        }

        static void ViewAllProducts(ApplicationDbContext dbContext)
        {
            Console.Clear();
            Console.WriteLine("--- Каталог товаров ---");

            var products = dbContext.Products.ToList();
            if (!products.Any())
            {
                Console.WriteLine("В каталоге нет товаров.");
            }
            else
            {
                foreach (var product in products)
                {
                    Console.WriteLine($"ID: {product.Id} | Название: {product.Name} | Цена: {product.Price:C}");
                }
            }

            Console.WriteLine("\nНажмите любую клавишу для возврата в меню...");
            Console.ReadKey();
        }
        #endregion

        #region Purchase Management
        static void CreateNewPurchase(ApplicationDbContext dbContext)
        {
            Console.Clear();
            Console.WriteLine("--- Оформление новой покупки ---");

            // 1. Выбор клиента
            Console.Write("Введите ID клиента, совершающего покупку: ");
            int.TryParse(Console.ReadLine(), out int clientId);
            var client = dbContext.Clients.Include(c => c.LoyaltyCard).FirstOrDefault(c => c.Id == clientId);

            if (client == null)
            {
                Console.WriteLine("Клиент с таким ID не найден.");
                Console.ReadKey();
                return;
            }
            Console.WriteLine($"Выбран клиент: {client.FullName}");

            // 2. Формирование "корзины"
            var purchaseItems = new List<PurchaseItem>();
            while (true)
            {
                Console.Clear();
                ViewAllProducts(dbContext); // Показываем список товаров для удобства
                Console.WriteLine("\n--- Добавление товара в чек ---");
                Console.Write("Введите ID товара для добавления (или 'готово' для завершения): ");
                string input = Console.ReadLine();
                if (input.ToLower() == "готово") break;

                int.TryParse(input, out int productId);
                var product = dbContext.Products.Find(productId);

                if (product == null)
                {
                    Console.WriteLine("Товар с таким ID не найден.");
                    Console.ReadKey();
                    continue;
                }

                Console.Write($"Введите количество для '{product.Name}': ");
                int.TryParse(Console.ReadLine(), out int quantity);

                purchaseItems.Add(new PurchaseItem
                {
                    ProductId = product.Id,
                    Quantity = quantity,
                    PriceAtPurchase = product.Price // Фиксируем цену на момент покупки
                });

                Console.WriteLine($"Добавлено: {product.Name} x {quantity} шт.");
                Console.ReadKey();
            }

            if (!purchaseItems.Any())
            {
                Console.WriteLine("Корзина пуста. Оформление покупки отменено.");
                Console.ReadKey();
                return;
            }

            // 3. Расчет и подтверждение
            decimal totalAmount = purchaseItems.Sum(item => item.PriceAtPurchase * item.Quantity);
            decimal discount = 0;
            if (client.LoyaltyCard != null)
            {
                discount = totalAmount * (client.LoyaltyCard.DiscountPercent / 100);
            }
            decimal finalAmount = totalAmount - discount;

            Console.Clear();
            Console.WriteLine("--- Подтверждение покупки ---");
            Console.WriteLine($"Клиент: {client.FullName}");
            Console.WriteLine($"Общая сумма: {totalAmount:C}");
            Console.WriteLine($"Скидка по карте ({client.LoyaltyCard?.DiscountPercent ?? 0}%): {discount:C}");
            Console.WriteLine($"Итого к оплате: {finalAmount:C}");
            Console.Write("Введите способ оплаты (Карта/Наличные): ");
            string paymentMethod = Console.ReadLine();

            // 4. Сохранение покупки в БД
            var newPurchase = new Purchase
            {
                ClientId = clientId,
                PurchaseDate = DateTime.UtcNow,
                PaymentMethod = paymentMethod,
                PurchaseItems = purchaseItems // Привязываем "корзину" к покупке
            };

            dbContext.Purchases.Add(newPurchase);
            dbContext.SaveChanges();

            Console.WriteLine("\nПокупка успешно оформлена и сохранена!");
            Console.ReadKey();
        }
        #endregion
    }
}