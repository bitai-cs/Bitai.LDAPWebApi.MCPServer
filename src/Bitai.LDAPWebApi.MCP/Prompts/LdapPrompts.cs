using System.ComponentModel;
using ModelContextProtocol.Server;

namespace Bitai.LDAPWebApi.MCP;

[McpServerPromptType]
public sealed class LdapPrompts
{
    [McpServerPrompt(Name = "ldap_user_lookup_workflow")]
    [Description("Provides a guided workflow for locating an LDAP user and validating the result.")]
    public string UserLookupWorkflow(
        [Description("User account name to search for, usually sAMAccountName.")] string samAccountName,
        [Description("Optional LDAP server profile to use for the search.")] string? ldapServerProfile = null)
    {
        return $"""
               Use the ldap_search_users tool to locate the target account.
               1. Call ldap_search_users with filterValue="{samAccountName}" and ldapServerProfile="{ldapServerProfile ?? "<default>"}".
               2. If no entry is returned, call ldap_search_directory_by_identifier using identifier="{samAccountName}" for a broader lookup.
               3. Summarize matched directory attributes and suggest the next safe administrative action.
               """;
    }

    [McpServerPrompt(Name = "ldap_msad_user_create_workflow")]
    [Description("Provides a safe workflow for creating an MS AD user.")]
    public string MsAdUserCreateWorkflow(
        [Description("LDAP server profile for this action.")] string ldapServerProfile)
    {
        return $"""
               Create an MS AD user with ldap_create_msad_user using ldapServerProfile="{ldapServerProfile}".
               Provide all required identity and account fields.
               Validate the response and report the created account identifier.
               """;
    }

    [McpServerPrompt(Name = "ldap_msad_user_password_set_workflow")]
    [Description("Provides a safe workflow for setting an MS AD user's password.")]
    public string MsAdUserPasswordSetWorkflow(
        [Description("LDAP server profile for this action.")] string ldapServerProfile,
        [Description("Target account identifier, usually sAMAccountName.")] string samAccountName)
    {
        return $"""
               Set the password with ldap_set_msad_user_password for "{samAccountName}" using ldapServerProfile="{ldapServerProfile}".
               Do not echo or persist plaintext passwords.
               Report only operation status and relevant API response details.
               """;
    }

    [McpServerPrompt(Name = "ldap_msad_user_disable_workflow")]
    [Description("Provides a safe workflow for disabling an MS AD user.")]
    public string MsAdUserDisableWorkflow(
        [Description("LDAP server profile for this action.")] string ldapServerProfile,
        [Description("Target account identifier, usually sAMAccountName.")] string samAccountName)
    {
        return $"""
               Disable the account with ldap_disable_msad_user for "{samAccountName}" using ldapServerProfile="{ldapServerProfile}".
               Confirm business approval before execution.
               Return LDAP API response details and stop if the operation fails.
               """;
    }

    [McpServerPrompt(Name = "ldap_msad_user_remove_workflow")]
    [Description("Provides a guarded workflow for removing an MS AD user.")]
    public string MsAdUserRemoveWorkflow(
        [Description("LDAP server profile for this action.")] string ldapServerProfile,
        [Description("Target account identifier, usually sAMAccountName.")] string samAccountName)
    {
        return $"""
               Remove the account with ldap_remove_msad_user for "{samAccountName}" using ldapServerProfile="{ldapServerProfile}".
               Require explicit confirmation immediately before deletion.
               Report the API result and any failure details.
               """;
    }
}
