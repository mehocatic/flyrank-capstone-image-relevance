# Build Log

This file records the main implementation and documentation milestones, including AI-assisted work. It does not represent a commit history.

## Milestones

### 1. API foundation

- Established a .NET 8 ASP.NET Core Web API project with controller, contract, domain, service, data, and middleware folders.
- Added the image processing, post, post suggestion, and review routes.
- Added OpenAPI documentation and Problem Details error responses.

### 2. Image analysis and matching

- Added Gemini Flash structured image analysis and validation for analysis fields, tags, and confidence.
- Added Gemini embedding generation and cosine similarity ranking.
- Added an animal mismatch guard with a human-readable reason.
- Added in-memory repository implementations for posts, images, and suggestions.

### 3. Development test mode

- Added deterministic demo analysis and embedding services so the HTTP flow can be tested without Gemini credentials.
- Added seed data for three posts and two images.
- Added a sample wolf-versus-red-fox flow in the project `.http` file.
- Verified locally that the flow returns the fox/wolf rejection reason, guarded approval returns `409`, manual rejection succeeds, and seeded suggestions are available.

### 4. Documentation and evidence organization

- Moved `swagger.png`, `post.png`, and `post-images.png` from the solution root into `docs/images/`.
- Added this build log, a README, section 6 evidence mapping, and a YAML evaluation manifest.
- Recorded the batch-processing gap explicitly. The current API processes one image per request and has no batch task implementation.

## Validation

- `dotnet build FlyrankImageRelevance.slnx --no-restore`: passed with 0 warnings and 0 errors during implementation verification.
- Local HTTP smoke test: post creation, image processing, fox/wolf mismatch reason, guarded approval rejection, manual rejection, and seeded suggestions passed.
- No Git commits were created as part of this work.
