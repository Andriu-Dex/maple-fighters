<h1 align="center">🍁 Maple Fighters</h1>

<p align="center">
  <img src="docs/images/maplestory-icon.png" width="120px" height="120px" alt="Maple Fighters Logo"/>
</p>

<p align="center">
  <i>A small online game inspired by MapleStory - Battle monsters, level up, and explore!</i>
</p>

<p align="center">
  <a href="#features">Features</a> •
  <a href="#screenshots">Screenshots</a> •
  <a href="#quickstart">Quickstart</a> •
  <a href="#architecture">Architecture</a> •
  <a href="#contributing">Contributing</a>
</p>

<p align="center">
  <img src="https://img.shields.io/badge/Unity-2020.3.17-black?style=flat-square&logo=unity" alt="Unity"/>
  <img src="https://img.shields.io/badge/.NET-5.0-purple?style=flat-square&logo=dotnet" alt=".NET"/>
  <img src="https://img.shields.io/badge/React-18-blue?style=flat-square&logo=react" alt="React"/>
  <img src="https://img.shields.io/badge/Docker-Ready-blue?style=flat-square&logo=docker" alt="Docker"/>
  <img src="https://img.shields.io/badge/License-AGPL--3.0-green?style=flat-square" alt="License"/>
</p>

---

## 📖 About

Maple Fighters is an **open-source online multiplayer game** inspired by MapleStory where you battle monsters with others in real-time. The game runs entirely in your web browser using WebGL technology.

> ⭐ **Star this repo** if you like it! Made with ❤️ for the Open Source Community!

## ✨ Features

| Feature | Description |
|---------|-------------|
| 🎮 **WebGL Game** | Play directly in your browser, no installation required |
| 👤 **Account System** | Register with email and password, secure authentication |
| 🧙 **Character Classes** | Choose from Knight, Archer, or Wizard |
| ⚔️ **Combat System** | Battle monsters and gain experience points |
| 📈 **Progression** | Level up your character with persistent progress |
| 🗺️ **Multiple Maps** | Explore different areas like Lobby and The Dark Forest |
| 💬 **Chat System** | Communicate with other players in real-time |
| 🐳 **Docker Support** | Easy deployment with Docker Compose |
| ☸️ **Kubernetes Ready** | Scale with Kubernetes manifests included |

## 📸 Screenshots

<table>
  <tr>
    <td align="center"><b>Lobby</b></td>
    <td align="center"><b>The Dark Forest</b></td>
  </tr>
  <tr>
    <td><img src="docs/images/lobby.png" alt="Lobby" width="400"/></td>
    <td><img src="docs/images/the-dark-forest.png" alt="The Dark Forest" width="400"/></td>
  </tr>
</table>

## 🏗️ Architecture

```
maple-fighters/
├── src/
│   ├── frontend/          # React.js web application + Unity WebGL build
│   ├── game-service/      # .NET 5.0 game server (WebSocket)
│   └── maple-fighters/    # Unity client source code
├── lib/
│   └── interest-management/  # Spatial partitioning library
├── kustomize/             # Kubernetes configurations
├── release/               # Production manifests
└── docker-compose.yml     # Docker orchestration
```

### Technology Stack

| Layer | Technology | Description |
|-------|------------|-------------|
| **Game Engine** | Unity 2020.3.17 | Client-side game logic and rendering |
| **Client** | C# → WebAssembly | Compiled via IL2CPP for web browsers |
| **Frontend** | React.js | Web wrapper and UI components |
| **Server** | .NET 5.0 | Game server with WebSocket communication |
| **Proxy** | Nginx | Reverse proxy and static file serving |
| **Container** | Docker | Containerization and orchestration |

### Game Modes

| Mode | Description |
|------|-------------|
| **Offline Mode** | Single-player experience. Enemies are static (no AI behavior). Character progression is saved locally. |
| **Online Mode** | Full multiplayer experience with the game server. Enemies have AI behavior controlled by the server. Real-time interaction with other players. |

> ⚠️ **Note:** Enemy AI and behavior require the game server to be running. In offline/local mode, enemies will appear but won't move or attack.

## 🚀 Quickstart

### Prerequisites

- [Docker](https://docs.docker.com/get-docker/) and Docker Compose
- Git

### Running with Docker

```bash
# 1. Clone the repository
git clone https://github.com/Andriu-Dex/maple-fighters.git
cd maple-fighters

# 2. Build and start all services
docker compose up --build

# 3. Open in browser
# http://localhost:8080
```

### Services

| Service | Port | Description |
|---------|------|-------------|
| Frontend | 8080 | Web application (Nginx + React + WebGL) |
| Game Service | 50051 | Game server (WebSocket) |

### Stopping Services

```bash
docker compose down
```

## ☸️ Kubernetes Deployment

For production deployments, Kubernetes manifests are provided:

```bash
# Apply all resources
kubectl apply -f release/kubernetes-manifests.yaml

# Verify pods are running
kubectl get pods -n maple-fighters

# Get external IP
kubectl get service frontend-external -n maple-fighters
```

## 🎮 How to Play

1. **Start the game** - Open http://localhost:8080 in your browser
2. **Create account** - Enter your email and create a password
3. **Create character** - Choose a class (Knight, Archer, or Wizard) and name
4. **Explore** - Move with arrow keys, jump with spacebar
5. **Combat** - Attack monsters to gain experience and level up
6. **Chat** - Press Enter to chat with other players

### Controls

| Key | Action |
|-----|--------|
| ← → | Move left/right |
| ↑ | Jump |
| Space | Attack |
| Enter | Open chat |
| ESC | Game menu |

## 📁 Project Structure

```
src/
├── frontend/                    # Web frontend
│   ├── public/files/           # Unity WebGL build
│   ├── src/                    # React components
│   └── Dockerfile
│
├── game-service/               # Game server
│   ├── Game.Application/       # Application layer
│   ├── Game.Domain/           # Domain models
│   ├── Game.Infrastructure/   # Infrastructure
│   └── Game.Server/           # Server entry point
│
└── maple-fighters/            # Unity project
    └── Assets/
        └── Maple Fighters/
            └── Scripts/
                ├── Core/           # Infrastructure (SOLID)
                ├── Gameplay/       # Game mechanics
                ├── Services/       # API services
                └── UI/             # User interface
```

## 🔧 Development

### Unity Client

1. Open `src/maple-fighters` in Unity 2020.3.17
2. Open scene `Assets/Maple Fighters/Scenes/Main.unity`
3. Press Play to test in editor

### Building WebGL

1. In Unity: File → Build Settings → WebGL
2. Build to `src/frontend/public/files`
3. Rename output files from `files.*` to `WebGL.*`

### Game Server

```bash
cd src/game-service
dotnet restore
dotnet run --project Game.Server
```

## 📚 Documentation

- [Contributing Guidelines](CONTRIBUTING.md)
- [Code of Conduct](CODE_OF_CONDUCT.md)
- [Refactoring Documentation](REFACTORING_COMPLETE.md)

## 🤝 Contributing

Contributions are welcome! Please read the [contributing guidelines](CONTRIBUTING.md) before submitting a PR.

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 📜 License

This project is licensed under the [AGPL-3.0 License](LICENSE).

## ⚠️ Disclaimer

The artwork is owned by **Nexon Co., Ltd** and will never be used commercially. This is a fan-made project for educational purposes only.

---

<p align="center">
  <sub>Built with ❤️ by the open source community</sub>
</p>
