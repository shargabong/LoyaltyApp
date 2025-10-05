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

        #region Main Menu & Flow
        static void RunMainMenu()
        {
            using (var dbContext = new ApplicationDbContext())
            {
                while (true)
                {
                    Console.Clear();
                    Console.WriteLine("╔═════════════════════════════════════════════════╗");
                    Console.WriteLine("║    Система учета клиентов и карт лояльности     ║");
                    Console.WriteLine("╠═════════════════════════════════════════════════╣");
                    Console.WriteLine("║ 1. Клиенты (Добавить, Посмотреть, Удалить)      ║");
                    Console.WriteLine("║ 2. Товары (Добавить, Посмотреть)                ║");
                    Console.WriteLine("║ 3. Оформить новую покупку                       ║");
                    Console.WriteLine("║ 0. Выход                                        ║");
                    Console.WriteLine("╚═════════════════════════════════════════════════╝");

                    string choice = GetString("\nВаш выбор: ");

                    switch (choice)
                    {
                        case "1": RunClientsMenu(dbContext); break;
                        case "2": RunProductsMenu(dbContext); break;
                        case "3": CreateNewPurchase(dbContext); break;
                        case "0": Console.WriteLine("Выход из программы..."); return;
                        default: ShowError("Неверный выбор. Пожалуйста, попробуйте снова."); break;
                    }
                }
            }
        }

        static void RunClientsMenu(ApplicationDbContext dbContext)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("--- Меню управления клиентами ---");
                Console.WriteLine("1. Добавить нового клиента");
                Console.WriteLine("2. Посмотреть всех клиентов");
                Console.WriteLine("3. Удалить клиента");
                Console.WriteLine("0. Назад в главное меню");

                string choice = GetString("\nВаш выбор: ");

                switch (choice)
                {
                    case "1": AddNewClient(dbContext); break;
                    case "2": ViewAllClients(dbContext); break;
                    case "3": DeleteClient(dbContext); break;
                    case "0": return;
                    default: ShowError("Неверный выбор."); break;
                }
            }
        }

        static void RunProductsMenu(ApplicationDbContext dbContext)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("--- Меню управления товарами ---");
                Console.WriteLine("1. Добавить новый товар");
                Console.WriteLine("2. Посмотреть каталог товаров");
                Console.WriteLine("3. Удалить товар");
                Console.WriteLine("0. Назад в главное меню");

                string choice = GetString("\nВаш выбор: ");

                switch (choice)
                {
                    case "1": AddNewProduct(dbContext); break;
                    case "2": ViewAllProducts(dbContext); break;
                    case "3": DeleteProduct(dbContext); break;
                    case "0": return;
                    default: ShowError("Неверный выбор."); break;
                }
            }
        }
        #endregion

        #region Client Management
        static void AddNewClient(ApplicationDbContext dbContext)
        {
            ShowHeader("Добавление нового клиента");
            string fullName = GetRequiredString("Введите ФИО: ");
            string email = GetString("Введите Email (необязательно): ");
            string phoneNumber = GetString("Введите номер телефона (необязательно): ");
            decimal discount = GetPositiveDecimal("Введите процент скидки для карты (>= 0): ");

            var newClient = new Client
            {
                FullName = fullName,
                Email = email,
                PhoneNumber = phoneNumber,
                RegistrationDate = DateTime.UtcNow,
                LoyaltyCard = new LoyaltyCard
                {
                    CardNumber = $"CARD-{DateTime.UtcNow.Ticks}",
                    DiscountPercent = discount
                }
            };

            dbContext.Clients.Add(newClient);
            dbContext.SaveChanges();

            ShowSuccess($"\nКлиент '{fullName}' успешно добавлен с картой '{newClient.LoyaltyCard.CardNumber}'!");
            Pause();
        }

        static void ViewAllClients(ApplicationDbContext dbContext)
        {
            ShowHeader("Список всех клиентов");
            var clients = dbContext.Clients.Include(c => c.LoyaltyCard).ToList();

            if (!clients.Any())
            {
                Console.WriteLine("Клиенты не найдены.");
            }
            else
            {
                Console.WriteLine($"{"ID",-5} | {"ФИО",-25} | {"Email",-25} | {"Карта лояльности",-20} | {"Скидка"}");
                Console.WriteLine(new string('=', 90));
                foreach (var client in clients)
                {
                    string cardInfo = client.LoyaltyCard?.CardNumber ?? "Нет";
                    string discountInfo = client.LoyaltyCard != null ? $"{client.LoyaltyCard.DiscountPercent}%" : "N/A";
                    Console.WriteLine($"{client.Id,-5} | {client.FullName,-25} | {client.Email,-25} | {cardInfo,-20} | {discountInfo}");
                }
            }
            Pause();
        }

        static void DeleteClient(ApplicationDbContext dbContext)
        {
            ShowHeader("Удаление клиента");
            int clientId = GetPositiveInt("Введите ID клиента для удаления: ");

            var clientToDelete = dbContext.Clients.Find(clientId);

            if (clientToDelete == null)
            {
                ShowError("Клиент с таким ID не найден.");
                Pause();
                return;
            }

            Console.WriteLine($"Найден клиент: {clientToDelete.FullName}");
            if (GetConfirmation("Вы уверены, что хотите удалить этого клиента и все его данные? (да/нет): "))
            {
                dbContext.Clients.Remove(clientToDelete);
                dbContext.SaveChanges();
                ShowSuccess("Клиент успешно удален.");
            }
            else
            {
                Console.WriteLine("Удаление отменено.");
            }
            Pause();
        }
        #endregion

        #region Product Management
        static void AddNewProduct(ApplicationDbContext dbContext)
        {
            ShowHeader("Добавление нового товара");
            string name = GetRequiredString("Введите название товара: ");
            string description = GetString("Введите описание (необязательно): ");
            decimal price = GetPositiveDecimal("Введите цену товара (> 0): ");

            var newProduct = new Product { Name = name, Description = description, Price = price };
            dbContext.Products.Add(newProduct);
            dbContext.SaveChanges();

            ShowSuccess($"Товар '{name}' успешно добавлен в каталог!");
            Pause();
        }

        static void DeleteProduct(ApplicationDbContext dbContext)
        {
            ShowHeader("Удаление товара из каталога");
            ViewAllProducts(dbContext);

            int productId = GetPositiveInt("\nВведите ID товара, который хотите удалить: ");
            var productToDelete = dbContext.Products.Find(productId);

            if (productToDelete == null)
            {
                ShowError("Товар с таким ID не найден.");
                Pause();
                return;
            }

            Console.WriteLine($"Найден товар: {productToDelete.Name} (ID: {productToDelete.Id})");
            if (!GetConfirmation("Вы уверены, что хотите НАВСЕГДА удалить этот товар? (да/нет): "))
            {
                Console.WriteLine("Удаление отменено.");
                Pause();
                return;
            }

            try
            {
                dbContext.Products.Remove(productToDelete);
                dbContext.SaveChanges();

                ShowSuccess("Товар успешно удален.");
            }
            catch (DbUpdateException ex)
            {
                dbContext.ChangeTracker.Clear();

                ShowError("\nОШИБКА: Невозможно удалить этот товар, так как он уже приутствует в истории покупок.");
                ShowError("Сначала необходимо удалить все покупки, содержащие этот товар.");
            }
            catch (Exception ex)
            {
                ShowError($"\nПроизошла непредвиденная ошибка: {ex.Message}");
            }

            Pause();
        }

        static void ViewAllProducts(ApplicationDbContext dbContext)
        {
            ShowHeader("Каталог товаров");
            var products = dbContext.Products.ToList();
            if (!products.Any())
            {
                Console.WriteLine("В каталоге нет товаров.");
            }
            else
            {
                Console.WriteLine($"{"ID",-5} | {"Название",-30} | {"Цена",-15} | {"Описание"}");
                Console.WriteLine(new string('=', 80));
                foreach (var product in products)
                {
                    Console.WriteLine($"{product.Id,-5} | {product.Name,-30} | {product.Price:C,-15} | {product.Description}");
                }
            }
            Pause();
        }
        #endregion

        #region Purchase Management
        static void CreateNewPurchase(ApplicationDbContext dbContext)
        {
            ShowHeader("Оформление новой покупки");

            // 1. Выбор клиента
            ViewAllClients(dbContext);
            int clientId = GetPositiveInt("\nВведите ID клиента: ");
            var client = dbContext.Clients.Include(c => c.LoyaltyCard).FirstOrDefault(c => c.Id == clientId);
            if (client == null) { ShowError("Клиент не найден."); Pause(); return; }
            Console.Clear();
            ShowHeader($"Оформление покупки для клиента: {client.FullName}");

            // 2. Формирование корзины
            var purchaseItems = new List<PurchaseItem>();
            while (true)
            {
                ViewAllProducts(dbContext);
                Console.WriteLine("\n--- Добавление товара в чек ---");
                Console.WriteLine("Введите ID товара для добавления или '0' для завершения.");
                int productId = GetPositiveInt("Ваш выбор: ");

                if (productId == 0) break;

                var product = dbContext.Products.Find(productId);
                if (product == null) { ShowError("Товар с таким ID не найден."); continue; }

                int quantity = GetPositiveInt($"Введите количество для '{product.Name}': ");

                purchaseItems.Add(new PurchaseItem
                {
                    ProductId = product.Id,
                    Quantity = quantity,
                    PriceAtPurchase = product.Price
                });
                ShowSuccess($"Добавлено: {product.Name} x {quantity} шт.");
            }

            if (!purchaseItems.Any()) { Console.WriteLine("Корзина пуста. Покупка отменена."); Pause(); return; }

            // 3. Расчет и подтверждение
            decimal totalAmount = purchaseItems.Sum(item => item.PriceAtPurchase * item.Quantity);
            decimal discount = totalAmount * ((client.LoyaltyCard?.DiscountPercent ?? 0) / 100);
            decimal finalAmount = totalAmount - discount;

            Console.Clear();
            ShowHeader("Подтверждение покупки");
            Console.WriteLine($"Клиент: {client.FullName}");
            Console.WriteLine($"Общая сумма: {totalAmount:C}");
            Console.WriteLine($"Скидка по карте ({client.LoyaltyCard?.DiscountPercent ?? 0}%): {discount:C}");
            Console.WriteLine($"Итого к оплате: {finalAmount:C}");

            if (!GetConfirmation("\nОформить покупку? (да/нет): "))
            {
                Console.WriteLine("Покупка отменена.");
                Pause();
                return;
            }
            string paymentMethod = GetRequiredString("Введите способ оплаты (Карта/Наличные): ");

            // 4. Сохранение
            var newPurchase = new Purchase
            {
                ClientId = clientId,
                PurchaseDate = DateTime.UtcNow,
                PaymentMethod = paymentMethod,
                PurchaseItems = purchaseItems
            };

            dbContext.Purchases.Add(newPurchase);
            dbContext.SaveChanges();

            ShowSuccess("Покупка успешно оформлена и сохранена!");
            Pause();
        }
        #endregion

        #region Seeding
        static void SeedDatabase()
        {
            using (var dbContext = new ApplicationDbContext())
            {
                if (dbContext.Clients.Any()) return; // Если данные уже есть, ничего не делаем

                Console.WriteLine("База данных пуста. Добавляем тестовые данные...");

                dbContext.Clients.AddRange(
                    new Client { FullName = "Иван Петров", Email = "ivan@test.com", PhoneNumber = "111-111", RegistrationDate = DateTime.UtcNow, LoyaltyCard = new LoyaltyCard { CardNumber = "CARD-001", DiscountPercent = 5.0m } },
                    new Client { FullName = "Анна Сидорова", Email = "anna@test.com", PhoneNumber = "222-222", RegistrationDate = DateTime.UtcNow, LoyaltyCard = new LoyaltyCard { CardNumber = "CARD-002", DiscountPercent = 3.5m } }
                );

                dbContext.Products.AddRange(
                    new Product { Name = "Молоко", Description = "Молоко 3.2%", Price = 75.50m },
                    new Product { Name = "Хлеб", Description = "Хлеб Бородинский", Price = 52.00m },
                    new Product { Name = "Сыр", Description = "Сыр Гауда 100г", Price = 150.00m }
                );

                dbContext.SaveChanges();
            }
        }
        #endregion

        #region UI & Input Helpers
        // UI
        static void ShowHeader(string title)
        {
            Console.Clear();
            Console.WriteLine($"--- {title} ---");
        }

        static void ShowSuccess(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        static void ShowError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        static void Pause()
        {
            Console.WriteLine("\nНажмите любую клавишу для продолжения...");
            Console.ReadKey();
        }

        // ввода данных
        static string GetRequiredString(string prompt)
        {
            string input;
            while (true)
            {
                Console.Write(prompt);
                input = Console.ReadLine()?.Trim();
                if (!string.IsNullOrWhiteSpace(input)) return input;
                ShowError("Ошибка: Ввод не может быть пустым.");
            }
        }

        static string GetString(string prompt)
        {
            Console.Write(prompt);
            return Console.ReadLine()?.Trim();
        }

        static int GetPositiveInt(string prompt)
        {
            int number;
            while (true)
            {
                Console.Write(prompt);
                if (int.TryParse(Console.ReadLine(), out number) && number >= 0) return number;
                ShowError("Ошибка: Введите корректное положительное число.");
            }
        }

        static decimal GetPositiveDecimal(string prompt)
        {
            decimal number;
            while (true)
            {
                Console.Write(prompt);
                string input = Console.ReadLine()?.Replace('.', ',');
                if (decimal.TryParse(input, out number) && number >= 0) return number;
                ShowError("Ошибка: Введите корректное положительное число (например, 120.50).");
            }
        }

        static bool GetConfirmation(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                string input = Console.ReadLine()?.ToLower();
                if (input == "да") return true;
                if (input == "нет") return false;
                ShowError("Пожалуйста, введите 'да' или 'нет'.");
            }
        }
        #endregion
    }
}