VisualVariableMonitoring
========================

This is a Unity PlugIn to help you track any public variable visually in realtime, as an overlay when you play.
You can directly see what impact has your interaction with a specifically tracked variable.

HowTo Tutorial :

0) Download VisualVariableMonitoring from Unity's Asset Store (Category: Scripting>Other)

1) To track a variable, you just have to add, above its declaration: DBG_Track()
You can also give a specific color to the tracking curve using three ratio-floats (red, green, blue) or a color name (all W3C Color table names are supported).

2) Attach the Component "CG_VisualVariableMonitoring.cs" to the Camera you're playing.

3) Be sure to activate "Gizmos" filter on you game window (Debug drawings requirement)

Enjoy !


You can adjust how the curves are drawing in the Component of course, but also when playing using Margin GUIButtons:


Margin Side = Choose if Margin infos are displayed on the "LeftSide", "RightSide" or not al all.

Margin Width = The ratio of the screen for the margin width

Layout Mode = You can choose if you want your curves drawn overlapping themselves (with their own ratio), or stacked.

Absolute Mode = Manage if you want to check the values amount only (turned ON), or if sign matters (turned OFF) and in that case 0.0f value will be the median value.

Opacity : Allow you to tweak the opacity of the curves.


========================
Author: Cyrille PAULHIAC
Email: contact@cosmogonies.net
WebSite: www.cosmogonies.net
========================


Known Limitations (or future roadmap goals):
* only float or float-castable are supported (incomming Vector3 and Quaternions).
* only public fields on MonoBehaviour's components are supported.

