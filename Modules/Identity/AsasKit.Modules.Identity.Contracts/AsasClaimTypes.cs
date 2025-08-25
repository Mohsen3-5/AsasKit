namespace AsasKit.Modules.Identity.Contracts;

public static class AsasClaimTypes
{
    // JWT/OIDC well-knowns
    public const string Sub = "sub";
    public const string Email = "email";
    public const string Name = "name";
    public const string FamilyName = "family_name";
    public const string PreferredUsername = "preferred_username";
    public const string PhoneNumber = "phone_number";
    public const string Jti = "jti";

    // Multi-tenancy
    public const string TenantId = "tid"; // we also read MS mapped URI

    // Impersonation (reserve – emit only if you implement it later)
    public const string ImpersonatorUserId = "impersonator_user_id";
    public const string ImpersonatorTenantId = "impersonator_tenant_id";
}
