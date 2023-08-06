# [4D Viewer Web](https://dearsip.github.io/FourDViewerWeb/)
[4D Blocks](http://www.urticator.net/blocks/) on WebXR

## How to Play

### Touch

Check "**Show Hints**" in the **Touch/Stereo** tab.

Align mode is toggled by double-clicking the alignment button.

### XR

- **Right Primary Button (A)**: Rotate by moving
- **Left Primary Button (X)**: Move by moving (twist to move forward)
- **Right Secondary Button (B)**: Slice
- **Left Secondary Button (Y)**: Restrict to 3D
- **Left/Right Grip**: Move the retina
- **Right Trigger**: Select Block, Jump
- **Left Trigger**: Alignment
- **Right Stick Right**: Add Block, Shoot
- **Right Stick Left**: Remove Block

You cannot open the menu in XR environment. When you exit XR mode, the menu will open automatically.

### Keyboard

- **WERASDFZ**: Move
- (Shift +) **UIOJKL**: Rotate
- **Esc**: Open/Close Menu
- **Return**: Alignment
- **G**: Slice
- **T**: Restrict to 3D
- **Space**: Select Block, Jump
- **M**: Add Block, Shoot
- **N**: Remove Block
- **1-9,0**: Change Texture
- **XCV**: Change Train Speed
- **Q**: Change Track
- **Y**: Fisheye Mode
- **P**: Paint
- **H**: Use Edge Color
- **B**: Invert Normal
- **,**: Hide Selection
- **.**: Separate

Only move and rotate are supported in the key config. Menu items can only be operated by touch or mouse. Some of the setting edit keys only work in the related scenes.

## Reference

For basic functions and scene files, see the documentation for [4D Maze](http://www.urticator.net/maze/) and [4D Blocks](http://www.urticator.net/blocks/v6/). The following are notes on the functions unique to this application.

- "**3D**" button generates a three-dimensional maze by turning on the three-dimensional operation restrictions and slicing function instead of making the dimension of space three. Conversely, the "**4D**" button generates a four-dimensional maze by releasing the operation restriction and slice. The settings are shared between three and four dimensions. If you want to switch the spatial dimensions, turn on the "**3D Maze in 3D Scene**" in the **Seed/Fisheye** tab. "**New Game**" starts a new maze while retaining the above settings.
- Settings related to drawing are reflected immediately. Also, the settings are saved to IndexedDB when the menu window is closed. Note that some settings can only be changed via touch or mouse.
- Sliders are placed in the numerical setting items, but you can also enter directly into the input field, and set values outside the range of the slider for some items. The in-game keyboard edits the last character of the text regardless of the cursor position. Be aware that sometimes the text is too long to see the end.

### Map

- The original map generation algorithm generates branches until the density is satisfied even if the "**Branch Probability**" is set to 0, so "**Allow Reserved Path**" property is added. By turning this off together with the branch probability setting, you can generate a *single path maze*.

### Color/View

- The "**By Trace**" color mode paints the rooms you have reached black. Also, if you turn on "**Use Arrow**", an arrow will be displayed to indicate the direction you have come from. Both can be used to solve the maze. "**Reset Win**" (not "**Restart**") clears these records as well.
- To simplify the appearance of the start and finish, the interior color itself is changed instead of drawing additional textures. You can revert to the original display with "**Use Start & Finish Mark**".

### Display

This tab contains the settings that accepted only key input in the original and the settings for the additional functions related to visuals.

- This application draws 2D planes. The transparency of the planes and the width of the lines can be set with "**Transparency**" and "**Line Thickness**". (The width of  lines in the cut plane and the 3D scene is doubled.) Since drawing the planes can be extremely expensive, especially in scenes with many blocks, you can reduce the load by unchecking the box. (In this case, the cut plane will not be drawn either.)
- "**Size**" changes the size of the retina, mainly for XR.
- "**Allow Glide**" removes the friction of the blocks and allows you to move along the walls. While it makes it less likely to get stuck on walls, it may cause the input and the direction of movement to be different, so if you prefer the original behavior, turn it off.
- "**Toggle Skybox**" changes the background to black, the *black lines of the frame to white*, to make it look more like the original. Changing the color of the lines may also be useful in AR mode.
- "**Show Map**" is realized by arranging blocks with the same configuration as the maze. The cells facing the unreached path are drawn in white. The blocks are transparent so that they do not put a load on the drawing, and you can toggle the transparency by turning off "**Glass**". "**Distance**" changes the distance between the map and the retina. "**Focus**" moves the camera to the position of the map.
- "**Camera Distance**" moves the rendering position to the back of the player position (except in Maze).

### Control/Slice

This tab contains the settings for the additional functions related to the control.

- Motion controller and touchpad control can be selected from two types. The default ("**Drag**") reflects the amount of movement of the controller directly in the amount of movement of the target. In "**Joystick**", the target moves at a constant speed proportional to the difference while the button is pressed. The movement speed with Joystick is affected by the speed setting in the "**Motion**" tab, but not by Drag.
- "**Invert**" reverses the input. If you want to operate the map like a block, turn on everything except "**Left & Right**".
- Turning on "**Slice Mode**" allows you to select slices in different directions. It currently has no effect on controls, so it is recommended to use it in conjunction with align mode. The two sliders below, "**Base Transparency**" and "**Slice Transparency**", determine the transparency of the original image and the slice, respectively.
- "**Show Input**" displays the touch position and an arrow indicating  motion controller input.
- "**Keep Up & Down**" prevents the camera from tilting sideways. The change will take effect the next time you restart or load. While this is functioning, you cannot align or switch to align mode. It does not affect block motion.

### Cmd/Motion

This tab contains the settings for the commands related to the scene and the original movement speed.

- "**Add**" and "**Paint**" share the Color property. If you enter a color code starting with # in "**Custom Color**", it will be applied with priority.
- "**Paint with Add Button**" replaces the Add button with Paint. If you combine it with the "**in order**" property added to the Color property, you may be able to color multiple colors quickly.

### Touch/Stereo

This tab controls the settings for touch input and the camera (projecting the three-dimensional retina onto the screen).

- If you turn off "**Allow Diagonal Movements**", you will not move left or right when moving forward or backward in touch input. In XR, horizontal movement itself is prohibited.
- Touch control in the 3D scene is somewhat redundant for compatibility with 4D. Turning on "**Alternative Control in 3D Scene**" eliminates the need for modifiers (see Hints).
- "**Modifiers Toggle Mode**" replaces the behavior of modifiers from momentary to toggle. This is essential when you want to operate with a mouse. Note: "Left" edits *the left button that modifies the right hand control*.
- Even if you turn off "**Show Controller**", you can still input normally.
- The display of "**Show Hints**" reflects the state of modifiers and Alternative Control. Other settings have no effect.
- **"Reset**" resets the camera position to the front.
- The result of touch input is not affected by the direction in which the camera projects the retina. If you always want to input as it looks, turn on "**Horizontal Input Following**". Note that this setting currently does not apply to key input.
- "**Invert**" changes the direction of camera rotation.
- "**FoV**" widens the field of view and moves the retina to the center of the screen. Since the stereo mode occupies the entire screen including the controller, it is better to change this setting.

### I/O

- "**Load**" allows you to select the original scene files ("**data**") and *meager* additional files ("**levels**") with specifications described in the next section. Each file in "data" is arranged in the order of [the original reference](http://www.urticator.net/blocks/v6/examples.html).
- "**Import/Export**" allows you to read and write local files. The property file is not compatible with the original. The following Action, Block scenes support saving, but the Shooter scene does not.

### Scene Language

- Scene files with "**finishinfo**" become **Action** scene. Gravity and jumping are added to the scene, and the coordinates specified in finishinfo become the goal. The rendering position is 0.5 points above the collision detection point. "**Footinfo**" displays a mark at your current position. Other than the goal, the features are common to the following scenes.
- Scene files with "**blockinfo**" become **Block** scene. The Add command adds a block in the direction of sight. The block to be added is the object that was deleted immediately before. These functions are not currently supported except for the unit hypercube.
- Scene files with "**shootinfo**" become **Shooter** scene. The Add command fires a bullet and clears the target.
- You can edit the map structure directly in the save data. Edit "**map**", "**start**", and "finish" in the save data. You cannot specify the color directly.
- You can read URLs with "**include**". You cannot load a scene directly from a URL.
