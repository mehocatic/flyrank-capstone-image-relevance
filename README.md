# Flyrank Image Relevance API

ASP.NET Core Web API capstone project for analyzing images, creating text and image embeddings, ranking image suggestions against posts, and rejecting obvious animal-category mismatches.

## Features

- Gemini Flash image analysis with a structured JSON response.
- Server-side checks for required tags, string lengths, and confidence values between 0 and 1.
- Gemini embeddings and cosine similarity ranking.
- Mismatch Guard explanations, for example `Animal category mismatch: expected fox, detected wolf`.
- Manual review of image and post suggestions. A suggestion rejected by the Mismatch Guard cannot be approved through the review endpoint.
- Development-only deterministic demo services and in-memory seed data.

## Architecture

The solution currently contains one API project with separated responsibilities:

- `Flyrank.ImageRelevance.Api/Controllers/` contains the HTTP controllers for images, posts, and reviews.
- `Flyrank.ImageRelevance.Api/Contracts/` contains API request and response records.
- `Flyrank.ImageRelevance.Api/Domain/Models/` contains image, post, and suggestion models.
- `Flyrank.ImageRelevance.Api/Services/` contains Gemini integration, embeddings, cosine similarity, matching, and review logic.
- `Flyrank.ImageRelevance.Api/Data/` contains the repository abstraction, in-memory implementation, and development seed data.
- `Flyrank.ImageRelevance.Api/Middleware/` contains API exception handling.

The in-memory repository is a starter implementation. It is not persistent and is replaced with empty runtime state whenever the application restarts.

## Requirements

- .NET 8 SDK
- Gemini API key for non-demo runs

## Run in development demo mode

From the solution root:

```powershell
dotnet build .\FlyrankImageRelevance.slnx
dotnet run --project .\Flyrank.ImageRelevance.Api\Flyrank.ImageRelevance.Api.csproj
```

The development launch profile starts HTTP on `http://localhost:5273`. The Development environment enables `DemoMode` in `Flyrank.ImageRelevance.Api/appsettings.Development.json`. Demo mode uses deterministic image analysis and embeddings, so no Gemini key or external API call is needed.

Open Swagger at `http://localhost:5273/swagger`.

At startup the demo seeder loads three posts and two images. The seeded red fox post has this stable ID:

```text
10000000-0000-0000-0000-000000000001
```

For example, fetch its seeded image suggestions with:

```text
GET http://localhost:5273/api/posts/10000000-0000-0000-0000-000000000001/images
```

## Run with Gemini

Set the key in the environment rather than committing it to configuration:

```powershell
$env:Gemini__ApiKey = "YOUR_GEMINI_API_KEY"
dotnet run --project .\Flyrank.ImageRelevance.Api\Flyrank.ImageRelevance.Api.csproj
```

Gemini is used when the application is not running in Development demo mode. The image processing endpoint accepts HTTPS image URLs as metadata and Base64 image bytes in the request. The API sends the Base64 bytes to Gemini; it does not download the supplied URL.

## HTTP endpoints

| Method | Route | Purpose |
| --- | --- | --- |
| `POST` | `/api/posts` | Create and embed a post. Returns `201 Created`. |
| `POST` | `/api/images/process` | Analyze a single image and rank it against existing posts. |
| `GET` | `/api/posts/{id}/images` | Get ranked image suggestions for a post. |
| `POST` | `/api/reviews` | Approve or reject an image and post suggestion. |

Full requests for the end-to-end wolf versus fox demo are in [Flyrank.ImageRelevance.Api.http](./Flyrank.ImageRelevance.Api/Flyrank.ImageRelevance.Api.http). Open the file in VS Code with the REST Client extension or in Visual Studio and run requests in order. The file uses response variables to pass the created post ID and processed image ID to subsequent requests.

Expected demo behavior:

1. Create the red fox post.
2. Process the sample image. In demo mode the analyzer deterministically describes it as a wolf.
3. Get suggestions for the created fox post. The wolf suggestion has the reason `Animal category mismatch: expected fox, detected wolf`.
4. Attempting to approve that suggestion returns `409 Conflict`; rejecting it returns `200 OK`.

## Evidence and build history

- Screenshots and their scope are documented in [EVIDENCE.md](./EVIDENCE.md).
- AI assistance and implementation milestones are recorded in [BUILDLOG.md](./BUILDLOG.md).
- Evaluation routes and current capability notes are listed in [capstone.yaml](./capstone.yaml).

Batch image processing is not implemented in the current API. The image processing endpoint accepts one image per request. Do not treat the screenshots or this README as evidence of batch-job processing.
