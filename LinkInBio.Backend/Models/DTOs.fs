module LinkInBio.Backend.Models.DTOs

open System
open LinkInBio.Backend.Models.Domain

// Authentication DTOs
type RegisterRequest = {
    Email: string
    Password: string
    Username: string
}

type LoginRequest = {
    Email: string
    Password: string
}

type AuthResponse = {
    Token: string
    UserId: Guid
    Username: string
    Email: string
}

// Profile DTOs
type CreateProfileRequest = {
    Slug: string
    DisplayName: string
    Bio: string option
}

type UpdateProfileRequest = {
    DisplayName: string option
    Bio: string option
    AvatarUrl: string option
    Theme: Theme option
    IsPublished: bool option
}

type ProfileResponse = {
    Id: Guid
    Slug: string
    DisplayName: string
    Bio: string option
    AvatarUrl: string option
    Theme: Theme
    Links: LinkResponse list
    IsPublished: bool
}

// Link DTOs
and LinkResponse = {
    Id: Guid
    Title: string
    Url: string
    IconUrl: string option
    Position: int
    IsActive: bool
}

type CreateLinkRequest = {
    Title: string
    Url: string
    IconUrl: string option
    Position: int option
}

type UpdateLinkRequest = {
    Title: string option
    Url: string option
    IconUrl: string option
    Position: int option
    IsActive: bool option
}

// Analytics DTOs
type ClickEventRequest = {
    LinkId: Guid
    IpAddress: string option
    UserAgent: string option
    Referrer: string option
}

type AnalyticsResponse = {
    TotalViews: int64
    TotalClicks: int64
    ClicksByLink: Map<Guid, int64>
    ClicksByDate: Map<DateTime, int64>
    TopCountries: (string * int64) list
}

type DateRangeQuery = {
    StartDate: DateTime option
    EndDate: DateTime option
}

// A/B Testing DTOs
type CreateABTestRequest = {
    Name: string
    VariantATheme: Theme
    VariantBTheme: Theme
    Duration: int // days
}

type ABTestResponse = {
    Id: Guid
    Name: string
    IsActive: bool
    VariantAResults: ABTestResult
    VariantBResults: ABTestResult
    Winner: string option // "A", "B", or null if no clear winner
}
