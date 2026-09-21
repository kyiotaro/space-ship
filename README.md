# Space Ship

A 2D space shooter demo made with Unity.

## Play the Demo

Play in your browser on GitHub Pages:

**https://kyiotaro.github.io/space-ship/**

For the best experience, use a desktop browser and click the fullscreen button in the game window.

## How to Play

Destroy enemy ships, earn experience, level up, and survive as long as possible. Enemies give experience when destroyed. Leveling up improves your progress and gives upgrade points for the player stats screen.

There is no separate victory screen in the current demo. The run ends when the player's health reaches zero.

## Controls

| Action | Control |
| --- | --- |
| Aim | Move the mouse |
| Fire | Left mouse button |
| Thrust forward | Hold right mouse button |
| Dash | Press `W`, `A`, `S`, or `D` |
| Open stats | `Tab` |
| Pause | `Esc` |
| Restart after game over | Left mouse button |

Your weapon has limited ammunition and reloads automatically. Watch the health and ammunition displays while fighting.

## Run Locally

1. Clone the repository.
2. Open the `Game/index.html` file through a local web server.
3. Open the local server URL in a desktop browser.

Do not open the HTML file directly with `file://`; Unity WebGL builds need to be served over HTTP.

## Project Structure

- `Assets/` - Unity project files and source scripts
- `Game/` - Built Unity WebGL demo
- `ProjectSettings/` - Unity project settings
