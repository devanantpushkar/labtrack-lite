# LabTrack Lite

**LabTrack Lite** is a production-ready R&D Asset and Ticketing platform designed for modern laboratory environments. It combines high-performance asset tracking with an intelligent ticketing system and a natural language query chatbot.

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![Deployed on Render](https://img.shields.io/badge/Backend-Render-blue)](https://labtrack-api.onrender.com)
[![Deployed on Vercel](https://img.shields.io/badge/Frontend-Vercel-black)](https://vercel.com)

##  Features

- **Asset Management**: Full lifecycle tracking of laboratory equipment with unique QR codes and status monitoring.
- **Ticketing System**: Streamlined maintenance and issue reporting with controlled status transitions (Open → In Progress → Resolved → Closed).
- **NLQ Chatbot**: Query your lab data using natural language. "How many spectrometers are in Lab Room 103?"
- **Enterprise Security**:
  - JWT-based Authentication.
  - Role-Based Access Control (Admin, Engineer, Technician).
  - BCrypt password hashing.
  - API Rate Limiting for brute-force protection.
- **Accessibility**: WCAG 2.2 AA compliant UI with ARIA support and high-contrast design.
- **Responsive UI**: Seamless experience across Desktop, Tablet, and Mobile.

##  Tech Stack

### Backend
- **Framework**: .NET 8.0 Minimal APIs
- **Database**: PostgreSQL (Production), SQLite (Local Development)
- **ORM**: Entity Framework Core
- **Security**: JWT, Rate Limiting, CORS Hardening
- **Containerization**: Docker

### Frontend
- **Framework**: React 18 with Vite
- **Icons**: Lucide React
- **HTTP Client**: Axios
- **State Management**: React Context API
- **Styling**: Vanilla CSS (CSS Variables, Glassmorphism)

##  Getting Started

### Prerequisites
- .NET 8.0 SDK
- Node.js (v18+)
- Local PostgreSQL or SQLite

### Local Installation

1. **Clone the repository**:
   ```bash
   git clone https://github.com/devanantpushkar/labtrack-lite.git
   cd labtrack-lite
   ```

2. **Backend Setup**:
   ```bash
   cd backend/LabTrackApi
   dotnet run
   ```
   *The API will start at `http://localhost:5000`*

3. **Frontend Setup**:
   ```bash
   cd frontend
   npm install
   npm run dev
   ```
   *The app will start at `http://localhost:5173`*

##  Deployment

### Backend (Render)
The backend is containerized using a multi-stage Dockerfile and deployed on Render.
- **URL**: [https://labtrack-api.onrender.com](https://labtrack-api.onrender.com)

### Frontend (Vercel)
The frontend is optimized for production and deployed on Vercel with SPA routing support.
- **URL**: [https://labtrack-lite.vercel.app]

## 📜 License
This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.


