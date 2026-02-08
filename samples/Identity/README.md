# DRAFT: Identity

Management of `Email`s, `Username`s, `User`s, `Tenant`s and their `Role`s in each tenant.

**TODO:**
Define the self-registration and invitation process.

## `Role`

### Description

Roles are statically defined by the application per module.
They have a name, an optional `TenantId` and an optional (string) value.
They should be stored as claims in the auth-token.
The name and the tenant-id can be interpreted as the claim-type, and the value as the claim-value.

### Identity-Roles

- Roles belonging to the (physical) system (no claim-value)
  - `System.Operator`
  - `System.Manager`? (with limited permissions)
- Roles belonging to a tenant (claim-value is `TenantId`)
  - `Tenant.Owner`
  - `Tenant.Admin`
  - `Tenant.Manager`?
  - `Tenant.Member`

## `Email`

### Description

E-mail addresses are key to the identity management.

- They have to be unique and there cannot be a user without an e-mail.
- They are used to invite users to the platform.
- They can be "requested" in the process of self-onboarding without invitation.

So there has to be a "source of truth" for them.

- The e-mail-key is normalized (e.g. similar to Microsoft.AspNetCore.Identity with `string.Normalize().ToLowerInvariant()`).
- There can be multiple invitations in parallel for different tenants.
- A new invitation for a tenant overwrites the previous one and invalidates the verification code and role.
- Invitations are invalidated after some timeout (to be specified).
- A new request overwrites the previous one and invalidates the verification code.
- Requests and invitations can run in parallel.
- Verified invitations also verify ongoing requests.
- Verifying a request doesn't verify invitations.
- After abandoning an e-mail it can be re-requested/invited for a new user with a new user-id.
- If in the "abandoned" state it can be restored for the current user.

### Events

- `Reserved`
  - `NormalizedEmail`
  - `Email`
  - `ReservedAt`
  - `ForUserId`
  - `VerificationCode`
- `Invited`
  - `NormalizedEmail`
  - `Email`
  - `InvitedAt`
  - `InvitedByUserId`
  - `ForUserId`
  - `VerificationCode`
  - `TenantId`
  - `Role`
- `InvitationRevoked`
  - `NormalizedEmail`
  - `RevokedAt`
  - `RevokedByUserId`
  - `TenantId`
- `VerificationCodeConfirmed`
  - `NormalizedEmail`
  - `ConfirmedAt`
  - `VerificationCode`
- `VerificationCodeExpired`
  - `NormalizedEmail`
  - `ExpiredAt`
  - `VerificationCode`
- `Abandoned`
  - `NormalizedEmail`
  - `AbandonedAt`
  - `AbandonedByUserId`
- `Restored`
  - `NormalizedEmail`
  - `RestoredAt`
  - `RestoredByUserId`

## `Username`

### Description

- Usernames have to be unique to the system.
- Usernames and users have a 1:1 relationship.
- The username-key is normalized (e.g. similar to Microsoft.AspNetCore.Identity with `string.Normalize().ToLowerInvariant()`).
- Requests for usernames expire after a timeout (to be specified).

### Events

- `Reserved`
  - `NormalizedUsername`
  - `Username`
  - `ReservedAt`
  - `ForUserId`
- `ReservationConfirmed`
  - `NormalizedUsername`
  - `ConfirmedAt`
- `ReservationExpired`
  - `NormalizedUsername`
  - `ExpiredAt`

## `User`

### Description

- Signing in can be done via passkey or "code by email".
- For each sign-in a unique `SessionId` is generated and stored in the auth token.
- Users can register tenants.
- Users can be invited to tenants.
- The roles of an user are bound to a tenant.
- Profile (with avatar/picture)?

### Events

- `Registered`
  - `UserId`
  - `RegisteredAt`
  - `Username`
  - `DisplayName`
  - `Email`
- `VerificationCodeConfirmed`
  - `UserId`
  - `ComfirmedAt`
  - `VerificationCode` (links to the originating event)
- `VerificationCodeExpired`
  - `UserId`
  - `ExpiredAt`
  - `VerificationCode` (links to the originating event)
- `SignInCodeSent`
  - `UserId`
  - `SentAt`
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
- `UsernameChanged`
  - `UserId`
  - `ChangedAt`
  - `ChangedByUserId`
  - `Username`
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

Later:

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

## `Tenant`

### Description

A tenant is a logical unit of users who belong together like an "organization" or "workspace".

- Logo?
- Profile?

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
- `Activated`
  - `TenantId`
  - `ActivatedAt`
  - `ActivatedByUserId`
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
