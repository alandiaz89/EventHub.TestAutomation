---
name: new-locator-object
description: Create an elements class (holds all element locators for one page or component) in EventHub.UiTests/Elements. Use when adding locators for a new page/component, or when locators are inline in a page class and should be extracted. For a full new page, use new-page-object.
argument-hint: <PageName> [url-or-description-of-the-page]
---

# New elements class

An elements class owns the **where** (locators) for one page or component. The page class (see `new-page-object`) owns the **what** (actions) and receives the elements class through its constructor.

## Steps

1. **Read first.** Look at `EventHub.UiTests/Elements/` and follow existing naming. Reuse or extend an existing class for the same page or component. Do not duplicate.
2. **Inspect the live page** (Playwright MCP or a headless browser) for real labels, roles and test ids. The app may be client-rendered, so a plain HTTP fetch shows nothing. Check that each locator matches exactly one element.
3. **Create** `EventHub.UiTests/Elements/<PageName>Elements.cs`.
4. **Wire it up** by adding one property to `ElementFixture` (`public <PageName>Elements <PageName>Elements { get; } = new(page);`), then pass `elements.<PageName>Elements` to the page class in `PageFixture`. Do not create elements classes anywhere else.
5. **Build** with `dotnet build EventHub.UiTests` and report the result.

## Template

```csharp
using Microsoft.Playwright;

namespace EventHub.UiTests.Elements;

public sealed class LoginElements(IPage page)
{
    public ILocator EmailInput => page.GetByLabel("Email");
    public ILocator PasswordInput => page.GetByLabel("Password");
    public ILocator SignInButton => page.GetByRole(AriaRole.Button, new() { Name = "Sign In" });
}
```

## Rules

- **Namespace** `EventHub.UiTests.Elements`; class name `<PageName>Elements`; primary constructor taking `IPage` (or an `ILocator` root for a component scoped to a container).
- **Expression-bodied `ILocator` properties.** They resolve lazily at use time. Do not cache locators or use element handles.
- **Locator priority:** `GetByRole` (with `Name`) > `GetByLabel` > `GetByTestId` > `GetByPlaceholder` > `GetByText` > CSS. No XPath. If CSS is unavoidable, add a one-line comment explaining why.
- **Names describe the element:** `SignInButton`, not `BtnPrimary` or `Div3`.
- **No logic, actions, assertions or waits.** Only locator definitions. Dynamic locators are methods returning `ILocator`, e.g. `EventCard(string title)`.
- **Scope repeated structures** with a root locator and chain from it.
- **No hardcoded URLs or environment values.**
- **One class per page or component.** Shared parts (navbar, modal, toast) get their own elements class.
