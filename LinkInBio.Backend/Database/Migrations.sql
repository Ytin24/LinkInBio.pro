-- LinkInBio.pro Database Schema
-- PostgreSQL migration script

-- Drop tables if they exist (for development)
DROP TABLE IF EXISTS ab_test_results CASCADE;
DROP TABLE IF EXISTS ab_tests CASCADE;
DROP TABLE IF EXISTS click_events CASCADE;
DROP TABLE IF EXISTS links CASCADE;
DROP TABLE IF EXISTS profiles CASCADE;
DROP TABLE IF EXISTS users CASCADE;

-- Users table
CREATE TABLE users (
    id UUID PRIMARY KEY,
    email VARCHAR(255) UNIQUE NOT NULL,
    password_hash VARCHAR(255) NOT NULL,
    username VARCHAR(100) UNIQUE NOT NULL,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    subscription_tier VARCHAR(50) NOT NULL DEFAULT 'Free',
    CONSTRAINT check_subscription_tier CHECK (subscription_tier IN ('Free', 'Basic', 'Premium'))
);

CREATE INDEX idx_users_email ON users(email);
CREATE INDEX idx_users_username ON users(username);

-- Profiles table (one user can have multiple profiles)
CREATE TABLE profiles (
    id UUID PRIMARY KEY,
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    slug VARCHAR(100) UNIQUE NOT NULL,
    display_name VARCHAR(255) NOT NULL,
    bio TEXT,
    avatar_url TEXT,
    theme JSONB NOT NULL DEFAULT '{
        "BackgroundColor": "#ffffff",
        "TextColor": "#000000",
        "ButtonStyle": "Rounded",
        "FontFamily": "Inter"
    }'::jsonb,
    is_published BOOLEAN NOT NULL DEFAULT false,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_profiles_user_id ON profiles(user_id);
CREATE INDEX idx_profiles_slug ON profiles(slug);

-- Links table
CREATE TABLE links (
    id UUID PRIMARY KEY,
    profile_id UUID NOT NULL REFERENCES profiles(id) ON DELETE CASCADE,
    title VARCHAR(255) NOT NULL,
    url TEXT NOT NULL,
    icon_url TEXT,
    position INTEGER NOT NULL DEFAULT 0,
    is_active BOOLEAN NOT NULL DEFAULT true,
    created_at TIMESTAMP NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_links_profile_id ON links(profile_id);
CREATE INDEX idx_links_position ON links(profile_id, position);

-- Click events table (analytics)
CREATE TABLE click_events (
    id UUID PRIMARY KEY,
    link_id UUID NOT NULL REFERENCES links(id) ON DELETE CASCADE,
    profile_id UUID NOT NULL REFERENCES profiles(id) ON DELETE CASCADE,
    clicked_at TIMESTAMP NOT NULL DEFAULT NOW(),
    ip_address VARCHAR(45),
    user_agent TEXT,
    referrer TEXT,
    country VARCHAR(100),
    city VARCHAR(100)
);

CREATE INDEX idx_click_events_link_id ON click_events(link_id);
CREATE INDEX idx_click_events_profile_id ON click_events(profile_id);
CREATE INDEX idx_click_events_clicked_at ON click_events(clicked_at);

-- A/B Tests table
CREATE TABLE ab_tests (
    id UUID PRIMARY KEY,
    profile_id UUID NOT NULL REFERENCES profiles(id) ON DELETE CASCADE,
    name VARCHAR(255) NOT NULL,
    variant_a_theme JSONB NOT NULL,
    variant_b_theme JSONB NOT NULL,
    is_active BOOLEAN NOT NULL DEFAULT true,
    start_date TIMESTAMP NOT NULL DEFAULT NOW(),
    end_date TIMESTAMP,
    created_at TIMESTAMP NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_ab_tests_profile_id ON ab_tests(profile_id);
CREATE INDEX idx_ab_tests_is_active ON ab_tests(is_active);

-- A/B Test Results table
CREATE TABLE ab_test_results (
    id UUID PRIMARY KEY,
    test_id UUID NOT NULL REFERENCES ab_tests(id) ON DELETE CASCADE,
    variant VARCHAR(1) NOT NULL CHECK (variant IN ('A', 'B')),
    views BIGINT NOT NULL DEFAULT 0,
    clicks BIGINT NOT NULL DEFAULT 0,
    updated_at TIMESTAMP NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_ab_test_results_test_id ON ab_test_results(test_id);

-- Profile Views table (для подсчета просмотров профиля)
CREATE TABLE profile_views (
    id UUID PRIMARY KEY,
    profile_id UUID NOT NULL REFERENCES profiles(id) ON DELETE CASCADE,
    viewed_at TIMESTAMP NOT NULL DEFAULT NOW(),
    ip_address VARCHAR(45),
    user_agent TEXT,
    referrer TEXT
);

CREATE INDEX idx_profile_views_profile_id ON profile_views(profile_id);
CREATE INDEX idx_profile_views_viewed_at ON profile_views(viewed_at);

-- Sample data for testing
-- Password for all test users: "password123"
-- Hash generated with BCrypt

INSERT INTO users (id, email, password_hash, username, subscription_tier) VALUES
    ('11111111-1111-1111-1111-111111111111', 'test@example.com', '$2a$11$YourBCryptHashHere', 'testuser', 'Free'),
    ('22222222-2222-2222-2222-222222222222', 'premium@example.com', '$2a$11$YourBCryptHashHere', 'premiumuser', 'Premium');

INSERT INTO profiles (id, user_id, slug, display_name, bio) VALUES
    ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', '11111111-1111-1111-1111-111111111111', 'testuser', 'Test User', 'Welcome to my LinkInBio!');

INSERT INTO links (id, profile_id, title, url, position) VALUES
    ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', 'My Website', 'https://example.com', 1),
    ('cccccccc-cccc-cccc-cccc-cccccccccccc', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', 'Instagram', 'https://instagram.com/testuser', 2),
    ('dddddddd-dddd-dddd-dddd-dddddddddddd', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', 'YouTube', 'https://youtube.com/@testuser', 3);

COMMENT ON TABLE users IS 'Registered users of the LinkInBio platform';
COMMENT ON TABLE profiles IS 'User profile pages (linkinbio.pro/{slug})';
COMMENT ON TABLE links IS 'Links displayed on profile pages';
COMMENT ON TABLE click_events IS 'Analytics: tracks every link click';
COMMENT ON TABLE ab_tests IS 'A/B testing experiments for profiles';
COMMENT ON TABLE profile_views IS 'Analytics: tracks profile page views';
