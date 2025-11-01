module LinkInBio.Backend.Services.AuthService

open System
open System.IdentityModel.Tokens.Jwt
open System.Security.Claims
open System.Text
open Microsoft.IdentityModel.Tokens
open LinkInBio.Backend.Models.Domain
open LinkInBio.Backend.Models.DTOs

// JWT Configuration
let private jwtSecret =
    Environment.GetEnvironmentVariable("JWT_SECRET")
    |> Option.ofObj
    |> Option.defaultValue "your-super-secret-jwt-key-change-this-in-production"

let private jwtIssuer = "LinkInBio.pro"
let private jwtAudience = "LinkInBio.pro"
let private jwtExpirationHours = 24.0 * 7.0 // 7 days

// Password hashing using BCrypt
let hashPassword (password: string) =
    BCrypt.Net.BCrypt.HashPassword(password, 11)

let verifyPassword (password: string) (hash: string) =
    BCrypt.Net.BCrypt.Verify(password, hash)

// JWT Token generation
let generateToken (user: User) : string =
    let tokenHandler = JwtSecurityTokenHandler()
    let key = Encoding.ASCII.GetBytes(jwtSecret)

    let claims = [
        Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
        Claim(ClaimTypes.Email, user.Email)
        Claim(ClaimTypes.Name, user.Username)
        Claim("subscription_tier", string user.SubscriptionTier)
    ]

    let tokenDescriptor = SecurityTokenDescriptor(
        Subject = ClaimsIdentity(claims),
        Expires = DateTime.UtcNow.AddHours(jwtExpirationHours) |> Nullable,
        Issuer = jwtIssuer,
        Audience = jwtAudience,
        SigningCredentials = SigningCredentials(
            SymmetricSecurityKey(key),
            SecurityAlgorithms.HmacSha256Signature
        )
    )

    let token = tokenHandler.CreateToken(tokenDescriptor)
    tokenHandler.WriteToken(token)

// Validate JWT token
let validateToken (token: string) : Result<Guid, string> =
    try
        let tokenHandler = JwtSecurityTokenHandler()
        let key = Encoding.ASCII.GetBytes(jwtSecret)

        let validationParameters = TokenValidationParameters(
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        )

        let principal = tokenHandler.ValidateToken(token, validationParameters, ref null)
        let userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)

        match userIdClaim with
        | null -> Error "User ID claim not found"
        | claim ->
            match Guid.TryParse(claim.Value) with
            | true, userId -> Ok userId
            | false, _ -> Error "Invalid user ID format"
    with
    | ex -> Error ex.Message

// Register new user
let register (request: RegisterRequest) =
    async {
        let! existingUsers = Database.DbContext.Users.findByEmail request.Email

        match List.isEmpty existingUsers with
        | false -> return Error "Email already registered"
        | true ->
            let newUser = {
                Id = Guid.NewGuid()
                Email = request.Email
                PasswordHash = hashPassword request.Password
                Username = request.Username
                CreatedAt = DateTime.UtcNow
                SubscriptionTier = Free
            }

            try
                let! _ = Database.DbContext.Users.create newUser
                let token = generateToken newUser

                let response = {
                    Token = token
                    UserId = newUser.Id
                    Username = newUser.Username
                    Email = newUser.Email
                }

                return Ok response
            with
            | ex -> return Error $"Failed to create user: {ex.Message}"
    }

// Login user
let login (request: LoginRequest) =
    async {
        let! users = Database.DbContext.Users.findByEmail request.Email

        match users with
        | [] -> return Error "Invalid email or password"
        | user :: _ ->
            match verifyPassword request.Password user.PasswordHash with
            | false -> return Error "Invalid email or password"
            | true ->
                let token = generateToken user

                let response = {
                    Token = token
                    UserId = user.Id
                    Username = user.Username
                    Email = user.Email
                }

                return Ok response
    }

// Get user from token
let getUserFromToken (token: string) =
    async {
        match validateToken token with
        | Error msg -> return Error msg
        | Ok userId ->
            let! users = Database.DbContext.Users.findById userId

            match users with
            | [] -> return Error "User not found"
            | user :: _ -> return Ok user
    }
