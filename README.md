# LabTrack Lite - R&D Asset & Ticketing Platform

A lightweight asset and ticket management system built with **ASP.NET Core 8 Minimal API** and **React**.

## Quick Start

### Prerequisites
- **.NET SDK 8.0**
- **Node.js** (v18 or later recommended)

### 1. Start the Backend API
Navigate to the backend directory and run:
```bash
cd backend/LabTrackApi
dotnet run
```
The API serves at: `http://localhost:5000`
Swagger Documentation: `http://localhost:5000/swagger`

### 2. Start the Frontend App
Open a new terminal, navigate to the frontend directory and run:
```bash
cd frontend
npm install  # (Only needed first time)
npm run dev
```
The Web App serves at: `http://localhost:5173`

## Demo Credentials
The system is pre-seeded with the following accounts:

| Role      | Username   | Password      | Access Level                    |
|-----------|------------|---------------|---------------------------------|
| **Admin** | `admin`    | `admin123`    | Full Access (Users, Assets, Tickets) |
| **Engineer**| `engineer` | `engineer123` | Manage Assets, View/Comment Tickets |
| **Technician**| `technician`| `tech123`  | View Assets, Create Tickets     |

## Project Structure
- `backend/LabTrackApi`: ASP.NET Core Web API with SQLite.
- `frontend`: React + Vite application (Vanilla CSS).
- `database`: SQL schema and seed data.

## Features
- **Authentication**: JWT-based login with Role-Based Access Control (RBAC).
- **Assets**: Track lab equipment with status and categories.
- **Tickets**: Report issues, track status (Open, InProgress, Resolved).
- **Dashboard**: Real-time overview of asset and ticket counts.
