# NEXUS ISP Management System

NEXUS is a workflow-based Internet Service Provider management system developed using ASP.NET Core MVC, C#, Entity Framework Core, and SQL Server.

The system manages the complete lifecycle of a customer connection request, starting from customer registration and order placement through retail processing, technical feasibility assessment, billing, and connection management.

## Live Application

https://nexuseproject.runasp.net/

## Project Overview

NEXUS was developed to simulate and manage the operational workflow of an Internet Service Provider.

Instead of handling customer requests through disconnected processes, the system provides dedicated role-based interfaces where each department handles its part of the connection workflow.

The application follows a structured process:

```text
Customer Registration
        |
        v
Order Placement
        |
        v
Retail Department
        |
        v
Order Acceptance
        |
        v
Technical Department
        |
        v
Feasibility Check
        |
        v
Feasible / Rejected
        |
        v
Billing & Connection Processing
        |
        v
Active Connection
```

## Core Features

### Customer Portal

Customers can register and access their own portal to manage their interaction with NEXUS.

Key functionality includes:

- Customer registration
- Customer login
- Connection order placement
- Application status tracking
- Bill viewing
- Payment-related interface
- Customer account information

### Retail Department

The Retail department receives connection requests submitted by customers.

Retail employees can:

- View incoming customer orders
- Review order information
- Accept connection requests
- Forward accepted requests into the technical workflow
- Track order status

### Technical Department

The Technical department handles feasibility assessment for accepted connection requests.

Technical employees can:

- View pending technical requests
- Review customer and connection details
- Perform feasibility checks
- Mark connections as feasible
- Reject connections where service cannot be provided
- Track feasibility status

### Feasibility Workflow

One of the primary workflows of NEXUS is determining whether a requested connection can be provided in the customer's area.

Typical statuses include:

```text
Pending
Feasibility Pending
Feasible
Rejected
Active
```

This provides a structured lifecycle for every connection request.

### Billing

The system also includes billing-related functionality for customer accounts and active services.

Customers can access billing information through their portal while the internal workflow supports the management of connection-related billing operations.

### Role-Based Dashboards

NEXUS provides separate interfaces and functionality according to the responsibilities of different users.

The project includes role-oriented functionality for areas such as:

- Administration
- Customer
- Retail
- Technical
- Billing / Feasibility

## Technology Stack

### Backend

- ASP.NET Core MVC
- C#
- Entity Framework Core
- Razor Views

### Frontend

- HTML5
- CSS3
- JavaScript
- Bootstrap
- Bootstrap Icons

### Database

- Microsoft SQL Server
- SQL Server Management Studio
- Entity Framework Core
- Code First approach
- EF Core Migrations

### Development Tools

- Visual Studio
- Git
- GitHub
- MonsterASP.NET
- WebDeploy

## Architecture

The application follows the ASP.NET Core MVC architecture.

```text
NEXUS-eProject/
|
|-- Controllers/
|-- Models/
|-- Views/
|-- wwwroot/
|   |-- css/
|   |-- js/
|   |-- images/
|
|-- Migrations/
|-- Properties/
|-- Program.cs
|-- appsettings.json
|-- NEXUS-eProject.csproj
```

The MVC architecture separates the application's data models, request handling, user interfaces, and static resources to keep the project organized and maintainable.

## Database

NEXUS uses Microsoft SQL Server with Entity Framework Core.

The database was developed using the Code First approach, allowing the application's models and relationships to be managed through Entity Framework migrations.

The application handles data related to areas such as:

- Customers
- Orders
- Connection requests
- Feasibility
- Application statuses
- Billing
- User roles

## Application Workflow

A typical connection request follows this process:

### 1. Customer Registration

A new customer creates an account and accesses the customer portal.

### 2. Order Placement

The customer submits a request for an internet connection.

### 3. Retail Processing

The Retail department receives the request, reviews its information, and accepts it for further processing.

### 4. Technical Processing

After Retail acceptance, the request becomes available to the Technical department.

### 5. Feasibility Check

The Technical department determines whether the requested service can be provided at the customer's location.

The request can then be marked as:

```text
Feasible
```

or:

```text
Rejected
```

### 6. Further Processing

Feasible requests continue through the required billing and connection workflow.

### 7. Active Connection

After the required workflow has been completed, the customer connection can reach Active status.

## Running the Project Locally

### 1. Clone the repository

```bash
git clone https://github.com/mdhashir22/NEXUS-eProject.git
```

### 2. Open the project

Open the solution in Visual Studio.

### 3. Restore dependencies

Visual Studio should automatically restore the required NuGet packages.

Alternatively:

```bash
dotnet restore
```

### 4. Configure the database

Review the connection string inside:

```text
appsettings.json
```

Configure it for your local SQL Server environment if necessary.

### 5. Apply migrations

Using the Package Manager Console:

```powershell
Update-Database
```

Or using the .NET CLI:

```bash
dotnet ef database update
```

### 6. Run the application

Run the project through Visual Studio or execute:

```bash
dotnet run
```

## Security

Sensitive production credentials, API keys, email passwords, and deployment credentials should not be committed to the repository.

The public repository configuration does not contain production passwords or API keys.

## Deployment

NEXUS is deployed using MonsterASP.NET and Visual Studio WebDeploy.

Live application:

https://nexuseproject.runasp.net/

## Purpose of the Project

NEXUS was developed as a practical full-stack project to demonstrate the implementation of a multi-stage business workflow using ASP.NET Core MVC.

The project focuses on:

- MVC architecture
- Role-based workflows
- Database-driven applications
- Entity Framework Core
- SQL Server integration
- Customer portals
- Department-based processing
- Status management
- Responsive user interfaces
- Real-world workflow implementation

## Developer

**Muhammad Hashir**

ASP.NET Core Developer

Portfolio:  
https://muhammadhashir.runasp.net/

GitHub:  
https://github.com/mdhashir22

LinkedIn:  
https://www.linkedin.com/in/muhammad-hashir-980707310

## Repository

Source Code:

https://github.com/mdhashir22/NEXUS-eProject
