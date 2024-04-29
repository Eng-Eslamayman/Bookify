# Bookify Admin Dashboard

Welcome to the Bookify Admin Dashboard! This README provides an overview of the features, architecture, and technologies used in developing this application.

## Overview

Bookify Admin Dashboard is a web application built using ASP.NET Core MVC. It serves as a comprehensive tool for managing various aspects of the Bookify platform from an administrative perspective. The dashboard is designed to provide an intuitive user interface for handling tasks such as user management, content management, reporting, and integration with external services like WhatsApp and Gmail.

## Features

- Content Management: Create, read, update, and delete functionalities for managing content.
- Reporting: Generate reports in Excel and PDF formats to analyze data.
- Integration: Integration with WhatsApp for communication and Gmail for sending emails.
- Error Handling: Utilizes Serilog for effective error logging and management.
- Validation: Implements Fluent Validation for client and server-side validation.
- DataTables: Enhances tabular data presentation and interactivity using the DataTables library.
- Modal Bootstrap: Utilizes Bootstrap modals for interactive user interface elements.
- Background Jobs: Background jobs managed with Hangfire for asynchronous tasks.
- Searching Module: Specialized module for searching books efficiently.
- User Management: CRUD operations for managing user accounts with identity pages.

## Architecture

The application follows the Clean Architecture pattern, emphasizing separation of concerns and maintainability. It consists of the following layers:

1. **Presentation Layer**: ASP.NET Core MVC for handling user interactions and rendering views.
2. **Application Layer**: Contains business logic and application-specific rules.
3. **Domain Layer**: Defines the domain entities and business logic, ensuring independence from external concerns.
4. **Infrastructure Layer**: Deals with external concerns such as database access, external services integration, logging, and background jobs.

## Technologies Used

- ASP.NET Core MVC: Provides the framework for building web applications.
- Clean Architecture: Organizes codebase for better maintainability and scalability.
- Repository Pattern: Abstracts data access logic for improved separation of concerns.
- Unit of Work: Manages database transactions and ensures data consistency.
- AJAX Requests: Enables asynchronous communication between client and server.
- DataTables: Enhances table functionality and interactivity.
- Bootstrap Modal: Utilizes Bootstrap modals for displaying dynamic content.
- WhatsApp Integration: Integrates with WhatsApp for seamless communication.
- Gmail Integration: Utilizes Gmail for sending emails.
- Identity Pages: Provides user authentication and authorization functionality.
- Excel and PDF Reporting: Generates reports in Excel and PDF formats for data analysis.
- Serilog: Handles error logging and management for improved application monitoring.
- Fluent Validation: Ensures robust client and server-side data validation.
- Hangfire: Manages background jobs for executing asynchronous tasks.
- Libraries: Animate.css, Bootbox.js, Handlebars.js, SweetAlert2, Typeahead for enhanced user experience and functionality.

## Deployment

The application is deployed to Smarter Aspnet hosting for public access. Ensure proper configuration and maintenance of hosting environment for optimal performance.

## Getting Started

To set up the application locally, follow these steps:

1. Clone the repository.
2. Configure the database connection string in `appsettings.json`.
3. Run migrations to create the database schema.
4. Build and run the application.

## Contributions

Contributions to the Bookify Admin Dashboard are welcome! Please follow the guidelines outlined in the CONTRIBUTING.md file.

---

Thank you for choosing Bookify Admin Dashboard! If you have any questions or feedback, feel free to reach out to us. Happy administering!
