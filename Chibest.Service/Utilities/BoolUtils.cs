namespace Chibest.Service.Utilities;
public static class BoolUtils
{
    public static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return true;
        }
        catch
        { return false; }
    }

    //Example: BoolUtils.IsEmptyString(inputData.Code, inputData.Name)
    public static bool IsEmptyString(params string?[] fields)
    {
        return fields.Any(string.IsNullOrWhiteSpace);
    }
}