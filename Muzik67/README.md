# Muzik 67 module integration

The profile, admin dashboard and subscription-management pages use the SQLite database
and Entity Framework Core models already initialized by the group. This project change
does not add a separate schema or SQL setup script.

## Database connection

The existing `DefaultConnection` setting points to `Muzik67.db`. The app creates and
seeds that SQLite database at startup through `DbInitializer`; no MySQL server or
connection string is required.

## Login hand-off

The database-backed login sets the session `UserId` and `Role`. Profile pages require
a signed-in session. Admin pages also require `Role` to be `"Admin"`; other users are
sent to the access-denied page. Logging out clears the session.

The seeded demo accounts are `admin` / `Admin@123` and `student1` / `Student@123`.
Choose the matching account role on the login form.

## Included module operations

- **User profile:** follows the member/admin profile wireframes. Users can review their
  account details, then choose Edit, Save or Cancel; only username and email can be
  changed.
- **Admin dashboard:** provides an admin-only entry point to the user and content
  management sections, following the project wireframe. Subscription management is
  available from the User Management card.
- **Subscription management:** lists plans and the latest 100 member subscriptions;
  admins can create and edit plans in a form dialog. The page uses the User Management
  side navigation from the wireframe. Plans with subscription records cannot be
  deleted, preserving their history even though the database relationship cascades.

The subscription module includes `INSERT` because creating plans is part of its
description and the lecturer's feedback asks each member to handle CRUD. Data access
uses the shared EF Core `MuzikDbContext` and SQLite provider.
