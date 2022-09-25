using Figgle;
using MidtermProject.Attributes;
using MidtermProject.DAL;
using MidtermProject.Entities;
using MidtermProject.Enums;
using MidtermProject.Services;
using MidtermProject.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MidtermProject
{
    internal static class ApplicationWorker
    {

        public static void Init(int max_width, int max_height)
        {
            MAX_WIDTH = max_width;
            MAX_HEIGHT = max_height;
            CustomConsole.Init(max_width, max_height);
        }

        public static void CheckState()
        {
            if (MAX_WIDTH == 0 && MAX_HEIGHT == 0) throw new Exception("Helper Class was not initialized!");
        }

        // Stateful Properties & Methods (Requiring Init())

        public static ApplicationUser? currentUser;


        private static int MAX_WIDTH = 0;
        private static int MAX_HEIGHT = 0;

        public static int cursorSelection = 0;
        public static int cursorSelectionValue = -1;
        public static int cursorSelectionLimit = 1;
        public static bool showSelectionDescription = false;

        public static int tableCursor = 1;

        public static bool navigateToPreviousPage = false;

        public static int persistedCursorSelection = 0;
        public static bool overrideSelection = false;

        public delegate void FuncNoParam();
        public delegate void Func2Param(ApplicationStateFlagsEnum destination, bool isPrev);
        public static Transaction newTransaction;
        public static bool showNewTransactionInfo = true;
        public static bool showNewTransactionItems = true;
        public static bool isEditTransaction = false;
        public static Func2Param forceRedirect;
        public static FuncNoParam singleExternalCall;
        public static FuncNoParam popNavigationHistory;
        public static void DrawPage(string pageName)
        {
            CheckState();

            Console.Clear();
            DrawBox(0, MAX_WIDTH + 1, 0, MAX_HEIGHT + 1);

            CustomConsole.Write(SystemName, 1, 1, MAX_WIDTH, MAX_HEIGHT, flag: CustomConsole.ConsoleFormatFlags.CENTER);
            CustomConsole.Write($"({pageName})", 1, 2, MAX_WIDTH, MAX_HEIGHT, flag: CustomConsole.ConsoleFormatFlags.CENTER);
        }
        public static void DrawBottomPanel()
        {
            CheckState();
            int height = MAX_HEIGHT - 1;
            DrawClosedPipe(height, 0, MAX_WIDTH + 1);

            string currentDate = DateTime.Now.ToString("MMMM dd, yyyy");
            int datePillarLoc = MAX_WIDTH - currentDate.Length - 1;
            Console.SetCursorPosition(1, height + 1);

            if (ApplicationWorker.currentUser != null)
            {
                CustomConsole.Write(
                    $"Logged in as: {ApplicationWorker.currentUser.CompleteName}",
                    1,
                    height + 1,
                    null,
                    null,
                    flag: CustomConsole.ConsoleFormatFlags.LEFT);
            }
            CustomConsole.Write(currentDate, 1, height + 1, null, null, flag: CustomConsole.ConsoleFormatFlags.RIGHT);
            CustomConsole.Write("║", 1, height + 1, datePillarLoc, null, flag: CustomConsole.ConsoleFormatFlags.RIGHT);
            CustomConsole.Write($"╦", 1, height, datePillarLoc, null, flag: CustomConsole.ConsoleFormatFlags.RIGHT);
            CustomConsole.Write($"╩", 1, height + 2, datePillarLoc, null, flag: CustomConsole.ConsoleFormatFlags.RIGHT);

        }
        public static void DrawLauncher()
        {
            CheckState();
            DrawPage("Launcher Page");
            DrawBottomPanel();
            int instructionX = MAX_WIDTH - DateTime.Now.ToString("MMMM dd, yyyy").Length - 1;
            CustomConsole.Write(
                "Use up & down arrow keys or 'W' & 'S' to navigate.",
                1,
                MAX_HEIGHT,
                instructionX,
                MAX_HEIGHT,
                flag: CustomConsole.ConsoleFormatFlags.MIDDLE_CENTER);


            SelectionViewModel[] selectionVM = new SelectionViewModel[]
            {
                new SelectionViewModel
                {
                    Label = "[Authenticate]",
                    IntValue = (int)ApplicationStateFlagsEnum.LOGIN_ADMIN,
                    Description = "Redirects to authentication page"
                },
                  new SelectionViewModel
                {
                    Label = "[Exit]",
                    IntValue = (int)ApplicationStateFlagsEnum.EXIT,
                    Description = "Exits the program"
                }
            };

            cursorSelectionLimit = selectionVM.Count();

            if (cursorSelection >= cursorSelectionLimit)
            {
                cursorSelection = 0;
                cursorSelectionValue = -1;
            }

            cursorSelectionValue = selectionVM[cursorSelection].IntValue;

            string x = FiggleFonts.Standard.Render("ZINVSYS", 0);
            x = x.Replace("\r", "");
            var message = x.Split('\n');

            CustomConsole.Write(message, 1, 3, MAX_WIDTH, MAX_HEIGHT, flag: CustomConsole.ConsoleFormatFlags.MIDDLE_CENTER);

            CustomConsole.WriteCursorSelection(
                selectionVM,
                MAX_HEIGHT / 2 + 2,
                MAX_HEIGHT,
                0,
                MAX_WIDTH,
                flag: CustomConsole.ConsoleFormatFlags.MIDDLE_CENTER,
                selectedValueIndex: cursorSelection);
        }
        public static AuthenticateViewModel DrawAuthenticationPage(bool hasLoginFailed, string message)
        {
            CheckState();


            DrawPage("Authentication Page");
            DrawBottomPanel();

            int midpoint = CustomConsole.GetMidPoint(1, MAX_HEIGHT);
            CustomConsole.Write("Please Input Username & Password", 1, 8, MAX_WIDTH, midpoint, flag: CustomConsole.ConsoleFormatFlags.CENTER);

            if (hasLoginFailed)
            {
                CustomConsole.Write($"Authentication Failed: {message}", 1, 9, MAX_WIDTH, midpoint, flag: CustomConsole.ConsoleFormatFlags.CENTER);
            }

            AuthenticateViewModel authenticateViewModel = new AuthenticateViewModel();


            CustomConsole.Write("Would you like to login (Y/N):", 1, 11, MAX_WIDTH, MAX_HEIGHT, flag: CustomConsole.ConsoleFormatFlags.CENTER);
            var temp = Console.ReadKey();

            if (temp.Key != ConsoleKey.Y) return null;

            CustomConsole.Write("Enter Username: ", 1, 12, (MAX_WIDTH / 2), null, flag: CustomConsole.ConsoleFormatFlags.RIGHT);
            authenticateViewModel.Username = Console.ReadLine();

            Console.SetCursorPosition(24, 13);
            CustomConsole.Write("Enter Password: ", 1, 13, (MAX_WIDTH / 2), null, flag: CustomConsole.ConsoleFormatFlags.RIGHT);
            authenticateViewModel.Password = Console.ReadLine();

            return authenticateViewModel;
        }
        public static void DrawUserMenuPage()
        {
            CheckState();

            DrawPage("User Menu");
            DrawBottomPanel();

            SelectionViewModel[] selectionVM = new SelectionViewModel[]
            {
                new SelectionViewModel
                {
                    Label = "[Manage Users]",
                    IntValue = (int)ApplicationStateFlagsEnum.MENU_MANAGE_USER,
                    Description = "This module is responsible for any and all actions pertaining to the " +
                    "management of administrative users such as: creation of new user, update of existing user, etc.."
                },
                 new SelectionViewModel
                {
                    Label = "[Manage Customers]",
                    IntValue = (int)ApplicationStateFlagsEnum.MENU_MANAGE_CUSTOMER,
                    Description =  "This module is responsible for any and all actions pertaining to the " +
                    "management of customers such as: creation of new customers, update of existing customers, etc.."
                },  new SelectionViewModel
                {
                    Label = "[Manage Product Profiles]",
                    IntValue = (int)ApplicationStateFlagsEnum.MENU_MANAGE_PRODUCTPROFILE,
                    Description =  "This module is responsible for any and all actions pertaining to the " +
                    "management of product profiles such as: adding of new product profile, update of existing product profile, etc.. " +
                    "Additionally, a product profile must first be created before stock is added to the inventory."
                },
                  new SelectionViewModel
                {
                    Label = "[Manage Stock]",
                    IntValue = (int)ApplicationStateFlagsEnum.MENU_MANAGE_STOCK,
                    Description =  "This module is responsible for any and all actions pertaining to the " +
                    "management of stock such as: adding of new stock, update of existing stock, etc.."
                },
                    new SelectionViewModel
                {
                    Label = "[Manage Transactions]",
                    IntValue = (int)ApplicationStateFlagsEnum.MENU_MANAGE_TRANSACTIONS,
                    Description =  "This module is responsible for any and all actions pertaining to the " +
                    "management of transactions such as: creation of transactions, monitoring, etc.."
                },
                  new SelectionViewModel
                {
                    Label = "[SignOut]",
                    IntValue = (int)ApplicationStateFlagsEnum.SIGN_OUT,
                    Description =  "Log out of the current account"
                }
            };

            cursorSelectionLimit = selectionVM.Count();

            if (cursorSelection >= cursorSelectionLimit)
            {
                cursorSelection = 0;
                cursorSelectionValue = -1;
            }
            var _selection = selectionVM[cursorSelection];
            cursorSelectionValue = _selection.IntValue;

            DrawSelectionDescription(_selection);
            CustomConsole.WriteCursorSelection(
                selectionVM,
                5,
                MAX_HEIGHT,
                1,
                MAX_WIDTH,
                flag: CustomConsole.ConsoleFormatFlags.TOP_LEFT,
                selectedValueIndex: cursorSelection);

        }

        // Manage Application Users
        public static void DrawManageApplicationUserPage()
        {
            CheckState();

            DrawPage("Manage Users");
            DrawBottomPanel();

            SelectionViewModel[] selectionVM = new SelectionViewModel[]
            {
                new SelectionViewModel
                {
                    Label = "[List All Users]",
                    IntValue = (int)ApplicationStateFlagsEnum.USER_LIST,
                    Description = "Lists all users in a table"
                },
                new SelectionViewModel
                {
                    Label = "[Create User]",
                    IntValue = (int)ApplicationStateFlagsEnum.USER_CREATE,
                    Description = "Creates a new application user."
                },
                 new SelectionViewModel
                {
                    Label = "[Edit User]",
                    IntValue = (int)ApplicationStateFlagsEnum.USER_EDIT,
                    Description =  "This module is responsible for any and all actions pertaining to the " +
                    "management of customers such as: creation of new customers, update of existing customers, etc.."
                },
                  new SelectionViewModel
                {
                    Label = "[Delete User]",
                    IntValue = (int)ApplicationStateFlagsEnum.USER_DELETE,
                    Description =  "This module is responsible for any and all actions pertaining to the " +
                    "management of stock such as: adding of new stock, update of existing stock, etc.."
                },
                  new SelectionViewModel
                {
                    Label = "[Back]",
                    IntValue = (int)ApplicationStateFlagsEnum.BACK,
                    Description =  "Redirects to the Main Menu"
                }
            };

            cursorSelectionLimit = selectionVM.Count();

            if (cursorSelection >= cursorSelectionLimit)
            {
                cursorSelection = 0;
                cursorSelectionValue = -1;
            }
            var _selection = selectionVM[cursorSelection];
            cursorSelectionValue = selectionVM[cursorSelection].IntValue;
            if (!overrideSelection) persistedCursorSelection = cursorSelection;

            DrawSelectionDescription(_selection);
            CustomConsole.WriteCursorSelection(selectionVM, 5, MAX_HEIGHT, 1, MAX_WIDTH, flag: CustomConsole.ConsoleFormatFlags.TOP_LEFT, selectedValueIndex: (overrideSelection) ? persistedCursorSelection : cursorSelection);

        }

        // Manage Customers
        public static void DrawManageCustomersPage()
        {
            CheckState();

            DrawPage("Manage Customers");
            DrawBottomPanel();

            SelectionViewModel[] selectionVM = new SelectionViewModel[]
            {
                new SelectionViewModel
                {
                    Label = "[List All Customers]",
                    IntValue = (int)ApplicationStateFlagsEnum.CUSTOMER_LIST,
                    Description = "Lists all customers in a table"
                },
                new SelectionViewModel
                {
                    Label = "[Create Customer]",
                    IntValue = (int)ApplicationStateFlagsEnum.CUSTOMER_CREATE,
                    Description = "Creates a new customer."
                },
                 new SelectionViewModel
                {
                    Label = "[Edit Customer]",
                    IntValue = (int)ApplicationStateFlagsEnum.CUSTOMER_EDIT,
                    Description =  "Edit Existing Customer Information"
                },
                  new SelectionViewModel
                {
                    Label = "[Delete Customer]",
                    IntValue = (int)ApplicationStateFlagsEnum.CUSTOMER_DELETE,
                    Description =  "Deletes Existing Customer Information"
                },
                  new SelectionViewModel
                {
                    Label = "[Back]",
                    IntValue = (int)ApplicationStateFlagsEnum.BACK,
                    Description =  "Redirects to the Main Menu"
                }
            };

            cursorSelectionLimit = selectionVM.Count();

            if (cursorSelection >= cursorSelectionLimit)
            {
                cursorSelection = 0;
                cursorSelectionValue = -1;
            }
            var _selection = selectionVM[cursorSelection];
            cursorSelectionValue = selectionVM[cursorSelection].IntValue;
            if (!overrideSelection) persistedCursorSelection = cursorSelection;

            DrawSelectionDescription(_selection);
            CustomConsole.WriteCursorSelection(selectionVM, 5, MAX_HEIGHT, 1, MAX_WIDTH, flag: CustomConsole.ConsoleFormatFlags.TOP_LEFT, selectedValueIndex: (overrideSelection) ? persistedCursorSelection : cursorSelection);

        }

        // Manage Product Profiles
        public static void DrawManageProductProfilesPage()
        {
            CheckState();

            DrawPage("Manage Product Profiles");
            DrawBottomPanel();

            SelectionViewModel[] selectionVM = new SelectionViewModel[]
            {
                new SelectionViewModel
                {
                    Label = "[List All Product Profiles]",
                    IntValue = (int)ApplicationStateFlagsEnum.PRODUCT_LIST,
                    Description = "Lists all Product Profiles in a table"
                },
                new SelectionViewModel
                {
                    Label = "[Create Product Profile]",
                    IntValue = (int)ApplicationStateFlagsEnum.PRODUCT_CREATE,
                    Description = "Creates a new Product Profile in which stocks will be added/removed"
                },
                new SelectionViewModel
                {
                    Label = "[Edit Product Profile]",
                    IntValue = (int)ApplicationStateFlagsEnum.PRODUCT_EDIT,
                    Description = "Edits an Existing Product Profile in which stocks will be added/removed"
                },
                new SelectionViewModel
                {
                    Label = "[Delete Product Profile]",
                    IntValue = (int)ApplicationStateFlagsEnum.PRODUCT_DELETE,
                    Description = "Deletes an Existing Product Profile in which stocks will be added/removed"
                },
                  new SelectionViewModel
                {
                    Label = "[Back]",
                    IntValue = (int)ApplicationStateFlagsEnum.BACK,
                    Description =  "Redirects to the Main Menu"
                }
            };

            cursorSelectionLimit = selectionVM.Count();

            if (cursorSelection >= cursorSelectionLimit)
            {
                cursorSelection = 0;
                cursorSelectionValue = -1;
            }
            var _selection = selectionVM[cursorSelection];
            cursorSelectionValue = selectionVM[cursorSelection].IntValue;
            if (!overrideSelection) persistedCursorSelection = cursorSelection;

            DrawSelectionDescription(_selection);
            CustomConsole.WriteCursorSelection(selectionVM, 5, MAX_HEIGHT, 1, MAX_WIDTH, flag: CustomConsole.ConsoleFormatFlags.TOP_LEFT, selectedValueIndex: (overrideSelection) ? persistedCursorSelection : cursorSelection);

        }

        // Manage Stock
        public static void DrawManageStocksPage()
        {
            CheckState();

            DrawPage("Manage Stocks");
            DrawBottomPanel();

            SelectionViewModel[] selectionVM = new SelectionViewModel[]
            {
                new SelectionViewModel
                {
                    Label = "[List Available Stocks]",
                    IntValue = (int)ApplicationStateFlagsEnum.STOCK_LIST_IN,
                    Description = "Lists all products stocks in a table"
                },
                new SelectionViewModel
                {
                    Label = "[List Removed Stocks]",
                    IntValue = (int)ApplicationStateFlagsEnum.STOCK_LIST_OUT,
                    Description = "Lists all products stocks in a table"
                },
                 new SelectionViewModel
                {
                    Label = "[Add Product Stock]",
                    IntValue = (int)ApplicationStateFlagsEnum.STOCK_CREATE,
                    Description =  "Adds Stock to the specified product"
                },
                  new SelectionViewModel
                {
                    Label = "[Remove Product Stock]",
                    IntValue = (int)ApplicationStateFlagsEnum.STOCK_DELETE,
                    Description =  "Removes Stock from the specified product profile"
                },
                  new SelectionViewModel
                {
                    Label = "[Back]",
                    IntValue = (int)ApplicationStateFlagsEnum.BACK,
                    Description =  "Redirects to the Main Menu"
                }
            };

            cursorSelectionLimit = selectionVM.Count();

            if (cursorSelection >= cursorSelectionLimit)
            {
                cursorSelection = 0;
                cursorSelectionValue = -1;
            }
            var _selection = selectionVM[cursorSelection];
            cursorSelectionValue = selectionVM[cursorSelection].IntValue;
            if (!overrideSelection) persistedCursorSelection = cursorSelection;

            DrawSelectionDescription(_selection);
            CustomConsole.WriteCursorSelection(selectionVM, 5, MAX_HEIGHT, 1, MAX_WIDTH, flag: CustomConsole.ConsoleFormatFlags.TOP_LEFT, selectedValueIndex: (overrideSelection) ? persistedCursorSelection : cursorSelection);

        }
        public static void DrawCreateStocksPage()
        {
            start:
            string tableName = string.Empty;
            string successMessage = string.Empty;
            List<ProductProfile> data = DBContext<ProductProfile>.GetAllExisting();

            tableName = "Add Stock";

            ProductProfile selectedData = DrawTableSelection<ProductProfile>(tableName, data, DrawManageStocksPage);

            if (selectedData == null) return;
            ProductProfile selectedObject = ((ProductProfile)selectedData).Clone<ProductProfile>();

            overrideSelection = true;
            string error = "Error: ";
            int exceptionLength = error.Length;

            edit:

            int panelX1 = MAX_WIDTH / 5;
            int panelY2 = (MAX_HEIGHT - 2);
            DrawManageStocksPage();
            DrawBox(panelX1 - 1, MAX_WIDTH - 1, 4, panelY2);
            DrawClosedPipe(6, panelX1 - 1, MAX_WIDTH - 1);

            if (error.Length != exceptionLength)
            {
                ClearArea(panelX1, panelY2 - 1, MAX_WIDTH - 1, panelY2 - 1);
                DrawClosedPipe(panelY2 - 2, panelX1 - 1, MAX_WIDTH - 1);
                CustomConsole.Write(error, panelX1, panelY2 - 1, MAX_WIDTH - 1, null, flag: CustomConsole.ConsoleFormatFlags.CENTER);
                error = "Error: ";
            }


            InputProductStockViewModel viewModel = new InputProductStockViewModel();


            CustomConsole.Write("Add Stock", panelX1, 5, MAX_WIDTH - 1, null, flag: CustomConsole.ConsoleFormatFlags.CENTER);

            CustomConsole.Write($"Product: ({selectedObject.SKU}) {selectedObject.ProductName}", panelX1, 7, MAX_WIDTH - 1, null, flag: CustomConsole.ConsoleFormatFlags.TOP_LEFT);
            CustomConsole.Write($"Enter Quantity: ", panelX1, 8, MAX_WIDTH - 1, null, flag: CustomConsole.ConsoleFormatFlags.TOP_LEFT);
            viewModel.Quantity = Console.ReadLine();

            CustomConsole.Write($"Enter Manufacturing Date: ", panelX1, 9, MAX_WIDTH - 1, null, flag: CustomConsole.ConsoleFormatFlags.TOP_LEFT);
            viewModel.MfgDate = Console.ReadLine();

            CustomConsole.Write($"Enter Expiry Date: ", panelX1, 10, MAX_WIDTH - 1, null, flag: CustomConsole.ConsoleFormatFlags.TOP_LEFT);
            viewModel.ExpDate = Console.ReadLine();


            if (string.IsNullOrWhiteSpace(viewModel.Quantity))
            {
                error += "Quantity";
                goto validateData;
            }
            if (string.IsNullOrWhiteSpace(viewModel.MfgDate))
            {
                error += "MfgDate";
                goto validateData;
            }
            if (string.IsNullOrWhiteSpace(viewModel.ExpDate))
            {
                error += "ExpDate";
                goto validateData;
            }

            DateTime mfgDate, expDate;
            int quantity;

            if (!int.TryParse(viewModel.Quantity, out quantity))
            {
                error += "Please Input A Valid Quantity Value! (Not A Valid Number)";
                goto validateData;
            }

            if (!DateTime.TryParse(viewModel.MfgDate, out mfgDate))
            {
                error += "Please Input A Valid Manufacturing Date! (yyyy-MM-dd)";
                goto validateData;
            }

            if (!DateTime.TryParse(viewModel.ExpDate, out expDate))
            {
                error += "Please Input A Valid Expiry Date! (yyyy-MM-dd)";
                goto validateData;
            }

            selectedObject.AddStock(quantity, mfgDate, expDate);
            successMessage = "Successfully Added Stock!";


            validateData:
            if (error.Length != exceptionLength) goto edit;


            try
            {
                DBContext<ProductProfile>.Update(selectedObject);
                ClearArea(panelX1, panelY2 - 1, MAX_WIDTH - 1, panelY2 - 1);
                DrawClosedPipe(panelY2 - 2, panelX1 - 1, MAX_WIDTH - 1);
                CustomConsole.Write(successMessage, panelX1, panelY2 - 1, MAX_WIDTH - 1, null, flag: CustomConsole.ConsoleFormatFlags.CENTER);
                CustomConsole.Write("Press any key to continue! ", panelX1, 12, MAX_WIDTH - 1, null, flag: CustomConsole.ConsoleFormatFlags.TOP_LEFT);
                ConsoleKeyInfo input = Console.ReadKey();
                goto start;
            }
            catch (Exception ex)
            {
                error = "Error : " + ex.Message;
                goto validateData;
            }
        }
        public static void DrawRemoveStocksPage()
        {
            start:
            string tableName = string.Empty;
            string successMessage = string.Empty;
            List<ProductProfile> data = DBContext<ProductProfile>.GetAllExisting();

            tableName = "Remove Stock";

            selectProduct:
            var selectedData = DrawTableSelection<ProductProfile>(tableName, data, DrawManageStocksPage);

            if (selectedData == null) return;
            ProductProfile selectedObject = ((ProductProfile)selectedData).Clone<ProductProfile>();

            var selectedStocks = DrawTableSelection<ProductStock>($"Remove Stock ({selectedData.ProductIdentifier})", selectedObject.ProductStocks, DrawManageStocksPage);
            if (selectedStocks == null)
            {
                navigateToPreviousPage = false;
                goto selectProduct;
            }


            overrideSelection = true;
            string error = "Error: ";
            int exceptionLength = error.Length;

            edit:

            int panelX1 = MAX_WIDTH / 5;
            int panelY2 = (MAX_HEIGHT - 2);
            DrawManageStocksPage();
            DrawBox(panelX1 - 1, MAX_WIDTH - 1, 4, panelY2);
            DrawClosedPipe(6, panelX1 - 1, MAX_WIDTH - 1);

            if (error.Length != exceptionLength)
            {
                ClearArea(panelX1, panelY2 - 1, MAX_WIDTH - 1, panelY2 - 1);
                DrawClosedPipe(panelY2 - 2, panelX1 - 1, MAX_WIDTH - 1);
                CustomConsole.Write(error, panelX1, panelY2 - 1, MAX_WIDTH - 1, null, flag: CustomConsole.ConsoleFormatFlags.CENTER);
                error = "Error: ";
            }


            InputProductStockViewModel viewModel = new InputProductStockViewModel();


            CustomConsole.Write("Remove Stock", panelX1, 5, MAX_WIDTH - 1, null, flag: CustomConsole.ConsoleFormatFlags.CENTER);

            CustomConsole.Write($"Product: ({selectedObject.SKU}) {selectedObject.ProductName}", panelX1, 7, MAX_WIDTH - 1, null, flag: CustomConsole.ConsoleFormatFlags.TOP_LEFT);
            CustomConsole.Write($"Stock Batch No: {selectedStocks.Batch}", panelX1, 8, MAX_WIDTH - 1, null, flag: CustomConsole.ConsoleFormatFlags.TOP_LEFT);
            CustomConsole.Write($"Manufacturing Date: {selectedStocks.MfgDate.ToString("MMMM dd, yyyy")}", panelX1, 9, MAX_WIDTH - 1, null, flag: CustomConsole.ConsoleFormatFlags.TOP_LEFT);
            CustomConsole.Write($"Expiry Date: {selectedStocks.ExpDate.ToString("MMMM dd, yyyy")}", panelX1, 10, MAX_WIDTH - 1, null, flag: CustomConsole.ConsoleFormatFlags.TOP_LEFT);

            CustomConsole.Write($"Enter Quantity to Remove: ", panelX1, 11, MAX_WIDTH - 1, null, flag: CustomConsole.ConsoleFormatFlags.TOP_LEFT);
            var qty = Console.ReadLine();

            CustomConsole.Write($"Enter Remarks: ", panelX1, 12, MAX_WIDTH - 1, null, flag: CustomConsole.ConsoleFormatFlags.TOP_LEFT);
            string remarks = Console.ReadLine();


            if (string.IsNullOrWhiteSpace(qty))
            {
                error += "Quantity";
                goto validateData;
            }

            if (string.IsNullOrWhiteSpace(remarks))
            {
                error += "Remarks";
                goto validateData;
            }

            int quantity;

            if (!int.TryParse(qty, out quantity))
            {
                error += "Please Input A Valid Quantity Value! (Not A Valid Number)";
                goto validateData;
            }

            if (quantity > selectedObject.QtyInStock)
            {
                error += "Quantity Entered Exceeds amount in inventory! Please try a smaller value!";
                goto validateData;
            }


            selectedObject.UnlistStock(quantity, selectedStocks.MfgDate, selectedStocks.ExpDate, selectedStocks.Batch, remarks);
            successMessage = "Successfully Removed/Unlisted Stock!";


            validateData:
            if (error.Length != exceptionLength) goto edit;


            try
            {
                DBContext<ProductProfile>.Update(selectedObject);
                ClearArea(panelX1, panelY2 - 1, MAX_WIDTH - 1, panelY2 - 1);
                DrawClosedPipe(panelY2 - 2, panelX1 - 1, MAX_WIDTH - 1);
                CustomConsole.Write(successMessage, panelX1, panelY2 - 1, MAX_WIDTH - 1, null, flag: CustomConsole.ConsoleFormatFlags.CENTER);
                CustomConsole.Write("Press any key to continue! ", panelX1, 14, MAX_WIDTH - 1, null, flag: CustomConsole.ConsoleFormatFlags.TOP_LEFT);
                ConsoleKeyInfo input = Console.ReadKey();
                goto start;
            }
            catch (Exception ex)
            {
                error = "Error : " + ex.Message;
                goto validateData;
            }
        }
        public static void DrawListInStocksPage()
        {
            tableCursor = 1;
            ConsoleKeyInfo entry = new ConsoleKeyInfo('z', ConsoleKey.Z, false, false, false);
            List<ListInStocksViewModel> data = null;

            var products = DBContext<ProductProfile>.GetAllExisting();
            var productStocks = products.SelectMany(x => x.ProductStocks).Where(x => x.Quantity > 0);
            data = productStocks.Select(x => new ListInStocksViewModel
            {
                Batch = x.Batch,
                ExpiryDate = x.ExpDate,
                MfgDate = x.MfgDate,
                Quantity = x.Quantity,
                ProductName = DBContext<ProductProfile>.GetById(x.ProductId).ProductIdentifier,
                ReceivedOn = x.ReceivedOn
            }).ToList();

            while (entry.Key != ConsoleKey.Escape)
            {
                overrideSelection = true;
                DrawManageStocksPage();
                DrawTable<ListInStocksViewModel>(data, tableCursor, (MAX_WIDTH / 5) - 1, MAX_WIDTH - 1, 4, (MAX_HEIGHT + 1) - 3, "Product Available Stocks");

                entry = Console.ReadKey();

                switch (entry.Key)
                {
                    case ConsoleKey.UpArrow:
                    case ConsoleKey.W:
                        tableCursor--;
                        break;
                    case ConsoleKey.DownArrow:
                    case ConsoleKey.S:
                        tableCursor++;
                        break;
                    default:
                        break;
                }
                int rowCount = ((MAX_HEIGHT + 1) - 3) - 8;
                int pageCount = (int)Math.Ceiling((decimal)data.Count() / rowCount);
                pageCount += (pageCount == 0) ? 1 : 0;

                if (tableCursor == 0) tableCursor = pageCount;
                else if (tableCursor > pageCount) tableCursor = 1;
            }

            navigateToPreviousPage = true;



        }
        public static void DrawListOutStocksPage()
        {
            tableCursor = 1;
            ConsoleKeyInfo entry = new ConsoleKeyInfo('z', ConsoleKey.Z, false, false, false);
            List<ListOutStocksViewModel> data = null;

            var products = DBContext<ProductProfile>.GetAllExisting();
            var productStocks = products.SelectMany(x => x.UnlistedProductStock).Where(x => x.Quantity > 0);
            data = productStocks.Select(x => new ListOutStocksViewModel
            {
                Batch = x.Batch,
                Quantity = x.Quantity,
                ProductName = DBContext<ProductProfile>.GetById(x.ProductId).ProductIdentifier,
                ReceivedOn = x.ReceivedOn,
                Remarks = x.Remarks,
                RemovedOn = x.UnlistedOn
            }).ToList();

            while (entry.Key != ConsoleKey.Escape)
            {
                overrideSelection = true;
                DrawManageStocksPage();
                DrawTable<ListOutStocksViewModel>(data, tableCursor, (MAX_WIDTH / 5) - 1, MAX_WIDTH - 1, 4, (MAX_HEIGHT + 1) - 3, "Product Removed Stocks");

                entry = Console.ReadKey();

                switch (entry.Key)
                {
                    case ConsoleKey.UpArrow:
                    case ConsoleKey.W:
                        tableCursor--;
                        break;
                    case ConsoleKey.DownArrow:
                    case ConsoleKey.S:
                        tableCursor++;
                        break;
                    default:
                        break;
                }
                int rowCount = ((MAX_HEIGHT + 1) - 3) - 8;
                int pageCount = (int)Math.Ceiling((decimal)data.Count() / rowCount);
                pageCount += (pageCount == 0) ? 1 : 0;

                if (tableCursor == 0) tableCursor = pageCount;
                else if (tableCursor > pageCount) tableCursor = 1;
            }

            navigateToPreviousPage = true;

        }

        // Manage Transactions
        public static void DrawManageTransactionsPage()
        {
            CheckState();

            DrawPage("Manage Transactions");
            DrawBottomPanel();

            SelectionViewModel[] selectionVM = new SelectionViewModel[]
            {
                new SelectionViewModel
                {
                    Label = "[List All Transactions]",
                    IntValue = (int)ApplicationStateFlagsEnum.MENU_LIST_ALL_TRANSACTIONS,
                    Description = "Lists All Transactions."
                },
                new SelectionViewModel
                {
                    Label = "[List Customer Transactions]",
                    IntValue = (int)ApplicationStateFlagsEnum.MENU_LIST_CUSTOMER_TRANSACTIONS,
                    Description = "Lists All Transactions of a selected customer."
                },
                new SelectionViewModel
                {
                    Label = "[List Admin Transactions]",
                    IntValue = (int)ApplicationStateFlagsEnum.MENU_LIST_ADMIN_TRANSACTIONS,
                    Description = "Lists All Transactions of a selected customer."
                },
                new SelectionViewModel
                {
                    Label = "[Create New Transaction]",
                    IntValue = (int)ApplicationStateFlagsEnum.MENU_CREATE_TRANSACTION,
                    Description = "Redirects To the Transaction Creation Page where all transactions are created."
                },
                new SelectionViewModel
                {
                    Label = "[Edit Transaction]",
                    IntValue = (int)ApplicationStateFlagsEnum.TRANSACTION_EDIT,
                    Description = "Redirects To the Transaction Creation Page where all transactions are created."
                },
                  new SelectionViewModel
                {
                    Label = "[Back]",
                    IntValue = (int)ApplicationStateFlagsEnum.BACK,
                    Description =  "Redirects to the Main Menu"
                }
            };

            cursorSelectionLimit = selectionVM.Count();

            if (cursorSelection >= cursorSelectionLimit)
            {
                cursorSelection = 0;
                cursorSelectionValue = -1;
            }
            var _selection = selectionVM[cursorSelection];
            cursorSelectionValue = selectionVM[cursorSelection].IntValue;
            if (!overrideSelection) persistedCursorSelection = cursorSelection;

            DrawSelectionDescription(_selection);
            CustomConsole.WriteCursorSelection(selectionVM, 5, MAX_HEIGHT, 1, MAX_WIDTH, flag: CustomConsole.ConsoleFormatFlags.TOP_LEFT, selectedValueIndex: (overrideSelection) ? persistedCursorSelection : cursorSelection);

        }
        public static void DrawListTransactionsPage()
        {
            start:
            overrideSelection = true;
            FuncNoParam drawParentPage = DrawManageTransactionsPage;
            drawParentPage();
            tableCursor = 1;
            int rowCursor = 8;
            ConsoleKeyInfo entry = new ConsoleKeyInfo('z', ConsoleKey.Z, false, false, false);
            TransactionsViewModel selectedObject;
            List<TransactionsViewModel> data = DBContext<Transaction>.GetAllExisting().Select(x => new TransactionsViewModel
            {
                TotalAmountDue = x.TotalAmountDue,
                CreatedOn = x.CreatedOn,
                Customer = DBContext<ApplicationUser>.GetById(x.CustomerId.Value).CompleteName,
                TransactionId = x.TransactionId,   
                Id = x.Uid,
                LastModifiedOn = x.LastModifiedOn,
                Status = x.TransactionStatus.ToString()
            }).OrderByDescending(x => x.Id).ToList();

            selectedObject = DrawTableSelection<TransactionsViewModel>("All Transactions", data, drawParentPage);
            if (selectedObject == null) return;

            ConsoleKeyInfo info = new ConsoleKeyInfo('z', ConsoleKey.Z, false, false,false);
            newTransaction = DBContext<Transaction>.GetById(selectedObject.Id);

            while(info.Key != ConsoleKey.Escape)
            {
                drawParentPage();
                DrawTransactionInformation(28, true);
                info = Console.ReadKey();
            }

            goto start;
        }
        public static void DrawListCustomerTransactionsPage()
        {
            start:
            overrideSelection = true;
            FuncNoParam drawParentPage = DrawManageTransactionsPage;
            drawParentPage();
            tableCursor = 1;
            int rowCursor = 8;
            ConsoleKeyInfo entry = new ConsoleKeyInfo('z', ConsoleKey.Z, false, false, false);

            Customer selectedCustomer;
            List<ApplicationUser> users = DBContext<ApplicationUser>.GetByCustomQuery(x => x.UserType == ApplicationUserTypeEnum.CUSTOMER);
            selectedCustomer = (Customer)(object)DrawTableSelection<ApplicationUser>("Select Customer", users, drawParentPage);
            if (selectedCustomer == null) return;

            TransactionsViewModel selectedTransaction;
            List<TransactionsViewModel> data = DBContext<Transaction>.GetByCustomQuery(x=>x.CustomerId.Value == selectedCustomer.Uid)
                .Select(x => new TransactionsViewModel
            {
                TotalAmountDue = x.TotalAmountDue,
                CreatedOn = x.CreatedOn,
                Customer = DBContext<ApplicationUser>.GetById(x.CustomerId.Value).CompleteName,
                TransactionId = x.TransactionId,
                Id = x.Uid,
                LastModifiedOn = x.LastModifiedOn,
                Status = x.TransactionStatus.ToString()
            }).OrderByDescending(x => x.Id).ToList();

            selectedTransaction = DrawTableSelection<TransactionsViewModel>($"Customer Transactions ({selectedCustomer.CompleteName})", data, drawParentPage);
            if (selectedTransaction == null) goto start;

            ConsoleKeyInfo info = new ConsoleKeyInfo('z', ConsoleKey.Z, false, false, false);
            newTransaction = DBContext<Transaction>.GetById(selectedTransaction.Id);

            while (info.Key != ConsoleKey.Escape)
            {
                drawParentPage();
                DrawTransactionInformation(28, true);
                info = Console.ReadKey();
            }

            goto start;
        
        }
        public static void DrawListAdminTransactionsPage()
        {
            start:
            overrideSelection = true;
            FuncNoParam drawParentPage = DrawManageTransactionsPage;
            drawParentPage();
            tableCursor = 1;
            int rowCursor = 8;
            ConsoleKeyInfo entry = new ConsoleKeyInfo('z', ConsoleKey.Z, false, false, false);

            AdministrativeUser selectedAdmin;
            List<ApplicationUser> users = DBContext<ApplicationUser>.GetByCustomQuery(x => x.UserType == ApplicationUserTypeEnum.ADMIN);
            selectedAdmin = (AdministrativeUser)(object)DrawTableSelection<ApplicationUser>("Select Admin User", users, drawParentPage);
            if (selectedAdmin == null) return;

            TransactionsViewModel selectedTransaction;
            List<TransactionsViewModel> data = DBContext<Transaction>.GetByCustomQuery(x => x.CreatedBy == selectedAdmin.Uid)
                .Select(x => new TransactionsViewModel
                {
                    TotalAmountDue = x.TotalAmountDue,
                    CreatedOn = x.CreatedOn,
                    Customer = DBContext<ApplicationUser>.GetById(x.CustomerId.Value).CompleteName,
                    TransactionId = x.TransactionId,
                    Id = x.Uid,
                    LastModifiedOn = x.LastModifiedOn,
                    Status = x.TransactionStatus.ToString()
                }).OrderByDescending(x => x.Id).ToList();

            selectedTransaction = DrawTableSelection<TransactionsViewModel>($"Admin Transactions ({selectedAdmin.CompleteName})", data, drawParentPage);
            if (selectedTransaction == null) goto start;

            ConsoleKeyInfo info = new ConsoleKeyInfo('z', ConsoleKey.Z, false, false, false);
            newTransaction = DBContext<Transaction>.GetById(selectedTransaction.Id);

            while (info.Key != ConsoleKey.Escape)
            {
                drawParentPage();
                DrawTransactionInformation(28, true);
                info = Console.ReadKey();
            }

            goto start;
        }
        public static void DrawUpsertTransactionMenuPage()
        {
            start:

            CheckState();

            if (isEditTransaction)
            {
                if(newTransaction == null)
                {
                    TransactionsViewModel selectedTransaction;
                    List<TransactionsViewModel> data = DBContext<Transaction>.GetAllExisting()
                        .Select(x => new TransactionsViewModel
                        {
                            TotalAmountDue = x.TotalAmountDue,
                            CreatedOn = x.CreatedOn,
                            Customer = DBContext<ApplicationUser>.GetById(x.CustomerId.Value).CompleteName,
                            TransactionId = x.TransactionId,
                            Id = x.Uid,
                            LastModifiedOn = x.LastModifiedOn,
                            Status = x.TransactionStatus.ToString()
                        }).OrderByDescending(x => x.Id).ToList();

                    selectedTransaction = DrawTableSelection<TransactionsViewModel>("Select Transaction for Edit", data, DrawManageTransactionsPage);
                    if (selectedTransaction == null)
                    {
                        navigateToPreviousPage = true;
                        return;
                    }

                    newTransaction = DBContext<Transaction>.GetById(selectedTransaction.Id);
                    forceRedirect(ApplicationStateFlagsEnum.MENU_CREATE_TRANSACTION, false);
                    return;
                }
                DrawPage("Edit Transaction");

            }
            else
            {
                DrawPage("Create Transaction");
            }

            DrawBottomPanel();

            if (newTransaction == null) newTransaction = new Transaction();

            SelectionViewModel[] selectionVM = new SelectionViewModel[]
            {
                new SelectionViewModel
                {
                    Label = "[Set Customer]",
                    IntValue = (int)ApplicationStateFlagsEnum.TRANSACTION_UPSERT_SETCUSTOMER,
                    Description = "Lists all products stocks in a table"
                },
                new SelectionViewModel
                {
                    Label = "[Set Items]",
                    IntValue = (int)ApplicationStateFlagsEnum.TRANSACTION_UPSERT_ADDITEMS,
                    Description = "Lists all products stocks in a table"
                },
                 new SelectionViewModel
                {
                    Label = "[Set Additional Fee]",
                    IntValue = (int)ApplicationStateFlagsEnum.TRANSACTION_UPSERT_SETAFEE,
                    Description =  "Adds Stock to the specified product"
                },
                 new SelectionViewModel
                {
                    Label = "[Set Remarks]",
                    IntValue = (int)ApplicationStateFlagsEnum.TRANSACTION_UPSERT_SETREMARKS,
                    Description =  "Adds Stock to the specified product"
                },
                  new SelectionViewModel
                {
                    Label = "[Save Transaction]",
                    IntValue = (int)ApplicationStateFlagsEnum.TRANSACTION_UPSERT_SAVE,
                    Description =  "Adds Stock to the specified product"
                },
                  new SelectionViewModel
                {
                    Label = "[Back]",
                    IntValue = (int)ApplicationStateFlagsEnum.BACK,
                    Description =  "Redirects to the Main Menu"
                }
            };

            cursorSelectionLimit = selectionVM.Count();

            if (cursorSelection >= cursorSelectionLimit)
            {
                cursorSelection = 0;
                cursorSelectionValue = -1;
            }
            var _selection = selectionVM[cursorSelection];
            cursorSelectionValue = selectionVM[cursorSelection].IntValue;
            if (!overrideSelection) persistedCursorSelection = cursorSelection;

            DrawSelectionDescription(_selection);
            int maxSelectionItemWidth = selectionVM.Max(x => x.Label.Length + 3);
            if (showNewTransactionInfo) DrawTransactionInformation(maxSelectionItemWidth, true);
            CustomConsole.WriteCursorSelection(selectionVM, 5, MAX_HEIGHT, 1, MAX_WIDTH, flag: CustomConsole.ConsoleFormatFlags.TOP_LEFT, selectedValueIndex: (overrideSelection) ? persistedCursorSelection : cursorSelection);


        }
        public static void DrawTransactionInformation(int maxSelectionItemWidth, bool printTransactionInfo)
        {
            tableCursor = 1;
            ConsoleKeyInfo entry = new ConsoleKeyInfo('z', ConsoleKey.Z, false, false, false);
            List<TransactionItemViewModel> data = null;

            var productStocks = newTransaction.TransactionItems.Where(x => x.Quantity > 0);
            data = productStocks.Select(x => new TransactionItemViewModel
            {
                Batch = x.Batch,
                ExpiryDate = x.ExpDate,
                MfgDate = x.MfgDate,
                Quantity = x.Quantity,
                ProductName = DBContext<ProductProfile>.GetById(x.ProductId).ProductIdentifier,
                Price = x.Price,
                SubTotal = x.SubTotalPrice
            }).OrderBy(x => x.ProductName).ToList();

            int startX = (printTransactionInfo) ? (MAX_WIDTH / 3) : (MAX_WIDTH / 5);
            DrawTable<TransactionItemViewModel>(data, tableCursor, startX - 1, MAX_WIDTH - 1, 4, (MAX_HEIGHT + 1) - 3, "Transaction Items");

            ApplicationUser appuser = null;

            if (printTransactionInfo)
            {
                ClearArea(startX, 5, (MAX_WIDTH / 3) + 12, 5);
                DrawBox(maxSelectionItemWidth + 4, (MAX_WIDTH / 3) - 2, 4, (MAX_HEIGHT + 1) - 3);
                CustomConsole.Write("Transaction Info", maxSelectionItemWidth + 5, 5, startX - 2, null, flag: CustomConsole.ConsoleFormatFlags.CENTER);
                DrawClosedPipe(6, maxSelectionItemWidth + 4, startX - 2);


                if (newTransaction.CustomerId.HasValue)
                {
                    appuser = DBContext<ApplicationUser>.GetById(newTransaction.CustomerId.Value);
                }

                CustomConsole.Write($"Customer: {((appuser == null) ? "[Please Set Customer]" : appuser.CompleteName)}", maxSelectionItemWidth + 5, 7, (MAX_WIDTH / 3) - 2, null);
                CustomConsole.Write($"Status: {newTransaction.TransactionStatus.ToString()}", maxSelectionItemWidth + 5, 9, (MAX_WIDTH / 3) - 2, null);
                CustomConsole.Write($"Additional Fees: P{newTransaction.AdditionalFees.ToString("N2")}", maxSelectionItemWidth + 5, 11, (MAX_WIDTH / 3) - 2, null);
                CustomConsole.Write($"Total Amount Due: P{newTransaction.TotalAmountDue.ToString("N2")}", maxSelectionItemWidth + 5, 13, (MAX_WIDTH / 3) - 2, null);
                CustomConsole.Write($"Remarks:", maxSelectionItemWidth + 5, 15, (MAX_WIDTH / 3) - 2, null);
                CustomConsole.Write($"{newTransaction.Remarks}", maxSelectionItemWidth + 5, 16, (MAX_WIDTH / 3) - 2, (MAX_HEIGHT + 1) - 3, flag: CustomConsole.ConsoleFormatFlags.TOP_LEFT);

                var createdBy = DBContext<ApplicationUser>.GetById(newTransaction.CreatedBy);
                string _createdBy = (createdBy == null) ? string.Empty : createdBy.CompleteName;

                var resolvedBy = DBContext<ApplicationUser>.GetById(newTransaction.LastModifiedBy ?? -1);
                string _resolvedBy = (resolvedBy == null) ? string.Empty : resolvedBy.CompleteName;

                CustomConsole.Write($"Created On: {newTransaction.CreatedOn.ToString("MM/dd/yyyy HH:mm")}", maxSelectionItemWidth + 5, (MAX_HEIGHT+1)-9, (MAX_WIDTH/3)-2, (MAX_HEIGHT+1)-4, flag:CustomConsole.ConsoleFormatFlags.TOP_LEFT);
                CustomConsole.Write($"Created By: {newTransaction.CreatedOn.ToString("MM/dd/yyyy HH:mm")}", maxSelectionItemWidth + 5, (MAX_HEIGHT + 1) - 8, (MAX_WIDTH / 3) - 2, (MAX_HEIGHT + 1) - 4, flag: CustomConsole.ConsoleFormatFlags.TOP_LEFT);

                CustomConsole.Write($"Resolved On: {newTransaction.LastModifiedOn?.ToString("MM/dd/yyyy HH:mm")}", maxSelectionItemWidth + 5, (MAX_HEIGHT + 1) - 6, (MAX_WIDTH / 3) - 2, (MAX_HEIGHT + 1) - 4, flag: CustomConsole.ConsoleFormatFlags.TOP_LEFT);
                CustomConsole.Write($"Resolved By: {_resolvedBy}", maxSelectionItemWidth + 5, (MAX_HEIGHT + 1) - 5, (MAX_WIDTH / 3) - 2, (MAX_HEIGHT + 1) - 4, flag: CustomConsole.ConsoleFormatFlags.TOP_LEFT);


            }
            else
            {
                ClearArea(startX, 5, (MAX_WIDTH / 3) + 12, 5);

            }

        }
        public static void DrawUpsertTransaction_SetCustomer()
        {
            DrawUpsertTransactionMenuPage();
            Customer selectedCustomer = (Customer)DrawTableSelection<ApplicationUser>("Select Customer", DBContext<ApplicationUser>.GetByCustomQuery(x => x.UserType == ApplicationUserTypeEnum.CUSTOMER), DrawUpsertTransactionMenuPage);

            if (selectedCustomer == null) return;
            newTransaction.CustomerId = selectedCustomer.Uid;

            navigateToPreviousPage = true;
        }
        public static void DrawUpsertTransaction_SetAdditionalFees()
        {
            overrideSelection = true;

            string error = "Error: ";
            int exceptionLength = error.Length;

            create:

            int panelX1 = MAX_WIDTH / 5;
            int panelY2 = (MAX_HEIGHT - 2);

            DrawUpsertTransactionMenuPage();

            DrawBox(panelX1 - 1, MAX_WIDTH - 1, 4, panelY2);
            DrawClosedPipe(6, panelX1 - 1, MAX_WIDTH - 1);

            if (error.Length != exceptionLength)
            {
                ClearArea(panelX1, panelY2 - 1, MAX_WIDTH - 1, panelY2 - 1);
                DrawClosedPipe(panelY2 - 2, panelX1 - 1, MAX_WIDTH - 1);
                CustomConsole.Write(error, panelX1, panelY2 - 1, MAX_WIDTH - 1, null, flag: CustomConsole.ConsoleFormatFlags.CENTER);
                error = "Error: ";
            }

            string successMessage = string.Empty;
            string confirmationMessage = string.Empty;


            CustomConsole.Write("SET TRANSACTION ADDITIONAL FEES", panelX1, 5, MAX_WIDTH - 1, null, flag: CustomConsole.ConsoleFormatFlags.CENTER);

            string additionalFee = string.Empty;
            CustomConsole.Write("Additional Fee: ", panelX1, 7, MAX_WIDTH - 1, null, flag: CustomConsole.ConsoleFormatFlags.TOP_LEFT);
            additionalFee = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(additionalFee)) error += "Please Enter A Value!";

            decimal _additionalFee;
            if (!decimal.TryParse(additionalFee, out _additionalFee)) error += "Please Enter A Valid Amoun!";

            successMessage = "Successfully Set Additional Fees for this Transaction!";
            confirmationMessage = "Press any key to continue!";


            if (error.Length != exceptionLength) goto create;

            ClearArea(panelX1, panelY2 - 1, MAX_WIDTH - 1, panelY2 - 1);
            DrawClosedPipe(panelY2 - 2, panelX1 - 1, MAX_WIDTH - 1);
            CustomConsole.Write(successMessage, panelX1, panelY2 - 1, MAX_WIDTH - 1, null, flag: CustomConsole.ConsoleFormatFlags.CENTER);
            CustomConsole.Write(confirmationMessage, panelX1, 12, MAX_WIDTH - 1, null, flag: CustomConsole.ConsoleFormatFlags.TOP_LEFT);
            ConsoleKeyInfo input = Console.ReadKey();

            newTransaction.AdditionalFees = _additionalFee;

            navigateToPreviousPage = true;
        }
        public static void DrawUpsertTransaction_SetRemarks()
        {
            overrideSelection = true;

            string error = "Error: ";
            int exceptionLength = error.Length;

            create:

            int panelX1 = MAX_WIDTH / 5;
            int panelY2 = (MAX_HEIGHT - 2);

            DrawUpsertTransactionMenuPage();

            DrawBox(panelX1 - 1, MAX_WIDTH - 1, 4, panelY2);
            DrawClosedPipe(6, panelX1 - 1, MAX_WIDTH - 1);

            if (error.Length != exceptionLength)
            {
                ClearArea(panelX1, panelY2 - 1, MAX_WIDTH - 1, panelY2 - 1);
                DrawClosedPipe(panelY2 - 2, panelX1 - 1, MAX_WIDTH - 1);
                CustomConsole.Write(error, panelX1, panelY2 - 1, MAX_WIDTH - 1, null, flag: CustomConsole.ConsoleFormatFlags.CENTER);
                error = "Error: ";
            }

            string successMessage = string.Empty;
            string confirmationMessage = string.Empty;


            CustomConsole.Write("SET TRANSACTION REMARKS", panelX1, 5, MAX_WIDTH - 1, null, flag: CustomConsole.ConsoleFormatFlags.CENTER);

            string remarks = string.Empty;
            CustomConsole.Write("Remarks: ", panelX1, 7, MAX_WIDTH - 1, null, flag: CustomConsole.ConsoleFormatFlags.TOP_LEFT);
            remarks = Console.ReadLine();

            successMessage = "Successfully Set Remarks for this Transaction!";
            confirmationMessage = "Press any key to continue!";


            if (error.Length != exceptionLength) goto create;

            ClearArea(panelX1, panelY2 - 1, MAX_WIDTH - 1, panelY2 - 1);
            DrawClosedPipe(panelY2 - 2, panelX1 - 1, MAX_WIDTH - 1);
            CustomConsole.Write(successMessage, panelX1, panelY2 - 1, MAX_WIDTH - 1, null, flag: CustomConsole.ConsoleFormatFlags.CENTER);
            CustomConsole.Write(confirmationMessage, panelX1, 12, MAX_WIDTH - 1, null, flag: CustomConsole.ConsoleFormatFlags.TOP_LEFT);
            ConsoleKeyInfo input = Console.ReadKey();

            newTransaction.Remarks = remarks;

            navigateToPreviousPage = true;
        }
        public static void DrawUpsertTransaction_SetStatus()
        {
            overrideSelection = true;

            string error = "Error: ";
            int exceptionLength = error.Length;

            create:

            int panelX1 = MAX_WIDTH / 5;
            int panelY2 = (MAX_HEIGHT - 2);

            DrawUpsertTransactionMenuPage();

            DrawBox(panelX1 - 1, MAX_WIDTH - 1, 4, panelY2);
            DrawClosedPipe(6, panelX1 - 1, MAX_WIDTH - 1);

            if (error.Length != exceptionLength)
            {
                ClearArea(panelX1, panelY2 - 1, MAX_WIDTH - 1, panelY2 - 1);
                DrawClosedPipe(panelY2 - 2, panelX1 - 1, MAX_WIDTH - 1);
                CustomConsole.Write(error, panelX1, panelY2 - 1, MAX_WIDTH - 1, null, flag: CustomConsole.ConsoleFormatFlags.CENTER);
                error = "Error: ";
            }

            string successMessage = string.Empty;
            string confirmationMessage = string.Empty;


            CustomConsole.Write("SET TRANSACTION STATUS", panelX1, 5, MAX_WIDTH - 1, null, flag: CustomConsole.ConsoleFormatFlags.CENTER);

            string choice = string.Empty;
            TransactionStatusEnum transactionStatusEnum = TransactionStatusEnum.PENDING;
            CustomConsole.Write("1] PENDING ", panelX1, 7, MAX_WIDTH - 1, null, flag: CustomConsole.ConsoleFormatFlags.TOP_LEFT);
            CustomConsole.Write("2] PAID", panelX1, 8, MAX_WIDTH - 1, null, flag: CustomConsole.ConsoleFormatFlags.TOP_LEFT);
            CustomConsole.Write("Please Enter Selection: ", panelX1, 11, MAX_WIDTH - 1, null, flag: CustomConsole.ConsoleFormatFlags.TOP_LEFT);

            choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    transactionStatusEnum = TransactionStatusEnum.PENDING;
                    break;
                case "2":
                    transactionStatusEnum = TransactionStatusEnum.PAID;
                    break;
                default:
                    error += "Invalid Selection!";
                    break;
            }

            successMessage = "Successfully Set Status for this Transaction!";
            confirmationMessage = "Press any key to continue!";


            if (error.Length != exceptionLength) goto create;

            ClearArea(panelX1, panelY2 - 1, MAX_WIDTH - 1, panelY2 - 1);
            DrawClosedPipe(panelY2 - 2, panelX1 - 1, MAX_WIDTH - 1);
            CustomConsole.Write(successMessage, panelX1, panelY2 - 1, MAX_WIDTH - 1, null, flag: CustomConsole.ConsoleFormatFlags.CENTER);
            CustomConsole.Write(confirmationMessage, panelX1, 12, MAX_WIDTH - 1, null, flag: CustomConsole.ConsoleFormatFlags.TOP_LEFT);
            ConsoleKeyInfo input = Console.ReadKey();

            newTransaction.TransactionStatus = transactionStatusEnum;

            navigateToPreviousPage = true;
        }
        public static void DrawUpsertTransaction_SetItemsSelection()
        {
            start:

            CheckState();

            DrawPage("Set Transaction Items");
            DrawBottomPanel();

            if (newTransaction == null) newTransaction = new Transaction();

            SelectionViewModel[] selectionVM = new SelectionViewModel[]
            {
                new SelectionViewModel
                {
                    Label = "[Add Item]",
                    IntValue = (int)ApplicationStateFlagsEnum.TRANSACTION_MENU_SETITEMS_ADD,
                    Description = "Adds item to transaction"
                },
                new SelectionViewModel
                {
                    Label = "[Remove Item]",
                    IntValue = (int)ApplicationStateFlagsEnum.TRANSACTION_MENU_SETITEMS_REMOVE,
                    Description = "Removes item from transaction"
                },
                  new SelectionViewModel
                {
                    Label = "[Back]",
                    IntValue = (int)ApplicationStateFlagsEnum.BACK,
                    Description =  "Redirects to the Main Menu"
                }
            };

            cursorSelectionLimit = selectionVM.Count();

            if (cursorSelection >= cursorSelectionLimit)
            {
                cursorSelection = 0;
                cursorSelectionValue = -1;
            }
            var _selection = selectionVM[cursorSelection];
            cursorSelectionValue = selectionVM[cursorSelection].IntValue;
            if (!overrideSelection) persistedCursorSelection = cursorSelection;

            DrawSelectionDescription(_selection);
            int maxSelectionItemWidth = selectionVM.Max(x => x.Label.Length + 3);
            if (showNewTransactionItems)
            {
                DrawTransactionInformation(maxSelectionItemWidth, false);
            }
            CustomConsole.WriteCursorSelection(selectionVM, 5, MAX_HEIGHT, 1, MAX_WIDTH, flag: CustomConsole.ConsoleFormatFlags.TOP_LEFT, selectedValueIndex: (overrideSelection) ? persistedCursorSelection : cursorSelection);

        }
        public static void DrawUpsertTransaction_SetItemsSelection_AddItems()
        {
            selectProductProfile:
            List<ProductProfile> data = DBContext<ProductProfile>.GetAllExisting();
            int quantity = 0;


            ProductProfile selectedData = DrawTableSelection<ProductProfile>("Add Transaction Items", data, DrawUpsertTransaction_SetItemsSelection);

            if (selectedData == null) return;
            ProductProfile selectedObject = ((ProductProfile)selectedData).Clone<ProductProfile>();

            List<ListInStocksViewModel> data2 = null;

            var productStocks = selectedData.ProductStocks.Where(x => x.Quantity > 0);
            data2 = productStocks.Select(x => new ListInStocksViewModel
            {
                Batch = x.Batch,
                ExpiryDate = x.ExpDate,
                MfgDate = x.MfgDate,
                Quantity = x.Quantity,
                ProductName = DBContext<ProductProfile>.GetById(x.ProductId).ProductIdentifier,
                ReceivedOn = x.ReceivedOn
            }).ToList();


            var selectedStock = DrawTableSelection<ListInStocksViewModel>("Add Transaction Items", data2, DrawUpsertTransaction_SetItemsSelection);
            if (selectedStock == null) goto selectProductProfile;

            overrideSelection = true;
            string error = "Error: ";
            int exceptionLength = error.Length;

            edit:

            int panelX1 = MAX_WIDTH / 5;
            int panelY2 = (MAX_HEIGHT - 2);
            DrawUpsertTransaction_SetItemsSelection();
            DrawBox(panelX1 - 1, MAX_WIDTH - 1, 4, panelY2);
            DrawClosedPipe(6, panelX1 - 1, MAX_WIDTH - 1);

            if (error.Length != exceptionLength)
            {
                ClearArea(panelX1, panelY2 - 1, MAX_WIDTH - 1, panelY2 - 1);
                DrawClosedPipe(panelY2 - 2, panelX1 - 1, MAX_WIDTH - 1);
                CustomConsole.Write(error, panelX1, panelY2 - 1, MAX_WIDTH - 1, null, flag: CustomConsole.ConsoleFormatFlags.CENTER);
                error = "Error: ";
            }

            InputProductStockViewModel viewModel = new InputProductStockViewModel();
            string successMessage = "Successfully Added Transaction Item!";

            CustomConsole.Write("Add Transaction Item", panelX1, 5, MAX_WIDTH - 1, null, flag: CustomConsole.ConsoleFormatFlags.CENTER);

            CustomConsole.Write($"Product: ({selectedObject.SKU}) {selectedObject.ProductName}", panelX1, 7, MAX_WIDTH - 1, null, flag: CustomConsole.ConsoleFormatFlags.TOP_LEFT);
            CustomConsole.Write($"Stock Batch No: {selectedStock.Batch}", panelX1, 8, MAX_WIDTH - 1, null, flag: CustomConsole.ConsoleFormatFlags.TOP_LEFT);
            CustomConsole.Write($"Manufacturing Date: {selectedStock.MfgDate.ToString("MMMM dd, yyyy")}", panelX1, 9, MAX_WIDTH - 1, null, flag: CustomConsole.ConsoleFormatFlags.TOP_LEFT);
            CustomConsole.Write($"Expiry Date: {selectedStock.ExpiryDate.ToString("MMMM dd, yyyy")}", panelX1, 10, MAX_WIDTH - 1, null, flag: CustomConsole.ConsoleFormatFlags.TOP_LEFT);

            CustomConsole.Write($"Enter Quantity: ", panelX1, 11, MAX_WIDTH - 1, null, flag: CustomConsole.ConsoleFormatFlags.TOP_LEFT);
            viewModel.Quantity = Console.ReadLine();


            if (string.IsNullOrWhiteSpace(viewModel.Quantity))
            {
                error += "Quantity";
                goto validateData;
            }


            if (!int.TryParse(viewModel.Quantity, out quantity))
            {
                error += "Please Input A Valid Quantity Value! (Not A Valid Number)";
                goto validateData;
            }

            if (quantity > selectedStock.Quantity)
            {
                error += "Insufficient Stock in Inventory! Please Enter a smaller quantity!";
            }

            validateData:
            if (error.Length != exceptionLength) goto edit;

            var transactionItem = newTransaction.TransactionItems.Where(x => x.ProductId == selectedData.Uid && x.Batch == selectedStock.Batch).FirstOrDefault();

            newTransaction.AddTransactionItem(selectedData.Uid, selectedStock.Batch, quantity, selectedData.Price, selectedStock.MfgDate, selectedStock.ExpiryDate);
            navigateToPreviousPage = true;

        }
        public static void DrawUpsertTransaction_SetItemsSelection_RemoveItems()
        {
            int quantity = 0;
            string remarks = string.Empty;
            var items = newTransaction.TransactionItems.Select(x =>
            new TransactionItemViewModel
            {
                Batch = x.Batch,
                ExpiryDate = x.ExpDate,
                MfgDate = x.MfgDate,
                Quantity = x.Quantity,
                ProductName = DBContext<ProductProfile>.GetById(x.ProductId).ProductIdentifier,
                ProductId = x.ProductId
            }).ToList();

            var selectedStocks = DrawTableSelection<TransactionItemViewModel>($"Remove Transaction Items", items, DrawUpsertTransaction_SetItemsSelection);
            if (selectedStocks == null)
            {
                navigateToPreviousPage = true;
                return;
            }

            var selectedObject = DBContext<ProductProfile>.GetById(selectedStocks.ProductId);

            overrideSelection = true;
            string error = "Error: ";
            int exceptionLength = error.Length;

            edit:

            int panelX1 = MAX_WIDTH / 5;
            int panelY2 = (MAX_HEIGHT - 2);
            DrawUpsertTransaction_SetItemsSelection();
            DrawBox(panelX1 - 1, MAX_WIDTH - 1, 4, panelY2);
            DrawClosedPipe(6, panelX1 - 1, MAX_WIDTH - 1);

            if (error.Length != exceptionLength)
            {
                ClearArea(panelX1, panelY2 - 1, MAX_WIDTH - 1, panelY2 - 1);
                DrawClosedPipe(panelY2 - 2, panelX1 - 1, MAX_WIDTH - 1);
                CustomConsole.Write(error, panelX1, panelY2 - 1, MAX_WIDTH - 1, null, flag: CustomConsole.ConsoleFormatFlags.CENTER);
                error = "Error: ";
            }


            InputProductStockViewModel viewModel = new InputProductStockViewModel();


            CustomConsole.Write("Remove Stock", panelX1, 5, MAX_WIDTH - 1, null, flag: CustomConsole.ConsoleFormatFlags.CENTER);

            CustomConsole.Write($"Product: ({selectedObject.SKU}) {selectedObject.ProductName}", panelX1, 7, MAX_WIDTH - 1, null, flag: CustomConsole.ConsoleFormatFlags.TOP_LEFT);
            CustomConsole.Write($"Stock Batch No: {selectedStocks.Batch}", panelX1, 8, MAX_WIDTH - 1, null, flag: CustomConsole.ConsoleFormatFlags.TOP_LEFT);
            CustomConsole.Write($"Manufacturing Date: {selectedStocks.MfgDate.ToString("MMMM dd, yyyy")}", panelX1, 9, MAX_WIDTH - 1, null, flag: CustomConsole.ConsoleFormatFlags.TOP_LEFT);
            CustomConsole.Write($"Expiry Date: {selectedStocks.ExpiryDate.ToString("MMMM dd, yyyy")}", panelX1, 10, MAX_WIDTH - 1, null, flag: CustomConsole.ConsoleFormatFlags.TOP_LEFT);

            CustomConsole.Write($"Enter Quantity to Remove: ", panelX1, 11, MAX_WIDTH - 1, null, flag: CustomConsole.ConsoleFormatFlags.TOP_LEFT);
            
            var qty = Console.ReadLine();


            if (string.IsNullOrWhiteSpace(qty))
            {
                error += "Quantity";
                goto validateData;
            }

            if (isEditTransaction)
            {
                CustomConsole.Write($"Enter Reason of Refund: ", panelX1, 12, MAX_WIDTH - 1, null, flag: CustomConsole.ConsoleFormatFlags.TOP_LEFT);
                remarks = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(remarks))
                {
                    error += "Please Enter Reason for Refund of Item!";
                    goto validateData;
                }
            }

            if (!int.TryParse(qty, out quantity))
            {
                error += "Please Input A Valid Quantity Value! (Not A Valid Number)";
                goto validateData;
            }

            if (quantity > selectedObject.QtyInStock)
            {
                error += "Quantity Entered Exceeds amount in transaction! Please try a smaller value!";
                goto validateData;
            }


            validateData:
            if (error.Length != exceptionLength) goto edit;

            if (isEditTransaction)
            {
                newTransaction.RefundTransactionItem(selectedObject.Uid, selectedStocks.Batch, quantity, remarks);
            }
            else
            {
                newTransaction.RemoveTransactionItem(selectedObject.Uid, selectedStocks.Batch, quantity);
            }


            navigateToPreviousPage = true;
        }
        public static void DrawUpsertTransaction_SaveTransaction()
        {
            DrawUpsertTransactionMenuPage();

            int panelX1 = 28;
            int panelY2 = (MAX_HEIGHT - 2);
            try
            {
                if (newTransaction.CustomerId == null)
                {
                    throw new Exception("Please Set Customer!");
                }

                if (newTransaction.TransactionItems.Count == 0)
                {
                    throw new Exception("Please Add Items to the Transaction!");
                }


                ClearArea(panelX1, panelY2 - 1, (MAX_WIDTH / 3) - 3, panelY2 - 1);
                DrawClosedPipe(panelY2 - 3, panelX1 - 1, (MAX_WIDTH / 3) - 2);

                if (isEditTransaction)
                {
                    var oldTransaction = DBContext<Transaction>.GetById(newTransaction.Uid);
                    foreach(var item in newTransaction.RefundedItems)
                    {
                        var product = DBContext<ProductProfile>.GetById(item.ProductId);
                        product.ReturnStock(item.Quantity, item.Batch);
                        DBContext<ProductProfile>.Update(product);
                    }
                    DBContext<Transaction>.Add(newTransaction);
                    CustomConsole.Write("Successfully Edited Transaction!", panelX1, panelY2 - 2, (MAX_WIDTH / 3), null, flag: CustomConsole.ConsoleFormatFlags.LEFT);
                    CustomConsole.Write("Press any key to reset the form!", panelX1, panelY2 - 1, (MAX_WIDTH / 3), null, flag: CustomConsole.ConsoleFormatFlags.LEFT);
                    newTransaction = new Transaction();

                    popNavigationHistory();
                    popNavigationHistory();
                    popNavigationHistory();
                }
                else
                {
                    foreach (var item in newTransaction.TransactionItems)
                    {
                        var product = DBContext<ProductProfile>.GetById(item.ProductId);
                        product.RemoveStock(item.Quantity, item.Batch);
                        DBContext<ProductProfile>.Update(product);
                    }

                    DBContext<Transaction>.Add(newTransaction);
                    CustomConsole.Write("Successfully Created Transaction!", panelX1, panelY2 - 2, (MAX_WIDTH / 3), null, flag: CustomConsole.ConsoleFormatFlags.LEFT);
                    CustomConsole.Write("Press any key to reset the form!", panelX1, panelY2 - 1, (MAX_WIDTH / 3), null, flag: CustomConsole.ConsoleFormatFlags.LEFT);
                    newTransaction = new Transaction();
                }
                ConsoleKeyInfo input = Console.ReadKey();
                navigateToPreviousPage = true;
                isEditTransaction = false;
                return;
            }
            catch(Exception ex)
            {
                ClearArea(panelX1, panelY2 - 1, (MAX_WIDTH / 3) - 3, panelY2 - 1);
                DrawClosedPipe(panelY2 - 3, panelX1 - 1, (MAX_WIDTH / 3) - 2);
                CustomConsole.Write(ex.Message, panelX1, panelY2 - 2, (MAX_WIDTH / 3), panelY2, flag: CustomConsole.ConsoleFormatFlags.TOP_LEFT);
                ConsoleKeyInfo input = Console.ReadKey();
                navigateToPreviousPage = true;
            }
        }

        private static void DrawSelectionDescription(SelectionViewModel selection, int x1 = 0)
        {
            x1 = (x1 == 0) ? (MAX_WIDTH / 5) : x1;
            if (showSelectionDescription)
            {
                int height = CustomConsole.Write(selection.Description, x1, 5, MAX_WIDTH, MAX_HEIGHT, flag: CustomConsole.ConsoleFormatFlags.TOP_LEFT);
                DrawBox(x1 - 1, MAX_WIDTH - 2, 4, 4 + (height + 1));
            }
        }

        // Boilerplate Methods (Template Displays)
        public const string SystemName = "ZAMMY'S INVENTORY SYSTEM";
        public static void DrawBox(int x1, int x2, int y1, int y2)
        {
            CheckState();
            Console.SetCursorPosition(x1, y1);
            Console.Write('╔');

            Console.SetCursorPosition(x2, y1);
            Console.Write('╗');

            Console.SetCursorPosition(x2, y2);
            Console.Write("╝");

            Console.SetCursorPosition(x1, y2);
            Console.WriteLine("╚");


            DrawPipe(y1, x1 + 1, x2);
            DrawPipe(y2, x1 + 1, x2);


            DrawPillar(x1, y1 + 1, y2);
            DrawPillar(x2, y1 + 1, y2);


        }
        public static void DrawPillar(int x, int y1, int y2)
        {
            for (int i = y1; i < y2; i++)
            {
                Console.SetCursorPosition(x, i);
                Console.Write("║");
            }
        }
        public static void DrawPipe(int y, int x1, int x2, char material = '═')
        {
            for (int i = x1; i < x2; i++)
            {
                Console.SetCursorPosition(i, y);
                Console.Write('═');
            }
        }
        public static void DrawClosedPipe(int y, int x1, int x2)
        {
            DrawPipe(y, x1, x2);

            Console.SetCursorPosition(x1, y);
            Console.Write("╠");

            Console.SetCursorPosition(x2, y);
            Console.Write("╣");
        }
        public static void DrawExit()
        {
            Console.Clear();
            Console.WriteLine(FiggleFonts.Standard.Render("thank you for using!"));
            Console.WriteLine(FiggleFonts.Standard.Render("made by:"));
            Console.WriteLine(FiggleFonts.Standard.Render("michael zamoras"));
        }
        public static void DrawTable<T>(List<T> rows, int page, int x1, int x2, int y1, int y2, string title)
        {
            DrawBox(x1, x2, y1, y2);
            DrawClosedPipe(y1 + 2, x1, x2);
            CustomConsole.Write(title, x1, y1 + 1, x2, null, flag: CustomConsole.ConsoleFormatFlags.CENTER);
            CustomConsole.Write("(ESC to Quit)", x1 + 1, y1 + 1, x2, null, flag: CustomConsole.ConsoleFormatFlags.LEFT);



            int rowCount = y2 - (y1 + 4);
            int pageCount = (int)Math.Ceiling((decimal)rows.Count() / rowCount);

            CustomConsole.Write($"(Page {page} of {pageCount})", x1, y1 + 1, x2, null, flag: CustomConsole.ConsoleFormatFlags.RIGHT, xOffset: 1);

            var display = rows.Skip(rowCount * (page - 1)).Take(rowCount).ToList();

            var columns = typeof(T).GetProperties()
                .Where(x => Attribute.IsDefined(x, typeof(ConsoleTableColumn)))
                .Select(x => new
                {
                    Name = x.GetCustomAttribute<ConsoleTableColumn>()?.displayName,
                    Order = x.GetCustomAttribute<ConsoleTableColumn>()?.order,
                    _PropertyInfo = x
                })
                .OrderBy(x => x.Order);

            int x = columns.Count();


            int colWidth = (x2 - x1) / columns.Count();

            for (int j = 0; j < columns.Count(); j++)
            {

                int colX1 = x1 + (j * colWidth);
                int colX2 = x1 + ((j + 1) * colWidth);

                if (j == 0)
                {
                    colX1 = x1 + 5;
                    colX2 = x1 + 7;
                }
                else if (j == 1)
                {
                    colX1 = x1 + 9;
                    colX2 = x1 + ((j + 1) * colWidth) + 7;
                }

                var column = columns.ElementAt(j);
                CustomConsole.Write(column.Name, colX1, y1 + 3, colX2, null, flag: CustomConsole.ConsoleFormatFlags.CENTER);
            }

            for (int i = 0; i < display.Count(); i++)
            {
                for (int j = 0; j < columns.Count(); j++)
                {
                    int colX1 = x1 + (j * colWidth);
                    int colX2 = x1 + ((j + 1) * colWidth);

                    if (j == 0)
                    {
                        colX1 = x1 + 5;
                        colX2 = x1 + 7;
                    }
                    else if (j == 1)
                    {
                        colX1 = x1 + 9;
                        colX2 = x1 + ((j + 1) * colWidth) + 7;
                    }

                    var column = columns.ElementAt(j)._PropertyInfo;
                    var item = column.GetValue(display[i]);

                    string text;

                    if(item == null)
                    {
                        text = string.Empty;
                    }else if (item.GetType() == typeof(decimal))
                    {
                        text = "P " + ((decimal)item).ToString("N2");
                    }
                    else if (item.GetType() == typeof(DateTime))
                    {
                        text = ((DateTime)item).ToString("MM/dd/yyyy HH:mm");
                    }
                    else
                    {
                        text = item.ToString();
                    }


                    CustomConsole.Write(text, colX1, (y1 + 4) + i, colX2, null, flag: CustomConsole.ConsoleFormatFlags.CENTER);

                }

            }

            if (display.Count == 0)
            {
                CustomConsole.Write("No Data Found", x1, (y1 + 4), null, null, flag: CustomConsole.ConsoleFormatFlags.CENTER);
            }

        }
        public static T DrawTableSelection<T>(string tableTitle, List<T> data, FuncNoParam drawParentContainer)
        {

            overrideSelection = true;
            tableCursor = 1;
            int rowCursor = 8;
            ConsoleKeyInfo entry = new ConsoleKeyInfo('z', ConsoleKey.Z, false, false, false);
            T selectedObject;
            selectedObject = (data.Count == 0) ? default(T) : data[0];
            while (entry.Key != ConsoleKey.Escape && entry.Key != ConsoleKey.Enter)
            {
                overrideSelection = true;

                int rowCount = ((MAX_HEIGHT + 1) - 3) - 8;
                int pageCount = (int)Math.Ceiling((decimal)data.Count() / rowCount);
                pageCount += (pageCount == 0) ? 1 : 0;

                int rowIndex = (rowCursor - 8) + (rowCount * (pageCount - 1));

                if (rowIndex >= data.Count)
                {
                    rowCursor = 8;
                    tableCursor = 1;
                    rowIndex = 0;
                }
                else if (rowIndex < 0)
                {
                    rowCursor = (MAX_HEIGHT + 1) - 4;
                    tableCursor = pageCount;
                    rowIndex = (rowCursor - 8) + (rowCount * (pageCount - 1));
                    if (rowIndex >= data.Count)
                    {
                        rowIndex = data.Count - 1;
                        rowCursor = 7 + (data.Count % rowCount);
                    }
                }

                if (data.Count == 0)
                {
                    navigateToPreviousPage = true;
                    selectedObject = default(T);
                }
                else
                {
                    selectedObject = data[rowIndex];
                }

                if (rowCursor == 7)
                {
                    rowCursor = (MAX_HEIGHT + 1) - 4;
                    tableCursor--;
                }
                else if (rowCursor > (MAX_HEIGHT + 1) - 4)
                {
                    tableCursor++;
                    rowCursor = 8;
                }

                if (tableCursor == 0) tableCursor = pageCount;
                else if (tableCursor > pageCount) tableCursor = 1;

                drawParentContainer();
                DrawTable<T>(data, tableCursor, (MAX_WIDTH / 5) - 1, MAX_WIDTH - 1, 4, (MAX_HEIGHT + 1) - 3, tableTitle);
                CustomConsole.Write("=>", MAX_WIDTH / 5, rowCursor, MAX_WIDTH - 1, null, flag: CustomConsole.ConsoleFormatFlags.LEFT);


                entry = Console.ReadKey();

                switch (entry.Key)
                {
                    case ConsoleKey.UpArrow:
                    case ConsoleKey.W:
                        rowCursor--;
                        break;
                    case ConsoleKey.DownArrow:
                    case ConsoleKey.S:
                        rowCursor++;
                        break;
                    default:
                        break;
                }

                if (entry.Key == ConsoleKey.Escape)
                {
                    navigateToPreviousPage = true;
                    return default(T);
                }

                if (entry.Key == ConsoleKey.Enter)
                {
                    return selectedObject;
                }
            }

            return default(T);
        }
        public static void ClearArea(int x1, int y1, int x2, int y2)
        {
            Console.SetCursorPosition(x1, y1);
            for (int i = y1; i <= y2; i++)
            {
                for (int j = x1; j <= x2; j++)
                {
                    Console.Write(" ");
                }
                Console.WriteLine();
            }
        }
        public static void DrawCreatePage<T, U>()
            where T : BaseEntity
            where U : T
        {
            overrideSelection = true;

            reset:
            string error = "Error: ";
            int exceptionLength = error.Length;

            create:

            int panelX1 = MAX_WIDTH / 5;
            int panelY2 = (MAX_HEIGHT - 2);

            if (typeof(U) == typeof(AdministrativeUser)) DrawManageApplicationUserPage();
            else if (typeof(U) == typeof(Customer)) DrawManageCustomersPage();
            else if (typeof(U) == typeof(ProductProfile)) DrawManageProductProfilesPage();

            DrawBox(panelX1 - 1, MAX_WIDTH - 1, 4, panelY2);
            DrawClosedPipe(6, panelX1 - 1, MAX_WIDTH - 1);

            if (error.Length != exceptionLength)
            {
                ClearArea(panelX1, panelY2 - 1, MAX_WIDTH - 1, panelY2 - 1);
                DrawClosedPipe(panelY2 - 2, panelX1 - 1, MAX_WIDTH - 1);
                CustomConsole.Write(error, panelX1, panelY2 - 1, MAX_WIDTH - 1, null, flag: CustomConsole.ConsoleFormatFlags.CENTER);
                error = "Error: ";
            }

            U createdObject = default(U);

            string successMessage = string.Empty;
            string confirmationMessage = string.Empty;

            if (typeof(U) == typeof(AdministrativeUser))
            {
                InputApplicationUserViewModel viewModel = new InputApplicationUserViewModel();


                CustomConsole.Write("CREATE NEW USER", panelX1, 5, MAX_WIDTH - 1, null, flag: CustomConsole.ConsoleFormatFlags.CENTER);

                CustomConsole.Write("Enter First Name: ", panelX1, 7, MAX_WIDTH - 1, null, flag: CustomConsole.ConsoleFormatFlags.TOP_LEFT);
                viewModel.FirstName = Console.ReadLine();

                CustomConsole.Write("Enter Last Name: ", panelX1, 8, MAX_WIDTH - 1, null, flag: CustomConsole.ConsoleFormatFlags.TOP_LEFT);
                viewModel.LastName = Console.ReadLine();

                CustomConsole.Write("Enter Username: ", panelX1, 9, MAX_WIDTH - 1, null, flag: CustomConsole.ConsoleFormatFlags.TOP_LEFT);
                viewModel.Username = Console.ReadLine();

                CustomConsole.Write("Enter Password: ", panelX1, 10, MAX_WIDTH - 1, null, flag: CustomConsole.ConsoleFormatFlags.TOP_LEFT);
                viewModel.Password = Console.ReadLine();

                createdObject = (U)(object)new AdministrativeUser(viewModel.FirstName, viewModel.LastName, viewModel.Username, viewModel.Password);


                if (string.IsNullOrWhiteSpace(viewModel.FirstName)) error += "FirstName,";
                if (string.IsNullOrWhiteSpace(viewModel.LastName)) error += "LastName,";
                if (string.IsNullOrWhiteSpace(viewModel.Username)) error += "Username,";
                if (string.IsNullOrWhiteSpace(viewModel.Password)) error += "Password";

                successMessage = "Successfully Created New Admin User!";
                confirmationMessage = "Would you like to create another user? (Y/N): ";
            }
            else if (typeof(U) == typeof(Customer))
            {
                InputCustomerViewModel viewModel = new InputCustomerViewModel();


                CustomConsole.Write("CREATE NEW CUSTOMER", panelX1, 5, MAX_WIDTH - 1, null, flag: CustomConsole.ConsoleFormatFlags.CENTER);

                CustomConsole.Write("Enter First Name: ", panelX1, 7, MAX_WIDTH - 1, null, flag: CustomConsole.ConsoleFormatFlags.TOP_LEFT);
                viewModel.FirstName = Console.ReadLine();

                CustomConsole.Write("Enter Last Name: ", panelX1, 8, MAX_WIDTH - 1, null, flag: CustomConsole.ConsoleFormatFlags.TOP_LEFT);
                viewModel.LastName = Console.ReadLine();

                CustomConsole.Write("Enter Address: ", panelX1, 9, MAX_WIDTH - 1, null, flag: CustomConsole.ConsoleFormatFlags.TOP_LEFT);
                viewModel.Address = Console.ReadLine();

                createdObject = (U)(object)new Customer(viewModel.FirstName, viewModel.LastName, viewModel.Address);

                if (string.IsNullOrWhiteSpace(viewModel.FirstName)) error += "FirstName,";
                if (string.IsNullOrWhiteSpace(viewModel.LastName)) error += "LastName,";
                if (string.IsNullOrWhiteSpace(viewModel.Address)) error += "Address";


                successMessage = "Successfully Created New Customer!";
                confirmationMessage = "Would you like to create another customer? (Y/N): ";
            }
            else if (typeof(U) == typeof(ProductProfile))
            {
                InputProductProfileViewModel viewModel = new InputProductProfileViewModel();


                CustomConsole.Write("CREATE NEW PRODUCT PROFILE", panelX1, 5, MAX_WIDTH - 1, null, flag: CustomConsole.ConsoleFormatFlags.CENTER);

                CustomConsole.Write("Enter SKU: ", panelX1, 7, MAX_WIDTH - 1, null, flag: CustomConsole.ConsoleFormatFlags.TOP_LEFT);
                viewModel.SKU = Console.ReadLine() ?? string.Empty;

                CustomConsole.Write("Enter Product Name: ", panelX1, 8, MAX_WIDTH - 1, null, flag: CustomConsole.ConsoleFormatFlags.TOP_LEFT);
                viewModel.ProductName = Console.ReadLine() ?? string.Empty;

                CustomConsole.Write("Enter Price: ", panelX1, 9, MAX_WIDTH - 1, null, flag: CustomConsole.ConsoleFormatFlags.TOP_LEFT);
                viewModel.ProductPrice = Console.ReadLine() ?? string.Empty;



                if (string.IsNullOrWhiteSpace(viewModel.ProductName)) error += "Product Name,";
                if (string.IsNullOrWhiteSpace(viewModel.ProductPrice)) error += "Product Price,";
                if (string.IsNullOrWhiteSpace(viewModel.SKU)) error += "SKU";

                decimal price;
                if (!decimal.TryParse(viewModel.ProductPrice, out price)) error += "Invalid Price Value! (Must be a valid amount!)";

                createdObject = (U)(object)new ProductProfile(viewModel.ProductName, price, viewModel.SKU);


                successMessage = "Successfully Created New Product Profile!";
                confirmationMessage = "Would you like to create another product profile? (Y/N): ";
            }


            validateData:
            if (error.Length != exceptionLength) goto create;


            try
            {
                DBContext<T>.Add(createdObject);
                ClearArea(panelX1, panelY2 - 1, MAX_WIDTH - 1, panelY2 - 1);
                DrawClosedPipe(panelY2 - 2, panelX1 - 1, MAX_WIDTH - 1);
                CustomConsole.Write(successMessage, panelX1, panelY2 - 1, MAX_WIDTH - 1, null, flag: CustomConsole.ConsoleFormatFlags.CENTER);
                CustomConsole.Write(confirmationMessage, panelX1, 12, MAX_WIDTH - 1, null, flag: CustomConsole.ConsoleFormatFlags.TOP_LEFT);
                ConsoleKeyInfo input = Console.ReadKey();

                if (input.Key == ConsoleKey.Y) goto reset;
                else navigateToPreviousPage = true;

            }
            catch (Exception ex)
            {
                error = "Error : " + ex.Message;
                goto validateData;
            }
        }
        public static void DrawEditPage<T, U>()
            where T : BaseEntity
            where U : T
        {
            start:
            string tableName = string.Empty;
            string successMessage = string.Empty;
            FuncNoParam drawParentPage = null;
            List<T> data = null; // DBContext<T>.GetByCustomQuery(x => x.UserType == ApplicationUserTypeEnum.ADMIN);

            if (typeof(U) == typeof(AdministrativeUser))
            {
                tableName = "Edit User";
                data = (List<T>)(object)DBContext<ApplicationUser>.GetByCustomQuery(x => x.UserType == ApplicationUserTypeEnum.ADMIN);
                drawParentPage = DrawManageApplicationUserPage;
            }
            else if (typeof(U) == typeof(Customer))
            {
                tableName = "Edit Customer";
                data = (List<T>)(object)DBContext<ApplicationUser>.GetByCustomQuery(x => x.UserType == ApplicationUserTypeEnum.CUSTOMER);
                drawParentPage = DrawManageCustomersPage;
            }
            else if (typeof(U) == typeof(ProductProfile))
            {
                tableName = "Edit Product Profile";
                data = (List<T>)(object)DBContext<ProductProfile>.GetAllExisting();
                drawParentPage = DrawManageProductProfilesPage;
            }

            T selectedData = DrawTableSelection<T>(tableName, data, drawParentPage);

            if (selectedData == null) return;
            U selectedObject = ((U)selectedData).Clone<U>();

            overrideSelection = true;
            string error = "Error: ";
            int exceptionLength = error.Length;

            edit:

            int panelX1 = MAX_WIDTH / 5;
            int panelY2 = (MAX_HEIGHT - 2);
            drawParentPage();
            DrawBox(panelX1 - 1, MAX_WIDTH - 1, 4, panelY2);
            DrawClosedPipe(6, panelX1 - 1, MAX_WIDTH - 1);

            if (error.Length != exceptionLength)
            {
                ClearArea(panelX1, panelY2 - 1, MAX_WIDTH - 1, panelY2 - 1);
                DrawClosedPipe(panelY2 - 2, panelX1 - 1, MAX_WIDTH - 1);
                CustomConsole.Write(error, panelX1, panelY2 - 1, MAX_WIDTH - 1, null, flag: CustomConsole.ConsoleFormatFlags.CENTER);
                error = "Error: ";
            }

            if (typeof(U) == typeof(AdministrativeUser))
            {

                InputApplicationUserViewModel viewModel = new InputApplicationUserViewModel();

                AdministrativeUser selectedAdminUser = (AdministrativeUser)(object)selectedObject;

                CustomConsole.Write("EDIT USER", panelX1, 5, MAX_WIDTH - 1, null, flag: CustomConsole.ConsoleFormatFlags.CENTER);

                CustomConsole.Write($"Edit First Name ('{selectedAdminUser.FirstName}'): ", panelX1, 7, MAX_WIDTH - 1, null, flag: CustomConsole.ConsoleFormatFlags.TOP_LEFT);
                viewModel.FirstName = Console.ReadLine();

                CustomConsole.Write($"Edit Last Name ('{selectedAdminUser.LastName}'): ", panelX1, 8, MAX_WIDTH - 1, null, flag: CustomConsole.ConsoleFormatFlags.TOP_LEFT);
                viewModel.LastName = Console.ReadLine();

                CustomConsole.Write($"Edit Username ('{selectedAdminUser.Username}'): ", panelX1, 9, MAX_WIDTH - 1, null, flag: CustomConsole.ConsoleFormatFlags.TOP_LEFT);
                viewModel.Username = Console.ReadLine();

                CustomConsole.Write($"Edit Password ('{selectedAdminUser.Password}'): ", panelX1, 10, MAX_WIDTH - 1, null, flag: CustomConsole.ConsoleFormatFlags.TOP_LEFT);
                viewModel.Password = Console.ReadLine();


                if (string.IsNullOrWhiteSpace(viewModel.FirstName)) error += "FirstName,";
                if (string.IsNullOrWhiteSpace(viewModel.LastName)) error += "LastName,";
                if (string.IsNullOrWhiteSpace(viewModel.Username)) error += "Username,";
                if (string.IsNullOrWhiteSpace(viewModel.Password)) error += "Password";

                if (error.Length != exceptionLength) goto edit;

                selectedAdminUser.FirstName = viewModel.FirstName;
                selectedAdminUser.LastName = viewModel.LastName;
                selectedAdminUser.Username = viewModel.Username;
                selectedAdminUser.Password = viewModel.Password;


                successMessage = "Successfully Edited User!";
            }
            else if (typeof(U) == typeof(Customer))
            {
                InputCustomerViewModel viewModel = new InputCustomerViewModel();

                Customer selectedCustomer = (Customer)(object)selectedObject;

                CustomConsole.Write("EDIT CUSTOMER", panelX1, 5, MAX_WIDTH - 1, null, flag: CustomConsole.ConsoleFormatFlags.CENTER);

                CustomConsole.Write($"Edit First Name ('{selectedCustomer.FirstName}'): ", panelX1, 7, MAX_WIDTH - 1, null, flag: CustomConsole.ConsoleFormatFlags.TOP_LEFT);
                viewModel.FirstName = Console.ReadLine();

                CustomConsole.Write($"Edit Last Name ('{selectedCustomer.LastName}'): ", panelX1, 8, MAX_WIDTH - 1, null, flag: CustomConsole.ConsoleFormatFlags.TOP_LEFT);
                viewModel.LastName = Console.ReadLine();

                CustomConsole.Write($"Edit Address ('{selectedCustomer.Address}'): ", panelX1, 9, MAX_WIDTH - 1, null, flag: CustomConsole.ConsoleFormatFlags.TOP_LEFT);
                viewModel.Address = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(viewModel.FirstName)) error += "FirstName,";
                if (string.IsNullOrWhiteSpace(viewModel.LastName)) error += "LastName,";
                if (string.IsNullOrWhiteSpace(viewModel.Address)) error += "Address";

                if (error.Length != exceptionLength) goto edit;

                selectedCustomer.FirstName = viewModel.FirstName;
                selectedCustomer.LastName = viewModel.LastName;
                selectedCustomer.Address = viewModel.Address;


                successMessage = "Successfully Edited User!";
            }
            else if (typeof(U) == typeof(ProductProfile))
            {
                InputProductProfileViewModel viewModel = new InputProductProfileViewModel();

                ProductProfile selectedProductProfile = (ProductProfile)(object)selectedObject;


                CustomConsole.Write("EDIT PRODUCT PROFILE", panelX1, 5, MAX_WIDTH - 1, null, flag: CustomConsole.ConsoleFormatFlags.CENTER);

                CustomConsole.Write($"Edit SKU ('{selectedProductProfile.SKU}'): ", panelX1, 7, MAX_WIDTH - 1, null, flag: CustomConsole.ConsoleFormatFlags.TOP_LEFT);
                viewModel.SKU = Console.ReadLine() ?? string.Empty;

                CustomConsole.Write($"Edit Product Name ('{selectedProductProfile.ProductName}'): ", panelX1, 8, MAX_WIDTH - 1, null, flag: CustomConsole.ConsoleFormatFlags.TOP_LEFT);
                viewModel.ProductName = Console.ReadLine() ?? string.Empty;

                CustomConsole.Write($"Edit Price ('PHP {selectedProductProfile.Price}'): ", panelX1, 9, MAX_WIDTH - 1, null, flag: CustomConsole.ConsoleFormatFlags.TOP_LEFT);
                viewModel.ProductPrice = Console.ReadLine() ?? string.Empty;

                if (string.IsNullOrWhiteSpace(viewModel.SKU)) error += "SKU,";
                if (string.IsNullOrWhiteSpace(viewModel.ProductName)) error += "ProductName,";
                if (string.IsNullOrWhiteSpace(viewModel.ProductPrice)) error += "ProductPrice, ";


                decimal price;
                if (!decimal.TryParse(viewModel.ProductPrice, out price)) error += "Invalid Price Value! (Not A Valid Amount!)";

                if (error.Length != exceptionLength) goto edit;

                selectedProductProfile.SKU = viewModel.SKU;
                selectedProductProfile.ProductName = viewModel.ProductName;
                selectedProductProfile.Price = price;

                successMessage = "Successfully Edited Product Profile!";
            }

            try
            {
                DBContext<T>.Update(selectedObject);
                ClearArea(panelX1, panelY2 - 1, MAX_WIDTH - 1, panelY2 - 1);
                DrawClosedPipe(panelY2 - 2, panelX1 - 1, MAX_WIDTH - 1);
                CustomConsole.Write(successMessage, panelX1, panelY2 - 1, MAX_WIDTH - 1, null, flag: CustomConsole.ConsoleFormatFlags.CENTER);
                CustomConsole.Write("Press any key to continue! ", panelX1, 12, MAX_WIDTH - 1, null, flag: CustomConsole.ConsoleFormatFlags.TOP_LEFT);
                ConsoleKeyInfo input = Console.ReadKey();
                goto start;
            }
            catch (Exception ex)
            {
                error = "Error : " + ex.Message;
                goto edit;
            }
        }
        public static void DrawDeletePage<T, U>()
            where T : BaseEntity
            where U : T
        {
            start:
            string tableName = string.Empty;
            string successMessage = string.Empty;
            FuncNoParam drawParentPage = null;
            List<T> data = null; // DBContext<T>.GetByCustomQuery(x => x.UserType == ApplicationUserTypeEnum.ADMIN);

            if (typeof(U) == typeof(AdministrativeUser))
            {
                tableName = "Delete User";
                data = (List<T>)(object)DBContext<ApplicationUser>.GetByCustomQuery(x => x.UserType == ApplicationUserTypeEnum.ADMIN);
                drawParentPage = DrawManageApplicationUserPage;
            }
            else if (typeof(U) == typeof(Customer))
            {
                tableName = "Delete Customer";
                data = (List<T>)(object)DBContext<ApplicationUser>.GetByCustomQuery(x => x.UserType == ApplicationUserTypeEnum.CUSTOMER);
                drawParentPage = DrawManageCustomersPage;
            }
            else if (typeof(U) == typeof(ProductProfile))
            {
                tableName = "Delete Product Profile";
                data = (List<T>)(object)DBContext<ProductProfile>.GetAllExisting();
                drawParentPage = DrawManageProductProfilesPage;
            }

            T selectedData = DrawTableSelection<T>(tableName, data, drawParentPage);

            if (selectedData == null) return;
            U selectedObject = ((U)selectedData).Clone<U>();

            overrideSelection = true;
            string error = "Error: ";
            int exceptionLength = error.Length;


            int panelX1 = MAX_WIDTH / 5;
            int panelY2 = (MAX_HEIGHT - 2);
            drawParentPage();
            DrawBox(panelX1 - 1, MAX_WIDTH - 1, 4, panelY2);
            DrawClosedPipe(6, panelX1 - 1, MAX_WIDTH - 1);


            if (typeof(U) == typeof(AdministrativeUser))
            {


                CustomConsole.Write("DELETE USER", panelX1, 5, MAX_WIDTH - 1, null, flag: CustomConsole.ConsoleFormatFlags.CENTER);

                CustomConsole.Write($"Delete User ({((AdministrativeUser)(object)selectedObject).CompleteName})?  (Y/N): ", panelX1, 7, MAX_WIDTH - 1, null, flag: CustomConsole.ConsoleFormatFlags.TOP_LEFT);

                successMessage = "Successfully Deleted User!";
            }
            else if (typeof(U) == typeof(Customer))
            {
                CustomConsole.Write("DELETE CUSTOMER", panelX1, 5, MAX_WIDTH - 1, null, flag: CustomConsole.ConsoleFormatFlags.CENTER);

                CustomConsole.Write($"Delete Customer ({((Customer)(object)selectedObject).CompleteName})?  (Y/N): ", panelX1, 7, MAX_WIDTH - 1, null, flag: CustomConsole.ConsoleFormatFlags.TOP_LEFT);

                successMessage = "Successfully Deleted Customer!";
            }
            else if (typeof(U) == typeof(ProductProfile))
            {
                CustomConsole.Write("DELETE PRODUCT PROFILE", panelX1, 5, MAX_WIDTH - 1, null, flag: CustomConsole.ConsoleFormatFlags.CENTER);

                CustomConsole.Write($"Delete Product Profile ({((ProductProfile)(object)selectedObject).ProductName})?  (Y/N): ", panelX1, 7, MAX_WIDTH - 1, null, flag: CustomConsole.ConsoleFormatFlags.TOP_LEFT);

                successMessage = "Successfully Deleted Product Profile!";
            }


            if (error.Length != exceptionLength)
            {
                ClearArea(panelX1, panelY2 - 1, MAX_WIDTH - 1, panelY2 - 1);
                DrawClosedPipe(panelY2 - 2, panelX1 - 1, MAX_WIDTH - 1);
                CustomConsole.Write(error, panelX1, panelY2 - 1, MAX_WIDTH - 1, null, flag: CustomConsole.ConsoleFormatFlags.CENTER);
                error = "Error: ";
            }

            var choice = Console.ReadKey();


            if (choice.Key == ConsoleKey.Y)
            {
                DBContext<T>.Delete(selectedObject.Uid, currentUser.Uid);
                ClearArea(panelX1, panelY2 - 1, MAX_WIDTH - 1, panelY2 - 1);
                DrawClosedPipe(panelY2 - 2, panelX1 - 1, MAX_WIDTH - 1);
                CustomConsole.Write(successMessage, panelX1, panelY2 - 1, MAX_WIDTH - 1, null, flag: CustomConsole.ConsoleFormatFlags.CENTER);

                CustomConsole.Write("Press any key to continue! ", panelX1, 12, MAX_WIDTH - 1, null, flag: CustomConsole.ConsoleFormatFlags.TOP_LEFT);
                ConsoleKeyInfo input = Console.ReadKey();
            }
            goto start;

        }
        public static void DrawListPage<T, U>()
            where T : BaseEntity
            where U : T
        {
            tableCursor = 1;
            ConsoleKeyInfo entry = new ConsoleKeyInfo('z', ConsoleKey.Z, false, false, false);
            List<T> data = null;
            string tableTitle = string.Empty;
            FuncNoParam drawParentPage = null;

            if (typeof(U) == typeof(AdministrativeUser))
            {
                tableTitle = "Application Users";
                drawParentPage = DrawManageApplicationUserPage;
                data = (List<T>)(object)DBContext<ApplicationUser>.GetByCustomQuery(x => x.UserType == ApplicationUserTypeEnum.ADMIN);
            }
            else if (typeof(U) == typeof(Customer))
            {
                tableTitle = "Customers";
                drawParentPage = DrawManageCustomersPage;
                data = (List<T>)(object)DBContext<ApplicationUser>.GetByCustomQuery(x => x.UserType == ApplicationUserTypeEnum.CUSTOMER);
            }
            else if (typeof(U) == typeof(ProductProfile))
            {
                tableTitle = "Product Profiles";
                drawParentPage = DrawManageProductProfilesPage;
                data = (List<T>)(object)DBContext<ProductProfile>.GetAllExisting();
            }

            while (entry.Key != ConsoleKey.Escape)
            {
                overrideSelection = true;
                drawParentPage();
                DrawTable<T>(data, tableCursor, (MAX_WIDTH / 5) - 1, MAX_WIDTH - 1, 4, (MAX_HEIGHT + 1) - 3, tableTitle);

                entry = Console.ReadKey();

                switch (entry.Key)
                {
                    case ConsoleKey.UpArrow:
                    case ConsoleKey.W:
                        tableCursor--;
                        break;
                    case ConsoleKey.DownArrow:
                    case ConsoleKey.S:
                        tableCursor++;
                        break;
                    default:
                        break;
                }
                int rowCount = ((MAX_HEIGHT + 1) - 3) - 8;
                int pageCount = (int)Math.Ceiling((decimal)data.Count() / rowCount);
                pageCount += (pageCount == 0) ? 1 : 0;

                if (tableCursor == 0) tableCursor = pageCount;
                else if (tableCursor > pageCount) tableCursor = 1;
            }

            navigateToPreviousPage = true;
        }
    }
}
