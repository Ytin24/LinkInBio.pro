module LinkInBio.Backend.Database.DbContext

open System
open Npgsql.FSharp
open LinkInBio.Backend.Models.Domain

// Connection string helper
let getConnectionString () =
    Environment.GetEnvironmentVariable("DATABASE_URL")
    |> Option.ofObj
    |> Option.defaultValue "Host=localhost;Database=linkinbio;Username=postgres;Password=postgres"

// Database operations for Users
module Users =
    let create (user: User) =
        getConnectionString()
        |> Sql.connect
        |> Sql.query """
            INSERT INTO users (id, email, password_hash, username, created_at, subscription_tier)
            VALUES (@id, @email, @passwordHash, @username, @createdAt, @subscriptionTier)
        """
        |> Sql.parameters [
            "@id", Sql.uuid user.Id
            "@email", Sql.string user.Email
            "@passwordHash", Sql.string user.PasswordHash
            "@username", Sql.string user.Username
            "@createdAt", Sql.timestamp user.CreatedAt
            "@subscriptionTier", Sql.string (string user.SubscriptionTier)
        ]
        |> Sql.executeNonQueryAsync

    let findByEmail (email: string) =
        getConnectionString()
        |> Sql.connect
        |> Sql.query "SELECT * FROM users WHERE email = @email"
        |> Sql.parameters [ "@email", Sql.string email ]
        |> Sql.executeAsync (fun read ->
            {
                Id = read.uuid "id"
                Email = read.string "email"
                PasswordHash = read.string "password_hash"
                Username = read.string "username"
                CreatedAt = read.dateTime "created_at"
                SubscriptionTier =
                    match read.string "subscription_tier" with
                    | "Free" -> Free
                    | "Basic" -> Basic
                    | "Premium" -> Premium
                    | _ -> Free
            })

    let findById (userId: Guid) =
        getConnectionString()
        |> Sql.connect
        |> Sql.query "SELECT * FROM users WHERE id = @id"
        |> Sql.parameters [ "@id", Sql.uuid userId ]
        |> Sql.executeAsync (fun read ->
            {
                Id = read.uuid "id"
                Email = read.string "email"
                PasswordHash = read.string "password_hash"
                Username = read.string "username"
                CreatedAt = read.dateTime "created_at"
                SubscriptionTier =
                    match read.string "subscription_tier" with
                    | "Free" -> Free
                    | "Basic" -> Basic
                    | "Premium" -> Premium
                    | _ -> Free
            })

// Database operations for Profiles
module Profiles =
    let create (profile: Profile) =
        getConnectionString()
        |> Sql.connect
        |> Sql.query """
            INSERT INTO profiles
            (id, user_id, slug, display_name, bio, avatar_url, theme, is_published, created_at, updated_at)
            VALUES
            (@id, @userId, @slug, @displayName, @bio, @avatarUrl, @theme, @isPublished, @createdAt, @updatedAt)
        """
        |> Sql.parameters [
            "@id", Sql.uuid profile.Id
            "@userId", Sql.uuid profile.UserId
            "@slug", Sql.string profile.Slug
            "@displayName", Sql.string profile.DisplayName
            "@bio", Sql.stringOrNone profile.Bio
            "@avatarUrl", Sql.stringOrNone profile.AvatarUrl
            "@theme", Sql.jsonb profile.Theme
            "@isPublished", Sql.bool profile.IsPublished
            "@createdAt", Sql.timestamp profile.CreatedAt
            "@updatedAt", Sql.timestamp profile.UpdatedAt
        ]
        |> Sql.executeNonQueryAsync

    let findBySlug (slug: string) =
        getConnectionString()
        |> Sql.connect
        |> Sql.query "SELECT * FROM profiles WHERE slug = @slug"
        |> Sql.parameters [ "@slug", Sql.string slug ]
        |> Sql.executeAsync (fun read ->
            {
                Id = read.uuid "id"
                UserId = read.uuid "user_id"
                Slug = read.string "slug"
                DisplayName = read.string "display_name"
                Bio = read.stringOrNone "bio"
                AvatarUrl = read.stringOrNone "avatar_url"
                Theme = read.jsonb<Theme> "theme"
                IsPublished = read.bool "is_published"
                CreatedAt = read.dateTime "created_at"
                UpdatedAt = read.dateTime "updated_at"
            })

    let findByUserId (userId: Guid) =
        getConnectionString()
        |> Sql.connect
        |> Sql.query "SELECT * FROM profiles WHERE user_id = @userId"
        |> Sql.parameters [ "@userId", Sql.uuid userId ]
        |> Sql.executeAsync (fun read ->
            {
                Id = read.uuid "id"
                UserId = read.uuid "user_id"
                Slug = read.string "slug"
                DisplayName = read.string "display_name"
                Bio = read.stringOrNone "bio"
                AvatarUrl = read.stringOrNone "avatar_url"
                Theme = read.jsonb<Theme> "theme"
                IsPublished = read.bool "is_published"
                CreatedAt = read.dateTime "created_at"
                UpdatedAt = read.dateTime "updated_at"
            })

    let update (profile: Profile) =
        getConnectionString()
        |> Sql.connect
        |> Sql.query """
            UPDATE profiles
            SET display_name = @displayName,
                bio = @bio,
                avatar_url = @avatarUrl,
                theme = @theme,
                is_published = @isPublished,
                updated_at = @updatedAt
            WHERE id = @id
        """
        |> Sql.parameters [
            "@id", Sql.uuid profile.Id
            "@displayName", Sql.string profile.DisplayName
            "@bio", Sql.stringOrNone profile.Bio
            "@avatarUrl", Sql.stringOrNone profile.AvatarUrl
            "@theme", Sql.jsonb profile.Theme
            "@isPublished", Sql.bool profile.IsPublished
            "@updatedAt", Sql.timestamp profile.UpdatedAt
        ]
        |> Sql.executeNonQueryAsync

// Database operations for Links
module Links =
    let create (link: Link) =
        getConnectionString()
        |> Sql.connect
        |> Sql.query """
            INSERT INTO links
            (id, profile_id, title, url, icon_url, position, is_active, created_at)
            VALUES
            (@id, @profileId, @title, @url, @iconUrl, @position, @isActive, @createdAt)
        """
        |> Sql.parameters [
            "@id", Sql.uuid link.Id
            "@profileId", Sql.uuid link.ProfileId
            "@title", Sql.string link.Title
            "@url", Sql.string link.Url
            "@iconUrl", Sql.stringOrNone link.IconUrl
            "@position", Sql.int link.Position
            "@isActive", Sql.bool link.IsActive
            "@createdAt", Sql.timestamp link.CreatedAt
        ]
        |> Sql.executeNonQueryAsync

    let findByProfileId (profileId: Guid) =
        getConnectionString()
        |> Sql.connect
        |> Sql.query "SELECT * FROM links WHERE profile_id = @profileId ORDER BY position"
        |> Sql.parameters [ "@profileId", Sql.uuid profileId ]
        |> Sql.executeAsync (fun read ->
            {
                Id = read.uuid "id"
                ProfileId = read.uuid "profile_id"
                Title = read.string "title"
                Url = read.string "url"
                IconUrl = read.stringOrNone "icon_url"
                Position = read.int "position"
                IsActive = read.bool "is_active"
                CreatedAt = read.dateTime "created_at"
            })

    let update (link: Link) =
        getConnectionString()
        |> Sql.connect
        |> Sql.query """
            UPDATE links
            SET title = @title,
                url = @url,
                icon_url = @iconUrl,
                position = @position,
                is_active = @isActive
            WHERE id = @id
        """
        |> Sql.parameters [
            "@id", Sql.uuid link.Id
            "@title", Sql.string link.Title
            "@url", Sql.string link.Url
            "@iconUrl", Sql.stringOrNone link.IconUrl
            "@position", Sql.int link.Position
            "@isActive", Sql.bool link.IsActive
        ]
        |> Sql.executeNonQueryAsync

    let delete (linkId: Guid) =
        getConnectionString()
        |> Sql.connect
        |> Sql.query "DELETE FROM links WHERE id = @id"
        |> Sql.parameters [ "@id", Sql.uuid linkId ]
        |> Sql.executeNonQueryAsync

// Database operations for Click Analytics
module ClickEvents =
    let create (event: ClickEvent) =
        getConnectionString()
        |> Sql.connect
        |> Sql.query """
            INSERT INTO click_events
            (id, link_id, profile_id, clicked_at, ip_address, user_agent, referrer, country, city)
            VALUES
            (@id, @linkId, @profileId, @clickedAt, @ipAddress, @userAgent, @referrer, @country, @city)
        """
        |> Sql.parameters [
            "@id", Sql.uuid event.Id
            "@linkId", Sql.uuid event.LinkId
            "@profileId", Sql.uuid event.ProfileId
            "@clickedAt", Sql.timestamp event.ClickedAt
            "@ipAddress", Sql.stringOrNone event.IpAddress
            "@userAgent", Sql.stringOrNone event.UserAgent
            "@referrer", Sql.stringOrNone event.Referrer
            "@country", Sql.stringOrNone event.Country
            "@city", Sql.stringOrNone event.City
        ]
        |> Sql.executeNonQueryAsync

    let getAnalytics (profileId: Guid) (startDate: DateTime option) (endDate: DateTime option) =
        async {
            let! totalClicks =
                getConnectionString()
                |> Sql.connect
                |> Sql.query """
                    SELECT COUNT(*) as count
                    FROM click_events
                    WHERE profile_id = @profileId
                    AND (@startDate IS NULL OR clicked_at >= @startDate)
                    AND (@endDate IS NULL OR clicked_at <= @endDate)
                """
                |> Sql.parameters [
                    "@profileId", Sql.uuid profileId
                    "@startDate", Sql.timestampOrNone startDate
                    "@endDate", Sql.timestampOrNone endDate
                ]
                |> Sql.executeRowAsync (fun read -> read.int64 "count")

            let! clicksByLink =
                getConnectionString()
                |> Sql.connect
                |> Sql.query """
                    SELECT link_id, COUNT(*) as count
                    FROM click_events
                    WHERE profile_id = @profileId
                    AND (@startDate IS NULL OR clicked_at >= @startDate)
                    AND (@endDate IS NULL OR clicked_at <= @endDate)
                    GROUP BY link_id
                """
                |> Sql.parameters [
                    "@profileId", Sql.uuid profileId
                    "@startDate", Sql.timestampOrNone startDate
                    "@endDate", Sql.timestampOrNone endDate
                ]
                |> Sql.executeAsync (fun read ->
                    read.uuid "link_id", read.int64 "count")

            return totalClicks, clicksByLink
        }
