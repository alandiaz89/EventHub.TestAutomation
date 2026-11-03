using Microsoft.Playwright;

namespace EventHub.UiTests.Elements;

public sealed class FooterElements(IPage page)
{
    private ILocator Root => page.GetByRole(AriaRole.Contentinfo);

    public ILocator Footer => Root;

    // Section headings
    public ILocator AcademyHeading => Root.GetByRole(AriaRole.Heading, new() { Name = "Rahul Shetty Academy" });
    public ILocator PopularCoursesHeading => Root.GetByRole(AriaRole.Heading, new() { Name = "Popular Courses" });
    public ILocator JobHiringHeading => Root.GetByRole(AriaRole.Heading, new() { Name = "QA Job Hiring Platform" });
    public ILocator PracticeAppHeading => Root.GetByRole(AriaRole.Heading, new() { Name = "EventHub Practice App" });

    // EventHub links
    public ILocator BrowseEventsLink => Root.GetByRole(AriaRole.Link, new() { Name = "Browse Events" });
    public ILocator MyBookingsLink => Root.GetByRole(AriaRole.Link, new() { Name = "My Bookings" });
    public ILocator ManageEventsLink => Root.GetByRole(AriaRole.Link, new() { Name = "Manage Events" });
    public ILocator ApiDocumentationLink => Root.GetByRole(AriaRole.Link, new() { Name = "API Documentation" });

    // External links
    public ILocator CourseLink(string courseName) => Root.GetByRole(AriaRole.Link, new() { Name = courseName });
    public ILocator AcademyWebsiteLink => Root.GetByRole(AriaRole.Link, new() { Name = "rahulshettyacademy.com →" });

    public ILocator Copyright => Root.GetByText("All rights reserved.");
}
