#!/bin/bash

# Fix AuthService.fs
cd /home/user/LinkInBio.pro/LinkInBio.Backend/Services
sed -i '9 a open LinkInBio.Backend.Database.DbContext' AuthService.fs
sed -i 's/Database\.DbContext\.Users/Users/g' AuthService.fs

# Fix AnalyticsService.fs
sed -i '6 a open LinkInBio.Backend.Database.DbContext' AnalyticsService.fs

# Fix ProfileHandlers.fs
cd /home/user/LinkInBio.pro/LinkInBio.Backend/Handlers
sed -i '6 a open LinkInBio.Backend.Database.DbContext' ProfileHandlers.fs

# Fix LinkHandlers.fs
sed -i '6 a open LinkInBio.Backend.Database.DbContext' LinkHandlers.fs

# Fix AnalyticsHandlers.fs
sed -i '6 a open LinkInBio.Backend.Database.DbContext' AnalyticsHandlers.fs
sed -i 's/Services\.AuthService/AuthService/g' AnalyticsHandlers.fs
sed -i 's/AnalyticsService\./Services.AnalyticsService./g' AnalyticsHandlers.fs

echo "✅ Fixed all imports"
