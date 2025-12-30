# 🧠 AgiExperiment

<div align="center">

![AgiExperiment Banner](https://github.com/user-attachments/assets/8c08b77a-66a1-4d15-8b86-7c3a4ffa228f)

**A cutting-edge .NET platform for exploring Artificial General Intelligence concepts**

[![MIT License](https://img.shields.io/badge/License-MIT-green.svg)](https://choosealicense.com/licenses/mit/)
[![.NET](https://img.shields.io/badge/.NET-8.0-blue.svg)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/C%23-12.0-purple.svg)](https://docs.microsoft.com/en-us/dotnet/csharp/)
[![Blazor](https://img.shields.io/badge/Blazor-WebAssembly-ff69b4.svg)](https://dotnet.microsoft.com/apps/aspnet/web-apps/blazor)

[Features](#-features) • [Architecture](#-architecture) • [Getting Started](#-getting-started) • [Documentation](#-documentation) • [Contributing](#-contributing)

</div>

---

## 🌟 Overview

**AgiExperiment** is an advanced experimental platform built on .NET 8+ that serves as a sandbox for exploring and implementing Artificial General Intelligence (AGI) concepts. Combining the power of Blazor for rich interactive web experiences with AI capabilities through Azure OpenAI integration, this project provides a comprehensive environment for testing, visualizing, and experimenting with AGI-related ideas.

> **Note**: This project was built upon the excellent foundation provided by [BlazorGPT](https://github.com/magols/BlazorGPT) by [@magols](https://github.com/magols). Many thanks for creating such a solid starting point!

## ✨ Features

### 🎨 Modern Web Interface
- **Blazor Hybrid Architecture**: Interactive server-side and WebAssembly components
- **Fluent UI Design System**: Microsoft's latest design language for a polished, modern experience
- **Real-time Conversations**: Interactive chat interface with AI agents
- **Visual Diagrams**: Built-in diagram editor for visualizing AGI concepts and workflows

### 🤖 AI & AGI Capabilities
- **AI Cortex Pipeline**: Custom processing pipeline for AI interactions
- **Function Calling Framework**: Extensible function approval and execution system
- **Memory Management**: Persistent conversation history and context management
- **MCP Integration**: Model Context Protocol support via SSE (Server-Sent Events)

### 🔐 Enterprise-Ready
- **ASP.NET Identity**: Secure authentication and authorization
- **SQL Server Integration**: Robust data persistence layer
- **Service Discovery**: Built with .NET Aspire for cloud-native orchestration
- **Distributed Architecture**: Microservices-ready design with API service separation

### 🔧 Developer Experience
- **Aspire App Host**: Simplified local development and orchestration
- **Service Defaults**: Shared configuration across services
- **Comprehensive Testing**: Dedicated test project for reliability
- **Code-First Approach**: Entity Framework Core for database management

## 🏗️ Architecture

```
AgiExperiment/
├── 🎯 AgiExperiment.AppHost              # Aspire orchestration host
├── 🌐 AgiExperiment.Fluent.Web           # Blazor web frontend
│   ├── Server components (SSR)
│   └── Client components (WASM)
├── 🔌 AgiExperiment.ApiService           # REST API service
├── 🧠 AgiExperiment.AI.Cortex            # AI processing pipeline
├── 📦 AgiExperiment.AI.Domain            # Domain models & data
├── 🖥️ AgiExperiment.AI.ServiceHost       # AI service host
├── 🔗 AgiExperiment.MCP.AspNetCoreSseServer  # MCP protocol server
├── ⚙️ AgiExperiment.ServiceDefaults      # Shared configurations
└── 🧪 AgiExperiment.Tests                # Unit & integration tests
```

### Technology Stack

| Category | Technologies |
|----------|-------------|
| **Frontend** | Blazor Server, Blazor WebAssembly, Microsoft Fluent UI |
| **Backend** | ASP.NET Core 8.0, C# 12 |
| **AI/ML** | Azure OpenAI, Custom AI Pipeline |
| **Database** | SQL Server, Entity Framework Core |
| **Authentication** | ASP.NET Identity |
| **Orchestration** | .NET Aspire |
| **Protocols** | HTTP, SSE (Server-Sent Events), MCP |

## 🚀 Getting Started

### Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or later
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or [Visual Studio Code](https://code.visualstudio.com/)
- [SQL Server](https://www.microsoft.com/sql-server) (LocalDB or full installation)
- [Azure OpenAI](https://azure.microsoft.com/products/ai-services/openai-service) access (optional, for AI features)

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/julianls/AgiExperiment.git
   cd AgiExperiment
   ```

2. **Configure Azure OpenAI** (if using AI features)
   
   Update `appsettings.json` in `AgiExperiment.Fluent.Web` with your Azure OpenAI credentials:
   ```json
   {
     "AzureOpenAI": {
       "Endpoint": "your-endpoint",
       "ApiKey": "your-api-key",
       "DeploymentName": "your-deployment"
     }
   }
   ```

3. **Set up the database**
   ```bash
   dotnet ef database update --project AgiExperiment.Fluent.Web
   ```

4. **Run the application**
   ```bash
   dotnet run --project AgiExperiment.AppHost
   ```

5. **Open your browser**
   
   Navigate to the URL displayed in the console (typically `https://localhost:7xxx`)

### Quick Start with Aspire Dashboard

The Aspire AppHost provides a unified dashboard for managing all services:

```bash
cd AgiExperiment.AppHost
dotnet run
```

This launches:
- 🌐 Web Frontend (Fluent.Web)
- 🔌 API Service
- 🔗 MCP Server
- 📊 Aspire Dashboard for monitoring

## 📚 Documentation

### Key Components

#### 🧠 AI Cortex Pipeline
The AI processing pipeline handles:
- Request/response processing
- Function calling interception and approval
- Context management
- Memory persistence

#### 💬 Conversation System
- Real-time chat interface
- Streaming responses
- Function approval dialogs
- Conversation history and replay

#### 📊 Diagram Editor
Visual workflow and concept mapping using Z.Blazor.Diagrams for:
- AI agent workflow design
- Concept relationship mapping
- Process visualization

#### 🔗 MCP Integration
Model Context Protocol support enables:
- Standardized AI model communication
- Server-Sent Events for real-time updates
- Extensible protocol handlers

## 🎯 Use Cases

- **AGI Research**: Experiment with artificial general intelligence concepts
- **AI Agent Development**: Build and test autonomous AI agents
- **Workflow Visualization**: Design and visualize complex AI workflows
- **Function Calling**: Test and refine AI function execution patterns
- **Memory Systems**: Explore persistent context and memory management
- **Interactive Learning**: Educational platform for understanding AI architectures

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request. For major changes, please open an issue first to discuss what you would like to change.

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📝 License

This project is licensed under the MIT License - see the [LICENSE.txt](LICENSE.txt) file for details.

## 🔗 Resources

- [.NET Documentation](https://docs.microsoft.com/dotnet/)
- [Blazor Documentation](https://docs.microsoft.com/aspnet/core/blazor/)
- [Azure OpenAI Documentation](https://learn.microsoft.com/azure/ai-services/openai/)
- [.NET Aspire Documentation](https://learn.microsoft.com/dotnet/aspire/)

## 🙏 Acknowledgments

- **[@magols](https://github.com/magols)** for creating [BlazorGPT](https://github.com/magols/BlazorGPT), which served as the foundation and starting point for this project
- Microsoft Fluent UI team for the component library
- .NET Aspire team for cloud-native orchestration
- Azure OpenAI for AI capabilities
- The Blazor community for inspiration and support

---

<div align="center">

**Built with ❤️ using .NET and Blazor**

[Report Bug](https://github.com/julianls/AgiExperiment/issues) • [Request Feature](https://github.com/julianls/AgiExperiment/issues)

</div>