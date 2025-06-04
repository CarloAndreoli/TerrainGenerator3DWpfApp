# 🏔️ Terrain Generator and 3D WPF Representation

A lightweight WPF application that procedurally generates 3D terrain using the Diamond-Square algorithm and visualizes it in real-time with WPF's `Viewport3D`.

## ✨ Features

- 🧠 Procedural terrain generation with Diamond-Square algorithm  
- ⚙️ Adjustable parameters for detail and roughness  
- 🎮 Interactive 3D camera navigation  
- 🌄 Real-time 3D rendering using `MeshGeometry3D`  
- 💡 Lighting and material setup for enhanced realism  
- 🔁 Regenerate terrain on demand  

## 🖼️ Preview

*(Add a screenshot or GIF here if available)*

## 🚀 Getting Started

### Prerequisites

- [.NET Framework 4.5+](https://dotnet.microsoft.com/en-us/download/dotnet-framework)  
- Windows OS with WPF support  
- Visual Studio (recommended)

### Clone and Run

```bash
git clone https://github.com/your-username/terrain-generator-wpf.git
cd terrain-generator-wpf
Open the solution in Visual Studio and run the project.
```

## 🧩 Project Structure

- `terrainGenerator.cs` – Implements the Diamond-Square terrain generation algorithm.  
- `MainWindow.xaml` / `MainWindow.xaml.cs` – UI and 3D visualization logic.

## ⚙️ How It Works

The terrain is generated as a heightmap using the Diamond-Square algorithm, then translated into a 3D mesh rendered in a WPF `Viewport3D`. You can control the level of detail and randomness to simulate various landscapes.

## 📚 References

My original article on codeproject:  
[Terrain Generator and 3D WPF Representation on CodeProject](https://www.codeproject.com/Articles/1194994/Terrain-Generator-and-3D-WPF-Representation)

---

Made with ❤️ for learning and experimentation.
