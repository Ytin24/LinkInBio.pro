module LinkInBio.Backend.Services.AnalyticsService

open System
open LinkInBio.Backend.Models.Domain
open LinkInBio.Backend.Models.DTOs
open LinkInBio.Backend.Database.DbContext
open LinkInBio.Backend.Database.DbContext

// Track link click
let trackClick (request: ClickEventRequest) (profileId: Guid) =
    async {
        let clickEvent = {
            Id = Guid.NewGuid()
            LinkId = request.LinkId
            ProfileId = profileId
            ClickedAt = DateTime.UtcNow
            IpAddress = request.IpAddress
            UserAgent = request.UserAgent
            Referrer = request.Referrer
            Country = None // TODO: Implement geo-IP lookup
            City = None
        }

        try
            let! _ = ClickEvents.create clickEvent
            return Ok ()
        with
        | ex -> return Error $"Failed to track click: {ex.Message}"
    }

// Get analytics for a profile
let getAnalytics (profileId: Guid) (dateRange: DateRangeQuery option) =
    async {
        let startDate = dateRange |> Option.bind (fun dr -> dr.StartDate)
        let endDate = dateRange |> Option.bind (fun dr -> dr.EndDate)

        try
            let! totalClicks, clicksByLink = ClickEvents.getAnalytics profileId startDate endDate

            let clicksByLinkMap =
                clicksByLink
                |> List.fold (fun acc (linkId, count) -> Map.add linkId count acc) Map.empty

            // TODO: Implement more detailed analytics
            // - Clicks by date
            // - Top countries
            // - Conversion rates

            let response = {
                TotalViews = 0L // TODO: Implement profile views tracking
                TotalClicks = totalClicks
                ClicksByLink = clicksByLinkMap
                ClicksByDate = Map.empty // TODO: Implement
                TopCountries = [] // TODO: Implement
            }

            return Ok response
        with
        | ex -> return Error $"Failed to get analytics: {ex.Message}"
    }

// Calculate conversion rate
let calculateConversionRate (views: int64) (clicks: int64) : float =
    if views = 0L then 0.0
    else (float clicks / float views) * 100.0

// Get top performing links
let getTopLinks (profileId: Guid) (limit: int) =
    async {
        try
            let! _, clicksByLink = ClickEvents.getAnalytics profileId None None

            let topLinks =
                clicksByLink
                |> List.sortByDescending snd
                |> List.truncate limit

            return Ok topLinks
        with
        | ex -> return Error $"Failed to get top links: {ex.Message}"
    }

// Export analytics to CSV format
let exportToCSV (profileId: Guid) (dateRange: DateRangeQuery option) =
    async {
        let! analyticsResult = getAnalytics profileId dateRange

        match analyticsResult with
        | Error msg -> return Error msg
        | Ok analytics ->
            let csvHeader = "Link ID,Clicks\n"
            let csvRows =
                analytics.ClicksByLink
                |> Map.toList
                |> List.map (fun (linkId, clicks) -> $"{linkId},{clicks}")
                |> String.concat "\n"

            return Ok (csvHeader + csvRows)
    }

// A/B Testing Analytics
module ABTesting =

    // Calculate statistical significance (simple chi-square test)
    let calculateSignificance (variantAClicks: int64) (variantAViews: int64)
                               (variantBClicks: int64) (variantBViews: int64) : float =
        // Simplified chi-square calculation
        let totalClicks = float (variantAClicks + variantBClicks)
        let totalViews = float (variantAViews + variantBViews)

        if totalViews = 0.0 then 0.0
        else
            let expectedA = (float variantAViews) * (totalClicks / totalViews)
            let expectedB = (float variantBViews) * (totalClicks / totalViews)

            let chiSquare =
                ((float variantAClicks - expectedA) ** 2.0) / expectedA +
                ((float variantBClicks - expectedB) ** 2.0) / expectedB

            chiSquare

    // Determine winner (p-value < 0.05 is statistically significant)
    let determineWinner (variantAResult: ABTestResult) (variantBResult: ABTestResult) : string option =
        let chiSquare = calculateSignificance
                            variantAResult.Clicks variantAResult.Views
                            variantBResult.Clicks variantBResult.Views

        // Chi-square critical value for p=0.05 is ~3.84
        if chiSquare < 3.84 then None
        else if variantAResult.ConversionRate > variantBResult.ConversionRate then Some "A"
        else Some "B"
