﻿namespace MidtermProject.Enums
{
    public enum ApplicationStateFlagsEnum
    {
        NULL = -1,
        LAUNCHER,
        LOGIN_ADMIN,
        LOGIN_CUSTOMER,
        EXIT,
        BACK,
        SIGN_OUT,
        PRODUCT_CREATE = 101,
        PRODUCT_EDIT = 102,
        PRODUCT_DELETE = 103,
        PRODUCT_LIST = 104,
        USER_LIST = 200,
        USER_CREATE = 201,
        USER_EDIT = 202,
        USER_DELETE = 203,
        STOCK_LIST_OUT = 299,
        STOCK_LIST_IN = 300,
        STOCK_CREATE = 301,
        STOCK_DELETE = 303,
        CUSTOMER_LIST = 400,
        CUSTOMER_CREATE = 401,
        CUSTOMER_EDIT = 402,
        CUSTOMER_DELETE = 403,
        TRANSACTION_UPSERT_SETCUSTOMER = 50001,
        TRANSACTION_UPSERT_ADDITEMS = 50002,
        TRANSACTION_UPSERT_SETAFEE = 50003,
        TRANSACTION_UPSERT_SETSTATUS = 50004,
        TRANSACTION_UPSERT_SETREMARKS = 50005,
        TRANSACTION_UPSERT_SAVE = 50006,
        TRANSACTION_EDIT = 502,
        TRANSACTION_LIST_CANCELLED = 505,
        TRANSACTION_MENU_SETITEMS_ADD = 51001,
        TRANSACTION_MENU_SETITEMS_REMOVE = 51002,
        MENU_USER = 1001,
        MENU_MANAGE_USER = 1002,
        MENU_MANAGE_CUSTOMER = 1003,
        MENU_MANAGE_STOCK = 1004,
        MENU_MANAGE_PRODUCTPROFILE = 1005,
        MENU_MANAGE_TRANSACTIONS = 1006,
        MENU_LIST_ALL_TRANSACTIONS = 1007,
        MENU_LIST_CUSTOMER_TRANSACTIONS = 1008,
        MENU_LIST_ADMIN_TRANSACTIONS = 1009,
        MENU_CREATE_TRANSACTION = 1010
    }
}