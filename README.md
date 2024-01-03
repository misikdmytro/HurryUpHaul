# Hurry Up, Haul

## Description

This is a simple web application that allows to create orders in restaurant. It is built with ASP.NET 8.0. Web service is described in [this post](https://medium.com/@dmytro.misik/writing-a-web-service-using-c-ddbda1a4a21c).

## Installation

### Prerequisites

- [Docker](https://www.docker.com/)
- [Docker Compose](https://docs.docker.com/compose/)

### Steps

1. Clone the repository
2. Run `docker-compose up -d` in the root directory

## Usage

### Endpoints

Use Swagger UI to test the endpoints. It is available at `http://localhost:8081/swagger/index.html`.

### Authentication

The application uses JWT for authentication. To get a token:

1. Register a user using `POST /api/users`
2. Authenticate using `POST /api/users/token`
