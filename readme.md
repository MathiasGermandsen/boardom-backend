# Boardom API

<!-- Add your ER diagram here -->
![Database ER Diagram](docs/images/billede.png)

> A .NET 8 Web API for managing IoT devices and sensor data with PostgreSQL.

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-4169E1?logo=postgresql&logoColor=white)](https://www.postgresql.org/)
[![Docker](https://img.shields.io/badge/Docker-Ready-2496ED?logo=docker&logoColor=white)](https://www.docker.com/)
[![Kubernetes](https://img.shields.io/badge/Kubernetes-Ready-326CE5?logo=kubernetes&logoColor=white)](https://kubernetes.io/)

---

## 📋 Table of Contents

- [Features](#-features)
- [Tech Stack](#-tech-stack)
- [Getting Started](#-getting-started)
- [API Endpoints](#-api-endpoints)
- [Configuration](#-configuration)
- [Docker](#-docker)
- [Kubernetes Deployment](#-kubernetes-deployment)
- [Project Structure](#-project-structure)

---

## ✨ Features

- **Device Management** - Register and manage IoT devices
- **Sensor Data Collection** - Store temperature, humidity, pressure, light, and moisture readings
- **Device Grouping** - Organize devices into logical groups
- **RESTful API** - Clean, well-documented API endpoints
- **Swagger UI** - Interactive API documentation
- **Docker Support** - Containerized deployment ready
- **Kubernetes Ready** - Production-ready K8s manifests included

---

## 🛠 Tech Stack

| Technology              | Purpose               |
| ----------------------- | --------------------- |
| .NET 8                  | Web API Framework     |
| Entity Framework Core 8 | ORM / Database Access |
| PostgreSQL              | Database              |
| Npgsql                  | PostgreSQL Driver     |
| Swagger/OpenAPI         | API Documentation     |
| Docker                  | Containerization      |
| Kubernetes              | Orchestration         |

---

## 🚀 Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [PostgreSQL](https://www.postgresql.org/download/) (or Docker)
- [Docker](https://www.docker.com/) (optional)

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/yourusername/boardomapi.git
   cd boardomapi
   ```

2. **Configure the database connection**
   
   Update `boardomapi/appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Host=localhost;Database=boardom;Username=postgres;Password=yourpassword"
     }
   }
   ```

3. **Run the application**
   ```bash
   cd boardomapi
   dotnet run
   ```

4. **Open Swagger UI**
   
   Navigate to: [http://localhost:5000/swagger](http://localhost:5000/swagger)

---

## 📡 API Endpoints

### Device Controller

| Method | Endpoint            | Description                              |
| ------ | ------------------- | ---------------------------------------- |
| `POST` | `/Device/addDevice` | Register a new device or update existing |

**Request Body:**
```json
{
  "deviceId": "sensor-001",
  "friendlyName": "Living Room Sensor"
}
```

**Response (201 Created):**
```json
{
  "message": "Device registered",
  "deviceId": "sensor-001",
  "friendlyName": "Living Room Sensor"
}
```

---

### Data Controller

| Method | Endpoint           | Description            |
| ------ | ------------------ | ---------------------- |
| `POST` | `/Data/sensorData` | Submit sensor readings |

**Request Body:**
```json
{
  "deviceId": "sensor-001",
  "temperature": 22.5,
  "humidity": 45.0,
  "pressure": 1013.25,
  "light": 500.0,
  "moisture": 30.0
}
```

**Response (201 Created):**
```json
{
  "message": "Sensor data recorded",
  "id": 1,
  "deviceId": "sensor-001",
  "dateAdded": "2026-01-22T10:30:00Z"
}
```

---

### Group Controller

| Method | Endpoint           | Description               |
| ------ | ------------------ | ------------------------- |
| `POST` | `/Group/create`    | Create a new device group |
| `PUT`  | `/Group/edit`      | Rename an existing group  |
| `POST` | `/Group/addDevice` | Add a device to a group   |

**Create Group:**
```json
{
  "groupName": "Kitchen Sensors"
}
```

**Edit Group:**
```json
{
  "groupName": "Kitchen Sensors",
  "newName": "Kitchen & Dining Sensors"
}
```

**Add Device to Group:**
```json
{
  "groupName": "Kitchen Sensors",
  "deviceId": "sensor-001"
}
```

---

## ⚙️ Configuration

### Environment Variables

| Variable                               | Description                          | Default       |
| -------------------------------------- | ------------------------------------ | ------------- |
| `ConnectionStrings__DefaultConnection` | PostgreSQL connection string         | -             |
| `ASPNETCORE_ENVIRONMENT`               | Environment (Development/Production) | `Development` |

### appsettings.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=boardom;Username=postgres;Password=password"
  }
}
```

---

## 🐳 Docker

### Build the Image

```bash
cd boardomapi
docker build -t boardomapi:latest .
```

### Run with Docker

```bash
docker run -d \
  -p 8080:8080 \
  -e ConnectionStrings__DefaultConnection="Host=host.docker.internal;Database=boardom;Username=postgres;Password=password" \
  --name boardomapi \
  boardomapi:latest
```

### Docker Compose (with PostgreSQL)

```yaml
version: '3.8'
services:
  api:
    build: ./boardomapi
    ports:
      - "8080:8080"
    environment:
      - ConnectionStrings__DefaultConnection=Host=db;Database=boardom;Username=postgres;Password=password
    depends_on:
      - db

  db:
    image: postgres:16
    environment:
      - POSTGRES_DB=boardom
      - POSTGRES_USER=postgres
      - POSTGRES_PASSWORD=password
    volumes:
      - postgres_data:/var/lib/postgresql/data
    ports:
      - "5432:5432"

volumes:
  postgres_data:
```

---

## ☸️ Kubernetes Deployment

Kubernetes manifests are available in the `k8s/` folder:

- `k8s/dev/` - Development environment
- `k8s/prod/` - Production environment

### Deploy to Kubernetes

```bash
# Development
kubectl apply -f k8s/dev/deployment.yaml

# Production
kubectl apply -f k8s/prod/deployment.yaml
```

---

## 📁 Project Structure

```
boardomapi/
├── .github/
│   └── copilot-instructions.md    # GitHub Copilot review guidelines
├── boardomapi/
│   ├── api/
│   │   ├── DataController/        # Sensor data endpoints
│   │   ├── DeviceController/      # Device management
│   │   └── GroupController/       # Group management
│   ├── database/
│   │   ├── AppDbContext.cs        # EF Core DbContext
│   │   └── DbConfig.cs            # Database configuration
│   ├── Models/
│   │   ├── Device.cs              # Device entity
│   │   ├── DeviceGroup.cs         # Device-Group relationship
│   │   ├── Group.cs               # Group entity
│   │   ├── Requests.cs            # API request records
│   │   └── SensorData.cs          # Sensor reading entity
│   ├── Program.cs                 # Application entry point
│   ├── Dockerfile                 # Container definition
│   └── appsettings.json           # Configuration
├── k8s/
│   ├── dev/
│   │   └── deployment.yaml
│   └── prod/
│       └── deployment.yaml
└── readme.md
```

---

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'feat: Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

