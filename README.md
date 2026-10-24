# OneDrive Transfer Verify

## Setup: How I set up the project

### Personal development/testing approach - Dedicated test Microsoft account
A separate free Microsoft account was created for development and testing to avoid using my personal account.
The same account created the tenant and the app registration, so everything is managed from one place.

### Registering the app that will be built + Tenant creation
Signing into Microsoft entra admin center with test user was not allowed.
Reason: Personal Microsoft accounts are not linked to an Entra tenant.
Fixed by signing up for an Azure account which creates a personal tenant (Default Directory)
Added App registration (same name as project: 'OneDriveTransferVerify') and added API Permissions: Files.ReadWrite
Supported account types: any Entra ID tenant + personal Microsoft accounts (so personal accounts like outlook.com can sign in).
Platform: public client with redirect URI `http://localhost`

### Sign in mode
Choice: Interactive browser. Decided this based on a quick search on best safety measures


## Planned flow
Config: 
- Client ID (App registration)
- Intended Folder -> TransferVerify
- Sample File to Upload

Authentication - MSAL Library:
- check if a valid token is cached
- No valid token -> Sign in via Entra (through Interactive browser): This caches the tokens

Graph API:
- Does Intended Folder exist?
- No Folder -> Create Folder
- Upload file -> capture response
- Download file to local temp folder

Local Verification:
- Verify with sha256 both files
- assert hashes and report


## Key Terms

| Term | Meaning |
|---|---|
| **Microsoft Graph** | Microsoft's unified REST API for Microsoft 365 data (OneDrive, Outlook, users…) |
| **Microsoft Entra ID** | Microsoft's identity and access management service |
| **Tenant** | A dedicated Entra ID instance (directory) holding users and app registrations |
| **Client ID** | Public identifier of the registered app. |
| **MSAL** | Microsoft Authentication Library (`Microsoft.Identity.Client`); handles sign-in, token caching and refresh |
| **OAuth 2.0** | Authorisation framework: the app gets a token to act on the user's behalf without seeing their password |
| **OpenID Connect (OIDC)** | Identity layer on top of OAuth 2.0 that adds authentication |
| **ID token** | Describes the signed-in user (result of authentication) |
| **Access token** | Short-lived Credential sent to Graph |
| **Refresh token** | Lets MSAL get new access tokens without another sign-in |

## Links

### MSAL (authentication)
| Name | Link |
|---|---|
| **MSAL.NET overview** | [MSAL.NET](https://learn.microsoft.com/en-us/entra/msal/dotnet/) |
| **Acquiring tokens** | [Acquiring tokens overview](https://learn.microsoft.com/en-gb/entra/msal/dotnet/acquiring-tokens/overview) |
| **Token through interactive browser** | [Desktop app: acquire a token interactively](https://learn.microsoft.com/en-us/Entra/identity-platform/scenario-desktop-acquire-token-interactive?tabs=dotnet) |

### Graph API contracts (OneDrive)
| Name | Link |
|---|---|
| **Get driveItem** (check if folder exists) | [Get driveItem](https://learn.microsoft.com/en-gb/graph/api/driveitem-get?view=graph-rest-1.0&tabs=http) |
| **Create folder** | [Create a new folder in a drive](https://learn.microsoft.com/en-us/graph/api/driveitem-post-children?view=graph-rest-1.0&tabs=http) |
| **Upload file** | [Upload or replace contents of a driveItem](https://learn.microsoft.com/en-us/graph/api/driveitem-put-content?view=graph-rest-1.0&tabs=http) |
| **Download file** | [Download driveItem content](https://learn.microsoft.com/en-us/graph/api/driveitem-get-content?view=graph-rest-1.0) |