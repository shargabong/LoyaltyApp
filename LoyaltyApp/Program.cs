using LoyaltyApp.Data;
using LoyaltyApp.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Mail;
using System.Text.RegularExpressions;

namespace LoyaltyApp
{
    class Program
    {
        #region Entry Point & Main Menus
        static void Main(string[] args)
        {
            var culture = new CultureInfo("ru-RU");
            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = culture;

            SeedDatabase();
            RunMainMenu();
        }

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
                    Console.WriteLine("║ Что Вас интересует?                             ║");
                    Console.WriteLine("║ 1. Клиенты                                      ║");
                    Console.WriteLine("║ 2. Товары                                       ║");
                    Console.WriteLine("║ 3. Покупки                                      ║");
                    Console.WriteLine("║ 0. Выход из программы                           ║");
                    Console.WriteLine("╚═════════════════════════════════════════════════╝");

                    string choice = GetString("\nВаш выбор: ");

                    switch (choice)
                    {
                        case "1": RunClientsMenu(dbContext); break;
                        case "2": RunProductsMenu(dbContext); break;
                        case "3": RunPurchasesMenu(dbContext); break;
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
                Console.WriteLine("3. Найти клиента");
                Console.WriteLine("4. Редактировать данные клиента");
                Console.WriteLine("5. Удалить клиента");
                Console.WriteLine("0. Назад в главное меню");
                string choice = GetString("\nВаш выбор: ");
                switch (choice)
                {
                    case "1": AddNewClient(dbContext); break;
                    case "2": ViewAllClients(dbContext); break;
                    case "3": SearchClient(dbContext); break;
                    case "4": EditClient(dbContext); break;
                    case "5": DeleteClient(dbContext); break;
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
                Console.WriteLine("3. Найти товар");
                Console.WriteLine("4. Удалить товар");
                Console.WriteLine("0. Назад в главное меню");

                string choice = GetString("\nВаш выбор: ");

                switch (choice)
                {
                    case "1": AddNewProduct(dbContext); break;
                    case "2": ViewAllProducts(dbContext); break;
                    case "3": SearchProduct(dbContext); break;
                    case "4": DeleteProduct(dbContext); break;
                    case "0": return;
                    default: ShowError("Неверный выбор."); break;
                }
            }
        }

        static void RunPurchasesMenu(ApplicationDbContext dbContext)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("--- Меню управления покупками ---");
                Console.WriteLine("1. Оформить новую покупку");
                Console.WriteLine("2. Посмотреть историю покупок клиента");
                Console.WriteLine("3. Найти покупку");
                Console.WriteLine("4. Редактировать покупку");
                Console.WriteLine("5. Удалить покупку");
                Console.WriteLine("0. Назад в главное меню");
                string choice = GetString("\nВаш выбор: ");
                switch (choice)
                {
                    case "1": CreateNewPurchase(dbContext); break;
                    case "2": ViewClientPurchaseHistory(dbContext); break;
                    case "3": SearchPurchase(dbContext); break;
                    case "4": EditPurchase(dbContext); break;
                    case "5": DeletePurchase(dbContext); break;
                    case "0": return;
                    default: ShowError("Неверный выбор."); break;
                }
            }
        }
        #endregion

        #region Client Management
        static Client FindClient(ApplicationDbContext dbContext)
        {
            Console.WriteLine("\n--- Список доступных клиентов ---");
            var allClients = dbContext.Clients.Include(c => c.LoyaltyCard).ToList();
            if (!allClients.Any())
            {
                ShowError("В базе данных еще нет клиентов.");
                return null;
            }

            Console.WriteLine($"{"ID",-5} | {"ФИО",-30} | {"Карта лояльности"}");
            Console.WriteLine(new string('=', 60));
            foreach (var c in allClients)
            {
                Console.WriteLine($"{c.Id,-5} | {c.FullName,-30} | {c.LoyaltyCard?.CardNumber ?? "Нет"}");
            }

            Console.WriteLine("\nКак найти клиента?");
            Console.WriteLine("1. По ID");
            Console.WriteLine("2. По ФИО");
            string choice = GetString("Ваш выбор: ");

            if (choice == "1")
            {
                int id = GetPositiveInt("Введите ID клиента: ");
                return dbContext.Clients.Include(c => c.LoyaltyCard).FirstOrDefault(c => c.Id == id);
            }
            if (choice == "2")
            {
                string name = GetRequiredString("Введите ФИО (можно часть): ");
                var clientsFound = dbContext.Clients
                                    .Include(c => c.LoyaltyCard)
                                    .Where(c => c.FullName.ToLower().Contains(name.ToLower()))
                                    .ToList();

                if (!clientsFound.Any()) return null;

                if (clientsFound.Count == 1) return clientsFound.First();

                Console.WriteLine("Найдено несколько клиентов. Пожалуйста, выберите нужного по ID из списка выше:");
                int id = GetPositiveInt("Введите точный ID: ");
                return clientsFound.FirstOrDefault(c => c.Id == id);
            }

            ShowError("Неверный выбор способа поиска.");
            return null;
        }

        static void SearchClient(ApplicationDbContext dbContext)
        {
            ShowHeader("Поиск клиента");

            Console.WriteLine("Выберите способ поиска:");
            Console.WriteLine("1. По ID");
            Console.WriteLine("2. По ФИО");
            Console.WriteLine("3. По номеру телефона");
            Console.WriteLine("4. По email");

            string choice = GetString("Ваш выбор: ");

            switch (choice)
            {
                case "1":
                    int id = GetPositiveInt("Введите ID клиента: ");
                    var clientById = dbContext.Clients.Include(c => c.LoyaltyCard).FirstOrDefault(c => c.Id == id);
                    DisplayClientDetails(clientById, "Найденный клиент");
                    break;

                case "2":
                    string name = GetRequiredString("Введите ФИО (можно часть): ");
                    var clientsByName = dbContext.Clients.Include(c => c.LoyaltyCard).Where(c => c.FullName.ToLower().Contains(name.ToLower())).ToList();
                    DisplayClientsList(clientsByName, $"Результаты поиска по ФИО: '{name}'");
                    break;

                case "3":
                    string phone = GetString("Введите номер телефона (можно часть): ");
                    var clientsByPhone = dbContext.Clients.Include(c => c.LoyaltyCard).Where(c => c.PhoneNumber != null && c.PhoneNumber.Contains(phone)).ToList();
                    DisplayClientsList(clientsByPhone, $"Результаты поиска по телефону: '{phone}'");
                    break;

                case "4":
                    string email = GetString("Введите email (можно часть): ");
                    var clientsByEmail = dbContext.Clients.Include(c => c.LoyaltyCard).Where(c => c.Email != null && c.Email.ToLower().Contains(email.ToLower())).ToList();
                    DisplayClientsList(clientsByEmail, $"Результаты поиска по email: '{email}'");
                    break;
                default: ShowError("Неверный выбор."); break;
            }
            Pause();
        }

        static void DisplayClientDetails(Client client, string title)
        {
            ShowHeader(title);
            if (client == null) { Console.WriteLine("Клиент не найден."); return; }

            Console.WriteLine($"ID: {client.Id}");
            Console.WriteLine($"ФИО: {client.FullName}");
            Console.WriteLine($"Email: {client.Email ?? "Не указан"}");
            Console.WriteLine($"Телефон: {client.PhoneNumber ?? "Не указан"}");
            Console.WriteLine($"Дата регистрации: {client.RegistrationDate:dd.MM.yyyy}");

            if (client.LoyaltyCard != null)
            {
                Console.WriteLine($"Карта лояльности: {client.LoyaltyCard.CardNumber}");
                Console.WriteLine($"Скидка: {client.LoyaltyCard.DiscountPercent:F2}%");
            }
            else
            {
                Console.WriteLine("Карта лояльности: Нет");
            }
        }

        static void DisplayClientsList(List<Client> clients, string title)
        {
            ShowHeader(title);
            if (!clients.Any()) { Console.WriteLine("Клиенты не найдены."); return; }

            Console.WriteLine($"{"ID",-5} | {"ФИО",-30} | {"Телефон",-20} | {"Email",-25} | {"Карта лояльности",-22}  | {"Скидка"}");
            Console.WriteLine(new string('=', 120));
            foreach (var client in clients)
            {
                string cardInfo = client.LoyaltyCard?.CardNumber ?? "Нет";
                string discountInfo = client.LoyaltyCard != null ? $"{client.LoyaltyCard.DiscountPercent:F2}%" : "N/A";
                Console.WriteLine($"{client.Id,-5} | {client.FullName,-30} | {client.PhoneNumber ?? "N/A",-20} | {client.Email ?? "N/A",-25} | {cardInfo,-22} | {discountInfo}");
            }
            Console.WriteLine($"\nНайдено клиентов: {clients.Count}");
        }

        static void AddNewClient(ApplicationDbContext dbContext)
        {
            ShowHeader("Добавление нового клиента");

            string fullName = GetValidFullName("Введите ФИО: ");
            string email = GetValidEmail("Введите Email: ", isOptional: false);
            string phoneNumber = GetMaskedPhoneNumber("Телефон: ", isOptional: false);
            decimal discount = GetPercentage("Введите процент скидки для карты (>= 0): ");

            var newClient = new Client
            {
                FullName = fullName,
                Email = email,
                PhoneNumber = phoneNumber,
                RegistrationDate = DateTime.UtcNow,
                LoyaltyCard = new LoyaltyCard { CardNumber = $"CARD-{DateTime.UtcNow.Ticks}", DiscountPercent = discount }
            };

            dbContext.Clients.Add(newClient);
            dbContext.SaveChanges();

            ShowSuccess($"\nКлиент '{fullName}' успешно добавлен с картой '{newClient.LoyaltyCard.CardNumber}'!");
            Pause();
        }

        static void EditClient(ApplicationDbContext dbContext)
        {
            ShowHeader("Редактирование данных клиента");
            var client = FindClient(dbContext);

            if (client == null) { ShowError("Клиент не найден."); Pause(); return; }

            bool hasChanges = false;
            while (true)
            {
                Console.Clear();
                ShowHeader($"Редактирование клиента: {client.FullName}");
                Console.WriteLine($"1. ФИО: {client.FullName}");
                Console.WriteLine($"2. Email: {client.Email}");
                Console.WriteLine($"3. Телефон: {client.PhoneNumber}");
                if (client.LoyaltyCard != null) { Console.WriteLine($"4. Скидка по карте: {client.LoyaltyCard.DiscountPercent:F2}%"); }
                Console.WriteLine("\n0. Сохранить изменения и вернуться в меню");

                string choice = GetString("\nВыберите поле для редактирования: ");

                switch (choice)
                {
                    case "1":
                        string newFullName = GetValidFullName("Новое ФИО: ");
                        if (newFullName != client.FullName) { client.FullName = newFullName; hasChanges = true; ShowSuccess("ФИО будет обновлено."); }
                        else { ShowError("Ввод совпадает с текущим значением."); }
                        Pause();
                        break;
                    case "2":
                        string newEmail = GetValidEmail("Новый Email: ", isOptional: false);
                        if (newEmail != client.Email)
                        {
                            bool isEmailTaken = dbContext.Clients.Any(c => c.Email == newEmail && c.Id != client.Id);
                            if (isEmailTaken) { ShowError("Этот Email уже используется другим клиентом."); }
                            else { client.Email = newEmail; hasChanges = true; ShowSuccess("Email будет обновлен."); }
                        }
                        else { ShowError("Ввод совпадает с текущим значением."); }
                        Pause();
                        break;
                    case "3":
                        string newPhone = GetMaskedPhoneNumber("Новый телефон: ", isOptional: false);
                        if (newPhone != client.PhoneNumber)
                        {
                            bool isPhoneTaken = dbContext.Clients.Any(c => c.PhoneNumber == newPhone && c.Id != client.Id);
                            if (isPhoneTaken) { ShowError("Этот номер телефона уже используется другим клиентом."); }
                            else { client.PhoneNumber = newPhone; hasChanges = true; ShowSuccess("Номер телефона будет обновлен."); }
                        }
                        else { ShowError("Ввод совпадает с текущим значением."); }
                        Pause();
                        break;
                    case "4":
                        if (client.LoyaltyCard == null) { ShowError("У этого клиента нет карты лояльности."); Pause(); break; }
                        decimal newDiscount = GetPercentage("Новая скидка (от 0 до 100): ");
                        if (newDiscount != client.LoyaltyCard.DiscountPercent)
                        {
                            client.LoyaltyCard.DiscountPercent = newDiscount;
                            hasChanges = true;
                            ShowSuccess("Скидка будет обновлена.");
                        }
                        else { ShowError("Новое значение совпадает с текущим."); }
                        Pause();
                        break;
                    case "0":
                        if (hasChanges) { dbContext.SaveChanges(); ShowSuccess("\nДанные клиента успешно обновлены!"); }
                        else { Console.WriteLine("\nНе было внесено никаких изменений."); }
                        Pause();
                        return;
                    default: ShowError("Неверный выбор."); Pause(); break;
                }
            }
        }

        static void ViewAllClients(ApplicationDbContext dbContext)
        {
            ShowHeader("Список всех клиентов");
            var clients = dbContext.Clients.Include(c => c.LoyaltyCard).ToList();
            if (!clients.Any()) { Console.WriteLine("Клиенты не найдены."); }
            else { DisplayClientsList(clients, "Список всех клиентов"); }
            Pause();
        }

        static void ViewClientPurchaseHistory(ApplicationDbContext dbContext)
        {
            ShowHeader("Просмотр истории покупок клиента");
            var client = FindClient(dbContext);
            if (client == null) { Pause(); return; }
            DisplayPurchasesList(
                dbContext.Purchases.Where(p => p.ClientId == client.Id).Include(p => p.Client).Include(p => p.PurchaseItems).ThenInclude(pi => pi.Product).OrderByDescending(p => p.PurchaseDate).ToList(),
                $"История покупок клиента: {client.FullName}"
            );
            Pause();
        }

        static void DeleteClient(ApplicationDbContext dbContext)
        {
            ShowHeader("Удаление клиента");
            var clientToDelete = FindClient(dbContext);
            if (clientToDelete == null) { ShowError("Клиент не найден."); Pause(); return; }
            Console.WriteLine($"\nНайден клиент: {clientToDelete.FullName} (ID: {clientToDelete.Id})");
            if (GetConfirmation("Вы уверены, что хотите удалить этого клиента и все его данные? (да/нет): "))
            {
                dbContext.Clients.Remove(clientToDelete);
                dbContext.SaveChanges();
                ShowSuccess("Клиент успешно удален.");
            }
            else { Console.WriteLine("Удаление отменено."); }
            Pause();
        }
        #endregion

        #region Product Management
        static void AddNewProduct(ApplicationDbContext dbContext)
        {
            ShowHeader("Добавление нового товара");
            string name = GetRequiredString("Введите название товара: ");
            string description = GetString("Введите описание (необязательно): ");
            decimal price = GetDecimal("Введите цену товара (> 0): ", 0.01m);
            var newProduct = new Product { Name = name, Description = description, Price = price };
            dbContext.Products.Add(newProduct);
            dbContext.SaveChanges();
            ShowSuccess($"Товар '{name}' успешно добавлен в каталог!");
            Pause();
        }

        static void SearchProduct(ApplicationDbContext dbContext)
        {
            ShowHeader("Поиск товара");
            Console.WriteLine("Выберите способ поиска:");
            Console.WriteLine("1. По ID");
            Console.WriteLine("2. По названию");
            Console.WriteLine("3. По диапазону цен");
            string choice = GetString("Ваш выбор: ");
            switch (choice)
            {
                case "1":
                    int id = GetPositiveInt("Введите ID товара: ");
                    var productById = dbContext.Products.Find(id);
                    DisplayProductDetails(productById, "Найденный товар");
                    break;
                case "2":
                    string name = GetRequiredString("Введите название (можно часть): ");
                    var productsByName = dbContext.Products.Where(p => p.Name.ToLower().Contains(name.ToLower())).ToList();
                    DisplayProductsList(productsByName, $"Результаты поиска по названию: '{name}'");
                    break;
                case "3":
                    decimal minPrice = GetDecimal("Введите минимальную цену: ", 0);
                    decimal maxPrice = GetDecimal("Введите максимальную цену: ", 0);
                    if (minPrice > maxPrice) { ShowError("Минимальная цена не может быть больше максимальной."); break; }
                    var productsByPrice = dbContext.Products.Where(p => p.Price >= minPrice && p.Price <= maxPrice).OrderBy(p => p.Price).ToList();
                    DisplayProductsList(productsByPrice, $"Результаты поиска по цене: от {minPrice:F2} до {maxPrice:F2} руб.");
                    break;
                default: ShowError("Неверный выбор."); break;
            }
            Pause();
        }

        static void DisplayProductDetails(Product product, string title)
        {
            ShowHeader(title);
            if (product == null) { Console.WriteLine("Товар не найден."); return; }
            Console.WriteLine($"ID: {product.Id}");
            Console.WriteLine($"Название: {product.Name}");
            Console.WriteLine($"Описание: {product.Description ?? "Нет описания"}");
            Console.WriteLine($"Цена: {product.Price:F2} руб.");
        }

        static void DisplayProductsList(List<Product> products, string title)
        {
            ShowHeader(title);
            if (!products.Any()) { Console.WriteLine("Товары не найдены."); return; }
            Console.WriteLine($"{"ID",-5} | {"Название",-30} | {"Цена",-15} | {"Описание"}");
            Console.WriteLine(new string('=', 80));
            foreach (var product in products)
            {
                string priceString = $"{product.Price:F2} руб.";
                Console.WriteLine($"{product.Id,-5} | {product.Name,-30} | {priceString,-15} | {product.Description}");
            }
            Console.WriteLine($"\nНайдено товаров: {products.Count}");
        }

        static void DeleteProduct(ApplicationDbContext dbContext)
        {
            ShowHeader("Удаление товара из каталога");
            ViewAllProducts(dbContext);
            int productId = GetPositiveInt("\nВведите ID товара, который хотите удалить: ");
            var productToDelete = dbContext.Products.Find(productId);
            if (productToDelete == null) { ShowError("Товар с таким ID не найден."); Pause(); return; }
            Console.WriteLine($"Найден товар: {productToDelete.Name} (ID: {productToDelete.Id})");
            if (!GetConfirmation("Вы уверены, что хотите НАВСЕГДА удалить этот товар? (да/нет): "))
            {
                Console.WriteLine("Удаление отменено."); Pause(); return;
            }
            try
            {
                dbContext.Products.Remove(productToDelete);
                dbContext.SaveChanges();
                ShowSuccess("Товар успешно удален.");
            }
            catch (DbUpdateException)
            {
                dbContext.ChangeTracker.Clear();
                ShowError("\nОШИБКА: Невозможно удалить этот товар, так как он уже присутствует в истории покупок.");
            }
            catch (Exception ex)
            {
                ShowError($"\nПроизошла непредвиденная ошибка: {ex.Message}");
            }
            Pause();
        }

        static void ViewAllProducts(ApplicationDbContext dbContext)
        {
            DisplayProductsList(dbContext.Products.ToList(), "Каталог товаров");
            Pause();
        }
        #endregion

        #region Purchase Management
        static void CreateNewPurchase(ApplicationDbContext dbContext)
        {
            ShowHeader("Оформление новой покупки");
            var client = FindClient(dbContext);
            if (client == null) { Pause(); return; }
            Console.Clear();
            ShowHeader($"Оформление покупки для клиента: {client.FullName}");
            var purchaseItems = new List<PurchaseItem>();
            while (true)
            {
                ShowHeader("Доступные товары");
                var availableProducts = dbContext.Products.ToList();
                DisplayProductsList(availableProducts, "");
                Console.WriteLine("\n--- Добавление товара в чек ---");
                Console.WriteLine("Введите ID товара для добавления или '0' для завершения.");
                int productId = GetPositiveInt("Ваш выбор: ");
                if (productId == 0) break;
                var product = availableProducts.FirstOrDefault(p => p.Id == productId);
                if (product == null) { ShowError("Товар с таким ID не найден."); continue; }
                int quantity = GetPositiveInt($"Введите количество для '{product.Name}': ");
                if (quantity == 0) { ShowError("Количество не может быть равно нулю."); continue; }
                var existingItem = purchaseItems.FirstOrDefault(item => item.ProductId == productId);
                if (existingItem != null)
                {
                    existingItem.Quantity += quantity;
                    ShowSuccess($"Количество товара '{product.Name}' увеличено на {quantity} шт.");
                }
                else
                {
                    purchaseItems.Add(new PurchaseItem { ProductId = product.Id, Quantity = quantity, PriceAtPurchase = product.Price });
                    ShowSuccess($"Добавлено: {product.Name} x {quantity} шт.");
                }
                Pause();
            }
            if (!purchaseItems.Any()) { Console.WriteLine("Корзина пуста. Покупка отменена."); Pause(); return; }
            decimal totalAmount = purchaseItems.Sum(item => item.PriceAtPurchase * item.Quantity);
            decimal discount = totalAmount * ((client.LoyaltyCard?.DiscountPercent ?? 0) / 100);
            decimal finalAmount = totalAmount - discount;
            Console.Clear();
            ShowHeader("Подтверждение покупки");
            Console.WriteLine($"Клиент: {client.FullName}");
            Console.WriteLine($"Общая сумма: {totalAmount:F2} руб.");
            Console.WriteLine($"Скидка по карте ({client.LoyaltyCard?.DiscountPercent ?? 0:F2}%): {discount:F2} руб.");
            Console.WriteLine($"Итого к оплате: {finalAmount:F2} руб.");
            if (!GetConfirmation("\nОформить покупку? (да/нет): ")) { Console.WriteLine("Покупка отменена."); Pause(); return; }
            string[] paymentOptions = { "Карта", "Наличные" };
            string paymentMethod = GetSpecificString("Введите способ оплаты (Карта/Наличные): ", paymentOptions);
            var newPurchase = new Purchase { ClientId = client.Id, PurchaseDate = DateTime.UtcNow, PaymentMethod = paymentMethod, PurchaseItems = purchaseItems };
            dbContext.Purchases.Add(newPurchase);
            dbContext.SaveChanges();
            ShowSuccess("Покупка успешно оформлена и сохранена!");
            Pause();
        }

        static void SearchPurchase(ApplicationDbContext dbContext)
        {
            ShowHeader("Поиск покупки");
            Console.WriteLine("Выберите способ поиска:");
            Console.WriteLine("1. По ID покупки");
            Console.WriteLine("2. По ID клиента");
            Console.WriteLine("3. По дате покупки");
            string choice = GetString("Ваш выбор: ");
            List<Purchase> purchasesFound = new List<Purchase>();
            string searchTitle = "Результаты поиска";
            switch (choice)
            {
                case "1":
                    int purchaseId = GetPositiveInt("Введите ID покупки: ");
                    var purchaseById = dbContext.Purchases.Include(p => p.Client).Include(p => p.PurchaseItems).ThenInclude(pi => pi.Product).FirstOrDefault(p => p.Id == purchaseId);
                    if (purchaseById != null) purchasesFound.Add(purchaseById);
                    searchTitle = $"Поиск по ID покупки: {purchaseId}";
                    break;
                case "2":
                    var client = FindClient(dbContext);
                    if (client == null) { Pause(); return; }
                    purchasesFound = dbContext.Purchases.Where(p => p.ClientId == client.Id).Include(p => p.Client).Include(p => p.PurchaseItems).ThenInclude(pi => pi.Product).OrderByDescending(p => p.PurchaseDate).ToList();
                    searchTitle = $"Покупки клиента: {client.FullName}";
                    break;
                case "3":
                    DateTime date = GetValidDate("Введите дату (дд.мм.гггг): ");
                    purchasesFound = dbContext.Purchases.Where(p => p.PurchaseDate.Date == date.Date).Include(p => p.Client).Include(p => p.PurchaseItems).ThenInclude(pi => pi.Product).OrderByDescending(p => p.PurchaseDate).ToList();
                    searchTitle = $"Покупки за {date:dd.MM.yyyy}";
                    break;
                default: ShowError("Неверный выбор."); break;
            }
            DisplayPurchasesList(purchasesFound, searchTitle);
            Pause();
        }

        static void DisplayPurchasesList(List<Purchase> purchases, string title)
        {
            ShowHeader(title);
            if (!purchases.Any()) { Console.WriteLine("Покупки не найдены."); return; }
            foreach (var purchase in purchases)
            {
                Console.WriteLine(new string('=', 70));
                Console.WriteLine($"Чек №{purchase.Id} от {purchase.PurchaseDate:dd.MM.yyyy HH:mm} (Клиент: {purchase.Client.FullName})");
                Console.WriteLine(new string('-', 70));
                DisplayPurchaseItems(purchase.PurchaseItems.ToList());
            }
            Console.WriteLine(new string('=', 70));
            Console.WriteLine($"\nНайдено покупок: {purchases.Count}");
        }

        static void EditPurchase(ApplicationDbContext dbContext)
        {
            ShowHeader("Редактирование покупки");
            var purchase = FindPurchase(dbContext);
            if (purchase == null) { Pause(); return; }
            bool hasChanges = false;
            while (true)
            {
                Console.Clear();
                ShowHeader($"Редактирование покупки №{purchase.Id}");
                DisplayPurchaseDetails(purchase, "");
                Console.WriteLine("\nЧто вы хотите сделать?");
                Console.WriteLine("1. Изменить способ оплаты");
                Console.WriteLine("2. Добавить товар");
                Console.WriteLine("3. Изменить количество товара");
                Console.WriteLine("4. Удалить товар из покупки");
                Console.WriteLine("0. Сохранить изменения и выйти");
                string choice = GetString("\nВаш выбор: ");
                switch (choice)
                {
                    case "1":
                        string[] paymentOptions = { "Карта", "Наличные" };
                        string newPaymentMethod = GetSpecificString("Новый способ оплаты (Карта/Наличные): ", paymentOptions);
                        if (newPaymentMethod != purchase.PaymentMethod) { purchase.PaymentMethod = newPaymentMethod; hasChanges = true; ShowSuccess("Способ оплаты будет обновлен."); }
                        else { ShowError("Новый способ оплаты совпадает с текущим."); }
                        Pause(); break;
                    case "2": if (AddProductToPurchase(dbContext, purchase)) hasChanges = true; break;
                    case "3": if (EditProductQuantityInPurchase(purchase)) hasChanges = true; break;
                    case "4": if (RemoveProductFromPurchase(dbContext, purchase)) hasChanges = true; break;
                    case "0":
                        if (hasChanges) { dbContext.SaveChanges(); ShowSuccess("Изменения в покупке успешно сохранены!"); }
                        else { Console.WriteLine("Изменений не было."); }
                        Pause(); return;
                    default: ShowError("Неверный выбор."); Pause(); break;
                }
            }
        }

        static Purchase FindPurchase(ApplicationDbContext dbContext)
        {
            ViewAllPurchases(dbContext);
            int purchaseId = GetPositiveInt("\nВведите ID покупки: ");
            return dbContext.Purchases.Include(p => p.Client).Include(p => p.PurchaseItems).ThenInclude(pi => pi.Product).FirstOrDefault(p => p.Id == purchaseId);
        }

        static void DisplayPurchaseDetails(Purchase purchase, string title)
        {
            ShowHeader(title);
            if (purchase == null) { Console.WriteLine("Покупка не найдена."); return; }
            Console.WriteLine($"ID покупки: {purchase.Id}");
            Console.WriteLine($"Клиент: {purchase.Client.FullName} (ID: {purchase.Client.Id})");
            Console.WriteLine($"Дата покупки: {purchase.PurchaseDate:dd.MM.yyyy HH:mm}");
            Console.WriteLine($"Способ оплаты: {purchase.PaymentMethod}");
            Console.WriteLine("\n--- Товары в покупке ---");
            DisplayPurchaseItems(purchase.PurchaseItems.ToList());
        }

        static bool AddProductToPurchase(ApplicationDbContext dbContext, Purchase purchase)
        {
            ViewAllProducts(dbContext);
            int productId = GetPositiveInt("\nВведите ID товара для добавления: ");
            var productToAdd = dbContext.Products.Find(productId);
            if (productToAdd == null) { ShowError("Товар с таким ID не найден."); Pause(); return false; }
            var existingItem = purchase.PurchaseItems.FirstOrDefault(pi => pi.ProductId == productId);
            if (existingItem != null) { ShowError("Этот товар уже есть в покупке. Используйте функцию изменения количества."); Pause(); return false; }
            int quantity = GetPositiveInt($"Введите количество товара '{productToAdd.Name}': ");
            if (quantity == 0) { ShowError("Количество не может быть равно нулю."); Pause(); return false; }
            var newItem = new PurchaseItem { ProductId = productToAdd.Id, Quantity = quantity, PriceAtPurchase = productToAdd.Price };
            purchase.PurchaseItems.Add(newItem);
            ShowSuccess($"Товар '{productToAdd.Name}' добавлен в покупку.");
            Pause();
            return true;
        }

        static bool EditProductQuantityInPurchase(Purchase purchase)
        {
            if (!purchase.PurchaseItems.Any()) { ShowError("В покупке нет товаров для редактирования."); Pause(); return false; }
            DisplayPurchaseItems(purchase.PurchaseItems.ToList());
            int productId = GetPositiveInt("\nВведите ID товара для изменения количества: ");
            var itemToEdit = purchase.PurchaseItems.FirstOrDefault(pi => pi.ProductId == productId);
            if (itemToEdit == null) { ShowError("Товар с таким ID в этой покупке не найден."); Pause(); return false; }
            int newQuantity = GetPositiveInt($"Введите новое количество для '{itemToEdit.Product.Name}' (текущее: {itemToEdit.Quantity}): ");
            if (newQuantity == 0) { ShowError("Количество не может быть равно нулю. Для удаления используйте соответствующий пункт меню."); Pause(); return false; }
            if (newQuantity == itemToEdit.Quantity) { ShowError("Новое количество совпадает с текущим."); Pause(); return false; }
            itemToEdit.Quantity = newQuantity;
            ShowSuccess($"Количество товара '{itemToEdit.Product.Name}' изменено на {newQuantity}.");
            Pause();
            return true;
        }

        static bool RemoveProductFromPurchase(ApplicationDbContext dbContext, Purchase purchase)
        {
            if (!purchase.PurchaseItems.Any()) { ShowError("В покупке нет товаров для удаления."); Pause(); return false; }
            DisplayPurchaseItems(purchase.PurchaseItems.ToList());
            int productId = GetPositiveInt("\nВведите ID товара для удаления: ");
            var itemToRemove = purchase.PurchaseItems.FirstOrDefault(pi => pi.ProductId == productId);
            if (itemToRemove == null) { ShowError("Товар с таким ID в этой покупке не найден."); Pause(); return false; }
            if (GetConfirmation($"Вы уверены, что хотите удалить '{itemToRemove.Product.Name}' из покупки? (да/нет): "))
            {
                dbContext.PurchaseItems.Remove(itemToRemove);
                ShowSuccess($"Товар '{itemToRemove.Product.Name}' удален из покупки.");
                Pause();
                return true;
            }
            Console.WriteLine("Удаление отменено."); Pause();
            return false;
        }

        static void DeletePurchase(ApplicationDbContext dbContext)
        {
            ShowHeader("Удаление покупки");
            var purchaseToDelete = FindPurchase(dbContext);
            if (purchaseToDelete == null) { ShowError("Покупка не найдена."); Pause(); return; }
            Console.Clear();
            DisplayPurchaseDetails(purchaseToDelete, $"Удаление покупки №{purchaseToDelete.Id}");
            if (GetConfirmation("\nВы уверены, что хотите удалить эту покупку? Это действие необратимо. (да/нет): "))
            {
                dbContext.Purchases.Remove(purchaseToDelete); // Cascade удалит PurchaseItems
                dbContext.SaveChanges();
                ShowSuccess("Покупка успешно удалена.");
            }
            else
            {
                Console.WriteLine("Удаление отменено.");
            }
            Pause();
        }

        static void ViewAllPurchases(ApplicationDbContext dbContext)
        {
            var purchases = dbContext.Purchases.Include(p => p.Client).OrderByDescending(p => p.PurchaseDate).ToList();
            ShowHeader("Список всех покупок");
            if (!purchases.Any())
            {
                Console.WriteLine("Покупок пока не было.");
                Pause();
                return;
            }
            Console.WriteLine($"{"ID",-5} | {"Дата",-20} | {"Клиент",-30} | {"Способ оплаты"}");
            Console.WriteLine(new string('=', 75));
            foreach (var p in purchases)
            {
                Console.WriteLine($"{p.Id,-5} | {p.PurchaseDate:dd.MM.yyyy HH:mm,-20} | {p.Client.FullName,-30} | {p.PaymentMethod}");
            }
        }

        static void DisplayPurchaseItems(List<PurchaseItem> items)
        {
            if (!items.Any()) { Console.WriteLine("Товары отсутствуют."); return; }
            decimal total = 0;
            foreach (var item in items)
            {
                decimal subtotal = item.PriceAtPurchase * item.Quantity;
                total += subtotal;
                Console.WriteLine($"  - {item.Product?.Name ?? "Удаленный товар",-25} | {item.Quantity} шт. x {item.PriceAtPurchase:F2} руб. = {subtotal:F2} руб.");
            }
            Console.WriteLine(new string('-', 60));
            Console.WriteLine($"  Итого: {total:F2} руб.");
        }
        #endregion

        #region Seeding
        static void SeedDatabase()
        {
            using (var dbContext = new ApplicationDbContext())
            {
                if (dbContext.Clients.Any()) return;
                Console.WriteLine("База данных пуста. Заполняем тестовыми данными...");
                var clients = new List<Client>
                {
                    new Client { FullName = "Иван Петров", Email = "ivan@test.com", PhoneNumber = "+79111234567", LoyaltyCard = new LoyaltyCard { CardNumber = "CARD-001", DiscountPercent = 5.0m } },
                    new Client { FullName = "Анна Сидорова", Email = "anna@test.com", PhoneNumber = "+79217654321", LoyaltyCard = new LoyaltyCard { CardNumber = "CARD-002", DiscountPercent = 3.5m } }
                };
                dbContext.Clients.AddRange(clients);
                var products = new List<Product>
                {
                    new Product { Name = "Молоко", Description = "Молоко 3.2%", Price = 75.50m },
                    new Product { Name = "Хлеб", Description = "Хлеб Бородинский", Price = 52.00m },
                    new Product { Name = "Сыр", Description = "Сыр Гауда 100г", Price = 150.00m }
                };
                dbContext.Products.AddRange(products);
                dbContext.SaveChanges();
            }
        }
        #endregion

        #region UI & Input Helpers
        static void ShowHeader(string title) { Console.Clear(); Console.WriteLine($"--- {title} ---"); }
        static void ShowSuccess(string message) { Console.ForegroundColor = ConsoleColor.Green; Console.WriteLine(message); Console.ResetColor(); }
        static void ShowError(string message) { Console.ForegroundColor = ConsoleColor.Red; Console.WriteLine(message); Console.ResetColor(); }
        static void Pause() { Console.WriteLine("\nНажмите любую клавишу для продолжения..."); Console.ReadKey(); }

        static DateTime GetValidDate(string qwe)
        {
            while (true)
            {
                Console.Write(qwe);
                if (DateTime.TryParseExact(Console.ReadLine(), "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date)) return date;
                ShowError("Ошибка: Введите дату в формате дд.мм.гггг");
            }
        }

        static string GetRequiredString(string qwe)
        {
            string input;
            while (true)
            {
                Console.Write(qwe);
                input = Console.ReadLine()?.Trim();
                if (!string.IsNullOrWhiteSpace(input)) return input;
                ShowError("Ошибка: Ввод не может быть пустым.");
            }
        }

        static string GetString(string qwe) { Console.Write(qwe); return Console.ReadLine()?.Trim(); }

        static int GetPositiveInt(string qwe)
        {
            int number;
            while (true)
            {
                Console.Write(qwe);
                if (int.TryParse(Console.ReadLine(), out number) && number >= 0) return number;
                ShowError("Ошибка: Введите корректное целое неотрицательное число.");
            }
        }

        static decimal GetDecimal(string qwe, decimal minValue)
        {
            decimal number;
            while (true)
            {
                Console.Write(qwe);
                string input = Console.ReadLine()?.Replace('.', ',');
                if (decimal.TryParse(input, out number) && number >= minValue) return number;
                ShowError($"Ошибка: Введите число, не меньше {minValue}.");
            }
        }

        static decimal GetPercentage(string qwe)
        {
            decimal number;
            while (true)
            {
                Console.Write(qwe);
                string input = Console.ReadLine()?.Replace('.', ',');
                if (decimal.TryParse(input, out number) && number >= 0 && number <= 100) return number;
                ShowError("Ошибка: Введите число от 0 до 100.");
            }
        }

        static bool GetConfirmation(string qwe)
        {
            while (true)
            {
                Console.Write(qwe);
                string input = Console.ReadLine()?.ToLower();
                if (input == "да") return true;
                if (input == "нет") return false;
                ShowError("Пожалуйста, введите 'да' или 'нет'.");
            }
        }

        static string GetSpecificString(string qwe, string[] validOptions)
        {
            string input;
            while (true)
            {
                Console.Write(qwe);
                input = Console.ReadLine()?.Trim();
                if (validOptions.Contains(input, StringComparer.OrdinalIgnoreCase))
                {
                    return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(input.ToLower());
                }
                ShowError($"Неверный ввод. Пожалуйста, введите один из вариантов: {string.Join(" / ", validOptions)}");
            }
        }

        static string GetValidFullName(string qwe)
        {
            var regex = new Regex(@"^[а-яА-ЯёЁa-zA-Z\s-]+$");
            while (true)
            {
                string input = GetRequiredString(qwe);
                if (regex.IsMatch(input)) return input;
                ShowError("Ошибка: ФИО должно содержать только буквы, пробелы и дефисы.");
            }
        }

        static string GetValidEmail(string qwe, bool isOptional)
        {
            while (true)
            {
                string input = GetString(qwe);
                if (isOptional && string.IsNullOrWhiteSpace(input)) return "";
                if (string.IsNullOrEmpty(input)) { ShowError("Email не может быть пустым."); continue; }
                try
                {
                    var mailAddress = new MailAddress(input);
                    return input;
                }
                catch (FormatException)
                {
                    ShowError("Ошибка: Введен некорректный формат Email (ожидается user@example.com).");
                }
            }
        }

        static string GetMaskedPhoneNumber(string qwe, bool isOptional)
        {
            while (true)
            {
                Console.Write(qwe + "+7");
                string digits = "";
                while (true)
                {
                    var key = Console.ReadKey(true);
                    if (key.Key == ConsoleKey.Enter)
                    {
                        Console.WriteLine();
                        if (isOptional && digits.Length == 0) return "";
                        if (digits.Length != 10)
                        {
                            ShowError("Номер должен содержать 10 цифр после +7.");
                            break;
                        }
                        return "+7" + digits;
                    }
                    else if (key.Key == ConsoleKey.Backspace)
                    {
                        if (digits.Length > 0)
                        {
                            digits = digits[..^1];
                            Console.Write("\b \b");
                        }
                    }
                    else if (char.IsDigit(key.KeyChar))
                    {
                        if (digits.Length < 10)
                        {
                            digits += key.KeyChar;
                            Console.Write(key.KeyChar);
                        }
                        else
                        {
                            Console.Beep();
                        }
                    }
                }
            }
        }
        #endregion
    }
}