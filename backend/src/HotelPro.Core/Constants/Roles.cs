namespace HotelPro.Core.Constants;

public static class Roles
{
    public const string Admin = "Admin";
    public const string Manager = "Manager";
    public const string Reception = "Reception";
    public const string Housekeeping = "Housekeeping";
    public const string Maintenance = "Maintenance";
    public const string Accountant = "Accountant";

    public static readonly string[] All = { Admin, Manager, Reception, Housekeeping, Maintenance, Accountant };
}
