# Freight Booking System

A microservices-based freight booking system with PostgreSQL database, JWT authentication, and RabbitMQ for inter-service communication.

## Architecture

- **Booking Service**: Manages freight bookings and customer information
- **Schedule Service**: Manages transportation schedules and vehicle availability
- **PostgreSQL**: Primary database for both services
- **RabbitMQ**: Message broker for inter-service communication
- **JWT**: Authentication and authorization

## Services

### Booking Service
- **Port**: 5001 (HTTP), 5002 (HTTPS)
- **Endpoints**:
  - `POST /api/auth/login` - User login
  - `POST /api/auth/register` - User registration
  - `GET /api/bookings` - Get all bookings (requires authentication)
  - `GET /api/bookings/{id}` - Get booking by ID
  - `POST /api/bookings` - Create new booking
  - `PUT /api/bookings/{id}` - Update booking
  - `DELETE /api/bookings/{id}` - Delete booking

### Schedule Service
- **Port**: 5003 (HTTP), 5004 (HTTPS)
- **Endpoints**:
  - `POST /api/auth/login` - User login
  - `POST /api/auth/register` - User registration
  - `GET /api/schedules` - Get all schedules (requires authentication)
  - `GET /api/schedules/{id}` - Get schedule by ID
  - `POST /api/schedules` - Create new schedule
  - `PUT /api/schedules/{id}` - Update schedule
  - `DELETE /api/schedules/{id}` - Delete schedule
  - `POST /api/schedules/{id}/book` - Book a schedule

## Getting Started

### Prerequisites
- Docker and Docker Compose
- .NET 8.0 SDK (for local development)

### Running with Docker Compose

1. Clone the repository
2. Navigate to the project directory
3. Run the following command:

```bash
docker-compose up --build
```

This will start:
- PostgreSQL database on port 5432
- RabbitMQ on ports 5672 (AMQP) and 15672 (Management UI)
- Booking API service
- Schedule API service
- API Gateway on port 8080

### Database Access
- **Host**: localhost
- **Port**: 5432
- **Database**: freightbooking
- **Username**: postgres
- **Password**: postgres123

### RabbitMQ Management
- **URL**: http://localhost:15672
- **Username**: guest
- **Password**: guest

## API Usage

### Authentication

1. **Register a new user**:
```bash
curl -X POST http://localhost:5001/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "username": "testuser",
    "email": "test@example.com",
    "password": "password123",
    "firstName": "Test",
    "lastName": "User"
  }'
```

2. **Login**:
```bash
curl -X POST http://localhost:5001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "username": "testuser",
    "password": "password123"
  }'
```

3. **Use the JWT token** in subsequent requests:
```bash
curl -X GET http://localhost:5001/api/bookings \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

### Creating a Booking

```bash
curl -X POST http://localhost:5001/api/bookings \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -d '{
    "customerId": "customer123",
    "customerName": "John Doe",
    "customerEmail": "john@example.com",
    "origin": "New York",
    "destination": "Los Angeles",
    "departureDate": "2024-01-15T10:00:00Z",
    "arrivalDate": "2024-01-16T18:00:00Z",
    "cargoType": "Electronics",
    "weight": 100.5,
    "volume": 2.5,
    "price": 1500.00,
    "notes": "Fragile items"
  }'
```

### Creating a Schedule

```bash
curl -X POST http://localhost:5003/api/schedules \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -d '{
    "routeName": "NYC to LA",
    "origin": "New York",
    "destination": "Los Angeles",
    "departureTime": "2024-01-15T10:00:00Z",
    "arrivalTime": "2024-01-16T18:00:00Z",
    "vehicleType": "Truck",
    "vehicleNumber": "TRK001",
    "capacity": 1000,
    "pricePerUnit": 1.50,
    "driverName": "Mike Johnson",
    "driverContact": "+1234567890"
  }'
```

### Booking a Schedule

```bash
curl -X POST http://localhost:5003/api/schedules/1/book \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -d '{
    "bookingId": 1,
    "customerId": "customer123",
    "quantity": 100,
    "notes": "Priority shipping"
  }'
```

## Default Admin User

A default admin user is created in both services:
- **Username**: admin
- **Password**: admin123
- **Email**: admin@freightbooking.com

## Message Flow

The system uses RabbitMQ for inter-service communication:

1. When a booking is created, a `BookingCreatedEvent` is published
2. The Schedule service consumes this event and can react accordingly
3. When a schedule is created, a `ScheduleCreatedEvent` is published
4. The Booking service consumes this event and can notify customers

## Development

### Local Development

1. Ensure PostgreSQL and RabbitMQ are running
2. Update connection strings in `appsettings.json` files
3. Run the services individually:

```bash
# Booking Service
cd FreightBooking/services/Booking/Booking.Api
dotnet run

# Schedule Service
cd FreightBooking/services/Schedule/Schedule.Api
dotnet run
```

### Database Migrations

The services will automatically create the database schema on startup using `EnsureCreated()`. For production, consider using proper migrations.

## Configuration

### Environment Variables

- `ConnectionStrings__DefaultConnection`: PostgreSQL connection string
- `JWT__SecretKey`: JWT signing key (must be at least 32 characters)
- `JWT__Issuer`: JWT issuer
- `JWT__Audience`: JWT audience
- `RabbitMQ__Host`: RabbitMQ host
- `RabbitMQ__Username`: RabbitMQ username
- `RabbitMQ__Password`: RabbitMQ password

## Security

- Passwords are hashed using BCrypt
- JWT tokens expire after 24 hours
- All API endpoints (except auth) require authentication
- CORS is configured for cross-origin requests

## Monitoring

- Swagger UI is available at `/swagger` for each service
- RabbitMQ Management UI provides message queue monitoring
- Application logs include message consumption events
