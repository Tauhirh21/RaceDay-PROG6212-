# RaceDay-PROG6212-
RaceDay – Event Management System for South African road events (PROG6212 POE) 
## 🚀 Part 2: Restful API Development

### API Overview
The RaceDay API is a RESTful ASP.NET Core Web API that powers the RaceDay platform. It handles user authentication, event management, category management, event enrolments, and race results.

### 🔐 User Roles
- **Organiser**: Can create, update, and delete events and categories. Can capture results and view all enrolments.
- **Participant**: Can browse events, enrol in categories, view personal enrolments, and track their results.

### 🛠️ Running the API Locally
1. Clone the repository.
2. Open `RaceDay.API.sln` in Visual Studio.
3. Restore NuGet packages (Visual Studio does this automatically, or run `dotnet restore`).
4. Update the database: In the Package Manager Console, run `Update-Database`.
5. Press **F5** to run the API.
6. Swagger opens at `https://localhost:7276/swagger`.

### 🧪 Running Tests
In the Package Manager Console or terminal:

### 🎥 Part 2 Video
[▶️ Watch the Part 2 Video Presentation](https://youtu.be/9WI5AoF5yrU?si=OLeVFsfgyxy91ho0)
