# OneDrive Transfer Verify

## How to run

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- A Microsoft account. A **personal account** (e.g. outlook.com) is recommended.

### 1. Code
```
git clone https://github.com/KinZammit/onedrive-transfer-verify.git
cd onedrive-transfer-verify
```

### 2. Run
**Visual Studio:** open `OneDriveTransferVerify/OneDriveTransferVerify.sln` and press **F5**.

**Command line** (from the repository root):
```
dotnet run --project OneDriveTransferVerify/OneDriveTransferVerify
```

### 3. Sign in
- A browser window opens. Sign in with **your own** Microsoft account.
- A consent screen asks for permission to read your profile and access your files. It shows the app as **unverified**; this is expected for a personal app registration.
- The file is uploaded to **your** OneDrive, into the folder set in `appsettings.json` Under `UploadFolderName`.

### 4. Expected output
```
<original SHA-256 hash>
<signed-in user profile, used to confirm sign-in>
Folder already exists          (or: Folder Created)
Uploaded sample.txt - File was created          (or: File was replaced)
Downloaded to: C:\Users\<you>\AppData\Local\Temp\OneDriveTaskDownload\sample.txt
<original SHA-256 hash>
<download SHA-256 hash>
File transferred correctly
```
Exit code **0** when the hashes match, **1** when they don't or an error occurs.

### Notes
- You need to sign in on **every run** (the token cache is kept in memory only).
- The uploaded file stays in your OneDrive; you can delete the folder afterwards.

## Configuration

Settings are in `OneDriveTransferVerify/OneDriveTransferVerify/appsettings.json`.
All three are **required**; the app stops with an error if any is missing or empty.

| Key | Meaning | Value in repo |
|---|---|---|
| `ClientId` | Application (client) ID of the Microsoft Entra app registration | `7667b210-…` |
| `UploadFolderName` | Folder in the **OneDrive root** where the file is uploaded (created if missing) | `TransferVerify` |
| `DownloadFolderName` | Sub-folder created under the system **temp** folder where the file is downloaded | `OneDriveTaskDownload` |

### File to upload
The app uploads `sample.txt`, which sits next to `Program.cs` and is copied to the output folder on build.
To test another file (e.g. a binary file), replace `sample.txt` with it, keeping the same name, and rebuild.

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


## Flow

1. Load `appsettings.json` (`ClientId`, `UploadFolderName`, `DownloadFolderName`).
2. Compute the **SHA-256 of the original file** first, before the transfer starts. This way, if the file changes during the run (or a bug overwrites it), the result is a mismatch rather than a false pass.
3. **Sign in** (`AuthService`, MSAL): try a cached token silently; if there is none, open the browser for interactive sign-in.
4. **Ensure the folder exists** (`GraphClient`):
   - `FolderExistsAsync`: `GET /me/drive/root:/{folder}`. 200 = exists, 404 = doesn't exist.
   - If it doesn't exist, `CreateFolderAsync`: `POST /me/drive/root/children` with `conflictBehavior: fail`.
     A 409 (the folder was created in the meantime) is treated as success, so existing content is never replaced.
5. **Upload** the file: `PUT /me/drive/root:/{folder}/{file}:/content` with the raw bytes (`application/octet-stream`). 201 = new file, 200 = replaced existing file.
6. **Size check:** the size returned by Graph must equal the local file size (early detection of truncation).
7. **Download** by the item ID returned from the upload: `GET /me/drive/items/{id}/content` into a temp sub-folder. The redirect to the pre-authenticated download URL is followed automatically. The download path is checked to never be the original file.
8. **Verify:** compute the SHA-256 of the downloaded file and compare it with the original: **match → exit code 0**, mismatch → exit code 1.


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
| **CORS** | Cross-Origin Resource Sharing: a browser security rule that only lets a web page read responses from another site if that site allows it. Encountered while testing downloads in Graph Explorer: the download redirect goes to a storage host that doesn't allow Graph Explorer's site, so the browser blocked the response. It doesn't affect this console app (CORS only applies to browsers). |

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