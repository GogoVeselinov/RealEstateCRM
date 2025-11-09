# Real Estate CRM

A comprehensive Customer Relationship Management system for real estate businesses, built with ASP.NET Core MVC.

## Features

### Property Management
- Create, edit, and manage property listings
- Track property status (Active, Sold, Rented, Archived)
- Store detailed property information including price, area, location, and specifications
- Property type categorization (Apartment, House, Office, Land, Commercial, Garage)

### Client Management
- Manage client information and contact details
- Track client status (Active, Inactive, Potential, Closed)
- Add comments for internal team communication
- Monitor client interactions and history

### Visit Scheduling
- Schedule and track property visits
- Link visits to specific properties and clients
- Record visit outcomes and next actions
- Track visit status (Scheduled, Completed, Cancelled, NoShow)
- Set duration and location for each visit

### User Roles
- **Admin**: Full system access and user management
- **Manager**: Can create, edit, and manage all records
- **Agent**: View-only access to track activities

### Dashboard
- Overview of key metrics and statistics
- Quick access to recent activities
- Property and client summaries

## Technology Stack

- **Framework**: ASP.NET Core 9.0 MVC
- **Database**: SQL Server with Entity Framework Core
- **Authentication**: ASP.NET Core Identity
- **Frontend**: Bootstrap 5, jQuery
- **Language**: C# (.NET 9.0)

## Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- SQL Server (LocalDB, Express, or full version)
- Visual Studio 2022 or VS Code

## Getting Started

### 1. Clone the Repository
```bash
git clone https://github.com/GogoVeselinov/RealEstateCRM.git
cd RealEstateCRM
```

### 2. Configure Database Connection
Update the connection string in `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=RealEstateCRM;Trusted_Connection=true;MultipleActiveResultSets=true"
  }
}
```

### 3. Apply Database Migrations
```bash
dotnet ef database update
```

### 4. Run the Application
```bash
dotnet run
```

The application will be available at `https://localhost:5001` or `http://localhost:5000`.

### 5. Default Login Credentials

After the first run, the system will seed default users:

**Admin Account:**
- Email: `admin@realestate.com`
- Password: `Admin@123`

**Manager Account:**
- Email: `manager@realestate.com`
- Password: `Manager@123`

**Agent Account:**
- Email: `agent@realestate.com`
- Password: `Agent@123`

## Project Structure

```
RealEstateCRM/
├── Controllers/          # MVC Controllers
│   ├── AdminController.cs
│   ├── AuthController.cs
│   ├── ClientsController.cs
│   ├── PropertiesController.cs
│   └── VisitsController.cs
├── Models/
│   ├── Common/          # Shared models and enums
│   ├── Entities/        # Database entities
│   ├── Identity/        # User and role models
│   └── ViewModels/      # View models for forms and lists
├── Views/               # Razor views
├── Data/                # Database context and seeders
├── Migrations/          # EF Core migrations
└── wwwroot/            # Static files (CSS, JS, images)
```

## Database Schema

### Main Entities

- **Property**: Real estate listings with details
- **Client**: Customer information and contacts
- **Visit**: Scheduled property viewings
- **ApplicationUser**: System users with role-based access

## Development

### Adding a New Migration
```bash
dotnet ef migrations add MigrationName
dotnet ef database update
```

### Building the Project
```bash
dotnet build
```

### Running Tests
```bash
dotnet test
```

## Configuration

### App Settings
- Modify `appsettings.json` for production settings
- Use `appsettings.Development.json` for development-specific configuration

## Features in Detail

### Property Management
- Decimal precision handling for prices (18,2)
- Image upload support (planned)
- Advanced search and filtering
- Export capabilities

### Client Management
- Contact information tracking
- Activity history
- Comment system for team collaboration
- Status tracking throughout sales pipeline

### Visit Management
- Calendar integration
- Automated reminders (planned)
- Visit outcome tracking
- Follow-up action scheduling

## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## License

This project is licensed under the MIT License.

## Support

For support, email support@realestate.com or open an issue in the GitHub repository.

## Roadmap

- [ ] Email notifications
- [ ] Calendar view for visits
- [ ] Advanced reporting and analytics
- [ ] Mobile responsive improvements
- [ ] Document management
- [ ] Integration with external property portals
- [ ] Multi-language support expansion

## Author

**Gogo Veselinov**
- GitHub: [@GogoVeselinov](https://github.com/GogoVeselinov)

---

Made with ❤️ for real estate professionals
