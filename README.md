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
- [Configuration](#configuration)
- [Project Structure](#project-structure)
- [Development](#development)
- [Deployment](#deployment)
- [Contributing](#contributing)

---

## 🎯 Overview

VNG is a web application that integrates with Dutch government APIs (BAG, DSO) to process spatial planning data, manage land registry information, and provide geometry processing capabilities.

---

## ✨ Features

- **Spatial Planning Data Processing** - Integration with DSO (Digitaal Stelsel Omgevingswet)
- **BAG API Integration** - Dutch Address and Building registry lookup
- **Geometry Processing** - Handle and process geographic data
- **Land Registry Services** - Manage cadastral information
- **Docker Support** - Fully containerized for easy deployment
- **Structured Logging** - Serilog integration with file-based logging
- **Large File Support** - Handles uploads up to 2GB

---

## 📦 Prerequisites

Before you begin, ensure you have the following installed:

- [Docker Desktop](https://www.docker.com/products/docker-desktop) (latest version)
- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) (for local development)
- [Git](https://git-scm.com/) (for version control)

---

## 🚀 Quick Start

### Using Docker (Recommended)

1. **Clone the repository**