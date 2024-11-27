# Prefab Painter

PrefabPainter is a Unity Editor extension designed to enhance the level design process by allowing artists and developers to paint prefabs directly onto the Unity scene with adjustable settings. It supports random rotation, scaling, and weighted prefab selection to create varied and dynamic environments quickly.

## Features

- **Prefab Selection with Odds**: Define multiple prefabs with specific odds to control their likelihood of being placed.
- **Random Rotation**: Option to rotate prefabs randomly around a specified axis.
- **Random Scaling**: Scale prefabs randomly within a defined range.
- **Brush Size**: Adjust the size of the area where prefabs can be painted.
- **Prefab Offset**: Set a vertical offset for prefab placement to ensure correct positioning relative to the ground.
- **Paint Sensitivity**: Control the sensitivity of the paint brush to mouse movement, allowing for denser or more spread out placement.
- **Undo System**: Easily undo the last painted prefab or clear all painted prefabs from the scene.
- **Customizable Rotation Axis**: Choose the axis on which the prefabs will rotate, offering further customization for alignment in the scene.

## Installation

To install PrefabPainter, follow these steps:

1. Clone this repository or download the latest release.
2. Copy the `PrefabPainter.cs` script into your Unity project's `Assets/Editor` folder.

## Usage

To use PrefabPainter, you can follow these simple steps:

1. Open Unity and navigate to `Tools > Prefab Painter` to open the PrefabPainter window.
2. Click "Add New Prefab Entry" to add prefabs to your tool. Set the odds for each prefab to control their placement frequency.
3. Adjust the brush size, prefab offset, rotation and scale settings as needed.
4. Toggle "Start Painting" to begin painting prefabs in your scene. Use your mouse to click or drag in the scene view where you want prefabs to appear.
5. Use the "Undo Last Paint" or "Clear All Painted Prefabs" to manage your prefab placements.

## Contributing

Contributions to PrefabPainter are always welcome, whether they be improvements to the code, bug reports, or suggestions for new features. Please feel free to fork this repository, make changes, and submit pull requests.

## License

Distributed under the MIT License. See `LICENSE` for more information.
