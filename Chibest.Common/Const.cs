namespace Chibest.Common;

public static class Const
{
    #region System Roles
    public static class Roles
    {
        public const string Admin = "Admin";
        public const string Supplier = "Supplier";
        public const string Cashier = "Cashier";
        public const string Staff = "Staff";
        // Other role ...;
    }
    #endregion

    #region General
    public static int ERROR_EXCEPTION = -1;
    public static string ERROR_EXCEPTION_MSG = "An unexpected error occurred.";
    public static int SUCCESS = 1;
    public static int FAILURE = 0;

    // Mapping HTTP Status Codes
    public static int HTTP_STATUS_OK = 200; // Success
    public static int HTTP_STATUS_CREATED = 201; // Created
    public static int HTTP_STATUS_BAD_REQUEST = 400; // Failure
    public static int HTTP_STATUS_INTERNAL_ERROR = 500; // Exception
    public static int HTTP_STATUS_UNAUTHORIZED = 401; // Unauthorized
    public static int HTTP_STATUS_FORBIDDEN = 403; // Forbidden
    public static int HTTP_STATUS_NOT_FOUND = 404; // Not Found
    public static int HTTP_STATUS_CONFLICT = 409; // Conflict
    #endregion

    #region Success Messages
    public static string SUCCESS_CREATE_MSG = "Data created successfully.";
    public static string SUCCESS_READ_MSG = "Data retrieved successfully.";
    public static string SUCCESS_UPDATE_MSG = "Data updated successfully.";
    public static string SUCCESS_DELETE_MSG = "Data deleted successfully.";
    public static string SUCCESS_LOGIN_MSG = "Login successful.";
    public static string SUCCESS_LOGOUT_MSG = "Logout successful.";
    #endregion

    #region Failure Messages
    public static string FAIL_CREATE_MSG = "Failed to create data.";
    public static string FAIL_READ_MSG = "Failed to retrieve data.";
    public static string FAIL_UPDATE_MSG = "Failed to update data.";
    public static string FAIL_DELETE_MSG = "Failed to delete data.";
    public static string FAIL_LOGIN_MSG = "Invalid email or password.";
    public static string FAIL_LOGOUT_MSG = "Failed to logout.";
    #endregion
}