---
name: new-page-object
description: Scaffold a new page action class (plus its elements class, PageFixture registration and, optionally, a test) in EventHub.UiTests following the project's elements/actions structure. Use when adding a new page or UI component to the UI test suite.
argument-hint: <PageName> [url-path] [actions to expose]
---

# New page object

The UI layer is split in two, joined by composition:

- **Elements** (`Elements/<PageName>Elements.cs`) own the *where*: the locators for one page or component. They take `IPage`.
- **Pages** (`Pages/<PageName>Page.cs`) own the *what*: user actions. They take the elements class they interact with as a constructor parameter. They hold **no assertions**.
- **`ElementFixture`** (`Elements/ElementFixture.cs`) takes `IPage` and exposes one instance of every elements class. It is the only place elements classes are created.
- **`PageFixture`** (`Pages/PageFixture.cs`) takes the `ElementFixture` and exposes the page objects, each built from its elements (`new(elements.LoginElements)`), and nothing else. It does not inherit anything.
- **`PlaywrightHooks`** creates the `IPage` in `[SetUp]`, then `Elements = new ElementFixture(Page)` and `Pages = new PageFixture(Elements)`, both `protected`. Test classes inherit `PlaywrightHooks`, call actions through `Pages.<PageName>` and assert on `Elements.<PageName>Elements`.

## Steps

1. **Read first.** Check `Elements/`, `Pages/`, `Hooks/PlaywrightHooks.cs` and an existing test to match conventions. Look for an existing page or component that already covers this page.
2. **Create the elements class** `Elements/<PageName>Elements.cs`: a primary constructor taking `IPage`, with one expression-bodied `ILocator` property per element. Prefer `GetByRole`, `GetByLabel`, `GetByTestId`, `GetByText` over CSS, and never use XPath. Inspect the live page for the real labels and roles instead of guessing.
3. **Create the page class** `Pages/<PageName>Page.cs` from the template below, taking `<PageName>Elements` in its constructor.
4. **Register the elements in `ElementFixture`** with one property: `public <PageName>Elements <PageName>Elements { get; } = new(page);`. Create `ElementFixture` (primary constructor taking `IPage`) if it doesn't exist.
5. **Register the page in `PageFixture`** with one property built from those elements:
   ```csharp
   public <PageName>Page <PageName>Page { get; } = new(elements.<PageName>Elements);
   ```
   Both fixtures are created per test by `PlaywrightHooks`, so each test gets its own objects bound to its own `IPage`. Create `PageFixture` (primary constructor taking `ElementFixture`) if it doesn't exist.
6. **Add a test only if asked**, in `Tests/`, deriving from `PlaywrightHooks`. Actions come from `Pages.<PageName>Page` (`await Pages.LoginPage.Login(...)`); assertions use `Elements.<PageName>Elements` (`await Expect(Elements.HeaderElements.UserEmail).ToHaveTextAsync(...)`).
7. **Run `dotnet build EventHub.UiTests`**. Run the test with `dotnet test` only if one was added and the target environment is reachable. State plainly what you ran and what you didn't.

## Template

```csharp
using EventHub.UiTests.Elements;

namespace EventHub.UiTests.Pages;

public sealed class LoginPage(LoginElements elements)
{
    public async Task Login(string email, string password)
    {
        await elements.EmailInput.FillAsync(email);
        await elements.PasswordInput.FillAsync(password);
        await elements.SignInButton.ClickAsync();
    }

    public async Task ClickRegisterLink() => await elements.RegisterLink.ClickAsync();
}
```

Elements class and fixtures:

```csharp
namespace EventHub.UiTests.Elements;

public sealed class LoginElements(IPage page)
{
    public ILocator EmailInput => page.GetByLabel("Email");
    public ILocator PasswordInput => page.GetByLabel("Password");
    public ILocator SignInButton => page.GetByRole(AriaRole.Button, new() { Name = "Sign In" });
}
```

```csharp
namespace EventHub.UiTests.Elements;

public sealed class ElementFixture(IPage page)
{
    public LoginElements LoginElements { get; } = new(page);
}
```

```csharp
namespace EventHub.UiTests.Pages;

public sealed class PageFixture(ElementFixture elements)
{
    public LoginPage LoginPage { get; } = new(elements.LoginElements);
}
```

## Rules

- **Naming:** `<PageName>Page` in `EventHub.UiTests.Pages`, `<PageName>Elements` in `EventHub.UiTests.Elements`. Primary constructors.
- **Method names:** no `Async` suffix on page or component methods, even though they return `Task` (write `Login`, not `LoginAsync`). A method that does one interaction is named for that interaction, with `Click` rather than `Open` or `Go`: `ClickEvents`, `ClickLogout`, `ClickRegisterLink`. A method that performs several steps is named for its intent: `Login`, `Register`, `CreateEvent`. Playwright's own calls inside the method (`ClickAsync`, `FillAsync`) keep their names.
- **Seal the classes.** Page, elements and `PageFixture` classes are `public sealed`. Nothing inherits from them. `PlaywrightHooks` stays abstract.
- **No assertions in page classes.** No `Expect(...)`, no `Assert`, no `Expect...Async` helpers. A page class only performs actions. Tests assert.
- **Page classes only use the elements passed to them.** They never build locators, never call `GetBy...`, and never use raw selectors.
- **`PageFixture` only builds and exposes page objects.** No browser lifecycle, no config, no elements properties, no inheritance. The lifecycle stays in `PlaywrightHooks`.
- **`ElementFixture` only builds and exposes elements classes.** It is the only place they are created, so a page and a test assertion share the same instance.
- **No inheritance chains and no extra abstractions.** No `BasePage`, no DI container. Link classes by constructor parameters.
- **Multi-step flows use intent-level names** (`Login`, `CreateEvent`), not a list of clicks. Single interactions use `Click<Element>` (see Method names).
- **Actions return `Task`; the test decides where it landed.** Page and component methods do not return the next page. A flow can end in more than one place (`Login` can reach the home page or show an error), and a page class only receives its own elements, so it cannot build another page. After a navigating action the test asserts on the destination through `Elements` (for example `Expect(Elements.HomeElements.Heading)`) and uses `Pages.<NextPage>` for the next actions. Return a page object only when the destination is certain and the extra elements class is worth injecting; in that case take the next page's elements as an additional constructor parameter, have `PageFixture` pass it in from `ElementFixture`, and record the exception here.
- **Waiting:** rely on Playwright auto-waiting. No `Thread.Sleep`, `Task.Delay` or `WaitForTimeoutAsync`.
- **No hardcoded environment values.** The base URL comes from config via `PlaywrightHooks`. If a page action genuinely needs the `IPage` (for example to navigate), add it as a second constructor parameter for that page only.
- **No state.** Page and elements classes hold only what their constructors receive. No static fields, no cached data, no test data.
- **Reuse components.** Parts shared across pages (navbar, modal, toast) get their own elements class and, if they have actions, their own action class. Never copy members between pages.
- **Keep it small.** Add only the elements and actions the tests need now.
