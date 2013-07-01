Character Model Detection
=========================
Using the Unity game engine to detect characters in images/screenshots with a list of provided parameters.
=========================

In this project we will be leveraging Unity to do the following things:

- Create images/screenshots of characters to be used as practice or training for the program. 
- The image takes the place of the real life application.
- Create an automatic element to generate scenes that will be used in comparison with the images.
- These scenes will make use of multiple camera angles and lighting angles to simulate the different possibilities.

- Most importantly, the scene models will be based off only the character outline, not a rectangle surrounding the model.


A more basic explanation:

- Take a picture
- Create models of different appearances
- The area in the picture that is of closest match is our 'person'

=========================

The expected result should be greater accuracy in detection due to the use of character outlines and no surrounding area.

=========================

Short list of ideas for starting points:

- The scripting in Unity allows for some dynamic GUI elements to be provided, therefore rather than creating new camera/lighting elements, it would be easier to adjust 2 constant ones, this will allow the user to see the manipulations more.
- Guide to adding GUI here: http://docs.unity3d.com/Documentation/Components/GUIScriptingGuide.html
- Free character models are available from the Unity Assets Store

=========================
