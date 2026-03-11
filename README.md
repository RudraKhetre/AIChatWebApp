# AI Chat Web App

An AI-powered chat application built using **ASP.NET Core** and the **Groq AI API**.
This project allows users to interact with an AI assistant through a simple web interface.

## 🚀 Live Demo

https://aichatwebapp.onrender.com

## 🧠 Features

* AI Chat interface
* Integration with Groq AI API
* Built with ASP.NET Core MVC
* Fast responses from AI models
* Simple and clean UI
* Deployed online

## 🛠️ Technologies Used

* ASP.NET Core
* C#
* HTML
* CSS
* JavaScript
* Groq AI API
* Docker
* Render (Hosting)

## 📂 Project Structure

```
AIChatWebApp
│
├── Controllers
│   └── ChatController.cs
│
├── Models
│   └── ChatRequest.cs
│
├── Services
│   └── GroqService.cs
│
├── Views
│
├── wwwroot
│
├── Program.cs
├── appsettings.json
└── Dockerfile
```

## ⚙️ Installation

Clone the repository:

```
git clone https://github.com/RudraKhetre/AIChatWebApp.git
```

Navigate to the project folder:

```
cd AIChatWebApp
```

Run the project:

```
dotnet run
```

Open in browser:

```
https://localhost:5001
```

## 🔑 Environment Variables

Before running the project, set your **Groq API Key**.

Example:

```
Groq__ApiKey=YOUR_GROQ_API_KEY
```

## 🐳 Docker Support

Build Docker image:

```
docker build -t aichatapp .
```

Run container:

```
docker run -p 5000:80 aichatapp
```

## 🌐 Deployment

The project is deployed using:

* Render
* Docker container

## 👨‍💻 Author

Rudra Khetre
Software Developer

GitHub: https://github.com/RudraKhetre

## 📜 License

This project is open source and available under the MIT License.
