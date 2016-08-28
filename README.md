# GuiItems
Gui system for Unity3d relying on the good ol' scripts in OnGui(), supporting any screen ratio!
I tried to make the system as efficient as possible and it works well on smartphones (no noticeable effect on performance). You should get as much draw calls as you would get using plain scripting.

The ratio adaptation is simply playing around the Gui.matrix. You just have to define a design resolution, let's say 1024x768 or 1920x1080, and depending on the ratio and the size of the screen, the elements will resize and be positioned accordingly.

There is a working rudimentary editor but it is not yet documented.

# Usage
Clone / download the entire repository to launch the example project which will give you a first idea of how it works.
Alternatively, you can just grab the unity package from the Package folder and import it in one of your projects. The package does not contain the example project.

# To do
* Make a "how to" which explains things, with screenshots.

# Version of Unity used
* Unity 4.7.1f1
* Should work on Unity 5+
