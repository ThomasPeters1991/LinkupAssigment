
# Project: Distributed Microservices with RabbitMQ

This project consists of four microservices: MQManager, OrderService, NotificationService, and UserService. These services collectively demonstrate an event-driven architecture using RabbitMQ for inter-service communication.

## Table of Contents

- [Overview](#overview)
- [Prerequisites](#prerequisites)
- [Installation](#installation)
- [Configuration](#configuration)
- [Running the Services](#running-the-services)
- [API Endpoints](#api-endpoints)
  - [MQManager](#mqmanager)
  - [OrderService](#orderservice)
  - [NotificationService](#notificationservice)
  - [UserService](#userservice)
- [Inter-Service Communication](#inter-service-communication)
- [Logging](#logging)
- [Retry Logic](#retry-logic)
- [Contributing](#contributing)
- [License](#license)

## Overview

This project demonstrates a microservices architecture with the following services:

1. **MQManager**: Manages RabbitMQ queues.
2. **OrderService**: Handles order-related operations and publishes events to RabbitMQ.
3. **NotificationService**: Listens to RabbitMQ events and sends notifications.
4. **UserService**: Manages user-related operations and provides user information upon request.

## Prerequisites

- .NET 6.0 SDK
- RabbitMQ instance
- PostgreSQL database

## Installation

1. Clone the repository:
   ```bash
   git clone https://github.com/yourusername/distributed-microservices.git
   cd distributed-microservices
   ```

2. Restore dependencies for all services:
   ```bash
   dotnet restore
   ```

## Configuration

Each service has its own configuration file (`appsettings.json`). Below are the configurations required for each service.

### MQManager Configuration

**appsettings.json**:
```json
{
  "RabbitMQ": {
    "Host": "your-rabbitmq-host",
    "UserName": "your-username",
    "Password": "your-password",
    "OrdersQueueName": "orders_queue",
    "UserInfoQueueName": "user_info_queue",
    "ReplyQueueName": "reply_queue"
  }
}
```

### OrderService Configuration

**appsettings.json**:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=your-postgresql-host;Database=orders_db;Username=your-username;Password=your-password"
  },
  "RabbitMQ": {
    "Host": "your-rabbitmq-host",
    "UserName": "your-username",
    "Password": "your-password"
  }
}
```

### NotificationService Configuration

**appsettings.json**:
```json
{
  "RabbitMQ": {
    "Host": "your-rabbitmq-host",
    "UserName": "your-username",
    "Password": "your-password",
    "OrdersQueueName": "orders_queue",
    "UserInfoQueueName": "user_info_queue",
    "ReplyQueueName": "reply_queue"
  }
}
```

### UserService Configuration

**appsettings.json**:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=your-postgresql-host;Database=users_db;Username=your-username;Password=your-password"
  },
  "RabbitMQ": {
    "Host": "your-rabbitmq-host",
    "UserName": "your-username",
    "Password": "your-password"
  }
}
```

## Running the Services

1. Build the projects:
   ```bash
   dotnet build
   ```

2. Run each service in separate terminal windows:
   ```bash
   dotnet run --project MQManager
   dotnet run --project OrderService
   dotnet run --project NotificationService
   dotnet run --project UserService
   ```

## API Endpoints

### MQManager

**Create Predefined Queues**
- **Endpoint:** `POST /api/queues/CreatePredefinedQueues`
- **Description:** Creates predefined queues as specified in the configuration.
- **Response:**
  ```json
  {
    "message": "Predefined queues created successfully."
  }
  ```

**Clear All Queues**
- **Endpoint:** `DELETE /api/queues/ClearAllQueues`
- **Description:** Clears all queues specified in the configuration.
- **Response:**
  ```json
  {
    "message": "All queues cleared successfully."
  }
  ```

### OrderService

**Create Order**
- **Endpoint:** `POST /api/orders`
- **Description:** Creates a new order.
- **Response:**
  ```json
  {
    "orderId": "order-id",
    "status": "Order created successfully."
  }
  ```

**Get Order**
- **Endpoint:** `GET /api/orders/{orderId}`
- **Description:** Retrieves order details by order ID.
- **Response:**
  ```json
  {
    "orderId": "order-id",
    "userId": "user-id",
    "items": [ ... ]
  }
  ```

### NotificationService

**Note:** NotificationService listens to RabbitMQ events and does not expose any public API endpoints.

### UserService

**Create User**
- **Endpoint:** `POST /api/users`
- **Description:** Creates a new user.
- **Response:**
  ```json
  {
    "userId": "user-id",
    "status": "User created successfully."
  }
  ```

**Get User**
- **Endpoint:** `GET /api/users/{userId}`
- **Description:** Retrieves user details by user ID.
- **Response:**
  ```json
  {
    "userId": "user-id",
    "name": "user-name",
    "email": "user-email"
  }
  ```

## Inter-Service Communication

The services communicate with each other using RabbitMQ. Here is a brief overview:

- **OrderService** publishes order creation events to `orders_queue`.
- **NotificationService** listens to `orders_queue`, processes order events, and requests user information from `UserService` via `user_info_queue`.
- **UserService** listens to `user_info_queue` and responds with user details.

## Logging

The application uses the built-in .NET logging framework. Logs are written to the console and include information about connection attempts, queue creation, and any errors encountered.

## Retry Logic

The `RabbitMQSetupService` includes a retry policy to establish a connection to RabbitMQ. It retries every second for up to 10 seconds. If the connection is not established within this time frame, an error is logged.

## Contributing

Contributions are welcome! Please follow these steps:

1. Fork the repository.
2. Create a new branch for your feature or bugfix.
3. Commit your changes.
4. Push to your branch and open a pull request.
