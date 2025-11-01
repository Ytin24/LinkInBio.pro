module LinkInBio.Backend.Models.Domain

open System

// Пользователь
type User = {
    Id: Guid
    Email: string
    PasswordHash: string
    Username: string
    CreatedAt: DateTime
    SubscriptionTier: SubscriptionTier
}

and SubscriptionTier =
    | Free
    | Basic
    | Premium

// Профиль пользователя (страница с ссылками)
type Profile = {
    Id: Guid
    UserId: Guid
    Slug: string // уникальный URL: linkinbio.pro/slug
    DisplayName: string
    Bio: string option
    AvatarUrl: string option
    Theme: Theme
    IsPublished: bool
    CreatedAt: DateTime
    UpdatedAt: DateTime
}

and Theme = {
    BackgroundColor: string
    TextColor: string
    ButtonStyle: ButtonStyle
    FontFamily: string
}

and ButtonStyle =
    | Rounded
    | Square
    | Pill

// Ссылка в профиле
type Link = {
    Id: Guid
    ProfileId: Guid
    Title: string
    Url: string
    IconUrl: string option
    Position: int
    IsActive: bool
    CreatedAt: DateTime
}

// Аналитика кликов
type ClickEvent = {
    Id: Guid
    LinkId: Guid
    ProfileId: Guid
    ClickedAt: DateTime
    IpAddress: string option
    UserAgent: string option
    Referrer: string option
    Country: string option
    City: string option
}

// A/B тестирование
type ABTest = {
    Id: Guid
    ProfileId: Guid
    Name: string
    VariantA: ProfileVariant
    VariantB: ProfileVariant
    IsActive: bool
    StartDate: DateTime
    EndDate: DateTime option
    CreatedAt: DateTime
}

and ProfileVariant = {
    Id: Guid
    Theme: Theme
    Links: Link list
}

type ABTestResult = {
    TestId: Guid
    VariantId: Guid
    Views: int64
    Clicks: int64
    ConversionRate: float
}
