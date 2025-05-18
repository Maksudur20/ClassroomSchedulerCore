# Classroom Scheduler

## 📝 Project Overview

Classroom Scheduler is a comprehensive web application built with ASP.NET Core (.NET 8.0) that helps educational institutions efficiently manage classroom bookings. The system features role-based access control, allowing administrators, faculty members, and students to interact with the application based on their assigned privileges.

## ✨ Key Features

- **Role-Based Access Control**: Different functionality for admins, faculty, and students
- **Interactive Booking System**: User-friendly interface for creating and managing room bookings
- **Emergency Booking Priority**: Allows priority bookings for emergency situations
- **Conflict Prevention**: Automatically prevents double-booking and handles scheduling conflicts
- **Visual Status Indicators**: Color-coded interface showing availability status
- **Comprehensive Audit Logging**: Tracks all system changes for accountability
- **Responsive Design**: Mobile-friendly interface built with Bootstrap

## 🔧 Technical Stack

- **Framework**: ASP.NET Core (.NET 8.0)
- **Database**: SQL Server with Entity Framework Core
- **Authentication**: ASP.NET Core Identity
- **Frontend**: Bootstrap 5, jQuery, HTML5/CSS3
- **Development Tools**: Visual Studio 2022 or VS Code

## 💻 System Requirements

- .NET 8.0 SDK
- SQL Server / SQL Express LocalDB
- Visual Studio 2022 or Visual Studio Code

## 🚀 Getting Started

### Setting Up the Development Environment

1. **Clone the Repository**
   ```bash
   git clone <repository-url>
   cd ClassroomSchedulerCore
   ```

2. **Restore Dependencies**
   ```bash
   dotnet restore
   ```

3. **Configure Database Connection**
   - Open `appsettings.json` in the ClassroomSchedulerCore project
   - Update the `DefaultConnection` string if needed to point to your SQL Server instance
   
4. **Apply Database Migrations**
   ```bash
   dotnet ef database update
   ```
   
5. **Run the Application**
   ```bash
   dotnet run
   ```

6. **Access the Application**
   - Open a web browser and navigate to `https://localhost:7005` or `http://localhost:5005`

### Default User Accounts

The system comes pre-configured with the following test accounts:

| Role | Email | Password |
|------|-------|----------|
| Admin | admin@example.com | Admin123! |
| Faculty | faculty@example.com | Faculty123! |
| Student | student@example.com | Student123! |
| User | user@example.com | User123! |

## 👥 User Roles and Permissions

### Admin
- Manage all rooms and bookings
- Create, edit, and delete any booking
- Access audit logs
- Manage user accounts
- Override any booking for emergency purposes

### Faculty
- Create, edit, and delete their own bookings
- Request emergency bookings (subject to admin approval)
- View bookings created by others

### Student
- View room availability
- View booking schedules
- Create and manage their own classroom bookings
- View their personal class schedule

### User (Basic Access)
- View room availability
- View booking schedules
- Cannot create or modify bookings

## 📅 Using the Application

### Booking Management

#### Creating a Booking
1. Navigate to the Bookings section
2. Click "Create New Booking" (or use "Create Booking" button on a specific room)
3. Select the room, date, and time
4. Add booking details (title, description)
5. Set booking status (Regular or Emergency)
6. Submit the form

#### Editing a Booking
1. Find the booking in the list view
2. Click the "Edit" action
3. Modify the required details
4. Save changes

#### Deleting a Booking
1. Find the booking in the list view
2. Click the "Delete" action
3. Confirm deletion on the confirmation page

#### Handling Conflicts
- System automatically checks for conflicts
- Emergency bookings can override regular bookings
- Admin can manually resolve conflicts

### Room Management (Admin Only)

#### Adding a Room
1. Navigate to the Rooms section
2. Click "Create New Room"
3. Enter room details (name, location, capacity, etc.)
4. Submit the form

#### Editing Room Information
1. Find the room in the list view
2. Click the "Edit" action
3. Modify room details
4. Save changes

#### Deleting a Room
1. Find the room in the list view
2. Click the "Delete" action
3. Confirm deletion

### Audit Logs (Admin Only)
1. Navigate to the Audit Logs section
2. Browse through the logs to see all actions performed in the system
3. Filter logs by user, action type, or date

## 🏗️ Project Structure

The application follows the standard MVC (Model-View-Controller) pattern:

### Models
Define data structures and business logic
- `ApplicationUser.cs`: User model extending ASP.NET Identity
- `Room.cs`: Classroom definitions
- `Booking.cs`: Reservation data structure (with nullable navigation properties)
- `AuditLog.cs`: System activity tracking (with nullable User property)

### Views
User interface components
- Organized by controller (Home, Rooms, Bookings)
- Shared layouts and partial views

### Controllers
Handle user interactions
- `HomeController.cs`: Dashboard and basic pages
- `RoomsController.cs`: Room management
- `BookingsController.cs`: Booking functionality with proper entity loading

### Data
Database context and initialization
- `ApplicationDbContext.cs`: EF Core database context
- `DbInitializer.cs`: Seed data and initial setup

## 🗃️ Database Schema

The application uses several key tables:

1. **AspNetUsers**: User accounts (extended from ASP.NET Identity)
2. **Rooms**: Available classrooms and their properties
3. **Bookings**: Reservation records linking users to rooms
4. **AuditLogs**: System activity tracking

## 🛠️ Development Guidelines

### Adding New Features
1. Create or modify models as needed
2. Update the database context and add migrations
3. Implement controller actions
4. Create or update views
5. Test thoroughly across different user roles

### Database Migrations
When making changes to the data model:
```bash
dotnet ef migrations add <MigrationName>
dotnet ef database update
```

## 🐛 Recent Fixes

### Booking System Fixes
1. Fixed validation errors in Bookings/Create:
   - Explicitly loaded Room and User navigation entities in the POST Create method
   - Updated Booking model to make navigation properties nullable (Room? and User?)

2. Fixed ArgumentNullException in Bookings/Edit:
   - Added ViewBag.Rooms to the GET Edit method
   - Explicitly loaded Room and User entities in the POST Edit method

3. Added missing CreateForRoom functionality:
   - Created a new CreateForRoom action method that redirects to Create with the roomId parameter

### Static Web Asset Conflicts
1. Fixed JavaScript conflicts:
   - Renamed bootstrap-check.js to bootstrap-check-main.js
   - Updated reference in _Layout.cshtml

2. Fixed CSS conflicts:
   - Renamed bootstrap-custom.css to bootstrap-custom-main.css
   - Renamed site.css to site-main.css
   - Updated references in _Layout.cshtml

### Code Improvements
1. Fixed code warnings:
   - Made User navigation property in AuditLog.cs nullable
   - Added null-coalescing operators for UserName properties in DbInitializer.cs

### Coding Standards

- Follow C# naming conventions
- Use async/await for database operations
- Add comments for complex functionality
- Validate input data using Data Annotations

## Troubleshooting

1. **Database Connection Issues**
   - Verify connection string in appsettings.json
   - Ensure SQL Server is running
   - Check if the database exists

2. **Migration Errors**
   - Try removing the Migrations folder and recreating migrations
   - Check for model errors or constraints

3. **Runtime Errors**
   - Check application logs in the logs directory
   - Verify permissions for operations

## Recent Fixes and Updates

- **Room Features Fix (May 18, 2025)**: Fixed an issue in RoomsController where `HasProjector` and `HasComputers` properties were not being properly bound during room creation and editing.
- **Code Improvements**: Performed general code review and optimizations throughout the application.
- **Documentation**: Updated this README with the latest project information.

## Extending the Application

The Classroom Scheduler can be extended in several ways:

1. **Integration with calendar systems** (Google Calendar, Outlook)
2. **Email notifications** for booking confirmations and changes
3. **Advanced reporting** features for usage analytics
4. **Mobile app** for easier on-the-go access
5. **API endpoints** for integration with other systems

## License

This project is for educational purposes only. All rights reserved.

---

Last updated: May 18, 2025
