# Muzik 67 — .NET Architecture, Tutor Lab Review & Node.js Developer Guide

> **Target Audience**: Written specifically for developers familiar with **Node.js, Hono / Express, TypeScript, and modern ORMs (Prisma / Drizzle)** who want to understand ASP.NET Core Razor Pages, simplify the codebase, and match their tutor's exact marking preferences.

---

## 1. The Rosetta Stone: Node.js / Hono / Prisma ➡️ .NET C#

Coming from Node.js / Hono / TypeScript, C# syntax looks heavy and funny at first (`public async Task<IActionResult>`), but it is doing the **exact same thing** you already do in JavaScript!

| Node.js / Hono / TypeScript | ASP.NET Core Razor Pages (C#) | What It Actually Is |
| :--- | :--- | :--- |
| `Promise<T>` | `Task<T>` | A promise of a value that will resolve in the future. |
| `async (): Promise<Response>` | `public async Task<IActionResult>` | An asynchronous handler returning an HTTP response. |
| `app.get('/path', async (c) => ...)` | `public async Task<IActionResult> OnGetAsync()` | Handles incoming **HTTP GET** requests. |
| `app.post('/path', async (c) => ...)` | `public async Task<IActionResult> OnPostAsync()` | Handles incoming **HTTP POST** form submissions. |
| `const body = await c.req.parseBody()` | `[BindProperty] public InputModel Input { get; set; }` | Automatic request body parsing / form binding. |
| `c.redirect('/dashboard')` | `return RedirectToPage("/Student/StudentDashboard");` | HTTP 302 Redirect. |
| `return c.html(<Page />)` | `return Page();` | Re-renders the current Razor Page HTML. |
| `c.get('session').userId` / `req.session` | `HttpContext.Session.GetInt32("UserId")` | In-memory / Cookie-backed server session. |
| `prisma.user.findMany({ where: ... })` | `await _context.Users.Where(...).ToListAsync()` | Querying the database with an ORM. |
| `prisma.user.create({ data: ... })` | `_context.Users.Add(user); await _context.SaveChangesAsync();` | Inserting a new row into the database. |
| `zod.object({ email: z.string().email() })` | `[Required] [EmailAddress] public string Email { get; set; }` | Model validation attributes. |

### Why is the syntax `public async Task<IActionResult>` so weird compared to JS?
In JavaScript:
```typescript
async function login(req, res): Promise<Response> { ... }
```
In C#:
- C# requires the return type to be placed **before** the function name.
- So `Promise<Response>` becomes `Task<IActionResult>`.
- `public` declares that ASP.NET Core's routing engine is allowed to call it from outside.
- Put together:
```csharp
public async Task<IActionResult> OnPostAsync()
```
It is literally just: *"An async function called `OnPost` that returns a `Promise` of an HTTP Response!"*

---

## 2. Review of the Tutor's Lab Exercises (`Wappp.zip`)

Reviewing your tutor's complete 12-lab curriculum reveals the **exact mental model and coding patterns** she expects:

### Lab Summary & Focus Areas:
- **Lab 1 & 2**: Project structure (`Program.cs`, `Pages/`, `_Layout.cshtml`, `_ViewStart.cshtml`).
- **Lab 3**: Razor syntax (`@if`, `@foreach`, `@Model`, inline vs external CSS).
- **Lab 4 (Working with Forms and PageModel)**:
  - Form controls mapped via `asp-for="PropertyName"`.
  - Handling `OnGet()` for initial render, and `OnPost()` for submissions.
  - Using `[BindProperty]` to capture form inputs.
- **Lab 5 & 6**: Database queries and CRUD operations.
- **Lab 7 (Authentication & Session)**:
  - Password hashing.
  - Storing user ID in session: `HttpContext.Session.SetInt32("UserId", user.Id);`.
  - Protecting pages with a simple guard clause:
    ```csharp
    int? userId = HttpContext.Session.GetInt32("UserId");
    if (userId == null)
    {
        return RedirectToPage("/Login");
    }
    ```
- **Lab 8 (Validation & Error Handling)**:
  - Using data annotations: `[Required]`, `[StringLength]`, `[EmailAddress]`.
  - Checking `if (!ModelState.IsValid) return Page();`.
  - `<div asp-validation-summary="ModelOnly"></div>` and `<span asp-validation-for="...">`.
- **Lab 9**: AJAX, JSON responses, and live updates.
- **Lab 10 & 11**: Logging, configuration (`appsettings.json`), and file management.
- **Lab 12 (Entity Framework Core & LINQ)**:
  - Defining entity classes (`User`, `Game`, etc.).
  - Creating a `DbContext` class with `DbSet<T>`.
  - Injecting `_context` via constructor.
  - Using direct LINQ queries:
    - `_context.Items.ToListAsync()`
    - `_context.Items.FirstOrDefaultAsync(x => x.Id == id)`
    - `_context.Items.Add(newItem)`
    - `await _context.SaveChangesAsync()`

### Key Takeaway for Our Project Refactoring:
Your tutor **heavily prefers clean, simple, direct Razor Pages code**:
1. Inject `MuzikDbContext _context` directly into the `PageModel`.
2. Check `HttpContext.Session.GetInt32("UserId")` at the start of protected pages.
3. Use standard LINQ queries directly in `OnGetAsync()` and `OnPostAsync()`.
4. Avoid overly complex abstraction layers (like separate custom repository wrappers) that hide what's happening.

---

## 3. Database & ERD: The Many-to-Many Relationships

In relational database design, you cannot directly connect two tables with a many-to-many relationship without a **junction (bridge) table**.

In this project, there are **two Many-to-Many relationships**:

```
┌─────────┐                ┌──────────────────┐                ┌──────────┐
│  User   │ 1 ────────── * │   GameProgress   │ * ────────── 1 │   Game   │
└─────────┘                └──────────────────┘                └──────────┘
 (Student)                    (Junction Table)                 (Music Game)
                               • ProgressId (PK)
                               • UserId (FK)
                               • GameId (FK)
                               • Score
                               • Attempts
                               • Status
                               • DateOfCompletion
```
- **Why M2M?** A student plays many different games; a game is played by many students.

```
┌─────────┐                ┌──────────────────┐                ┌──────────────────┐
│  User   │ 1 ────────── * │ UserSubscription │ * ────────── 1 │ SubscriptionPlan │
└─────────┘                └──────────────────┘                └──────────────────┘
 (Student)                    (Junction Table)                 (Free, Monthly, etc.)
                               • SubscriptionId (PK)
                               • UserId (FK)
                               • PlanId (FK)
                               • StartDate
                               • EndDate
                               • Status
```
- **Why M2M?** A student can subscribe to multiple/different plans over time; each subscription plan has many enrolled students.

---

## 4. Standard Page Template (To Keep All Code Consistent)

To make every file in the project clean, readable, and easy to explain during presentations, every Razor Page will follow this exact template:

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Muzik67.Data;
using Muzik67.Models;

namespace Muzik67.Pages.Student;

public class SamplePageModel : PageModel
{
    private readonly MuzikDbContext _context;

    // 1. Dependency Injection (Constructor)
    public SamplePageModel(MuzikDbContext context)
    {
        _context = context;
    }

    // 2. Data to display on the HTML page
    public List<Game> GamesList { get; set; } = new();

    // 3. Form input binding (if page has a form)
    [BindProperty]
    public string FormField { get; set; } = string.Empty;

    // 4. HTTP GET Handler (Page Load)
    public async Task<IActionResult> OnGetAsync()
    {
        // Simple auth guard (Tutor's preferred style from Lab 7)
        int? userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
        {
            return RedirectToPage("/Account/Login");
        }

        // Fetch data using EF Core LINQ (Tutor's Lab 12 style)
        GamesList = await _context.Games
            .Where(g => g.Status == "Active")
            .ToListAsync();

        return Page();
    }

    // 5. HTTP POST Handler (Form Submission)
    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page(); // Show validation errors
        }

        // Save data to database
        // await _context.SaveChangesAsync();

        return RedirectToPage("/Student/StudentDashboard");
    }
}
```

---

## 5. Summary of Next Steps
1. **Simplify**: Strip out any overly convoluted boilerplate, unifying all PageModels with the clean template above.
2. **Build Your 2 Modules**:
   - `Student/GameDetail.cshtml` (the interactive question & music audio game screen).
   - `Admin/UserProgress.cshtml` (admin table to monitor all students' game scores).
3. **Harmonize UI**: Apply consistent Bootstrap pills, font hierarchy, and card shadows across all views.
