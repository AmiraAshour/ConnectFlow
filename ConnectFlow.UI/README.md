# Connect Flow

A .NET-based Contacts Manager application that allows users to manage their contacts efficiently.  
This project follows a **clean architecture** pattern with separate layers for Core, Infrastructure, UI, and Testing.

---

## Features

- **CRUD Operations**: Add, update, delete, and view contacts.
- **MVC Architecture**: Separation of concerns for better maintainability.
- **Entity Framework Core**: Database interactions using EF Core.
- **Identity and Authentication**: Secure user authentication and authorization.
- **User Roles**: Admin and User roles with different access permissions.
- **Register/Login/Logout**: User authentication with ASP.NET Identity and Google OAuth.
- **Export Options**: Export contacts to PDF, CSV, or Excel files.
- **Bulk Import**: Upload Excel files to import country lists.
- **Advanced Search & Sort**: Filter and sort contact data easily.
- **Custom MVC Filters**: Action, Result, Resource, and Authorization filters for modularity.
- **Tag Helpers**: Reusable Razor components.
- **Structured Logging**: Logging with Serilog.
- **Unit and Integration Testing**: xUnit-based testing to ensure reliability.
- **Responsive UI**: User-friendly web interface with ASP.NET MVC.

---

## Project Structure

```
ConnectFlowSolution/
├── ConnectFlow.UI/              # User Interface (MVC project)
├── ConnectFlow.Core/            # Domain models and business logic
├── ConnectFlow.Infrastructure/  # Data access and external services
├── ConnectFlow.ControllerTests/ # Controller tests (xUnit)
├── ConnectFlow.ServiceTests/    # Service tests (xUnit)
├── ConnectFlow.IntegrationTests/# Integration tests (xUnit)
├── ConnectFlowSolution.sln      # Solution file
└── .gitignore
```

---

## Live Demo

🌐 [Connect Flow Live](https://lnkd.in/dvnZz7pz)

## GitHub Repository

🔗 [GitHub Repo](https://lnkd.in/d52hGTjq)

---

## Getting Started

```bash
# 1. Clone the repository
git clone https://github.com/YOUR_USERNAME/YOUR_REPO_NAME.git

# 2. Navigate to project directory
cd ConnectFlow

# 3. Apply migrations
dotnet ef database update

# 4. Run the application
dotnet run --project src/UI
```

Then open your browser at: `https://localhost:5001`

---

## Contributing

Pull requests are welcome! For major changes, please open an issue first to discuss what you would like to change.

---

## License

This project is licensed under the MIT License.
