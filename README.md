Order Management API

This project is a small Order Management API built in C# using ASP.NET Core and Entity Framework Core to practice and demonstrate real backend concepts beyond basic CRUD. 
It supports user registration and login using JWT authentication, enforces per-user access to data, and models a simple order lifecycle where orders can be created, modified while in a draft state, and then submitted, after which they become immutable. 
Orders are associated with users and contain multiple order items, with business rules enforced at the API level such as preventing item additions to submitted orders and requiring at least one item before submission. 
The project uses SQLite for persistence, EF Core relationships and navigation properties for data modeling, and minimal APIs for request handling. The goal of this project was to focus on API design, data modeling, authentication, authorization, validation through business rules, and debugging common backend issues such as serialization cycles and database state, rather than building a frontend or production-ready UI.





To run the project locally.

Clone the repository and ensure you have a recent .NET SDK installed, then restore dependencies and start the application using dotnet run from the project directory. 
The API will start on a local port and use a SQLite database file created automatically in the project directory. 
New users can be registered through the /auth/register endpoint and authenticated via /auth/login, which returns a JWT token that must be included as a Bearer token in the Authorization header for protected endpoints. 
Orders can then be created, queried, updated with items while in a draft state, and submitted using the provided API routes. 
The API is designed to be exercised using tools like curl, PowerShell Invoke-RestMethod, Postman, or similar HTTP clients rather than through a browser UI.