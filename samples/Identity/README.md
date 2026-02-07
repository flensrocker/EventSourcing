# DRAFT: Identity

Management of `User`s, `Tenant`s and their `Role`s in each tenant.

## `Role`

### Description

Roles are statically defined by the application per module.

They are implemented as claims with the name of the role as the "claim-type" and e.g. the id of the tenant as the "claim-value".

### Identity-Roles

- Roles belonging to the (physical) system (no claim-value)
  - `System.Admin`
  - `System.Supporter`? (with limited permissions)
- Roles belonging to a tenant (claim-value is `TenantId`)
  - `Tenant.Owner`
  - `Tenant.Admin`
  - `Tenant.Manager`?
  - `Tenant.Member`

## `Tenant`

### Description

A tenant is a logical unit of users who belong together like an "organization" or "workspace".

- Logo?

### Events

- `Registered`
  - `TenantId`
  - `RegisteredAt`
  - `RegisteredByUserId`
  - `DisplayName`
- `DisplayNameChanged`
  - `TenantId`
  - `ChangedAt`
  - `ChangedByUserId`
  - `DisplayName`
- `Suspended`
  - `TenantId`
  - `SuspendedAt`
  - `SuspendedByUserId`
- `Resumed`
  - `TenantId`
  - `ResumedAt`
  - `ResumedByUserId`
- `Terminated`
  - `TenantId`
  - `TerminatedAt`
  - `TerminatedByUserId`

## `User`

### Description

- Usernames have to be unique.
- E-mail addresses have to be unique.
- Signing in can be done via passkey or "code by email".
- For each sign-in a unique `SessionId` is generated and stored in the auth token.
- Users can register tenants.
- Users can be invited to tenants.
- The roles of an user are bound to a tenant.
- Profile picture?

### Events

- `Registered`
  - `UserId`
  - `Username`
  - `DisplayName`
  - `RegisteredAt`
  - `Email`
  - `VerificationCode`
- `VerificationConfirmed`
  - `UserId`
  - `ComfirmedAt`
  - `VerificationCode` (links to the originating event)
- `SignInCodeSent`
  - `UserId`
  - `SentAt`
  - `SentByUserId`
  - `Email`
  - `VerificationCode`
- `SignedIn`
  - `UserId`
  - `SignedInAt`
  - `SignInType` ("passkey", "verification-code" etc.)
  - `SignInData` (`CredentialId`, `VerificationCode`)
  - `SessionId`
  - `RemoteInfo`? (like IP address?)
- `SignedOut`
  - `UserId`
  - `SignedOutAt`
  - `SessionId`
- `DisplayNameChanged`
  - `UserId`
  - `ChangedAt`
  - `ChangedByUserId`
  - `DisplayName`
- `EmailChangeConfirmationSent`
  - `UserId`
  - `SentAt`
  - `SentByUserId`
  - `Email`
  - `VerificationCode`
- `PasskeyAdded`
  - `UserId`
  - `AddedAt`
  - `AddedByUserId`
  - `CredentialId`
  - `Data` (will be `Microsoft.AspNetCore.Identity.IdentityPasskeyData`)
- `PasskeyUpdated`
  - `UserId`
  - `UpdatedAt`
  - `UpdatedByUserId`
  - `CredentialId`
  - `Data` (will be `Microsoft.AspNetCore.Identity.IdentityPasskeyData`)
- `PasskeyRemoved`
  - `UserId`
  - `RemovedAt`
  - `RemovedByUserId`
  - `CredentialId`
- `Invited`
  - `UserId`
  - `InvitedAt`
  - `InvitedByUserId`
  - `TenantId`
  - `Role`
- `InvitationConfirmed`
  - `UserId`
  - `ConfirmedAt`
  - `ConfirmedByUserId`
  - `TenantId`
- `RoleChanged`
  - `UserId`
  - `ChangedAt`
  - `ChangedByUserId`
  - `TenantId`
  - `Role`
- `MembershipSuspended`
  - `UserId`
  - `SuspendedAt`
  - `SuspendedByUserId`
  - `TenantId`
- `MembershipResumed`
  - `UserId`
  - `ResumedAt`
  - `ResumedByUserId`
  - `TenantId`
- `MembershipRevoked`
  - `UserId`
  - `RevokedAt`
  - `RevokedByUserId`
  - `TenantId`
