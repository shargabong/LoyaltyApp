using LoyaltyApp.Data;
using LoyaltyApp.Models;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;
using System.Text.RegularExpressions;

namespace LoyaltyApp
{
    class Program
    {
        #region Entry Point & Main Menus
        static void Main(string[] args)
        {
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
                    var clientById = dbContext.Clients
                        .Include(c => c.LoyaltyCard)
                        .FirstOrDefault(c => c.Id == id);
                    DisplayClientDetails(clientById, "Найденный клиент");
                    break;

                case "2":
                    string name = GetRequiredString("Введите ФИО (можно часть): ");
                    var clientsByName = dbContext.Clients
                        .Include(c => c.LoyaltyCard)
                        .Where(c => c.FullName.ToLower().Contains(name.ToLower()))
                        .ToList();
                    DisplayClientsList(clientsByName, $"Результаты поиска по ФИО: '{name}'");
                    break;

                case "3":
                    string phone = GetString("Введите номер телефона начная с 7... или 8...: ");
                    var clientsByPhone = dbContext.Clients
                        .Include(c => c.LoyaltyCard)
                        .Where(c => c.PhoneNumber != null && c.PhoneNumber.Contains(phone))
                        .ToList();
                    DisplayClientsList(clientsByPhone, $"Результаты поиска по телефону: '{phone}'");
                    break;

                case "4":
                    string email = GetString("Введите email (можно часть): ");
                    var clientsByEmail = dbContext.Clients
                        .Include(c => c.LoyaltyCard)
                        .Where(c => c.Email != null && c.Email.ToLower().Contains(email.ToLower()))
                        .ToList();
                    DisplayClientsList(clientsByEmail, $"Результаты поиска по email: '{email}'");
                    break;

                default:
                    ShowError("Неверный выбор.");
                    break;
            }
            Pause();
        }

        static void DisplayClientDetails(Client client, string title)
        {
            ShowHeader(title);
            if (client == null)
            {
                Console.WriteLine("Клиент не найден.");
                return;
            }

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
            if (!clients.Any())
            {
                Console.WriteLine("Клиенты не найдены.");
                return;
            }

            Console.WriteLine($"{"ID",-5} | {"ФИО",-30} | {"Телефон",-15} | {"Email",-25} | {"Карта лояльности",-15} | {"Скидка"}");
            Console.WriteLine(new string('=', 110));
            foreach (var client in clients)
            {
                string cardInfo = client.LoyaltyCard?.CardNumber ?? "Нет";
                string discountInfo = client.LoyaltyCard != null ? $"{client.LoyaltyCard.DiscountPercent:F2}%" : "N/A";
                Console.WriteLine($"{client.Id,-5} | {client.FullName,-30} | {client.PhoneNumber ?? "N/A",-15} | {client.Email ?? "N/A",-25} | {cardInfo,-15} | {discountInfo}");
            }
            Console.WriteLine($"\nНайдено клиентов: {clients.Count}");
        }

        static void AddNewClient(ApplicationDbContext dbContext)
        {
            ShowHeader("Добавление нового клиента");

            string fullName;

            do
            {
                fullName = GetValidFullName("Введите ФИО: ");

                if (fullName.Length > 30)
                {
                    ShowError("ФИО не должно превышать 30 символов. Повторите ввод.");
                }

            } while (fullName.Length > 30);

            string email;
            do
            {
                email = GetValidEmail("Введите Email (необязательно): ", isOptional: true);

                if (!string.IsNullOrWhiteSpace(email) && email.Length > 25)
                {
                    ShowError("Email не должен превышать 25 символов. Повторите ввод.");
                }

            } while (!string.IsNullOrWhiteSpace(email) && email.Length > 25);

            string phoneNumber = GetValidPhoneNumber("Введите номер телефона: ", isOptional: false);

            decimal discount = GetPositiveDecimal("Введите процент скидки для карты (>= 0): ");

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
                        string newPhone = GetValidPhoneNumber("Новый телефон: ", isOptional: false);
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
                        decimal newDiscount = GetPositiveDecimal("Новая скидка (>= 0): ");
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

            if (!clients.Any())
            {
                Console.WriteLine("Клиенты не найдены.");
            }
            else
            {
                Console.WriteLine($"{"ID",-5} | {"ФИО",-30} | {"Email",-25} | {"Карта лояльности ",-22}  | {"Скидка"}");
                Console.WriteLine(new string('=', 100));
                foreach (var client in clients)
                {
                    string cardInfo = client.LoyaltyCard?.CardNumber ?? "Нет";
                    string discountInfo = client.LoyaltyCard != null ? $"{client.LoyaltyCard.DiscountPercent:F2}%" : "N/A";
                    Console.WriteLine($"{client.Id,-5} | {client.FullName,-30} | {client.Email,-25} | {cardInfo,-22} | {discountInfo}");
                }
            }
            Pause();
        }

        static void ViewClientPurchaseHistory(ApplicationDbContext dbContext)
        {
            ShowHeader("Просмотр истории покупок клиента");
            var client = FindClient(dbContext);

            if (client == null) { ShowError("Клиент не найден."); Pause(); return; }

            var purchases = dbContext.Purchases
                .Where(p => p.ClientId == client.Id)
                .Include(p => p.PurchaseItems)
                .ThenInclude(pi => pi.Product)
                .OrderByDescending(p => p.PurchaseDate)
                .ToList();

            Console.Clear();
            ShowHeader($"История покупок клиента: {client.FullName}");
            if (!purchases.Any())
            {
                Console.WriteLine("У клиента еще нет покупок.");
            }
            else
            {
                foreach (var purchase in purchases)
                {
                    Console.WriteLine(new string('=', 70));
                    Console.WriteLine($"Чек №{purchase.Id} от {purchase.PurchaseDate:dd.MM.yyyy HH:mm} (Способ оплаты: {purchase.PaymentMethod})");
                    Console.WriteLine(new string('-', 70));

                    decimal purchaseTotal = 0;
                    foreach (var item in purchase.PurchaseItems)
                    {
                        decimal itemSubtotal = item.PriceAtPurchase * item.Quantity;
                        purchaseTotal += itemSubtotal;
                        Console.WriteLine($"  - {item.Product.Name,-25} | {item.Quantity} шт. x {item.PriceAtPurchase:F2} руб. = {itemSubtotal:F2} руб.");
                    }
                    Console.WriteLine(new string('-', 70));
                    Console.WriteLine($"                                                 Итого по чеку: {purchaseTotal:F2} руб.");
                }
                Console.WriteLine(new string('=', 70));
            }
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
            decimal price = GetPositiveDecimal("Введите цену товара (>= 0): ");

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
            Console.WriteLine("3. По описанию");
            Console.WriteLine("4. По диапазону цен");

            string choice = GetString("Ваш выбор: ");

            switch (choice)
            {
                case "1":
                    int id = GetPositiveInt("Введите ID товара: ");
                    var productById = dbContext.Products.FirstOrDefault(p => p.Id == id);
                    DisplayProductDetails(productById, "Найденный товар");
                    break;

                case "2":
                    string name = GetRequiredString("Введите название (можно часть): ");
                    var productsByName = dbContext.Products
                        .Where(p => p.Name.ToLower().Contains(name.ToLower()))
                        .ToList();
                    DisplayProductsList(productsByName, $"Результаты поиска по названию: '{name}'");
                    break;

                case "3":
                    string description = GetString("Введите описание (можно часть): ");
                    var productsByDescription = dbContext.Products
                        .Where(p => p.Description != null && p.Description.ToLower().Contains(description.ToLower()))
                        .ToList();
                    DisplayProductsList(productsByDescription, $"Результаты поиска по описанию: '{description}'");
                    break;

                case "4":
                    decimal minPrice = GetPositiveDecimal("Введите минимальную цену: ");
                    decimal maxPrice = GetPositiveDecimal("Введите максимальную цену: ");
                    var productsByPrice = dbContext.Products
                        .Where(p => p.Price >= minPrice && p.Price <= maxPrice)
                        .OrderBy(p => p.Price)
                        .ToList();
                    DisplayProductsList(productsByPrice, $"Результаты поиска по цене: от {minPrice:F2} до {maxPrice:F2} руб.");
                    break;

                default:
                    ShowError("Неверный выбор.");
                    break;
            }
            Pause();
        }

        static void DisplayProductDetails(Product product, string title)
        {
            ShowHeader(title);
            if (product == null)
            {
                Console.WriteLine("Товар не найден.");
                return;
            }

            Console.WriteLine($"ID: {product.Id}");
            Console.WriteLine($"Название: {product.Name}");
            Console.WriteLine($"Описание: {product.Description ?? "Нет описания"}");
            Console.WriteLine($"Цена: {product.Price:F2} руб.");
        }

        static void DisplayProductsList(List<Product> products, string title)
        {
            ShowHeader(title);
            if (!products.Any())
            {
                Console.WriteLine("Товары не найдены.");
                return;
            }

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
                    string priceString = $"{product.Price:F2} руб.";
                    Console.WriteLine($"{product.Id,-5} | {product.Name,-30} | {priceString,-15} | {product.Description}");
                }
            }
            Pause();
        }
        #endregion

        #region Purchase Management
        static void CreateNewPurchase(ApplicationDbContext dbContext)
        {
            ShowHeader("Оформление новой покупки");

            ViewAllClients(dbContext);
            int clientId = GetPositiveInt("\nВведите ID клиента: ");
            var client = dbContext.Clients.Include(c => c.LoyaltyCard).FirstOrDefault(c => c.Id == clientId);
            if (client == null) { ShowError("Клиент не найден."); Pause(); return; }
            Console.Clear();
            ShowHeader($"Оформление покупки для клиента: {client.FullName}");

            var purchaseItems = new List<PurchaseItem>();
            while (true)
            {
                ShowHeader("Доступные товары");
                var availableProducts = dbContext.Products.ToList();
                if (!availableProducts.Any()) { ShowError("Нет доступных для продажи товаров."); Pause(); return; }

                Console.WriteLine($"{"ID",-5} | {"Название",-30} | {"Цена"}");
                Console.WriteLine(new string('=', 50));
                foreach (var p in availableProducts)
                {
                    Console.WriteLine($"{p.Id,-5} | {p.Name,-30} | {p.Price:F2} руб.");
                }

                Console.WriteLine("\n--- Добавление товара в чек ---");
                Console.WriteLine("Введите ID товара для добавления или '0' для завершения.");
                int productId = GetPositiveInt("Ваш выбор: ");

                if (productId == 0) break;

                var product = dbContext.Products.Find(productId);
                if (product == null) { ShowError("Товар с таким ID не найден."); continue; }

                int quantity = GetPositiveInt($"Введите количество для '{product.Name}': ");

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

            if (!GetConfirmation("\nОформить покупку? (да/нет): "))
            {
                Console.WriteLine("Покупка отменена."); Pause(); return;
            }
            string[] paymentOptions = { "Карта", "Наличные" };
            string paymentMethod = GetSpecificString("Введите способ оплаты (Карта/Наличные): ", paymentOptions);


            var newPurchase = new Purchase { ClientId = clientId, PurchaseDate = DateTime.UtcNow, PaymentMethod = paymentMethod, PurchaseItems = purchaseItems };
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
            Console.WriteLine("4. По способу оплаты");
            Console.WriteLine("5. По товару в покупке");

            string choice = GetString("Ваш выбор: ");

            switch (choice)
            {
                case "1":
                    int purchaseId = GetPositiveInt("Введите ID покупки: ");
                    var purchaseById = dbContext.Purchases
                        .Include(p => p.Client)
                        .Include(p => p.PurchaseItems)
                        .ThenInclude(pi => pi.Product)
                        .FirstOrDefault(p => p.Id == purchaseId);
                    DisplayPurchaseDetails(purchaseById, "Найденная покупка");
                    break;

                case "2":
                    int clientId = GetPositiveInt("Введите ID клиента: ");
                    var purchasesByClient = dbContext.Purchases
                        .Include(p => p.Client)
                        .Include(p => p.PurchaseItems)
                        .ThenInclude(pi => pi.Product)
                        .Where(p => p.ClientId == clientId)
                        .OrderByDescending(p => p.PurchaseDate)
                        .ToList();
                    DisplayPurchasesList(purchasesByClient, $"Покупки клиента ID: {clientId}");
                    break;

                case "3":
                    DateTime date = GetValidDate("Введите дату (дд.мм.гггг): ");
                    var purchasesByDate = dbContext.Purchases
                        .Include(p => p.Client)
                        .Include(p => p.PurchaseItems)
                        .ThenInclude(pi => pi.Product)
                        .Where(p => p.PurchaseDate.Date == date.Date)
                        .OrderByDescending(p => p.PurchaseDate)
                        .ToList();
                    DisplayPurchasesList(purchasesByDate, $"Покупки за {date:dd.MM.yyyy}");
                    break;

                case "4":
                    string[] paymentOptions = { "Карта", "Наличные" };
                    string paymentMethod = GetSpecificString("Введите способ оплаты (Карта/Наличные): ", paymentOptions);
                    var purchasesByPayment = dbContext.Purchases
                        .Include(p => p.Client)
                        .Include(p => p.PurchaseItems)
                        .ThenInclude(pi => pi.Product)
                        .Where(p => p.PaymentMethod == paymentMethod)
                        .OrderByDescending(p => p.PurchaseDate)
                        .ToList();
                    DisplayPurchasesList(purchasesByPayment, $"Покупки с оплатой: {paymentMethod}");
                    break;

                case "5":
                    string productName = GetRequiredString("Введите название товара (можно часть): ");
                    var purchasesByProduct = dbContext.Purchases
                        .Include(p => p.Client)
                        .Include(p => p.PurchaseItems)
                        .ThenInclude(pi => pi.Product)
                        .Where(p => p.PurchaseItems.Any(pi => pi.Product.Name.ToLower().Contains(productName.ToLower())))
                        .OrderByDescending(p => p.PurchaseDate)
                        .ToList();
                    DisplayPurchasesList(purchasesByProduct, $"Покупки с товаром: '{productName}'");
                    break;

                default:
                    ShowError("Неверный выбор.");
                    break;
            }
            Pause();
        }

        static void DisplayPurchaseDetails(Purchase purchase, string title)
        {
            ShowHeader(title);
            if (purchase == null)
            {
                Console.WriteLine("Покупка не найдена.");
                return;
            }

            Console.WriteLine($"ID покупки: {purchase.Id}");
            Console.WriteLine($"Клиент: {purchase.Client.FullName} (ID: {purchase.Client.Id})");
            Console.WriteLine($"Дата покупки: {purchase.PurchaseDate:dd.MM.yyyy HH:mm}");
            Console.WriteLine($"Способ оплаты: {purchase.PaymentMethod}");

            Console.WriteLine("\n--- Товары в покупке ---");
            DisplayPurchaseItems(purchase.PurchaseItems.ToList());
        }

        static void DisplayPurchasesList(List<Purchase> purchases, string title)
        {
            ShowHeader(title);
            if (!purchases.Any())
            {
                Console.WriteLine("Покупки не найдены.");
                return;
            }

            foreach (var purchase in purchases)
            {
                Console.WriteLine(new string('=', 70));
                Console.WriteLine($"Чек №{purchase.Id} от {purchase.PurchaseDate:dd.MM.yyyy HH:mm} (Клиент: {purchase.Client.FullName}, Способ оплаты: {purchase.PaymentMethod})");
                Console.WriteLine(new string('-', 70));
                decimal purchaseTotal = 0;
                foreach (var item in purchase.PurchaseItems)
                {
                    decimal itemSubtotal = item.PriceAtPurchase * item.Quantity;
                    purchaseTotal += itemSubtotal;
                    Console.WriteLine($" - {item.Product.Name,-25} | {item.Quantity} шт. x {item.PriceAtPurchase:F2} руб. = {itemSubtotal:F2} руб.");
                }
                Console.WriteLine(new string('-', 70));
                Console.WriteLine($" Итого по чеку: {purchaseTotal:F2} руб.");
            }
            Console.WriteLine(new string('=', 70));
            Console.WriteLine($"\nНайдено покупок: {purchases.Count}");
        }

        static void EditPurchase(ApplicationDbContext dbContext)
        {
            ShowHeader("Редактирование покупки");

            ViewAllPurchases(dbContext);

            int purchaseId = GetPositiveInt("\nВведите ID покупки для редактирования: ");
            var purchase = dbContext.Purchases
                .Include(p => p.Client)
                .Include(p => p.PurchaseItems)
                .ThenInclude(pi => pi.Product)
                .FirstOrDefault(p => p.Id == purchaseId);

            if (purchase == null)
            {
                ShowError("Покупка с таким ID не найдена.");
                Pause();
                return;
            }

            Console.Clear();
            ShowHeader($"Редактирование покупки №{purchase.Id}");
            Console.WriteLine($"Клиент: {purchase.Client.FullName}");
            Console.WriteLine($"Дата покупки: {purchase.PurchaseDate:dd.MM.yyyy HH:mm}");
            Console.WriteLine($"Способ оплаты: {purchase.PaymentMethod}");

            bool hasChanges = false;
            while (true)
            {
                Console.WriteLine("\n--- Текущие товары в покупке ---");
                DisplayPurchaseItems(purchase.PurchaseItems.ToList());

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
                        if (newPaymentMethod != purchase.PaymentMethod)
                        {
                            purchase.PaymentMethod = newPaymentMethod;
                            hasChanges = true;
                            ShowSuccess("Способ оплаты будет обновлен.");
                        }
                        else
                        {
                            ShowError("Новый способ оплаты совпадает с текущим.");
                        }
                        Pause();
                        break;

                    case "2":
                        AddProductToPurchase(dbContext, purchase);
                        hasChanges = true;
                        break;

                    case "3":
                        if (EditProductQuantityInPurchase(purchase))
                            hasChanges = true;
                        break;

                    case "4":
                        if (RemoveProductFromPurchase(purchase))
                            hasChanges = true;
                        break;

                    case "0":
                        if (hasChanges)
                        {
                            try
                            {
                                dbContext.SaveChanges();
                                ShowSuccess("Изменения в покупке успешно сохранены!");
                            }
                            catch (Exception ex)
                            {
                                ShowError($"Ошибка при сохранении: {ex.Message}");
                            }
                        }
                        else
                        {
                            Console.WriteLine("Изменений не было.");
                        }
                        Pause();
                        return;

                    default:
                        ShowError("Неверный выбор.");
                        Pause();
                        break;
                }
            }
        }

        static void DeletePurchase(ApplicationDbContext dbContext)
        {
            ShowHeader("Удаление покупки");

            ViewAllPurchases(dbContext);

            int purchaseId = GetPositiveInt("\nВведите ID покупки для удаления: ");
            var purchase = dbContext.Purchases
                .Include(p => p.Client)
                .Include(p => p.PurchaseItems)
                .FirstOrDefault(p => p.Id == purchaseId);

            if (purchase == null)
            {
                ShowError("Покупка с таким ID не найдена.");
                Pause();
                return;
            }

            Console.Clear();
            ShowHeader($"Удаление покупки №{purchase.Id}");
            Console.WriteLine($"Клиент: {purchase.Client.FullName}");
            Console.WriteLine($"Дата покупки: {purchase.PurchaseDate:dd.MM.yyyy HH:mm}");
            Console.WriteLine($"Способ оплаты: {purchase.PaymentMethod}");

            Console.WriteLine("\n--- Товары в покупке ---");
            DisplayPurchaseItems(purchase.PurchaseItems.ToList());

            if (!GetConfirmation("\nВы уверены, что хотите удалить эту покупку? (да/нет): "))
            {
                Console.WriteLine("Удаление отменено.");
                Pause();
                return;
            }

            try
            {
                dbContext.PurchaseItems.RemoveRange(purchase.PurchaseItems);
                dbContext.Purchases.Remove(purchase);
                dbContext.SaveChanges();
                ShowSuccess("Покупка успешно удалена!");
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка при удалении покупки: {ex.Message}");
            }

            Pause();
        }

        static void AddProductToPurchase(ApplicationDbContext dbContext, Purchase purchase)
        {
            Console.Clear();
            ShowHeader("Добавление товара в покупку");

            var availableProducts = dbContext.Products.ToList();
            if (!availableProducts.Any())
            {
                ShowError("Нет доступных товаров для добавления.");
                Pause();
                return;
            }

            Console.WriteLine("Доступные товары:");
            Console.WriteLine($"{"ID",-5} | {"Название",-30} | {"Цена"}");
            Console.WriteLine(new string('=', 50));
            foreach (var product in availableProducts)
            {
                Console.WriteLine($"{product.Id,-5} | {product.Name,-30} | {product.Price:F2} руб.");
            }

            int productId = GetPositiveInt("\nВведите ID товара для добавления: ");
            var productToAdd = availableProducts.FirstOrDefault(p => p.Id == productId);

            if (productToAdd == null)
            {
                ShowError("Товар с таким ID не найден.");
                Pause();
                return;
            }

            var existingItem = purchase.PurchaseItems.FirstOrDefault(pi => pi.ProductId == productId);
            if (existingItem != null)
            {
                ShowError("Этот товар уже есть в покупке. Используйте функцию изменения количества.");
                Pause();
                return;
            }

            int quantity = GetPositiveInt($"Введите количество товара '{productToAdd.Name}': ");

            purchase.PurchaseItems.Add(new PurchaseItem
            {
                ProductId = productToAdd.Id,
                Quantity = quantity,
                PriceAtPurchase = productToAdd.Price,
                PurchaseId = purchase.Id
            });

            ShowSuccess($"Товар '{productToAdd.Name}' добавлен в покупку.");
            Pause();
        }

        static bool EditProductQuantityInPurchase(Purchase purchase)
        {
            if (!purchase.PurchaseItems.Any())
            {
                ShowError("В покупке нет товаров для редактирования.");
                Pause();
                return false;
            }

            Console.WriteLine("\n--- Товары в покупке ---");
            for (int i = 0; i < purchase.PurchaseItems.Count; i++)
            {
                var item = purchase.PurchaseItems.ElementAt(i);
                Console.WriteLine($"{i + 1}. {item.Product.Name} - {item.Quantity} шт. x {item.PriceAtPurchase:F2} руб.");
            }

            int itemIndex = GetPositiveInt("Введите номер товара для изменения количества: ") - 1;

            if (itemIndex < 0 || itemIndex >= purchase.PurchaseItems.Count)
            {
                ShowError("Неверный номер товара.");
                Pause();
                return false;
            }

            var itemToEdit = purchase.PurchaseItems.ElementAt(itemIndex);
            int newQuantity = GetPositiveInt($"Введите новое количество для '{itemToEdit.Product.Name}' (текущее: {itemToEdit.Quantity}): ");

            if (newQuantity == 0)
            {
                purchase.PurchaseItems.Remove(itemToEdit);
                ShowSuccess($"Товар '{itemToEdit.Product.Name}' удален из покупки.");
            }
            else
            {
                itemToEdit.Quantity = newQuantity;
                ShowSuccess($"Количество товара '{itemToEdit.Product.Name}' изменено на {newQuantity}.");
            }

            Pause();
            return true;
        }

        static bool RemoveProductFromPurchase(Purchase purchase)
        {
            if (!purchase.PurchaseItems.Any())
            {
                ShowError("В покупке нет товаров для удаления.");
                Pause();
                return false;
            }

            Console.WriteLine("\n--- Товары в покупке ---");
            for (int i = 0; i < purchase.PurchaseItems.Count; i++)
            {
                var item = purchase.PurchaseItems.ElementAt(i);
                Console.WriteLine($"{i + 1}. {item.Product.Name} - {item.Quantity} шт. x {item.PriceAtPurchase:F2} руб.");
            }

            int itemIndex = GetPositiveInt("Введите номер товара для удаления: ") - 1;

            if (itemIndex < 0 || itemIndex >= purchase.PurchaseItems.Count)
            {
                ShowError("Неверный номер товара.");
                Pause();
                return false;
            }

            var itemToRemove = purchase.PurchaseItems.ElementAt(itemIndex);

            if (GetConfirmation($"Вы уверены, что хотите удалить '{itemToRemove.Product.Name}' из покупки? (да/нет): "))
            {
                purchase.PurchaseItems.Remove(itemToRemove);
                ShowSuccess($"Товар '{itemToRemove.Product.Name}' удален из покупки.");
                Pause();
                return true;
            }

            Console.WriteLine("Удаление отменено.");
            Pause();
            return false;
        }

        static void ViewAllPurchases(ApplicationDbContext dbContext)
        {
            var purchases = dbContext.Purchases
                .Include(p => p.Client)
                .Include(p => p.PurchaseItems)
                .ThenInclude(pi => pi.Product)
                .OrderByDescending(p => p.PurchaseDate)
                .ToList();

            if (!purchases.Any())
            {
                Console.WriteLine("Покупки не найдены.");
                return;
            }

            Console.WriteLine($"{"ID",-5} | {"Дата",-20} | {"Клиент",-25} | {"Способ оплаты",-12} | {"Товаров"}");
            Console.WriteLine(new string('=', 80));
            foreach (var purchase in purchases)
            {
                Console.WriteLine($"{purchase.Id,-5} | {purchase.PurchaseDate:dd.MM.yyyy HH:mm} | {purchase.Client.FullName,-25} | {purchase.PaymentMethod,-12} | {purchase.PurchaseItems.Count} шт.");
            }
        }

        static void DisplayPurchaseItems(List<PurchaseItem> items)
        {
            if (!items.Any())
            {
                Console.WriteLine("Товары отсутствуют.");
                return;
            }

            decimal total = 0;
            foreach (var item in items)
            {
                decimal subtotal = item.PriceAtPurchase * item.Quantity;
                total += subtotal;
                Console.WriteLine($"  - {item.Product.Name,-25} | {item.Quantity} шт. x {item.PriceAtPurchase:F2} руб. = {subtotal:F2} руб.");
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

                Console.WriteLine("База данных пуста. Добавляем тестовые данные...");


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
        static void ShowHeader(string title) { Console.Clear(); Console.WriteLine($"--- {title} ---"); }
        static void ShowSuccess(string message) { Console.ForegroundColor = ConsoleColor.Green; Console.WriteLine(message); Console.ResetColor(); }
        static void ShowError(string message) { Console.ForegroundColor = ConsoleColor.Red; Console.WriteLine(message); Console.ResetColor(); }
        static void Pause() { Console.WriteLine("\nНажмите любую клавишу для продолжения..."); Console.ReadKey(); }

        static DateTime GetValidDate(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                if (DateTime.TryParseExact(Console.ReadLine(), "dd.MM.yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime date))
                    return date;
                ShowError("Ошибка: Введите дату в формате дд.мм.гггг");
            }
        }

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

        static string GetString(string prompt) { Console.Write(prompt); return Console.ReadLine()?.Trim(); }

        static int GetPositiveInt(string prompt)
        {
            int number;
            while (true)
            {
                Console.Write(prompt);
                if (int.TryParse(Console.ReadLine(), out number) && number >= 0) return number;
                ShowError("Ошибка: Введите корректное целое неотрицательное число.");
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
                ShowError("Ошибка: Введите корректное неотрицательное число (например, 120,50).");
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

        static string GetSpecificString(string prompt, string[] validOptions)
        {
            string input;
            while (true)
            {
                Console.Write(prompt);
                input = Console.ReadLine()?.Trim();

                if (validOptions.Contains(input, StringComparer.OrdinalIgnoreCase))
                {
                    return input;
                  }

                ShowError($"Неверный ввод. Пожалуйста, введите один из вариантов: {string.Join(" / ", validOptions)}");
            }
        }

        static string GetValidFullName(string prompt)
        {
            var regex = new Regex(@"^[а-яА-ЯёЁa-zA-Z\s-]+$");
            while (true)
            {
                string input = GetRequiredString(prompt);
                if (regex.IsMatch(input)) return input;
                ShowError("Ошибка: ФИО должно содержать только буквы, пробелы и дефисы.");
            }
        }

        static string GetValidEmail(string prompt, bool isOptional)
        {
            while (true)
            {
                string input = GetString(prompt);
                if (isOptional && string.IsNullOrWhiteSpace(input)) return input;

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

        static string GetValidPhoneNumber(string prompt, bool isOptional)
        {
            while (true)
            {
                string input = GetString(prompt);
                if (isOptional && string.IsNullOrWhiteSpace(input)) return input;

                string digitsOnly = new string(input.Where(char.IsDigit).ToArray());

                if (digitsOnly.Length == 12 && (digitsOnly.StartsWith("7") || digitsOnly.StartsWith("8")))
                {
                    return input;
                }
                ShowError("Ошибка: Введите корректный российский номер телефона начиная с 7... или 8... (11 цифр).");
            }
        }
        #endregion
    }
}