# DRAFT: Identity

Management of `EmailAddress`es, `User`s, `Tenant`s and their `Role`s in each tenant.

## Processes

- C = Command
- E = Event
- T = Timer
- A = Action/Sideeffect

### Self-Registration

The user start registration with providing an email-address.
The system sends a verification code to this address.
The app navigates to a form for entering the email-address, verification code and a "display-name" for the user.
If the reservation is not confirmed within a timeout, it gets automatically marked as "expired".

- C: `EmailAddress.Reserve`  
  if not already reserved:
  - E: `EmailAddress.Reserved`
  - T: `EmailAddress.ExpireReservation`
  - A: Sent verification code
- C: `EmailAddress.ConfirmReservation`  
  if not already expired:
  - E: `EmailAddress.ReservationConfirmed`
  - C: `User.Register`
- C: `EmailAddress.ExpireReservation`:  
  if not already confirmed:
  - E: `EmailAddress.ReservationExpired`

### Invitation

**TODO**

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

## `EmailAddress`

### Description

E-mail addresses are key to the identity management.
They are essentially the "username".

- They have to be unique and there cannot be a user without an e-mail address.
- They are used to invite users to the platform.
- They can be "reserved" in the process of self-registration without invitation.

So there has to be a "source of truth" for them.

- The e-mail-address is normalized (e.g. similar to Microsoft.AspNetCore.Identity with `string.Normalize().ToLowerInvariant()`).
- There can be multiple invitations in parallel for different tenants.
- A new invitation for a tenant overwrites the previous one and invalidates the verification code and role.
- Invitations expire after some timeout (to be specified).
- A new reservation overwrites the previous one and invalidates the verification code.
- Reservations and invitations can run in parallel.
- Accpeted invitations also confirm ongoing reservations.
- Confirming a reservation doesn't accept invitations.
- After abandoning an e-mail address it can be reserved/invited for a new user with a new user-id.
- If in the "abandoned" state it can be restored for the current user.

### Events

Reservation:

- `Reserved`
  - `NormalizedEmailAddress`
  - `EmailAddress`
  - `ReservedAt`
  - `ForUserId`
  - `VerificationCode`
- `ReservationConfirmed`
  - `NormalizedEmailAddress`
  - `ConfirmedAt`
  - `VerificationCode`
  - `UserDisplayName`
- `ReservationExpired`
  - `NormalizedEmailAddress`
  - `ExpiredAt`
  - `VerificationCode`

Invitation:

- `InvitationSubmitted`
  - `NormalizedEmailAddress`
  - `EmailAddress`
  - `SubmittedAt`
  - `SubmittedByUserId`
  - `ForUserId`
  - `VerificationCode`
  - `TenantId`
  - `Role`
- `InvitationRevoked`
  - `NormalizedEmailAddress`
  - `RevokedAt`
  - `RevokedByUserId`
  - `TenantId`
- `InvitationAccpeted`
  - `NormalizedEmailAddress`
  - `AcceptedAt`
  - `AcceptedByUserId`
  - `VerificationCode`
- `InvitationExpired`
  - `NormalizedEmailAddress`
  - `ExpiredAt`
  - `VerificationCode`

Other:

- `Abandoned`
  - `NormalizedEmailAddress`
  - `AbandonedAt`
  - `AbandonedByUserId`
- `Restored`
  - `NormalizedEmailAddress`
  - `RestoredAt`
  - `RestoredByUserId`

## `User`

### Description

- Signing in can be done via "code by email" (later with passkey).
- For each sign-in a unique `SessionId` is generated and stored in the auth token.
- Accessing with an auth-token containing a "signed-out" session-id needs reauthentication.
- Each user has one role per invited tenant.
- Profile (with avatar/picture)?

### Events

- `Registered`
  - `UserId`
  - `RegisteredAt`
  - `EmailAddress`
  - `DisplayName`
- `DisplayNameChanged`
  - `UserId`
  - `ChangedAt`
  - `ChangedByUserId`
  - `DisplayName`
- `EmailAddressChangeConfirmationSent`
  - `UserId`
  - `SentAt`
  - `SentByUserId`
  - `NewEmailAddress`
  - `VerificationCode`
- `EmailAddressChangeConfirmed`
  - `UserId`
  - `ComfirmedAt`
  - `VerificationCode`
- `EmailAddressChangeExpired`
  - `UserId`
  - `ExpiredAt`
  - `VerificationCode`
- `SignInCodeSent`
  - `UserId`
  - `SentAt`
  - `EmailAddress`
  - `VerificationCode`
- `SignInCodeConfirmed`
  - `UserId`
  - `ComfirmedAt`
  - `VerificationCode`
  - `SessionId`
  - `RemoteInfo`? (like IP address?)
- `SignInCodeExpired`
  - `UserId`
  - `ExpiredAt`
  - `VerificationCode`
- `SignedOut`
  - `UserId`
  - `SignedOutAt`
  - `SignedOutByUserId`
  - `SessionId`
- `MembershipStarted`
  - `UserId`
  - `StartedAt`
  - `StartedByUserId`
  - `TenantId`
  - `Role`
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
- `PasskeySignedIn`
  - `UserId`
  - `SignedInAt`
  - `CredentialId`
  - `Data`? (what is needed?)
  - `SessionId`
  - `RemoteInfo`? (like IP address?)

## `Tenant`

### Description

A tenant is a logical unit of users who belong together like an "organization" or "workspace".

- Tenants are registered by users.
- Tenants have to be activated, e.g. by payment or "demo".
- Activations expire automatically and must be renewed.
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
  - `ActivationExpiresAt`
- `Deactivated`
  - `TenantId`
  - `DeactivatedAt`
  - `DeactivatedByUserId`
- `Terminated`
  - `TenantId`
  - `TerminatedAt`
  - `TerminatedByUserId`
