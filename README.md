[Unity Application link](https://www.dropbox.com/scl/fi/vnaha4d8x1lfj0r0sv5xi/Questionaire.apk?rlkey=sn71qeyf4cyufeo58iu3ziwbr&st=ubrphdei&dl=0) <br/>
[Web Application link](https://questionaire.runasp.net/)
# Questionaire Application

A full-stack application ecosystem for creating, managing, and taking questionnaires or quizzes. It features an ASP.NET Core backend API, multiple client applications (Blazor WebAssembly frontend, Unity client), and a suite of automated tests.

## Overview

The application allows users to interact with a database of questions via a central API.
Questions can have multiple answers, belong to categories, and have difficulty levels. 
Authenticated users can browse, filter, add new questions, update existing and "play" through these questions using either the web interface or the Unity application.
Game is imagined as a version of 'The Chase' but there is a "show/hide" button so users can play alone as well.
The system includes automated tests to ensure the reliability and correctness of the backend API logic.

## Application roles

1. Standard User (Authenticated User)

Core Functionality (Quiz Taking & Basic Interaction):

    Authentication:
        Register for a new account.
        Log in to their existing account.
        Log out.
        
    Question Discovery & Gameplay (Primary Role):
        View lists of available question categories.
        Filter questions based on selected categories and difficulty levels.
        Retrieve and view individual questions one by one (via Web or Unity client).
        Interact with the question display modes (Show Correct Answer Only, Show All Answers, Hide Answers).
        Navigate between questions (Previous/Next).

    History:
        Have their interaction history implicitly saved (e.g., which questions they've viewed/answered, tracked via UserQuestionHistory).
        
    Content Submission (Optional - Depends on Design):
        Add pending questions (these starts as unapproved).

Client Usage:

    Can use both the Blazor Web client and the Unity client.

2. Admin

Includes all Standard User functionalities, plus:

    Content Management (Moderation & Curation):
        View all questions, including those pending approval.
        Approve newly submitted questions, making them visible to standard users.
        Reject (or potentially edit and then approve) submitted questions, but not his own.
        Edit existing questions (text, difficulty).
        Manage Answers: Add, edit, or delete answers associated with a question. Mark the correct answer(s).
        Manage Categories: Add, edit, or delete question categories. Assign or unassign categories to questions.
        Soft Delete: Mark questions as deleted (setting IsDeleted=1, DeletedAt, DeletedById).

Client Usage:

    Primarily uses the Blazor Web client, which would need specific administrative sections/pages visible only to Admin/SuperAdmin roles. The Unity client is typically focused on the end-user quiz-taking experience.

3. SuperAdmin

Includes all Admin and Standard User functionalities, plus:

    System-Level User Management:
        Create, edit, and delete Admin accounts.
        Assign or revoke the Admin role from users.
        
    Role Management:
        (Potentially) Manage application roles themselves (if more roles than User/Admin/SuperAdmin are needed).
        
    Advanced Content Management:
        Perform bulk operations on questions/categories.
        Hard delete content permanently from the database.
        
    Audit / Log Viewing:
        Access more detailed system logs or audit trails through an administrative interface (beyond standard Serilog files).

Client Usage:
    
    Exclusively uses the Blazor Web client's administrative sections.

## Features

### Backend API (`Api`)

*   **Technology:** ASP.NET Core (.NET 9)
*   **Database:** Entity Framework Core with SQL Server (using code-first migrations).
*   **Authentication:** ASP.NET Core Identity with JWT Bearer Token authentication. Includes refresh token support.
*   **Authorization:** Standard ASP.NET Core authorization.
*   **API Endpoints:**
    *   CRUD operations for Questions, Answers, Categories.
    *   Retrieving questions based on criteria (categories, difficulty, single/multi-answer mode).
    *   User registration (`/register`), login (`/login`), token refresh (`/refresh`).
    *   User profile management (`/manage/info`).
    *   Receiving client-side logs (`/api/logs`).
*   **Logging:** Serilog configured for console and rolling file logging.
*   **Error Handling:** Global exception handler middleware.
*   **API Documentation:** Scalar UI support in development.
*   **CORS:** Configured to allow requests from known client origins (requires correct environment variables).
*   **Rate Limiting:** Basic fixed-window rate limiting.
*   **Services:** Business logic encapsulated in services.

### Frontend - Blazor Web (`Web`)

*   **Technology:** Blazor WebAssembly (.NET 9)
*   **UI:** Web-based interface for viewing questions, filtering, login, registration, etc.
*   **Authentication:** Client-side state management (`CustomAuthStateProvider`), token storage (`Blazored.LocalStorage`), automatic token handling (`CustomHttpMessageHandler`), token refresh.
*   **API Communication:** Uses `HttpClient` via `CustomHttpMessageHandler`.
*   **Logging:** Sends warning/error logs to the backend API (`ApiLogger`).

### Client - Unity (`App`)

*   **Technology:** Unity (6000.0.38f1 LTS), C#
*   **Purpose:** Provides an alternative, potentially more interactive or game-like, interface for viewing and answering questions retrieved from the `Api`.
*   **UI:** Built using Unity's **UI Toolkit** (including UXML and USS for structure and styling).
    *   Displays questions and answers dynamically.
    *   Supports single-answer and multiple-choice modes.
    *   Features show/hide answer functionality.
    *   Includes navigation controls (Previous/Next) with slide-in/out animations.
    *   Implements Safe Area handling for mobile displays.
*   **API Communication:** Uses Unity's networking capabilities (e.g., `UnityWebRequest` or `HttpClient` within Unity) to fetch questions and potentially send user history/results to the `Api`.
*   **State Management:** Manages the current question index, loaded questions, and UI state within the Unity scene/scripts.

*   ### Automated Tests (`Tests`)

*   **Purpose:** Contains automated tests designed to verify the functionality and correctness of the `Api` backend.
*   **Scope:**:
    *   **Unit Tests:** Testing individual components (services, helper classes) in isolation, often using mocking frameworks (like Moq or NSubstitute) to fake dependencies (like database context or other services).
    *   **Integration Tests:** Testing the interaction between different components of the API, potentially including interactions with an in-memory database or a dedicated test database to verify data access and API endpoint behavior more comprehensively.
*   **Technology:** Uses a testing framework like xUnit, along with assertion libraries (e.g., FluentAssertions) and mocking libraries. Tools like `Microsoft.EntityFrameworkCore.InMemory` are used for in-memory API testing.
*   **Benefits:** Helps ensure code quality, prevents regressions, and provides confidence when refactoring or adding new features to the backend API.

### Shared (`SharedStandard` - Assumed)

*   Contains shared models (DTOs - Data Transfer Objects) used for communication between the API and *both* the Blazor and Unity clients (e.g., `QuestionDto`, `AnswerDto`, `LogEntryDto`), but is primarily used for unity client because of its target framework .NET Standard 2.1.

## Technology Stack

*   **Backend:** ASP.NET Core, Entity Framework Core, ASP.NET Core Identity, Serilog, .NET 9, SQL Server
*   **Web Frontend:** Blazor WebAssembly, .NET Runtime for WebAssembly, Blazored.LocalStorage
*   **Unity Client:** Unity Engine, C#, UI Toolkit (UXML/USS)
*   **Testing:** xUnit, Moq, FluentAssertions, EF Core In-Memory Provider
*   **Database:** Microsoft SQL Server
*   **Shared Code:** .NET Standard / .NET Library

## Project Structure

*   **`Api/`**: The ASP.NET Core backend project.
*   **`Web/`**: The Blazor WebAssembly frontend project.
*   **`App/`**: The Unity project folder containing Assets, Packages, ProjectSettings etc.
*   **`Tests/`**: The Tests project folder containing intergation and unity tests.
*   **`SharedStandard/`**: Class library with shared DTOs/models mainly used for Unity.
*   **`Shared/`**: Class library with shared DTOs/models.

## Setup and Running Locally

### Prerequisites

1.  **.NET SDK:** .NET 9 SDK . [Download .NET](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)
2.  **Database:** Microsoft SQL Server instance.
3.  **IDE/Editor (for .NET):** Visual Studio 2022, JetBrains Rider, or VS Code.
4.  **Unity Hub & Unity Editor:** Install Unity Hub and the required Unity Editor version (check the `UnityClient/ProjectSettings/ProjectVersion.txt` file for the exact version). [Download Unity](https://unity.com/download)
5.  **Environment Variables Tool (Optional):** For managing API environment variables.

### Configuration (API)

Configure the `Api` as described previously:

1.  Navigate to the `Api` directory.
2.  Create/edit a `.env` file or set system/user environment variables:

    ```dotenv
    # Database Connection String (Update)
    DEFAULT_CONNECTION="Server=YOUR_SERVER_NAME;Database=QuestionaireDb;User ID=YOUR_USER;Password=YOUR_PASSWORD;TrustServerCertificate=True;MultipleActiveResultSets=true"

    # CRITICAL: URL the Blazor Web App runs on (For CORS)
    WEB_URL="https://localhost:5255" # Example - CHANGE THIS

    # CRITICAL: URL the Web Api runs on (For CORS)
    API_URL="https://localhost:5256" # Example - CHANGE THIS
    ```

    *   **Important:** Update `DEFAULT_CONNECTION`.
    *   **Crucial:** Set `WEB_URL` for the Blazor app.
    *   **Crucial:** Set `API_URL` for the Blazor app.

### Database Setup (API)

1.  Set everything up correctly and start the Api application, it should automatically apply migrations to database

### Running the Application

You need the API running for *either* client to function.

1.  **Run the API (`Api`):**
    *   Use your IDE or `dotnet run` from the `Api` directory. Note the URL (e.g., `https://localhost:5256`).

2.  **Run the Web Client (`Web`) (Optional):**
    *   Use your IDE (configured for multiple startup projects is best) or `dotnet run` from the `Web` directory.
    *   Access via its URL (e.g., `https://localhost:5255`).

3.  **Run the Unity Client (`App`):**
    *   **Open Project:** Use Unity Hub to "Add" or "Open" the `App` folder as a Unity project. Ensure you have the correct Unity Editor version installed.
    *   **Configure API Endpoint:** Inside the Unity Editor, find where the API base URL is configured. It is in `Assets\Resources` called `EnvironmentConfig`:
        *   **Set this URL** to the address where your `Api` is running (e.g., `https://localhost:5256`).
    *   **Enter Play Mode:** Press the Play button in the Unity Editor to run the client application.
    *   **(Optional) Build:** You can create standalone builds (PC, Mac, Mobile) via `File > Build Settings...`. Remember to configure the API endpoint appropriately for the built version (it needs to point to a publicly accessible API deployment).

### Running the Tests

1.  **Using IDE (Visual Studio / Rider):**
    *   Open the Solution (`.sln` file).
    *   Build the solution (usually Ctrl+Shift+B or Build > Build Solution).
    *   Open the Test Explorer window (usually under the Test menu).
    *   Discover tests (might happen automatically or require a refresh).
    *   Right-click on the test project (`Tests`), specific test classes, or individual tests and select "Run".

2.  **Using .NET CLI:**
    *   Open a terminal or command prompt.
    *   Navigate to the directory containing the solution file (`.sln`) **or** the test project directory (`Tests`).
    *   Run the tests using the command:
        ```bash
        dotnet test
        ```
        (If running from the solution directory, this command will discover and run tests in all test projects within the solution).

## API Documentation

Access Scalar UI via the API's base URL when running in Development mode (e.g., `https://localhost:5256/scalar/v1`).

## License

[![CC BY-NC-ND 4.0][cc-by-nc-nd-shield]][cc-by-nc-nd]

This work is licensed under a
[Creative Commons Attribution-NonCommercial-NoDerivs 4.0 International License][cc-by-nc-nd].

[![CC BY-NC-ND 4.0][cc-by-nc-nd-image]][cc-by-nc-nd]

[cc-by-nc-nd]: http://creativecommons.org/licenses/by-nc-nd/4.0/
[cc-by-nc-nd-image]: https://licensebuttons.net/l/by-nc-nd/4.0/88x31.png
[cc-by-nc-nd-shield]: https://img.shields.io/badge/License-CC%20BY--NC--ND%204.0-lightgrey.svg
