# Changelog

All notable changes to LinkInBio.pro will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Planned
- A/B testing functionality UI
- Frontend (Next.js)
- Payment integration (Stripe, ЮKassa)
- Email notifications
- Custom domains support
- Geo-IP location detection
- Advanced analytics (UTM parameters)
- Admin dashboard
- Mobile application

## [0.1.0] - 2024-01-01

### Added
- Initial release of LinkInBio.pro API
- F# backend with Giraffe framework
- PostgreSQL database integration
- User authentication with JWT tokens
- Profile management (create, read, update)
- Link management (CRUD operations)
- Click analytics tracking
- Analytics export to CSV
- Top performing links endpoint
- Docker Compose configuration
- Comprehensive API documentation
- Test scripts for API validation
- Deployment guides for multiple platforms

### API Endpoints

#### Authentication
- `POST /api/auth/register` - Register new user
- `POST /api/auth/login` - Login user
- `GET /api/auth/me` - Get current user

#### Profiles
- `POST /api/profiles` - Create profile
- `GET /api/profiles/:slug` - Get public profile
- `GET /api/profiles/my` - Get user's profiles
- `PUT /api/profiles/:id` - Update profile

#### Links
- `POST /api/profiles/:id/links` - Create link
- `GET /api/profiles/:id/links` - Get profile links
- `PUT /api/links/:id` - Update link
- `DELETE /api/links/:id` - Delete link

#### Analytics
- `POST /api/analytics/click` - Track click event
- `GET /api/analytics/:id` - Get analytics
- `GET /api/analytics/:id/export` - Export to CSV
- `GET /api/analytics/:id/top-links` - Get top links

### Security
- BCrypt password hashing (11 rounds)
- JWT authentication with configurable expiration
- CORS support
- Input validation for all endpoints

### Database
- Users table with subscription tiers
- Profiles table with JSONB theme support
- Links table with position ordering
- Click events tracking table
- Profile views table
- A/B testing tables (structure ready)

### Documentation
- `README.md` - Main documentation
- `API_EXAMPLES.md` - cURL examples for all endpoints
- `DEPLOYMENT.md` - Deployment guides
- `CONTRIBUTING.md` - Contribution guidelines
- `CHANGELOG.md` - This file

### Infrastructure
- Docker Compose setup
- Dockerfile for backend
- PostgreSQL migrations
- Environment variables configuration
- Health check endpoint
- Test script with full API coverage

## [0.0.1] - 2024-XX-XX

### Added
- Project initialization
- Repository structure
- Basic project setup

---

## Version History

### Version 0.1.0 Features

**User Management**
- Registration with email validation
- Password hashing with BCrypt
- JWT token generation (7-day expiration)
- Subscription tiers: Free, Basic, Premium

**Profile Features**
- Unique slug-based URLs (e.g., linkinbio.pro/username)
- Customizable themes (colors, fonts, button styles)
- Profile bio and avatar
- Publish/unpublish functionality
- Multiple profiles per user

**Link Management**
- Unlimited links per profile
- Custom titles and URLs
- Optional icon images
- Position ordering
- Active/inactive toggle

**Analytics**
- Click tracking with metadata (IP, User-Agent, Referrer)
- Date range filtering
- Export to CSV format
- Top performing links
- Click counts by link
- Total views and clicks

**Technical Features**
- REST API architecture
- PostgreSQL database with JSONB support
- Async/await throughout
- Comprehensive error handling
- Request validation
- CORS configuration

---

## Migration Guide

### From 0.0.1 to 0.1.0

No migration needed - this is the initial release.

---

## Breaking Changes

None yet.

---

## Deprecations

None yet.

---

## Known Issues

### Version 0.1.0
- A/B testing functionality is in database schema but not yet implemented in API
- No rate limiting on API endpoints
- No email verification for registration
- No password reset functionality
- Analytics doesn't include geographic data (country/city tracking structure exists)
- No pagination on list endpoints
- No WebSocket support for real-time updates

---

## Roadmap

### v0.2.0 (Next Release)
- [ ] Rate limiting
- [ ] Email verification
- [ ] Password reset flow
- [ ] Pagination for lists
- [ ] Geo-IP integration
- [ ] A/B testing API endpoints

### v0.3.0
- [ ] Frontend (Next.js)
- [ ] Admin dashboard
- [ ] Payment integration

### v1.0.0
- [ ] Production-ready release
- [ ] Custom domains
- [ ] Mobile app
- [ ] Advanced analytics

---

## Contributors

- [@yourusername](https://github.com/yourusername) - Initial work

---

## Support

For questions and support:
- GitHub Issues: https://github.com/yourusername/LinkInBio.pro/issues
- Email: support@linkinbio.pro
