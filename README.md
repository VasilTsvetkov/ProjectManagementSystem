# Project Management System (PMS)

A web-based project and task management system designed for team environments. The application provides comprehensive control over the project lifecycle, task distribution, team collaboration, and precise time tracking.

---

## 🚀 Key Features

* **User Authentication & Authorization (RBAC):** Secure login and registration with three distinct access levels (Admin, Manager, Member) managed via ASP.NET Core Identity.
* **Project Management (CRUD):** Full Create, Read, Update, and Delete capabilities for managing organization projects.
* **Task Management:** Complete lifecycle management for tasks with attributes including assignee, priority (Low to Critical), deadline, status, and task type (Bug, Feature, Task).
* **Interactive Kanban Board:** Full drag-and-drop workflow visualization (SortableJS) for real-time, asynchronous task status updates using the Fetch API.
* **Task Comments:** Real-time collaboration allowing team members to add, view, and edit comments on individual tasks.
* **Time Tracking (Logs):** A dedicated logging module where developers can log precise hours worked on specific tasks.
* **Monthly Dashboard:** An analytical reporting view summarizing logged hours, productivity, and work distributions by user and project.
* **Search & Filters:** Easy navigation allowing users to filter tasks on their dashboard based on status, priority, or assignee.

---

## 🛠 Technology Stack

| Component | Technology | Description |
| :--- | :--- | :--- |
| **Backend** | .NET 8 (ASP.NET Core MVC) | Powerful, modern web application framework |
| **Data Access** | EF Core (Repository Pattern) | Complete isolation of the data layer for modularity |
| **Database** | SQL Server | Relational database running in an isolated Docker container |
| **Frontend** | HTML5, CSS3, Bootstrap 5, JS | Clean, responsive, and modern user interface |
| **Libraries** | SortableJS, Fetch API | Interactive Kanban board with background async requests |
| **Logging** | Serilog | Structured event and error logging to the console |
| **Code Quality** | CodeMaid, Roslynator | Static code analysis, formatting, and linting tools |

---

## 🏗 Application Architecture

The project strictly follows the Separation of Concerns (SoC) principle:
1.  **ProjectManagementSystem.Web:** The presentation layer containing MVC controllers, views, view models, custom JavaScript scripts, custom middleware, and ViewComponents.
2.  **ProjectManagementSystem.BL (Business Logic):** The core business layer containing repositories, services, Data Transfer Objects (DTOs), Entity models, and the database context (`ApplicationDbContext`).
3.  **ProjectManagementSystem.Tests:** The automated testing layer using XUnit and EF Core InMemory to validate business rules and database repository behaviors.

---

## 🧪 Testing & Code Quality

All source code files have been programmatically cleaned and formatted according to industry C# coding standards using **CodeMaid** and **Roslynator Compiler Analyzers**, maintaining maximum readability and eliminating dead code.
