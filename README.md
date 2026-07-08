# VNG

A .NET 9 ASP.NET Core Razor Pages application for processing and serving Dutch spatial planning data (VNG standards).

[![.NET Version](https://img.shields.io/badge/.NET-9.0-purple)](https://dotnet.microsoft.com/)
[![Docker](https://img.shields.io/badge/Docker-Ready-blue)](https://www.docker.com/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

---

## 📋 Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Prerequisites](#prerequisites)
- [Quick Start](#quick-start)

---

<h2 id="features">✨ Features</h2>

VNG is a web application that integrates with Dutch government APIs (BAG, DSO) to process spatial planning data, manage land registry information, and provide geometry processing capabilities.

---

<h2 id="features">✨ Features</h2>

- **Spatial Planning Data Processing** - Integration with DSO (Digitaal Stelsel Omgevingswet)
- **BAG API Integration** - Dutch Address and Building registry lookup
- **Geometry Processing** - Handle and process geographic data
- **Land Registry Services** - Manage cadastral information
- **Jena-Fuseki SPARQL Endpoint** - SHACL-compliant SPARQL endpoint for semantic data queries
- **Docker Support** - Fully containerized for easy deployment
- **Structured Logging** - Serilog integration with file-based logging

---

<h2 id="prerequisites">📦 Prerequisites</h2>

Before you begin, ensure you have the following installed:

- [Docker Desktop](https://www.docker.com/products/docker-desktop) (latest version)
- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) (for local development)
- [Git](https://git-scm.com/) (for version control)

---

<h2 id="quick-start">🚀 Quick Start</h2>

### Using Docker (Recommended)

1. **Clone the repository**<br>
git clone https://github.com/peterrdf/VNG.git cd VNG

2. **Navigate to the Docker directory**<br>
cd Docker-Production

3. **Start the application**<br>
docker-compose up -d

4. **Set API Keys**<br>
   Ensure you have set the necessary API keys in the environment variables or configuration files<br>
     .\Tools\set-api-key.ps1 -ServiceName "bag" -APIKey "your-api-key-here"<br>
     .\Tools\set-api-key.ps1 -ServiceName "dso" -APIKey "your-api-key-here"<br>

5. **Access the application**<br>
   Open your browser and navigate to: http://localhost:1145/
   
6. **Follow the on-screen instructions**

### Manual execution
**Follow the instruction in .\Workflow.docx**<br>

## Third‑party Licenses

This project includes or uses code from `IFCtoLBD` by Jyrki Oraskari:
- Source: https://github.com/jyrkioraskari/IFCtoLBD/tree/master/IFCtoRDF
- License: [Apache License, Version 2.0] — see https://github.com/jyrkioraskari/IFCtoLBD/blob/master/IFCtoRDF/LICENSE
