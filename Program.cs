using Figgle;
using MidtermProject;
using MidtermProject.DAL;
using MidtermProject.Entities;
using MidtermProject.Enums;
using MidtermProject.Services;
using MidtermProject.ViewModels;

// Top-Level Statements


Console.BufferHeight = 45;
Console.BufferWidth = 200;



Console.WindowHeight = 50;
Console.WindowWidth = 210;

Database.Init(new Type[]
{
    typeof(ApplicationUser),
    typeof(ProductProfile),
    typeof(Transaction)
});


Stack<int> navigationHistory = new Stack<int>();

ApplicationWorker.Init(200, 40);
PageLoader? loadPage = null;



Run();


// Methods

void Run()
{
    SetNextPage(ApplicationStateFlagsEnum.LAUNCHER, false);
    Navigate();
}

void PopStack()
{
    navigationHistory.Pop();
}

void Navigate()
{
    start:

    ConsoleKeyInfo entry;
    loadPage();

    if (ApplicationWorker.navigateToPreviousPage)
    {
        if(navigationHistory.Peek() == (int)ApplicationStateFlagsEnum.MENU_CREATE_TRANSACTION)
        {
            ApplicationWorker.newTransaction = null;
        }
        SetNextPage(ApplicationStateFlagsEnum.BACK, true);
        ApplicationWorker.navigateToPreviousPage = false;
        goto start;
    }


    if (loadPage == Exit) return;
    entry = Console.ReadKey();


    switch (entry.Key)
    {

        case ConsoleKey.DownArrow:
        case ConsoleKey.S:
            ApplicationWorker.cursorSelection++;
            if (ApplicationWorker.cursorSelection == ApplicationWorker.cursorSelectionLimit) ApplicationWorker.cursorSelection = 0;
            break;
        case ConsoleKey.UpArrow:
        case ConsoleKey.W:
            ApplicationWorker.cursorSelection--;
            if (ApplicationWorker.cursorSelection == -1) ApplicationWorker.cursorSelection = ApplicationWorker.cursorSelectionLimit-1;
            break;
        case ConsoleKey.Enter:
            SetNextPage((ApplicationStateFlagsEnum)ApplicationWorker.cursorSelectionValue, false);
            break;
        default:
            ApplicationWorker.cursorSelection = 0;
            break;
    }

    Navigate();
}

void SetNextPage(ApplicationStateFlagsEnum destination, bool isPrev)
{
    ApplicationWorker.cursorSelection = 0;
    ApplicationWorker.cursorSelectionValue = -1;
    ApplicationWorker.cursorSelectionLimit = 1;
    ApplicationWorker.showSelectionDescription = true;
    ApplicationWorker.overrideSelection = false;

    if(destination == ApplicationStateFlagsEnum.MENU_MANAGE_TRANSACTIONS && !isPrev && navigationHistory.Peek() != (int)ApplicationStateFlagsEnum.TRANSACTION_EDIT)
    {
        ApplicationWorker.newTransaction = null;
    }

    switch (destination)
    {
        case ApplicationStateFlagsEnum.LAUNCHER:
            ApplicationWorker.showSelectionDescription = true;
            loadPage = ApplicationWorker.DrawLauncher;
            break;
        case ApplicationStateFlagsEnum.EXIT:
            loadPage = Exit;
            break;
        case ApplicationStateFlagsEnum.SIGN_OUT:
        case ApplicationStateFlagsEnum.BACK:
            isPrev = true;
            navigationHistory.Pop();
            SetNextPage((ApplicationStateFlagsEnum)navigationHistory.Peek(), true);
            break;
        case ApplicationStateFlagsEnum.LOGIN_ADMIN:
            ApplicationWorker.currentUser = null;
            loadPage = AuthenticateUser;
            break;
        case ApplicationStateFlagsEnum.MENU_USER:
            loadPage = ApplicationWorker.DrawUserMenuPage;
            break;
        case ApplicationStateFlagsEnum.MENU_MANAGE_USER:
            loadPage = ApplicationWorker.DrawManageApplicationUserPage;
            break;
        case ApplicationStateFlagsEnum.USER_CREATE:
            ApplicationWorker.showSelectionDescription = false;
            ApplicationWorker.DrawCreatePage<ApplicationUser, AdministrativeUser>();
            break;
        case ApplicationStateFlagsEnum.USER_LIST:
            ApplicationWorker.showSelectionDescription = false;
            ApplicationWorker.DrawListPage<ApplicationUser, AdministrativeUser>();
            break;
        case ApplicationStateFlagsEnum.USER_EDIT:
            ApplicationWorker.showSelectionDescription = false;
            ApplicationWorker.DrawEditPage<ApplicationUser, AdministrativeUser>();
            break;
        case ApplicationStateFlagsEnum.USER_DELETE:
            ApplicationWorker.showSelectionDescription = false;
            ApplicationWorker.DrawDeletePage<ApplicationUser, AdministrativeUser>();
            break;
        case ApplicationStateFlagsEnum.MENU_MANAGE_CUSTOMER:
            loadPage = ApplicationWorker.DrawManageCustomersPage;
            break;
        case ApplicationStateFlagsEnum.CUSTOMER_CREATE:
            ApplicationWorker.showSelectionDescription = false;
            ApplicationWorker.DrawCreatePage<ApplicationUser, Customer>();
            break;
        case ApplicationStateFlagsEnum.CUSTOMER_LIST:
            ApplicationWorker.showSelectionDescription = false;
            ApplicationWorker.DrawListPage<ApplicationUser, Customer>();
            break;
        case ApplicationStateFlagsEnum.CUSTOMER_EDIT:
            ApplicationWorker.showSelectionDescription = false;
            ApplicationWorker.DrawEditPage<ApplicationUser, Customer>();
            break;
        case ApplicationStateFlagsEnum.CUSTOMER_DELETE:
            ApplicationWorker.showSelectionDescription = false;
            ApplicationWorker.DrawDeletePage<ApplicationUser, Customer>();
            break;
        case ApplicationStateFlagsEnum.MENU_MANAGE_PRODUCTPROFILE:
            loadPage = ApplicationWorker.DrawManageProductProfilesPage;
            break;
        case ApplicationStateFlagsEnum.PRODUCT_CREATE:
            ApplicationWorker.showSelectionDescription = false;
            ApplicationWorker.DrawCreatePage<ProductProfile, ProductProfile>();
            break;
        case ApplicationStateFlagsEnum.PRODUCT_EDIT:
            ApplicationWorker.showSelectionDescription = false;
            ApplicationWorker.DrawEditPage<ProductProfile, ProductProfile>();
            break;
        case ApplicationStateFlagsEnum.PRODUCT_DELETE:
            ApplicationWorker.showSelectionDescription = false;
            ApplicationWorker.DrawDeletePage<ProductProfile, ProductProfile>();
            break;
        case ApplicationStateFlagsEnum.PRODUCT_LIST:
            ApplicationWorker.showSelectionDescription = false;
            ApplicationWorker.DrawListPage<ProductProfile, ProductProfile>();
            break;
        case ApplicationStateFlagsEnum.MENU_MANAGE_STOCK:
            loadPage = ApplicationWorker.DrawManageStocksPage;
            break;
        case ApplicationStateFlagsEnum.STOCK_CREATE:
            ApplicationWorker.showSelectionDescription = false;
            ApplicationWorker.DrawCreateStocksPage();
            break;
        case ApplicationStateFlagsEnum.STOCK_DELETE:
            ApplicationWorker.showSelectionDescription = false;
            ApplicationWorker.DrawRemoveStocksPage();
            break;
        case ApplicationStateFlagsEnum.STOCK_LIST_IN:
            ApplicationWorker.showSelectionDescription = false;
            ApplicationWorker.DrawListInStocksPage();
            break;
        case ApplicationStateFlagsEnum.STOCK_LIST_OUT:
            ApplicationWorker.showSelectionDescription = false;
            ApplicationWorker.DrawListOutStocksPage();
            break;
        case ApplicationStateFlagsEnum.MENU_MANAGE_TRANSACTIONS:
            ApplicationWorker.isEditTransaction = false;
            loadPage = ApplicationWorker.DrawManageTransactionsPage;
            break;
        case ApplicationStateFlagsEnum.MENU_LIST_ALL_TRANSACTIONS:
            ApplicationWorker.showSelectionDescription = false;
            loadPage = ApplicationWorker.DrawListTransactionsPage;
            break;
        case ApplicationStateFlagsEnum.MENU_LIST_CUSTOMER_TRANSACTIONS:
            ApplicationWorker.showSelectionDescription = false;
            loadPage = ApplicationWorker.DrawListCustomerTransactionsPage;
            break;
        case ApplicationStateFlagsEnum.MENU_LIST_ADMIN_TRANSACTIONS:
            ApplicationWorker.showSelectionDescription = false;
            loadPage = ApplicationWorker.DrawListAdminTransactionsPage;
            break;
        case ApplicationStateFlagsEnum.MENU_CREATE_TRANSACTION:
            ApplicationWorker.showSelectionDescription = false;
            ApplicationWorker.showNewTransactionInfo = true;
            loadPage = ApplicationWorker.DrawUpsertTransactionMenuPage;
            break;
        case ApplicationStateFlagsEnum.TRANSACTION_UPSERT_ADDITEMS:
            ApplicationWorker.showSelectionDescription = false;
            ApplicationWorker.showNewTransactionInfo = false;
            ApplicationWorker.showNewTransactionItems = true;
            loadPage = ApplicationWorker.DrawUpsertTransaction_SetItemsSelection;
            break;
        case ApplicationStateFlagsEnum.TRANSACTION_UPSERT_SETAFEE:
            ApplicationWorker.showSelectionDescription = false;
            ApplicationWorker.showNewTransactionInfo = false;
            ApplicationWorker.DrawUpsertTransaction_SetAdditionalFees();
            break;
        case ApplicationStateFlagsEnum.TRANSACTION_UPSERT_SETCUSTOMER:
            ApplicationWorker.showSelectionDescription = false;
            ApplicationWorker.showNewTransactionInfo = false;
            ApplicationWorker.DrawUpsertTransaction_SetCustomer();

            break;
        case ApplicationStateFlagsEnum.TRANSACTION_UPSERT_SETREMARKS:
            ApplicationWorker.showSelectionDescription = false;
            ApplicationWorker.showNewTransactionInfo = false;
            ApplicationWorker.DrawUpsertTransaction_SetRemarks();
            break;
        case ApplicationStateFlagsEnum.TRANSACTION_UPSERT_SETSTATUS:
            ApplicationWorker.showSelectionDescription = false;
            ApplicationWorker.showNewTransactionInfo = false;
            ApplicationWorker.DrawUpsertTransaction_SetStatus();
            break;
        case ApplicationStateFlagsEnum.TRANSACTION_MENU_SETITEMS_ADD:
            ApplicationWorker.showSelectionDescription = false;
            ApplicationWorker.showNewTransactionInfo = false;
            ApplicationWorker.showNewTransactionItems = false;
            ApplicationWorker.DrawUpsertTransaction_SetItemsSelection_AddItems();
            break;
        case ApplicationStateFlagsEnum.TRANSACTION_MENU_SETITEMS_REMOVE:
            ApplicationWorker.showSelectionDescription = false;
            ApplicationWorker.showNewTransactionInfo = false;
            ApplicationWorker.showNewTransactionItems = false;
            ApplicationWorker.DrawUpsertTransaction_SetItemsSelection_RemoveItems();
            break;
        case ApplicationStateFlagsEnum.TRANSACTION_UPSERT_SAVE:
            ApplicationWorker.showSelectionDescription = false;
            ApplicationWorker.showNewTransactionInfo = true;
            ApplicationWorker.DrawUpsertTransaction_SaveTransaction();
            break;
        case ApplicationStateFlagsEnum.TRANSACTION_EDIT:
            ApplicationWorker.showSelectionDescription = false;
            ApplicationWorker.showNewTransactionInfo = true;
            ApplicationWorker.isEditTransaction = true;
            ApplicationWorker.forceRedirect = SetNextPage;
            ApplicationWorker.newTransaction = null;
            ApplicationWorker.popNavigationHistory = PopStack;
            loadPage = ApplicationWorker.DrawUpsertTransactionMenuPage;
            break;
    }

    if (!isPrev) navigationHistory.Push((int)destination);
}

void AuthenticateUser()
{
    AuthenticateViewModel viewModel;
    bool isSuccessfulAuth = false;
    string? authError = null;

    while (!isSuccessfulAuth)
    {
        viewModel = ApplicationWorker.DrawAuthenticationPage(authError != null, authError ?? string.Empty);
        if (viewModel == null)
        {
            SetNextPage(ApplicationStateFlagsEnum.BACK, true);
            break;
        }

        var user = DBContext<ApplicationUser>.GetAllExisting()
            .Where(x => x.Username == viewModel.Username && x.UserType == ApplicationUserTypeEnum.ADMIN).FirstOrDefault();
        if (user == null)
        {
            isSuccessfulAuth = false;
            authError = "Invalid User!";
            continue;
        }
        isSuccessfulAuth = user.AuthenticateUser(viewModel.Password);
        authError = (!isSuccessfulAuth) ? "Invalid Login!" : string.Empty;
        ApplicationWorker.currentUser = isSuccessfulAuth ? user : null;

    }

    if (isSuccessfulAuth) SetNextPage(ApplicationStateFlagsEnum.MENU_USER, false);

    Navigate();
}


void Exit()
{
    ApplicationWorker.cursorSelection = 0;
    ApplicationWorker.DrawExit();
}

delegate void PageLoader();
